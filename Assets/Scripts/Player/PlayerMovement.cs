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
    [Tooltip("How fast the player moves when dodging in X direction")]
    [SerializeField] private float m_DodgeSpeedX = 20f;
    [Tooltip("How fast the player moves when dodging in Y direction")]
    [SerializeField] private float m_DodgeSpeedY = 20f;
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

    private Vector3 m_ControlPoint;         // Location of the controil point
    private Vector3 m_CurrentAngle;         // Currrent rotational angle in EulerAngles
    private float m_CurrControlSpeed;       // The current speed of the controller point movement
    private float m_MaxYValue;              // Max input Y value for dodge, clamped by m_MaxYDegrees
    private Rigidbody m_Rigidbody;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        // Get the max input y value during a dodge based on degree limit set, with x dodge input -1/1
        m_MaxYValue = Mathf.Tan(Mathf.Deg2Rad * m_MaxYDegrees);

        m_ControlObject.SetActive(m_IsShowControlObject);
        m_CurrControlSpeed = m_BaseControlSpeed;
        Camera.main.transform.localPosition = Vector3.forward * m_CameraOffsetFromParent;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        m_ControlPoint = transform.parent.InverseTransformPoint(Camera.main.ViewportToWorldPoint(new Vector3(.5f, .5f, -m_CameraOffsetFromParent)));
        m_ControlObject.transform.localPosition = m_ControlPoint;
        m_CurrentAngle = Vector3.zero;
    }

    public void MoveAlongXYPlane(Vector2 Vec2Movement)
    {
        float inputX = Vec2Movement.x;
        float inputY = Vec2Movement.y;

        // move control points
        m_ControlPoint += new Vector3(inputX, inputY, 0) * m_CurrControlSpeed * Time.deltaTime;
        m_ControlObject.transform.localPosition = m_ControlPoint;

        // Move towards control points 
        Vector3 distanceFromControl = (new Vector3(m_ControlPoint.x, m_ControlPoint.y, transform.localPosition.z) - transform.localPosition);
        transform.localPosition += distanceFromControl * Time.deltaTime * m_MoveSpeedPercent * m_CurrControlSpeed;
    }

    public void RotationLook()
    {
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            Quaternion.LookRotation(new Vector3(m_ControlPoint.x, m_ControlPoint.y, m_focalPointOffset) - transform.localPosition),
            m_RotateSpeed);
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

    public void UpdateCrossHair()
    {
        m_Crosshair.position = Camera.main.WorldToScreenPoint(CrossHairPoint);
    }

    public void TerrainCollision(ContactPoint contact)
    {
        Vector3 vecTranslate = new Vector3(contact.normal.x, contact.normal.y, 0).normalized * 10f * -1;
        m_ControlPoint += vecTranslate;
        m_ControlObject.transform.localPosition = m_ControlPoint;
        transform.localPosition += vecTranslate;
    }

    public IEnumerator PlayerDodge(System.Action callback, Vector2 vec2Move)
    {
        float inputX = vec2Move.x;
        float inputY = vec2Move.y;
        inputY = Mathf.Clamp(inputY, -m_MaxYValue, m_MaxYValue);
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

        while (currDuration < m_DodgeDuration)
        {
            float angle = rotationAmount * (Time.deltaTime / m_DodgeDuration);

            // spin player around z axis
            m_CurrentAngle = new Vector3(
                transform.localEulerAngles.x,
                transform.localEulerAngles.y,
                m_CurrentAngle.z + angle);

            transform.localEulerAngles = m_CurrentAngle;

            // Get the move addition in x and y direction
            float moveX = m_AnimationCurve.Evaluate(currDuration / m_DodgeDuration) * Time.deltaTime * m_DodgeSpeedX;
            float moveY = Time.deltaTime * m_DodgeSpeedY;

            transform.localPosition += Vector3.right * inputXDirection * moveX + Vector3.up * moveY * inputY;

            float controlPointMultiplierX = 1f;
            // if control point on opposite side X of the player fromthe direction player is dodging, apply speed multiplier
            if (inputXDirection < 0 && m_ControlPoint.x > transform.localPosition.x - m_ControlPointOffsetLimitX ||
                inputXDirection > 0 && m_ControlPoint.x < transform.localPosition.x + m_ControlPointOffsetLimitX)
            {
                controlPointMultiplierX = m_ControlPointMultiplier;
            }

            float controlPointMultiplierY = 1f;
            // if control point on opposite side Y of the player from the direction player is dodging, apply speed multiplier
            if (inputY < 0 && m_ControlPoint.y > transform.localPosition.y - m_ControlPointOffsetLimitY ||
                inputY > 0 && m_ControlPoint.y < transform.localPosition.y + m_ControlPointOffsetLimitY)
            {
                controlPointMultiplierY = m_ControlPointMultiplier;
            }

            // Update control point
            m_ControlPoint += Vector3.right * inputXDirection * moveX * controlPointMultiplierX
                           + Vector3.up * inputY * moveY * controlPointMultiplierY;
            m_ControlObject.transform.localPosition = m_ControlPoint;


            currDuration += Time.deltaTime;
            yield return null;
        }

        // Finish dodge state
        callback();
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
}
