using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile : MonoBehaviour
{ 
    public Vector3[] m_FollowPoint;
    public Vector3 m_StartPoint;
    public Vector3 m_EndPoint;

    public List<List<Vector3>> m_EnemyPointSet;

    private Vector3 m_VecStartToCenter;
    private Vector3 m_VecCenterToEnd;

    private void Awake()
    {
        m_VecStartToCenter = transform.position - transform.TransformPoint(m_StartPoint);
        m_VecCenterToEnd = transform.TransformPoint(m_EndPoint) - transform.position;
    }

    public void Reset()
    {
        m_FollowPoint = new Vector3[] { transform.InverseTransformPoint(Vector3.zero) };

        m_StartPoint = transform.InverseTransformPoint(new Vector3(0, 0, 10));

        m_EndPoint = transform.InverseTransformPoint(new Vector3(0, 0, -10));

        m_EnemyPointSet = new List<List<Vector3>>();
        m_EnemyPointSet.Add(new List<Vector3> { new Vector3(0, 0, 0) });
    }
    
    public float TileEndDistanceFromPlayer(PlayerTest player)
    {
        return Vector3.Distance(transform.TransformPoint(m_EndPoint), player.transform.position);
    }

    public float TileStartDistanceFromPlayer(PlayerTest player)
    {
        return Vector3.Distance(transform.TransformPoint(m_StartPoint), player.transform.position);
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

    public void AddEnemyPointSet()
    {
        m_EnemyPointSet.Add(new List<Vector3> { m_EnemyPointSet[m_EnemyPointSet.Count - 1][0] + Vector3.forward * 2 });
    }

    public void RemoveEnemyPointSet()
    {
        m_EnemyPointSet.RemoveAt(m_EnemyPointSet.Count - 1);
    }

    public void AddPointToEnemySet(int index)
    {
        // TODO:
    }

    public void RemovePointFromEnemySet(int index) 
    {
        // TODO:
    }

    public void SetPointInEnemySet(int index, Vector3 newPos)
    {
        // TODO:
    }

    public void UpdateAllPointsInSet(int index, Vector3 translateDirection)
    {
        List<Vector3> set = m_EnemyPointSet[index];
        for (int i = 0; i < set.Count; i++)
        {
            set[i] += translateDirection;
        }
    }

    public int PlayerFollowPointsCount 
    { 
        get { return m_FollowPoint.Length; }
    }

    public int EnemyFollowSetCount
    {
        get { return m_EnemyPointSet.Count; }
    }

    public List<Vector3> GetEnemyPointSet(int index)
    {
        if (index >= m_EnemyPointSet.Count || index < 0)
            throw new Exception("Not a valid index in m_EnemyPointSet");

        return m_EnemyPointSet[index];
    }

    public Vector3 GetPlayerFollowPoint(int index)
    {
        if (index >= PlayerFollowPointsCount || index < 0)
            throw new System.Exception("Not a valid index in m_PlayerFollowPoints");
        return m_FollowPoint[index];
    }

    public Vector3 GetPlayerFollowPointWorld(int index)
    {
        if (index >= PlayerFollowPointsCount || index < 0)
            throw new System.Exception("Not a valid index in m_PlayerFollowPoints");
        return transform.TransformPoint(m_FollowPoint[index]); 
    }

    public Vector3 StartPoint { 
        get { return m_StartPoint; } 
        set { m_StartPoint = value; }
    }
    public Vector3 EndPoint { 
        get { return m_EndPoint; } 
        set { m_EndPoint = value; }
    }

    public Vector3 StartPointWorld { get { return transform.TransformPoint(StartPoint); } }
    public Vector3 EndPointWorld { get { return transform.TransformPoint(EndPoint); } }

    public void SetPlayerFollowPoint(int index, Vector3 position)
    {
        if (index >= PlayerFollowPointsCount || index < 0)
            throw new System.Exception("Not a valid index in m_PlayerFollowPoints");
        m_FollowPoint[index] = position;
    }

    public Vector3 VecStartToCenter { get { return m_VecStartToCenter; } }
    public Vector3 VecCenterToEnd { get { return m_VecCenterToEnd; } }


    public enum LOCATION_TYPES
    {
        FOLLOW,
        START,
        END
    }
}
