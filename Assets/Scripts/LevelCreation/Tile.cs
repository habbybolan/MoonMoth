using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile : MonoBehaviour
{ 
    public Vector3[] m_FollowPoint;
    public Vector3 m_StartPoint;
    public Vector3 m_EndPoint; 

    public void Reset()
    {
        m_FollowPoint = new Vector3[] { transform.InverseTransformPoint(Vector3.zero) };

        m_StartPoint = transform.InverseTransformPoint(new Vector3(0, 0, 10));

        m_EndPoint = transform.InverseTransformPoint(new Vector3(0, 0, -10));
    }

    public float TileEndDistanceFromPlayer(PlayerTest player)
    {
        return Vector3.Distance(m_EndPoint, player.transform.position);
    }

    public float TileStartDistanceFromPlayer(PlayerTest player)
    {
        return Vector3.Distance(m_StartPoint, player.transform.position);
    }

    public void AddFollowPoint()
    {
        Vector3 followPoint = m_FollowPoint[PlayerFollowPointsCount - 1] + Vector3.forward * 2;
        Array.Resize(ref m_FollowPoint, PlayerFollowPointsCount + 1);
        m_FollowPoint[PlayerFollowPointsCount - 1] = followPoint;
    }

    public void RemoveFollowPoint()
    {
        if (PlayerFollowPointsCount == 1)
            throw new Exception("Cannot have an empty follow points list");

        Array.Resize(ref m_FollowPoint, PlayerFollowPointsCount - 1);
    }

    public int PlayerFollowPointsCount 
    { 
        get { return m_FollowPoint.Length; }
    }

    public Vector3 GetPlayerFollowPoint(int index)
    {
        if (index >= PlayerFollowPointsCount || index < 0)
            throw new System.Exception("Not a valid index in m_PlayerFollowPoints");
        return m_FollowPoint[index];
    }

    public Vector3 StartPoint { 
        get { return m_StartPoint; } 
        set { m_StartPoint = value; }
    }
    public Vector3 EndPoint { 
        get { return m_EndPoint; } 
        set { m_EndPoint = value; }
    }

    public void SetPlayerFollowPoint(int index, Vector3 position)
    {
        if (index >= PlayerFollowPointsCount || index < 0)
            throw new System.Exception("Not a valid index in m_PlayerFollowPoints");
        m_FollowPoint[index] = position;
    }


    public enum LOCATION_TYPES
    {
        FOLLOW,
        START,
        END
    }
}
