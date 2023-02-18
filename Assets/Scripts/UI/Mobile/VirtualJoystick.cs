using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using TMPro;

public class VirtualJoystick : MonoBehaviour
{
    [SerializeField] private Image m_VirtualJoystickImageBackground;
    [SerializeField] private Image m_VirtualJoystickImage;
    [SerializeField] private TextMeshProUGUI m_DebugText;

    private EnhancedTouch.Finger m_CurrFinger = null;
    private float m_RadiusBackground;
    private float m_RadiusJoystick;
    private float m_AmountJoystickCanMove;

    private RectTransform m_BackgroundRectTransform;
    private RectTransform m_JoystickRectTransform;

    protected void Start()
    {
        // background values
        m_BackgroundRectTransform = m_VirtualJoystickImageBackground.transform.GetComponent<RectTransform>();
        m_RadiusBackground = (m_BackgroundRectTransform.rect.width / 2) * m_BackgroundRectTransform.localScale.x;

        // joystick values
        m_JoystickRectTransform = m_VirtualJoystickImage.transform.GetComponent<RectTransform>();
        m_RadiusJoystick = (m_JoystickRectTransform.rect.width / 2) * m_JoystickRectTransform.localScale.x * m_BackgroundRectTransform.localScale.x;

        m_AmountJoystickCanMove = m_RadiusBackground;// - m_RadiusJoystick;

        SetVisibility(false);
    }

    protected void Update()
    {
        Input = Vector2.zero;

        // If joystick is active
        if (m_CurrFinger != null)
        {
            // if touch ended, hide joystick
            if (m_CurrFinger.lastTouch.ended)
            {
                m_CurrFinger = null;
                SetVisibility(false);
                m_VirtualJoystickImage.transform.localPosition = Vector2.zero;
            }
            // Update joystick position and input values
            else
            {
                Vector2 backgroundPos = new Vector2(m_BackgroundRectTransform.position.x, m_BackgroundRectTransform.position.y) +
                    new Vector2(m_RadiusBackground, m_RadiusBackground);
                Vector2 touchVecFromCenter = m_CurrFinger.lastTouch.screenPosition - backgroundPos;

                m_DebugText.SetText("Position: " + backgroundPos.ToString() + "\nVector: " + touchVecFromCenter.ToString());

                // Scale Inputs to radius of background
                Input.x = touchVecFromCenter.x / m_AmountJoystickCanMove;
                Input.y = touchVecFromCenter.y / m_AmountJoystickCanMove;

                // Normalize values to be within the background circle rather than a square
                float size = Input.magnitude;
                if (size > 1)
                {
                    Input /= size;
                }

                Vector3 JoystickVec = Input * m_AmountJoystickCanMove;
                m_JoystickRectTransform.localPosition = JoystickVec;
            }
        }
        PlayerManager.PropertyInstance.PlayerController.RecordInput(Input);

    }

    private void FixedUpdate()
    {
        // Update virtual joystick position
        if (m_CurrFinger != null)
        {
            
        }
    }

    public void StartJoystickTouch(EnhancedTouch.Finger finger)
    {
        //prevent multiple joysticks
        if (m_CurrFinger != null) return;

        m_CurrFinger = finger;
        SetVisibility(true);
        // Set position of joystick to position touched, offset from size
        m_BackgroundRectTransform.position = finger.lastTouch.screenPosition + 
            new Vector2(-m_RadiusBackground, -m_RadiusBackground);
    }

    private void SetVisibility(bool isVisible)
    {
        m_VirtualJoystickImageBackground.enabled = isVisible;
        m_VirtualJoystickImage.enabled = isVisible;
    }
    
    public static Vector2 Input = Vector2.zero;
}
