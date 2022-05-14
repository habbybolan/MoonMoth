using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement m_PlayerMovement;
    [SerializeField] private PlayerParentMovement m_PlayerParentMovement;

    [Tooltip("CameraMovement component")]
    [SerializeField] private CameraMovement m_CameraMovement;

    private InputActions playerInput;        // PlayerInput object to enable and create callbacks for inputs performed
    private InputAction m_MovementInput;    // Input object for moving player along x-y axis
    private PLAYER_STATE m_playerState;     // Current player state given the actions performed / effects applied
    Coroutine ShootCoroutine;               // Coroutine called when performed shooting action to allow cancelling the coroutine

    private void Awake()
    {
        playerInput = new InputActions();
    }

    private void Start()
    {
        m_playerState = PLAYER_STATE.FLYING;
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
    }

    void Update()
    {
        m_PlayerMovement.RotationLook();
        if (m_playerState == PLAYER_STATE.FLYING || m_playerState == PLAYER_STATE.DASHING)
        {

            m_PlayerMovement.HorizontalRotation(m_MovementInput.ReadValue<Vector2>().x);
            m_PlayerMovement.MoveAlongXYPlane(m_MovementInput.ReadValue<Vector2>());
        }

        m_PlayerMovement.UpdateCrossHair();
    }

    private void DoFire(InputAction.CallbackContext obj)
    {
        ShootCoroutine = StartCoroutine(m_PlayerMovement.Shooting());
    }

    private void StopFire(InputAction.CallbackContext obj)
    {
        StopCoroutine(ShootCoroutine);
    }

    private void DoDodge(InputAction.CallbackContext obj)
    {
        if (m_playerState == PLAYER_STATE.FLYING)
        {
            m_playerState = PLAYER_STATE.DODGING;
            StartCoroutine(m_PlayerMovement.PlayerDodge(FinishAction));
        }   
    }

    private void DoDash(InputAction.CallbackContext obj)
    {
        if (m_playerState == PLAYER_STATE.FLYING)
        {
            m_playerState = PLAYER_STATE.DASHING;
            m_CameraMovement.PerformCameraZoom(m_PlayerParentMovement.DashDuration);
            StartCoroutine(m_PlayerParentMovement.Dash(FinishAction));
        }
    }

    private void FinishAction()
    {
        m_playerState = PLAYER_STATE.FLYING;
    }

    public PlayerParentMovement PlayerParent { get { return m_PlayerParentMovement;  } }
    public PlayerMovement PlayerMovement { get { return m_PlayerMovement; } }
    public CameraMovement CameraMovement { get { return m_CameraMovement; } }

    enum PLAYER_STATE
    {
        FLYING,
        DODGING,
        DASHING,
        DAMAGED
    }
}
