using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stalag
{
    public Vector3 m_Position;
    public bool m_IsPointingUp;

    public Stalag(Vector3 position, bool isPointingUp = true) 
    {
        m_Position = position;
        m_IsPointingUp = isPointingUp;
    }

    public Vector3 Position { 
        get { return m_Position; } 
        set { m_Position = value; }
    }

    public bool GetIsPointUp()
    {
        return m_IsPointingUp;
    }

    public void ChangeStalgOrientation()
    {
        m_IsPointingUp = !m_IsPointingUp;
    }
}
