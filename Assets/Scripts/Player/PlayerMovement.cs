using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Moth movement speed while following control point")]
    [SerializeField] private float m_BaseMothMoveSpeed = 7f; 
    [Tooltip("To show visual representation of control point player follows")]
    [SerializeField] private bool m_IsShowControlObject = false;
    [Tooltip("Object representing the visuals of the control point")]
    [SerializeField] private GameObject m_ControlObject;
    [Tooltip("Base speed of the control point")]
    [SerializeField] private float m_BaseControlSpeed = 20f;
    [SerializeField] private float m_ControlPointAcceleration = 10f;
    [Range(0f, 1f)]
    [SerializeField] private float m_CrosshairLerpSmoothing = 10f;

    [Header("Aim Mode")]
    [Tooltip("The speed increase on player movement while in aim mode to move faster in relation to everything else")]
    [Range(1, 3)] 
    [SerializeField] private float m_AimModeSpeedIncrease = 1.8f;

    [Header("Dodge")]
    [Tooltip("The duration the dodge lasts")]
    [SerializeField] private float m_DodgeDuration = 1f;
    [Tooltip("Animation curve representing the movement modification during the dodge")]
    [SerializeField] private AnimationCurve m_AnimationCurve;
    [Tooltip("If the camera should follow the player during the dodge")]
    [SerializeField] private bool m_CameraFollowOnDodge = true;
    [Tooltip("control point speed multiplier if control point is on the opposite side to the player in relation to the direction of the dodge")]
    [SerializeField] private float m_ControlPointMultiplier = 1.4f;
    [Tooltip("control point offset to check if control point is on the opposite side to the player in relation to the X direction of the dodge")]
    [SerializeField] private float m_ControlPointOffsetLimitX = 3f;
    [Tooltip("control point offset to check if control point is on the opposite side to the player in relation to the Y direction of the dodge")]
    [SerializeField] private float m_ControlPointOffsetLimitY = 3f;
    [Tooltip("Max degrees the player can dodge in y direction")]
    [Range (0, 45)]
    [SerializeField] private float m_MaxYDegrees = 45f;
    [Tooltip("The moth's speed increase while dodging")]
    [Range(1, 10)]
    [SerializeField] private float m_DodgeSpeedIncrease = 2f;
    [SerializeField] private float m_ControlPointSpeedIncrDuringDodge = 5f;
    

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
    [Tooltip("Crosshair UI component")]
    [SerializeField] private RectTransform m_Crosshair;
    [Tooltip("Crosshair percent difference along y axis from the parent's origin to the control point's y location")]
    [Range(0, 1)]
    [SerializeField] private float m_CrosshairPercentY = 0.05f;
    [Tooltip("Crosshair percent difference along x axis from the parent's origin to the control point's x locatuin")]
    [Range(0, 1)]
    [SerializeField] private float m_CrosshairPercentX = 0.05f;
    [Tooltip("Camera z offset from the parent origin")]
    [SerializeField] private float m_CameraOffsetFromParent = -15;
    [Tooltip("Initial Y offset of crosshair")]
    [SerializeField] private float m_InitialYCrosshairOffset = 1f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem m_DodgeParticles;

    [Header("Mobile")]
    //[Tooltip("The resting offset Y position of phone for moving forward. If position, then resting position will be phone tilted downwards")]
    //[SerializeField] private float m_OriginYPhonePosition = 0.55f;
    [SerializeField] private float m_YMobileControlSpeedMult = 1.3f;
    [SerializeField] private float m_XMobileControlSpeedMult = 1f;

    //private Vector3 m_ControlPoint;         // Location of the controil point
    private Vector3 m_CurrentAngle;         // Currrent rotational angle in EulerAngles
    private float m_CurrControlSpeed;       // The current speed of the controller point movement
    private float m_MaxYValue;              // Max input Y value for dodge, clamped by m_MaxYDegrees
    private Rigidbody m_ControlRigidBody;

    private Rigidbody m_PlayerRigidBody;

    private float m_CurrMothMoveSpeed;

    public GameObject ControlObject => m_ControlObject;

    private Vector3 m_PrevCrosshairPoint;

    // Start Dodging properties
    private float m_CurrDodgeDuration = 0;
    private float m_DodgeInputXDirection;
    private float m_DodgeInputYDirection;
    // Amount to rotate to perform at least a full rotation, given current rotation
    private float m_DodgeRotationAmount;
    // End Dodging properties

    private void Awake()
    {
        m_ControlRigidBody = m_ControlObject.GetComponent<Rigidbody>();
        m_ControlObject.GetComponent<MeshRenderer>().enabled = m_IsShowControlObject;

        m_PlayerRigidBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        m_CurrMothMoveSpeed = m_BaseMothMoveSpeed;
        
        // Get the max input y value during a dodge based on degree limit set, with x dodge input -1/1
        m_MaxYValue = Mathf.Tan(Mathf.Deg2Rad * m_MaxYDegrees);

        m_CurrControlSpeed = m_BaseControlSpeed;
        Camera.main.transform.localPosition = Vector3.forward * m_CameraOffsetFromParent;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        m_ControlObject.transform.localPosition = transform.parent.InverseTransformPoint(Camera.main.ViewportToWorldPoint(new Vector3(.5f, .5f, -m_CameraOffsetFromParent)));
        m_CurrentAngle = Vector3.zero;

        m_ControlObject.transform.parent = null;
    }

    public void ControlPointXYMovement(Vector2 Vec2Movement, bool bGyroMovement = false)
    {
        float inputX = Vec2Movement.x;
        float inputY = Vec2Movement.y;

        // if mobile movement
        if (bGyroMovement)
        {
            //inputY += m_OriginYPhonePosition;
            inputX *= m_XMobileControlSpeedMult;
            inputY *= m_YMobileControlSpeedMult;
        }

        // calculate velocity differential
        Vector3 currentVelocity = m_ControlRigidBody.velocity;
        // differential for horizontal/vertical movements
        Vector3 targetVelocity = (transform.parent.right * inputX + transform.parent.up * inputY) * m_CurrControlSpeed * Time.fixedDeltaTime;

        // differential for parent catmull walker movement
        Vector3 ParentVelocity = PlayerManager.PropertyInstance.PlayerController.PlayerParent.RigidBody.velocity;
        targetVelocity += ParentVelocity;

        Vector3 velocityDifferential = targetVelocity - currentVelocity;
        float controlSpeedMultiplier = 1 + (ControlPosition.z * -1 * 100);
        m_ControlRigidBody.AddForce(velocityDifferential * m_ControlPointAcceleration +
            transform.parent.forward * controlSpeedMultiplier, ForceMode.Force);
    }

    public void MothXYMovemnent()
    {
        // Move towards control points
        Vector3 distanceFromControl = new Vector3(ControlPosition.x, ControlPosition.y, transform.localPosition.z) - transform.localPosition;
        m_PlayerRigidBody.transform.localPosition += distanceFromControl * m_CurrMothMoveSpeed * Time.fixedDeltaTime;
    }

    public void RotationLook()
    {
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            Quaternion.LookRotation(new Vector3(ControlPosition.x, ControlPosition.y, m_focalPointOffset) - transform.localPosition),
            m_RotateSpeed);
    }

    public void AimModeEnter()
    {
        m_CurrControlSpeed *= m_AimModeSpeedIncrease;
    }

    public void AimModeExit()
    {
        m_CurrControlSpeed = m_BaseControlSpeed;
    }

    public void HorizontalRotation(float xLook)
    {
        m_CurrentAngle = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            Mathf.LerpAngle(m_CurrentAngle.z,
                            (xLook < m_MinXInputForZRotation && xLook > -m_MinXInputForZRotation) ? 0 : xLook < 0 ? m_MaxZRotation : -m_MaxZRotation, m_ZRotationSpeed));

        transform.localEulerAngles = m_CurrentAngle;
    }

    public void UpdateCrossHair(Cinemachine.CinemachineBrain brain)
    {
        Vector3 NewCrosshairPoint = Vector3.Lerp(m_Crosshair.position, Camera.main.WorldToScreenPoint(CrossHairPoint), m_CrosshairLerpSmoothing);
        m_Crosshair.position = NewCrosshairPoint;
    }

    public void ResetPosition()
    {
        m_ControlRigidBody.velocity = Vector3.zero;
        m_ControlRigidBody.isKinematic = true;
        m_ControlRigidBody.transform.position = transform.parent.transform.position;
        m_ControlRigidBody.position = transform.parent.transform.position;
        m_ControlRigidBody.isKinematic = false;

        // make sure moth resets at parent location
        m_PlayerRigidBody.transform.localPosition = Vector3.zero;
    }

    public IEnumerator PlayerDodge(System.Action callback, Vector2 vec2Move)
    {
        // Play dodge particles
        if (m_DodgeParticles != null)
        {
            StartCoroutine(PlayDodgeParticles());
        }

        m_CurrMothMoveSpeed *= m_DodgeSpeedIncrease;

        float inputX = vec2Move.x;
        float inputY = vec2Move.y;
        m_DodgeInputYDirection = Mathf.Clamp(inputY, -m_MaxYValue, m_MaxYValue);
        float currZRot = transform.localRotation.eulerAngles.z;

        m_DodgeInputXDirection = inputX == 0 ? -1 : inputX < 0 ? -1 : 1;
 
        float difference = Mathf.Abs(360 - transform.localRotation.eulerAngles.z);
        // player angled to the right
        if (currZRot > 180)
        {
            // rotating to the right
            if (inputX > 0)
                m_DodgeRotationAmount = -(360 - difference);
            // otherwise rotating to the left
            else
                m_DodgeRotationAmount = difference + 360;
        }
        // otherwise player angled to the left
        else
        {
            // rotating to the right
            if (inputX > 0)
                m_DodgeRotationAmount = -(currZRot + 360);
            // otherwise rotating to the left
            else
                m_DodgeRotationAmount = 360 - currZRot;
        }

        m_CurrDodgeDuration = 0;

        while (m_CurrDodgeDuration < m_DodgeDuration)
        {
            yield return null;
        }

        m_CurrMothMoveSpeed = m_BaseMothMoveSpeed;
        // Finish dodge state
        callback();
    }

    public void PerformDodge()
    {
        float angle = m_DodgeRotationAmount * (Time.fixedDeltaTime / m_DodgeDuration);

        // spin player around z axis
        m_CurrentAngle = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            m_CurrentAngle.z + angle);

        transform.localEulerAngles = m_CurrentAngle;

        float controlPointMultiplierX = m_ControlPointSpeedIncrDuringDodge;
        // if control point on opposite side X of the player fromthe direction player is dodging, apply speed multiplier
        if (m_DodgeInputXDirection < 0 && ControlPosition.x > transform.localPosition.x - m_ControlPointOffsetLimitX ||
            m_DodgeInputXDirection > 0 && ControlPosition.x < transform.localPosition.x + m_ControlPointOffsetLimitX)
        {
            controlPointMultiplierX *= m_ControlPointMultiplier;
        }

        float controlPointMultiplierY = m_ControlPointSpeedIncrDuringDodge;
        // if control point on opposite side Y of the player from the direction player is dodging, apply speed multiplier
        if (m_DodgeInputYDirection < 0 && ControlPosition.y > transform.localPosition.y - m_ControlPointOffsetLimitY ||
            m_DodgeInputYDirection > 0 && ControlPosition.y < transform.localPosition.y + m_ControlPointOffsetLimitY)
        {
            controlPointMultiplierY *= m_ControlPointMultiplier;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
            controlPointMultiplierX *= m_XMobileControlSpeedMult;
            controlPointMultiplierY *= m_YMobileControlSpeedMult;
#endif

        // Update control point 
        Vector3 targetVelocity = transform.parent.transform.right * m_DodgeInputXDirection * controlPointMultiplierX * m_BaseControlSpeed * Time.fixedDeltaTime +
                                    transform.parent.transform.up * m_DodgeInputYDirection * controlPointMultiplierY * m_BaseControlSpeed * Time.fixedDeltaTime;
        //Vector3 parentVelocity = PlayerManager.PropertyInstance.PlayerController.PlayerParent.RigidBody.velocity;
        //targetVelocity += parentVelocity;
        m_ControlRigidBody.AddForce(targetVelocity - m_ControlRigidBody.velocity, ForceMode.Force);

        m_CurrDodgeDuration += Time.fixedDeltaTime;
    }

    public Vector3 ControlPosition => transform.parent.transform.InverseTransformPoint(m_ControlRigidBody.position);

    IEnumerator PlayDodgeParticles()
    {
        ParticleSystem dodgeParticles = Instantiate(m_DodgeParticles, transform.position, Quaternion.identity, transform);
        yield return new WaitForSeconds(m_DodgeDuration);
        dodgeParticles.Stop();
    }

    public Vector3 CrossHairPoint
    {
        get
        {
            float yOffset = ControlPosition.y * m_CrosshairPercentY;
            float xOffset = ControlPosition.x * m_CrosshairPercentX;
            return m_ControlObject.transform.position - transform.parent.transform.right * xOffset - transform.parent.transform.up * (yOffset - m_InitialYCrosshairOffset);
        }
    }

    public Ray CrosshairScreenRay
    {
        get { return Camera.main.ScreenPointToRay(m_Crosshair.position);  }
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

    public float GetDodgeDuration()
    {
        return m_DodgeDuration;
    }
}
