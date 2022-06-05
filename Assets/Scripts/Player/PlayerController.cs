using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;


/*
 * Deals with Player's Inputs, States and which methods to call each frame.
 * Acts as the central hub for interacting with all Player components.
 */
public class PlayerController : CharacterController<PlayerHealth>
{
    [SerializeField] private PlayerMovement m_PlayerMovement;
    [SerializeField] private PlayerParentMovement m_PlayerParentMovement;
    [SerializeField] private int m_LostMothCountWinCondition = 10;
    [Tooltip("CameraMovement component")]
    [SerializeField] private CameraMovement m_CameraMovement;
    [SerializeField] private PlayerWeapon m_Weapon;
    [SerializeField] private MoonBarAbility m_MoonBarAbility;

    [Header("Effects")]
    [SerializeField] private float m_SlowEffectDuration = 3f;
    [SerializeField] private float m_FogTransitionDuration = 1f;

    [Header("Lost Moth")]
    [SerializeField] private TextMeshProUGUI m_LostMothUI;
    [SerializeField] private float m_LostMothUIDisplayTime = 1.5f;

    [Header("Sound")]
    [SerializeField] private AudioSource m_AimModeStartSound;
    [SerializeField] private AudioSource m_AimModeEndSound; 

    private PLAYER_ACTION_STATE m_playerState;  // Current player state given the actions performed / effects applied
    Coroutine ShootCoroutine;                   // Coroutine called when performed shooting action to allow cancelling the coroutine
    private int m_LostMothCount = 0;
    private Vector2 m_MovementInput;            // Input object for moving player along x-y axis

    private DamageInfo.HIT_EFFECT m_CurrEffect;   // Current hit effect applied to player
    private Coroutine m_SlowEffectCoroutine;
    private Coroutine m_TransitionCoroutine;

    public MoonBarAbility MoonBarAbility { get { return m_MoonBarAbility; }}

    // Add a new enemy's boost duration to list
    public void OnEnemyKilled()
    {
        m_MoonBarAbility.AddEnemyKilled();
    }

    private void Awake()
    {
        m_LostMothUI.enabled = false;
        m_CurrEffect = DamageInfo.HIT_EFFECT.NORMAL;
    }

    protected override void Start()
    {
        base.Start();
        m_playerState = PLAYER_ACTION_STATE.FLYING;

        m_MoonBarAbility.d_AimModeStartDelegate = AimModeStart;
        m_MoonBarAbility.d_AimModeEndDelegate = AimModeEnd;
        m_MoonBarAbility.d_DashStartDelegate = DashModeStart;
        m_MoonBarAbility.d_DashEndDelegate = DashModeEnd;

        m_Health.d_DamageDelegate = OnDamageTaken;
    }

    private void OnDamageTaken(DamageInfo damageInfo) 
    {
        m_CameraMovement.StartCameraShake();
        ApplyEffect(damageInfo.m_HitEffect);
    }

    private void AimModeStart() {
        m_AimModeStartSound.Play();
        m_PlayerMovement.AimModeEnter();
        m_CameraMovement.CameraAimModeZoom();
    }
    private void AimModeEnd() {
        m_AimModeEndSound.Play();
        m_PlayerMovement.AimModeExit();
        m_CameraMovement.ResetZoom();
    }
    private void DashModeStart() {
        m_PlayerParentMovement.DashStart();
        m_CameraMovement.CameraDashZoom();
    }
    private void DashModeEnd() {
        m_PlayerParentMovement.DashEnd();
        m_CameraMovement.ResetZoom();
    }

    public void OnMove(InputValue value)
    {
        m_MovementInput = value.Get<Vector2>();
    }

    // Main Update controller for all Player components, Dealing with actions/effects that happen each frame
    void Update()
    {
        CheckWin();

        if (!TileManager.PropertyInstance.IsInitialized)
            return;

        m_Weapon.TryShoot();
        m_MoonBarAbility.UpdateAimModeEnemyKilledList();
        m_MoonBarAbility.UpdateAimModeReticleBar();

        m_Health.LosePassiveHealth();
        m_PlayerMovement.RotationLook();

        // move parent along spline
        m_PlayerParentMovement.TryMove();

        if (m_playerState == PLAYER_ACTION_STATE.FLYING || m_playerState == PLAYER_ACTION_STATE.DASHING)
        {
            m_PlayerMovement.HorizontalRotation(m_MovementInput.x);
        }

        m_PlayerMovement.MothXYMovemnent();
    }

    private void FixedUpdate()
    {
        if (m_playerState == PLAYER_ACTION_STATE.FLYING || m_playerState == PLAYER_ACTION_STATE.DASHING)
        {
            // move player body along local x, y plane based on inputs
            m_PlayerMovement.ControlPointXYMovement(m_MovementInput);
        }
        m_PlayerMovement.UpdateCrossHair();
    }

    public float DistanceFromPlayer(Vector3 pointToCompare)
    {
        return Vector3.Distance(pointToCompare, transform.position);
    }

