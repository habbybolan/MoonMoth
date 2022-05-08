using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile : MonoBehaviour
{
    [SerializeField] private GameObject m_FollowPointPrefab;
    [SerializeField] private GameObject m_StartPointPrefab;
    [SerializeField] private GameObject m_EndPointPrefab; 

    public GameObject[] m_PlayerFollowPoints;
    public GameObject m_TileStartPoint;
    public GameObject m_TileEndPoint;

    public void Reset()
    {
        GameObject firstFollowPoint = Instantiate(m_FollowPointPrefab, Vector3.zero, Quaternion.identity, transform);
        m_PlayerFollowPoints = new GameObject[] { firstFollowPoint };

        m_TileStartPoint = Instantiate(m_StartPointPrefab, new Vector3(0, 0, 10), Quaternion.identity, transform);

        m_TileEndPoint = Instantiate(m_EndPointPrefab, new Vector3(0, 0, -10), Quaternion.identity, transform);
    }

    public float TileEndDistanceFromPlayer(PlayerTest player)
    {
        return Vector3.Distance(m_TileEndPoint.transform.position, player.transform.position);
    }

    public float TileStartDistanceFromPlayer(PlayerTest player)
    {
        return Vector3.Distance(m_TileStartPoint.transform.position, player.transform.position);
    }

    public void AddFollowPoint()
    {
        GameObject followPoint = Instantiate(m_FollowPointPrefab, Vector3.zero, Quaternion.identity, transform);
        Array.Resize(ref m_PlayerFollowPoints, PlayerFollowPointsCount + 1);
        followPoint.transform.position = m_PlayerFollowPoints[PlayerFollowPointsCount - 2].transform.position + Vector3.forward;
        m_PlayerFollowPoints[PlayerFollowPointsCount - 1] = followPoint;
    }

    public void RemoveFollowPoint()
    {
        if (PlayerFollowPointsCount == 1)
            throw new Exception("Cannot have an empty follow points list");

        DestroyImmediate(m_PlayerFollowPoints[m_PlayerFollowPoints.Length - 1]);
        Array.Resize(ref m_PlayerFollowPoints, PlayerFollowPointsCount - 1);
    }

    public int PlayerFollowPointsCount 
    { 
        get { return m_PlayerFollowPoints.Length; }
    }

    public Vector3 GetPlayerFollowPoint(int index)
    {
        if (index >= PlayerFollowPointsCount || index < 0)
            throw new System.Exception("Not a valid index in m_PlayerFollowPoints");
        return m_PlayerFollowPoints[index].transform.position;
    }

    public void SetPlayerFollowPoint(int index, Vector3 position)
    {
        if (index >= PlayerFollowPointsCount || index < 0)
            throw new System.Exception("Not a valid index in m_PlayerFollowPoints");
        m_PlayerFollowPoints[index].transform.position = position;
    }


    public enum LOCATION_TYPES
    {
        FOLLOW,
        START,
        END
    }
}
