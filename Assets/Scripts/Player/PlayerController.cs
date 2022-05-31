using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Lost Moth")]
    [SerializeField] private TextMeshProUGUI m_LostMothUI;
    [SerializeField] private float m_LostMothUIDisplayTime = 1.5f;

    private PLAYER_ACTION_STATE m_playerState;  // Current player state given the actions performed / effects applied
    Coroutine ShootCoroutine;                   // Coroutine called when performed shooting action to allow cancelling the coroutine
    private int m_LostMothCount = 0;
    private Vector2 m_MovementInput;        // Input object for moving player along x-y axis

    public MoonBarAbility MoonBarAbility { get { return m_MoonBarAbility; }}

    // Add a new enemy's boost duration to list
    public void OnEnemyKilled()
    {
        m_MoonBarAbility.AddEnemyKilled();
    }

    private void Awake()
    {
        m_LostMothUI.enabled = false;
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

    private void OnDamageTaken()
    {
        m_CameraMovement.StartCameraShake();
    }

    private void AimModeStart() {
        m_PlayerMovement.AimModeEnter();
        m_CameraMovement.CameraAimModeZoom();
    }
    private void AimModeEnd() {
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
        // TODO: put logic inside Weapon
        ShootCoroutine = StartCoroutine(m_Weapon.Shooting());
    } 

    public void OnFireStop(InputValue value)
    {
        // TODO: put logic inside Weapon
        StopCoroutine(ShootCoroutine);
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
        m_MoonBarAbility.AimModeStartHelper();
    }

    public void OnAimModeEnd(InputValue value)
    {
        m_MoonBarAbility.AimModeEndHelper();
    }

    public void OnDashStart(InputValue value)
    {
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
        GameState.IsGameWon = false;
        SceneManager.LoadScene("WinLose");
    } 

    public void CheckWin() 
    {
        GameState.IsGameWon = true;
        if (m_LostMothCount >= m_LostMothCountWinCondition)
            SceneManager.LoadScene("WinLose");
    }

    private void FinishAction()
    {
        m_playerState = PLAYER_ACTION_STATE.FLYING;
    }

    protected override void ApplyEffect(DamageInfo.HIT_EFFECT effect)
    {
        // TODO: implement slow
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
