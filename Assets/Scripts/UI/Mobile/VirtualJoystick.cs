using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

public class VirtualJoystick : MonoBehaviour
{
    [SerializeField] private Image m_VirtualJoystickImageBackground;
    [SerializeField] private Image m_VirtualJoystickImage;

    private EnhancedTouch.Finger m_CurrFinger = null;
    private float m_RadiusBackground;
    private float m_RadiusJoystick;
    private float m_RadiusDifference;

    private RectTransform m_BackgroundRectTransform;

    protected void Start()
    {
        m_BackgroundRectTransform = m_VirtualJoystickImageBackground.transform.GetComponent<RectTransform>();
        m_RadiusBackground = m_BackgroundRectTransform.rect.width / 2;
        m_RadiusJoystick = m_VirtualJoystickImage.transform.GetComponent<RectTransform>().rect.width / 2;
        m_RadiusDifference = m_RadiusBackground - m_RadiusJoystick;

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
                Vector2 backgroundPos = new Vector2(m_VirtualJoystickImageBackground.transform.position.x, m_VirtualJoystickImageBackground.transform.position.y);
                Vector2 touchVecFromCenter = m_CurrFinger.lastTouch.screenPosition - backgroundPos;
                // normalize Vec for [-1,1] input
                Input.x = Mathf.Clamp(touchVecFromCenter.x / m_RadiusDifference, -1, 1);
                Input.y = Mathf.Clamp(touchVecFromCenter.y / m_RadiusDifference, -1, 1);
            }
        }
    }

    private void FixedUpdate()
    {
        // Update virtual joystick position
        if (m_CurrFinger != null)
        {
            m_VirtualJoystickImage.transform.localPosition = new Vector2(Input.x* m_RadiusDifference, Input.y* m_RadiusDifference);
        }
    }

    public void StartJoystickTouch(EnhancedTouch.Finger finger)
    {
        //prevent multiple joysticks
        if (m_CurrFinger != null) return;

        m_CurrFinger = finger;
        SetVisibility(true);
        // Set position of joystick to position touched, offset from size
        float offset = -m_RadiusBackground * m_BackgroundRectTransform.localScale.x;
        m_BackgroundRectTransform.position = finger.lastTouch.screenPosition + 
            new Vector2(offset, offset);
    }

    private void SetVisibility(bool isVisible)
    {
        m_VirtualJoystickImageBackground.enabled = isVisible;
        m_VirtualJoystickImage.enabled = isVisible;
    }
    
    public static Vector2 Input = Vector2.zero;
}
