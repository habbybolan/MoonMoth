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
    [Tooltip("CameraMovement component")]
    [SerializeField] private CameraMovement m_CameraMovement;
    [SerializeField] private PlayerWeapon m_Weapon;
    [SerializeField] private MoonBarAbility m_MoonBarAbility;

    [Header("Lost Moth")]
    [SerializeField] private TextMeshProUGUI m_LostMothUI;
    [SerializeField] private float m_LostMothUIDisplayTime = 1.5f;

    private InputActions playerInput;           // PlayerInput object to enable and create callbacks for inputs performed
    private InputAction m_MovementInput;        // Input object for moving player along x-y axis
    private PLAYER_ACTION_STATE m_playerState;  // Current player state given the actions performed / effects applied
    Coroutine ShootCoroutine;                   // Coroutine called when performed shooting action to allow cancelling the coroutine
    private int m_LostMothCount = 0;
   

    // Add a new enemy's boost duration to list
    public void OnEnemyKilled()
    {
        m_MoonBarAbility.AddEnemyKilled();
    }

    private void Awake()
    {
        playerInput = new InputActions();
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

    private void OnEnable()
    {
        // Movement
        m_MovementInput = playerInput.Player.Move;
        m_MovementInput.Enable();

        // Shoot
        playerInput.Player.Fire.performed += DoFire;
        playerInput.Player.Fire.canceled += StopFire;
        playerInput.Player.Fire.Enable();

        // Dodge
        playerInput.Player.Dodge.performed += DoDodge;
        playerInput.Player.Dodge.Enable();

        // Dash
        playerInput.Player.DashStart.performed += OnDashStart;
        playerInput.Player.DashStart.Enable();
        playerInput.Player.DashEnd.performed += OnDashEnd;
        playerInput.Player.DashEnd.Enable();

        // AimMode
        playerInput.Player.AimModeStart.performed += OnAimModeStart;
        playerInput.Player.AimModeStart.Enable();
        playerInput.Player.AimModeEnd.performed += OnAimModeEnd;
        playerInput.Player.AimModeEnd.Enable();
    }

    private void OnDisable()
    {
        // Shoot
        playerInput.Player.Fire.performed -= DoFire;
        playerInput.Player.Fire.canceled -= StopFire;
        playerInput.Player.Fire.Disable();

        // Dodge
        playerInput.Player.Dodge.performed -= DoDodge;
        playerInput.Player.Dodge.Disable();

        // Dash
        playerInput.Player.DashStart.performed -= OnDashStart;
        playerInput.Player.DashStart.Disable();
        playerInput.Player.DashEnd.performed -= OnDashEnd;
        playerInput.Player.DashEnd.Disable();

        // Aim Mode
        playerInput.Player.AimModeStart.performed -= OnAimModeStart;
        playerInput.Player.AimModeStart.Disable();
        playerInput.Player.AimModeEnd.performed -= OnAimModeEnd;
        playerInput.Player.AimModeEnd.Disable();
    }

    // Main Update controller for all Player components, Dealing with actions/effects that happen each frame
    void Update()
    {
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
            m_PlayerMovement.HorizontalRotation(m_MovementInput.ReadValue<Vector2>().x);
        }

        m_PlayerMovement.MothXYMovemnent();
    }

    private void FixedUpdate()
    {
        if (m_playerState == PLAYER_ACTION_STATE.FLYING || m_playerState == PLAYER_ACTION_STATE.DASHING)
        {
            // move player body along local x, y plane based on inputs
            m_PlayerMovement.ControlPointXYMovement(m_MovementInput.ReadValue<Vector2>());
        }
        m_PlayerMovement.UpdateCrossHair();
    }

    public float DistanceFromPlayer(Vector3 pointToCompare)
    {
        return Vector3.Distance(pointToCompare, transform.position);
    }
   

    private void DoFire(InputAction.CallbackContext obj)
    {
        ShootCoroutine = StartCoroutine(m_Weapon.Shooting());
    }

    private void StopFire(InputAction.CallbackContext obj)
    {
        StopCoroutine(ShootCoroutine);
    }

    private void DoDodge(InputAction.CallbackContext obj)
    {
        if (m_playerState == PLAYER_ACTION_STATE.FLYING)
        {
            m_playerState = PLAYER_ACTION_STATE.DODGING;
            StartCoroutine(m_PlayerMovement.PlayerDodge(FinishAction, m_MovementInput.ReadValue<Vector2>()));
        }   
    }

    private void OnAimModeStart(InputAction.CallbackContext obj)
    {
        m_MoonBarAbility.AimModeStartHelper();
    }

    private void OnAimModeEnd(InputAction.CallbackContext obj)
    {
        m_MoonBarAbility.AimModeEndHelper();
    }

    private void OnDashStart(InputAction.CallbackContext obj)
    {
        m_MoonBarAbility.OnDashStartHelper();
    }

    private void OnDashEnd(InputAction.CallbackContext obj)
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("TileMapTest");
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
