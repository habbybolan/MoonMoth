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

    [Header("Lost Moth")]
    [SerializeField] private TextMeshProUGUI m_LostMothUI;
    [SerializeField] private float m_LostMothUIDisplayTime = 1.5f;
     
    [Header("Aim Mode")]
    [Range(0.1f, 1)]
    [SerializeField] private float m_AimModeTimescaleChange = 0.5f;
    [SerializeField] private float m_AimModePercentLossPerSec = 20f;
    [SerializeField] private float m_AimModePercentGainedPerSec = 10f; 
    [SerializeField] private float m_AimModeCooldown = 3f;
    [SerializeField] private Image m_AimModeMoonReticle;

    private InputActions playerInput;           // PlayerInput object to enable and create callbacks for inputs performed
    private InputAction m_MovementInput;        // Input object for moving player along x-y axis
    private PLAYER_ACTION_STATE m_playerState;  // Current player state given the actions performed / effects applied
    Coroutine ShootCoroutine;                   // Coroutine called when performed shooting action to allow cancelling the coroutine
    private int m_LostMothCount = 0;

    private float m_AimModeCurrPercent = 100f;
    private Coroutine m_AimModeCoroutine;
    private bool m_IsAimModeCooldown = false;
    private bool m_IsAimMode = false;

    private void Awake()
    {
        playerInput = new InputActions();
        m_LostMothUI.enabled = false;
    }

    protected override void Start()
    {
        base.Start();
        m_playerState = PLAYER_ACTION_STATE.FLYING;
        m_Health.terrainCollisionDelegate = OnTerrainCollision;
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
        playerInput.Player.Dash.performed += DoDash;
        playerInput.Player.Dash.Enable();

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
        playerInput.Player.Dash.performed -= DoDash;
        playerInput.Player.Dash.Disable();
    }

    // Main Update controller for all Player components, Dealing with actions/effects that happen each frame
    void Update()
    { 
        if (!TileManager.PropertyInstance.IsInitialized)
            return;

        UpdateAimModeReticleBar();

        m_Health.LosePassiveHealth();
        m_PlayerMovement.RotationLook();

        // move parent along spline
        m_PlayerParentMovement.TryMove();
        
        if (m_playerState == PLAYER_ACTION_STATE.FLYING || m_playerState == PLAYER_ACTION_STATE.DASHING)
        {
            m_PlayerMovement.HorizontalRotation(m_MovementInput.ReadValue<Vector2>().x);
            // move player body along local x, y plane based on inputs
            m_PlayerMovement.MoveAlongXYPlane(m_MovementInput.ReadValue<Vector2>());
        }

        m_PlayerMovement.UpdateCrossHair();
    }

    private void OnAimModeStart(InputAction.CallbackContext obj)
    {
        AimModeStartHelper();
    }

    private void AimModeStartHelper()
    {
        // prevent going into aim mode if on cooldown
        if (m_IsAimModeCooldown) return;

        m_IsAimMode = true;
        m_AimModeCoroutine = StartCoroutine(AimModeDuration());
        Time.timeScale = m_AimModeTimescaleChange;
        m_PlayerMovement.AimModeEnter();
    }

    private void OnAimModeEnd(InputAction.CallbackContext obj)
    {
        AimModeEndHelper();
    }

    private void AimModeEndHelper()
    {
        // prevent leaving aim mode if currently not in it
        if (m_AimModeCoroutine == null) return;

        m_IsAimMode = false;
        StopCoroutine(m_AimModeCoroutine);
        m_AimModeCurrPercent = 0;
        Time.timeScale = 1f;
        m_PlayerMovement.AimModeExit();
        StartCoroutine(AimModeCooldown());
    }

    IEnumerator AimModeDuration()
    {
        while (m_AimModeCurrPercent > 0)
        {
            m_AimModeCurrPercent -= m_AimModePercentLossPerSec * Time.deltaTime;
            if (m_AimModeCurrPercent < 0) m_AimModeCurrPercent = 0;
            yield return null;
        }
        AimModeEndHelper();
    }

    IEnumerator AimModeCooldown()
    {
        m_IsAimModeCooldown = true;
        yield return new WaitForSeconds(m_AimModeCooldown);
        m_IsAimModeCooldown = false;
    }

    private void UpdateAimModeReticleBar()
    {
        // gain aimMode percent not in aim mode and not maxed out
        if (m_AimModeCurrPercent < 100 && !m_IsAimMode)
        {
            // TODO: Only for testing
            m_AimModeCurrPercent += m_AimModePercentGainedPerSec * Time.deltaTime;
        }
        m_AimModeMoonReticle.fillAmount = (m_AimModeCurrPercent / 100);
    }

    public void OnTerrainCollision(ContactPoint contact)
    {
        Vector3 normal = contact.normal;
        Vector3 contactPoint = contact.point;
        Debug.DrawRay(contactPoint, normal, Color.red, 10);
        StartCoroutine(m_PlayerParentMovement.TerrainCollision(FinishAction, contact));

        m_PlayerMovement.TerrainCollision(contact);
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


    private void DoDash(InputAction.CallbackContext obj)
    {
        if (m_playerState == PLAYER_ACTION_STATE.FLYING)
        {
            m_playerState = PLAYER_ACTION_STATE.DASHING;
            m_CameraMovement.PerformCameraZoom(m_PlayerParentMovement.DashDuration);
            StartCoroutine(m_PlayerParentMovement.Dash(FinishAction));
        }
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
