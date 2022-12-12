using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;


/*
 * Deals with Player's Inputs, States and which methods to call each frame.
 * Acts as the central hub for interacting with all Player components.
 */
public class PlayerController : CharacterController<PlayerHealth>
{
    [SerializeField] private PlayerMovement m_PlayerMovement;
    [SerializeField] private PlayerParentMovement m_PlayerParentMovement;

    [Tooltip("CameraMovement component")]
    [SerializeField] private CameraMovement m_CameraMovement;
    [SerializeField] private PlayerWeapon m_Weapon;
    [SerializeField] private MoonBarAbility m_MoonBarAbility;

    [Header("Effects")]
    [SerializeField] private float m_SlowEffectDuration = 3f;
    [SerializeField] private float m_FogInTransitionDuration = 1f;
    [SerializeField] private float m_FogOutTransitionDuration = 3f;

    [Header("Lost Moth")]
    [SerializeField] private TextMeshProUGUI m_LostMothUI;

    [Header("Sound")]
    [SerializeField] private AudioSource m_AimModeStartSound;
    [SerializeField] private AudioSource m_AimModeEndSound;

    [Header("Dodge")]
    [SerializeField] private float m_DodgeCooldown = 1;

    [Header("Animation")]
    [SerializeField] private float m_GlideFlapDelayMin = .2f;
    [SerializeField] private float m_GlideFlapDelayMax = .5f;

    private PlayerInput m_PlayerInput; 

    private PLAYER_ACTION_STATE m_playerState;  // Current player state given the actions performed / effects applied

    private int m_LostMothCount = 0;
    private Vector2 m_MovementInput;            // Input object for moving player along x-y axis

    private DamageInfo.HIT_EFFECT m_CurrEffect;   // Current hit effect applied to player
    private Coroutine m_SlowEffectCoroutine;

    private bool m_IsDodgeCooldown;
    private Animator m_Animator;
    private bool m_IsFirstUpdate = true;

    public MoonBarAbility MoonBarAbility { get { return m_MoonBarAbility; }}

    // Add a new enemy's boost duration to list
    public void OnEnemyKilled()
    {
        m_MoonBarAbility.AddEnemyKilled();
    }

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_CurrEffect = DamageInfo.HIT_EFFECT.NORMAL;
        m_PlayerInput = FindObjectOfType<PlayerInput>();
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
        UpdateLostMothText();

        //InputSystem.EnableDevice(Accelerometer.current);
        //InputSystem.EnableDevice(AttitudeSensor.current);
        //InputSystem.EnableDevice(GravitySensor.current);
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

        if (m_playerState == PLAYER_ACTION_STATE.FLYING || m_playerState == PLAYER_ACTION_STATE.DASHING)
        {
            m_PlayerMovement.HorizontalRotation(m_MovementInput.x);
        }