    public void OnFireStart(InputValue value)
    {
        m_Weapon.IsShooting = true;
    } 

    public void OnFireStop(InputValue value)
    {
        m_Weapon.IsShooting = false;
    }

    public void OnDodge(InputValue value)
    {
        if (m_playerState == PLAYER_ACTION_STATE.FLYING)
        {
            m_playerState = PLAYER_ACTION_STATE.DODGING;
            StartCoroutine(m_PlayerMovement.PlayerDodge(FinishAction, m_MovementInput));
        }   
    }

    public void OnAimModeStart(InputValue value)
    {
        if (m_CurrEffect == DamageInfo.HIT_EFFECT.SLOW) return;

        m_MoonBarAbility.AimModeStartHelper();
    }

    public void OnAimModeEnd(InputValue value)
    {
        m_MoonBarAbility.AimModeEndHelper();
    }

    public void OnDashStart(InputValue value)
    {
        if (m_CurrEffect == DamageInfo.HIT_EFFECT.SLOW) return;
        m_MoonBarAbility.OnDashStartHelper();
    }

    public void OnDashEnd(InputValue value)
    {
        m_MoonBarAbility.OnDashEndHelper();
    }

    public void LostMothCollected()
    {
        m_LostMothCount++;
        m_LostMothUI.text = m_LostMothCount.ToString();
        StartCoroutine(LostMothDisplay());
    }

    IEnumerator LostMothDisplay()
    {
        m_LostMothUI.enabled = true;
        yield return new WaitForSeconds(m_LostMothUIDisplayTime);
        m_LostMothUI.enabled = false;
    }

    public override void Death() 
    {
        GameManager.PropertyInstance.OnGameOver();
    } 

    public void CheckWin() 
    {
        // if not currently transitioning and met the lost moth win threshold for the tile set
        if (m_LostMothCount >= m_LostMothCountWinCondition)
        {
            m_TransitionCoroutine = StartCoroutine(TransitionPhase());
        }        
    }

    // Fogs up player's screen, and calls next logic for the transition
    private IEnumerator TransitionPhase()
    {
        m_LostMothCount = 0;
        Debug.Log("Start fog screen");

        // start fog transition
        float currDuration = 0;
        while (currDuration < m_FogTransitionDuration)
        {
            if (GameState.m_GameState == GameStateEnum.RUNNING) break;
            yield return null;
        }
        Debug.Log("Screen fogged");

        // Call rest of transition logic
        m_PlayerParentMovement.DisconnectFromSpline();
        GameManager.PropertyInstance.OnAllLostMothsCollected();
        
        // wait for transition state to be over
        while (GameState.m_GameState != GameStateEnum.RUNNING)
        {
            yield return null;
        }

        m_PlayerParentMovement.ConnectBackToSpline();

        m_PlayerMovement.ResetPosition();

        // start defog transition
        currDuration = 0;
        Debug.Log("Start defog screen");
        while (currDuration < m_FogTransitionDuration)
        {
            if (GameState.m_GameState == GameStateEnum.RUNNING) break;
            yield return null;
        }
        Debug.Log("Screen defogged");
    }

    private void FinishAction()
    {
        m_playerState = PLAYER_ACTION_STATE.FLYING;
    }

    protected override void ApplyEffect(DamageInfo.HIT_EFFECT effect)
    {
        if (effect == DamageInfo.HIT_EFFECT.SLOW)
        {
            ApplySlow();
            // Cancel any moon abilities that are currently being used
            m_MoonBarAbility.OnDashEndHelper();
            m_MoonBarAbility.AimModeEndHelper();
        }
    }

    private void ApplySlow()
    {
        // cancel currently running slow effect and reset if one running
        if (m_SlowEffectCoroutine != null)
        {
            StopCoroutine(m_SlowEffectCoroutine);
        }
        m_SlowEffectCoroutine = StartCoroutine(SlowEffectCoroutine());
    }

    IEnumerator SlowEffectCoroutine()
    {
        m_CurrEffect = DamageInfo.HIT_EFFECT.SLOW;
        m_PlayerParentMovement.SLowEffectStart();
        yield return new WaitForSeconds(m_SlowEffectDuration);
        m_CurrEffect = DamageInfo.HIT_EFFECT.NORMAL;
        m_PlayerParentMovement.SlowEffectStop();
    }

    // Converts a world position to local wrt player and returns z distance from the camera
    // Negative Z-distance means the positions is behind the player camera
    public float ZDistanceFromPlayerCamera(Vector3 position)
    {
        // COnvert firefly position to player's parent local space
        Vector3 localPlayerPos = PlayerParent.transform.InverseTransformPoint(position);
        return localPlayerPos.z;
    }

    public PlayerParentMovement PlayerParent { get { return m_PlayerParentMovement;  } }
    public PlayerMovement PlayerMovement { get { return m_PlayerMovement; } }
    public CameraMovement CameraMovement { get { return m_CameraMovement; } }

    enum PLAYER_ACTION_STATE
    {
        FLYING,
        DODGING,
        DASHING
    }
}
