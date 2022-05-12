using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement speed")]
    [Range(0, 1)]
    [Tooltip("Percent of control point's movement speed so player lags behind control point")]
    [SerializeField] private float m_MoveSpeedPercent = 0.2f;
    [Tooltip("To show visual representation of control point player follows")]
    [SerializeField] private bool m_IsShowControlObject = false;
    [Tooltip("Prefab representing the visuals of the control point")]
    [SerializeField] private GameObject m_ControlObject;
    [Tooltip("Base speed of the control point")]
    [SerializeField] private float m_BaseControlSpeed = 25f;

    [Header("Dodge")]
    [Tooltip("The duration the dodge lasts")]
    [SerializeField] private float m_DodgeDuration = 1f;
    [Tooltip("Animation curve representing the movement modification during the dodge")]
    [SerializeField] private AnimationCurve m_AnimationCurve;
    [Tooltip("How fast the player moves when dodging")]
    [SerializeField] private float m_DodgeSpeed = 20f;
    [Tooltip("If the camera should follow the player during the dodge")]
    [SerializeField] private bool m_CameraFollowOnDodge = true;
    [Tooltip("control point speed multiplier if control point is on the opposite side to the player in relation to the direction of the dodge")]
    [SerializeField] private float m_ControlPointMultiplier = 1.4f;
    [Tooltip("control point offset to check if control point is on the opposite side to the player in relation to the direction of the dodge")]
    [SerializeField] private float m_ControlPointOffsetLimit = 3f;

    [Header("Dash")]
    [Tooltip("Duration of the dash")]
    [SerializeField] private float m_DashDuration = 2.5f;

    [Header("Rotation")]
    [Tooltip("How quickly the player rotates along x-z axis")]
    [SerializeField] private float m_RotateSpeed = 10f;
    [Tooltip("The offset in front of the control point to make the player look towards")]
    [SerializeField] private float m_focalPointOffset = 5f;
    [Range(0, 1)]
    [Tooltip("How quickly the player rotates along z axis")]
    [SerializeField] private float m_ZRotationSpeed = .01f;
    [Tooltip("The maximum amount the player can z-rotate from resting position")]
    [SerializeField] private float m_MaxZRotation = 50f;
    [Tooltip("minimum amount of x input needed to rotate ")]
    [Range (0, 1)]
    [SerializeField] private float m_MinXInputForZRotation = .1f;

    [Header("Camera")]
    [Tooltip("CameraMovement component")]
    [SerializeField] private CameraMovement m_CameraMovement;
    [Tooltip("Crosshair UI component")]
    [SerializeField] private TextMeshProUGUI m_CrossHair;
    [Tooltip("Crosshair percent difference along y axis from the parent's origin to the control point's y location")]
    [Range(0, 1)]
    [SerializeField] private float m_CrosshairPercentY = 0.05f;
    [Tooltip("Crosshair percent difference along x axis from the parent's origin to the control point's x locatuin")]
    [Range(0, 1)]
    [SerializeField] private float m_CrosshairPercentX = 0.05f;
    [Tooltip("Camera z offset from the parent origin")]
    [SerializeField] private float m_CameraOffsetFromParent = -15;

    [Header("Weapon")]
    [Tooltip("WeaponBase component")]
    [SerializeField] private WeaponBase m_Weapon;
    [Tooltip("Offset forwards from the crosshair to shoot projectile towards on a missed shot (No Collider hit)")]
    [SerializeField] private float m_ShotMissedOffset = 1000f;

    private Vector3 m_ControlPoint;         // Location of the controil point
    private Vector3 m_CurrentAngle;         // Currrent rotational angle in EulerAngles
    private PLAYER_STATE m_playerState;     // Current player state given the actions performed / effects applied

    private float m_CurrControlSpeed;       // The current speed of the controller point movement

    private InputActions playerInput;        // PlayerInput object to enable and create callbacks for inputs performed
    private InputAction m_MovementInput;    // Input object for moving player along x-y axis

    LayerMask playerMask;                   // Player mask
    Coroutine ShootCoroutine;               // Coroutine called when performed shooting action to allow cancelling the coroutine

    private void Awake()
    {
        playerInput = new InputActions();
    }
    private void OnEnable()
    {
        // Movement
        m_MovementInput = playerInput.Player.Move;
        m_MovementInput.Enable();

        // Shoot
        playerInput.Player.Fire.performed += ctx => ShootCoroutine = StartCoroutine(Shooting());
        playerInput.Player.Fire.canceled += ctx => StopCoroutine(ShootCoroutine);
        playerInput.Player.Fire.Enable();

        // Dodge
        playerInput.Player.Dodge.performed += DoDodge;
        playerInput.Player.Dodge.Enable();

        // Dash
        playerInput.Player.Dash.performed += DoDash;
        playerInput.Player.Dash.Enable();
    }

    void Start()
    {
        m_ControlObject.SetActive(m_IsShowControlObject);

        m_playerState = PLAYER_STATE.FLYING;
        m_CurrControlSpeed = m_BaseControlSpeed;
        Camera.main.transform.localPosition = Vector3.forward * m_CameraOffsetFromParent;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerMask = LayerMask.GetMask("Player");

        m_ControlPoint = transform.parent.InverseTransformPoint(Camera.main.ViewportToWorldPoint(new Vector3(.5f, .5f, -m_CameraOffsetFromParent)));
        m_ControlObject.transform.localPosition = m_ControlPoint;
        m_CurrentAngle = Vector3.zero;
    }

    void Update()
    {
        RotationLook();
        if (m_playerState == PLAYER_STATE.FLYING || m_playerState == PLAYER_STATE.DASHING)
        {

            HorizontalRotation();
            MoveAlongXYPlane();
        }

        UpdateCrossHair();

    }

    void MoveAlongXYPlane()
    {
        float inputX = m_MovementInput.ReadValue<Vector2>().x;
        float inputY = m_MovementInput.ReadValue<Vector2>().y;

        // move control points
        m_ControlPoint += new Vector3(inputX, inputY, 0) * m_CurrControlSpeed * Time.deltaTime;
        m_ControlObject.transform.localPosition = m_ControlPoint;

        // Move towards control points
        Vector3 moveToDirection = (new Vector3(m_ControlPoint.x, m_ControlPoint.y, transform.localPosition.z) - transform.localPosition);
        transform.localPosition += moveToDirection * Time.deltaTime * m_MoveSpeedPercent * m_CurrControlSpeed;
    }

    void RotationLook()
    {
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            Quaternion.LookRotation(new Vector3(m_ControlPoint.x, m_ControlPoint.y, m_focalPointOffset) - transform.localPosition),
            m_RotateSpeed);
    }

    void HorizontalRotation()
    {
        float inputX = m_MovementInput.ReadValue<Vector2>().x;

        m_CurrentAngle = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            Mathf.LerpAngle(m_CurrentAngle.z,
                            (inputX < m_MinXInputForZRotation && inputX > -m_MinXInputForZRotation) ? 0 : inputX < 0 ? m_MaxZRotation : -m_MaxZRotation, m_ZRotationSpeed));

        transform.localEulerAngles = m_CurrentAngle;
    }

    private void UpdateCrossHair()
    {
        m_CrossHair.transform.position = Camera.main.WorldToScreenPoint(
            CrossHairPoint);
    }

    IEnumerator Shooting()
    {
        while (true)
        {
            m_Weapon.Shoot(GetLocationToFireAt());
            yield return null;
        }
    }

    private void DoDodge(InputAction.CallbackContext obj)
    {
        if (m_playerState == PLAYER_STATE.FLYING)
            StartCoroutine(PlayerDodge());
    }

    private void DoDash(InputAction.CallbackContext obj)
    {
        if (m_playerState == PLAYER_STATE.FLYING)
        {
            m_CameraMovement.PerformCameraZoom(m_DashDuration);
            PlayerManager.PropertyInstance.Player.PerformDash(m_DashDuration);
            StartCoroutine(PlayerDash());
        }
    }

    private IEnumerator PlayerDash()
    {
        m_playerState = PLAYER_STATE.DASHING;
        float currDuration = 0f;
        while (currDuration < m_DashDuration)
        {
            currDuration += Time.deltaTime;
            yield return null;
        }
        m_playerState = PLAYER_STATE.FLYING;
    }

    private IEnumerator PlayerDodge()
    {
        m_CameraMovement.IsCameraFollow = m_CameraFollowOnDodge;
        float inputX = Input.GetAxis("Horizontal") != 0 ? Input.GetAxis("Horizontal") : Input.GetAxis("Mouse X");
        float currZRot = transform.localRotation.eulerAngles.z;

        float inputXDirection = inputX == 0 ? -1 : inputX < 0 ? -1 : 1;

        float rotationAmount;
        float difference = Mathf.Abs(360 - transform.localRotation.eulerAngles.z);
        // player angled to the right
        if (currZRot > 180)
        {
            // rotating to the right
            if (inputX > 0)
                rotationAmount = -(360 - difference);
            // otherwise rotating to the left
            else
                rotationAmount = difference + 360;
        }
        // otherwise player angled to the left
        else
        {
            // rotating to the right
            if (inputX > 0)
                rotationAmount = -(currZRot + 360);
            // otherwise rotating to the left
            else
                rotationAmount = 360 - currZRot;
        }

        float currDuration = 0;

        m_playerState = PLAYER_STATE.DODGING;
        while (currDuration < m_DodgeDuration)
        {
            float angle = rotationAmount * (Time.deltaTime / m_DodgeDuration);

            // spin player around z axis
            m_CurrentAngle = new Vector3(
                transform.localEulerAngles.x,
                transform.localEulerAngles.y,
                m_CurrentAngle.z + angle);

            transform.localEulerAngles = m_CurrentAngle;

            // Get the move addition in x direction from dodge
            float moveX = m_AnimationCurve.Evaluate(currDuration / m_DodgeDuration) * Time.deltaTime * m_DodgeSpeed;

            // prevent the movement going slower than the max x/y move speed
            moveX = Mathf.Max(moveX, Mathf.Abs(Time.deltaTime * m_BaseControlSpeed * m_MoveSpeedPercent));

            transform.localPosition += Vector3.right * inputXDirection * moveX;

            float controlPointMultiplier = 1f;
            // if control point on opposite side of the player fromthe direction player is dodging, apply speed multiplier
            if (inputXDirection < 0 && m_ControlPoint.x > transform.localPosition.x - m_ControlPointOffsetLimit ||
                inputXDirection > 0 && m_ControlPoint.x < transform.localPosition.x + m_ControlPointOffsetLimit)
            {
                controlPointMultiplier = m_ControlPointMultiplier;
            }

            m_ControlPoint += Vector3.right * inputXDirection * moveX * controlPointMultiplier;
            m_ControlObject.transform.localPosition = m_ControlPoint;


            currDuration += Time.deltaTime;
            yield return null;
        }

        // Finish dodge state
        m_CameraMovement.IsCameraFollow = true;
        m_playerState = PLAYER_STATE.FLYING;
    }

    private Vector3 GetLocationToFireAt()
    {
        Vector3 crossHairPoint = Camera.main.ScreenToWorldPoint(m_CrossHair.transform.position);
        Vector3 crossHairOffset = Camera.main.ScreenToWorldPoint(m_CrossHair.transform.position + Vector3.forward);

        Debug.DrawRay(crossHairPoint, crossHairOffset - crossHairPoint, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(crossHairPoint, (crossHairOffset - crossHairPoint), out hit, Mathf.Infinity, ~playerMask))
        {
            return hit.point;
        }
        else
        {
            return crossHairPoint + (crossHairOffset - crossHairPoint).normalized * m_ShotMissedOffset;
        }
    }

    public Vector3 ControlPosition
    {
        get { return m_ControlPoint; }
    }

    public Vector3 CrossHairPoint
    {
        get
        {
            float yOffset = (transform.parent.position.y - m_ControlObject.transform.position.y) * m_CrosshairPercentY;
            float xOffset = (transform.parent.position.x - m_ControlObject.transform.position.x) * m_CrosshairPercentX;
            return new Vector3(m_ControlObject.transform.position.x + xOffset, m_ControlObject.transform.position.y + yOffset, transform.position.z);
        }
    }

    public Vector3 CrossHairPointLocal
    {
        get
        {
            float yOffset = (0 - m_ControlObject.transform.localPosition.y) * m_CrosshairPercentY;
            float xOffset = (0 - m_ControlObject.transform.localPosition.x) * m_CrosshairPercentX;
            return new Vector3(m_ControlObject.transform.localPosition.x + xOffset, m_ControlObject.transform.localPosition.y + yOffset, transform.localPosition.z);
        }
    }

    public float CameraOffset
    {
        get { return m_CameraOffsetFromParent; }
    }

    enum PLAYER_STATE
    {
        FLYING,
        DODGING,
        DASHING,
        DAMAGED
    }
}