        if (m_IsFirstUpdate)
        {
            UIManager.PropertyInstance.FadeOut(m_FogOutTransitionDuration);
            m_CameraMovement.ResetPosition();
            m_IsFirstUpdate = !m_IsFirstUpdate;
        }
    }

    private void FixedUpdate()
    {
        // Game ended or transitioning to new tileset
        if (GameState.m_GameState != GameStateEnum.RUNNING) return;

        // move parent along spline
        m_PlayerParentMovement.TryMove();

        if (m_playerState == PLAYER_ACTION_STATE.FLYING || m_playerState == PLAYER_ACTION_STATE.DASHING)
        {
            if (AttitudeSensor.current != null && AttitudeSensor.current.enabled)
            {
                Quaternion attitude = AttitudeSensor.current.attitude.ReadValue();
                float inputY = Math.Clamp(attitude.eulerAngles.x, -20, 20);
                inputY = (Math.Abs(inputY) / 20) * (inputY < 0 ? 1 : -1);

                float inputX = Math.Clamp(attitude.eulerAngles.z, -45, 45);
                inputX = (Math.Abs(inputX) / 45) * (inputX < 0 ? 1 : -1);
                // move player body along local x, y plane based on inputs
                m_PlayerMovement.ControlPointXYMovement(new Vector2(inputX, inputY));
            } else
            {
                // move player body along local x, y plane based on inputs
                m_PlayerMovement.ControlPointXYMovement(m_MovementInput);
            }
            
        } else
        {
            // move player only along parent movement
            m_PlayerMovement.ControlPointXYMovement(new Vector2(0, 0));
        }

        m_PlayerMovement.UpdateCrossHair();
        m_PlayerMovement.MothXYMovemnent();

        if (AttitudeSensor.current != null && !AttitudeSensor.current.enabled)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
        }
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
        if (m_playerState == PLAYER_ACTION_STATE.FLYING && !m_IsDodgeCooldown)
        {
            m_playerState = PLAYER_ACTION_STATE.DODGING;
            m_Health.SetProjectileInvulnFrames(m_PlayerMovement.GetDodgeDuration());
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
        UpdateLostMothText();
    }

    private void UpdateLostMothText()
    {
        m_LostMothUI.text = m_LostMothCount.ToString() + "/" + GameManager.PropertyInstance.CurrLostMothWinCondition();
    }

    public override void Death() 
    {
        GameManager.PropertyInstance.OnGameOver();
    } 

    public void CheckWin() 
    {
        // if not currently transitioning and met the lost moth win threshold for the tile set
        if (m_LostMothCount >= GameManager.PropertyInstance.CurrLostMothWinCondition())
        {
            WinLevel();
        }        
    }

    // Set current level as won, goto next or win game
    public void WinLevel()
    {
        StartCoroutine(TransitionPhase());
    }

    // Fogs up player's screen, and calls next logic for the transition
    private IEnumerator TransitionPhase()
    {
        // invincible during transition
        m_Health.SetAllInvulnFrames(3f);
        m_LostMothCount = 0;
        UIManager.PropertyInstance.FadeIn(m_FogInTransitionDuration);

        yield return new WaitForSeconds(m_FogInTransitionDuration);

        m_PlayerInput.enabled = false;
        // Call rest of transition logic
        m_PlayerParentMovement.DisconnectFromSpline();
        GameManager.PropertyInstance.OnAllLostMothsCollected();
        
        // wait for transition state to be over to reconnect to spline
        while (GameState.m_GameState != GameStateEnum.RUNNING)
        {
            yield return null;
        }

        m_PlayerParentMovement.ConnectBackToSpline();
        m_CameraMovement.ResetPosition();
       

        UIManager.PropertyInstance.FadeOut(m_FogOutTransitionDuration);
        UpdateLostMothText();
        m_PlayerInput.enabled = true;
    }

    private void FinishAction()
    {
        // if player was dodging, start dodge cooldown
        if (m_playerState == PLAYER_ACTION_STATE.DODGING)
        {
            StartCoroutine(DodgeCooldown());
        }

        m_playerState = PLAYER_ACTION_STATE.FLYING;
    }

    IEnumerator DodgeCooldown()
    {
        m_IsDodgeCooldown = true;
        yield return new WaitForSeconds(m_DodgeCooldown);
        m_IsDodgeCooldown = false;
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

    public void OnGlideFlapAnimationPerformed() 
    {
        StartCoroutine(GlideDelay());
    }

    IEnumerator GlideDelay()
    {
        m_Animator.SetBool("isGlideDelay", true);
        float rand = UnityEngine.Random.Range(m_GlideFlapDelayMin, m_GlideFlapDelayMax);
        yield return new WaitForSeconds(rand);
        int randAnim = UnityEngine.Random.Range(0, 2);
        if (randAnim == 0)
            m_Animator.SetBool("isLargeFlap", true);
        else
            m_Animator.SetBool("isLargeFlap", false);
        m_Animator.SetBool("isGlideDelay", false);
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
