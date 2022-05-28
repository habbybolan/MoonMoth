using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

[ExecuteInEditMode]
public class Tile : MonoBehaviour
{
    // Manually set points
    public Vector3[] m_FollowPoint;
    public Vector3 m_StartPoint;
    public Vector3 m_EndPoint;
    public List<Vector3> m_SpiderSpawns;
    public List<EnemySetWrapper> m_EnemyPointSet;
    public List<Stalag> m_Stalags;
    public List<Vector3> m_LostMothPoints;
    private Vector3 m_VecStartToCenter;
    private Vector3 m_VecCenterToEnd;
    
    // prefabs
    public StalagScriptable m_StalagPrefab;
    public LostMoth m_LostMoth;

    private bool m_IsTraversedByPlayer = false;   // If the tile has been traversed fully by the player
    private int m_ID;
    private BoxCollider m_EndCollider;

    private List<GameObject> m_SpawnedTileObjects;  // list of objects connected to tile for deletion

    private void Awake()
    {
        m_VecStartToCenter = transform.position - transform.TransformPoint(m_StartPoint);
        m_VecCenterToEnd = transform.TransformPoint(m_EndPoint) - transform.position;
        m_ID = TileManager.GetNewID();

        if (m_Stalags == null)
            m_Stalags = new List<Stalag>();
        if (m_SpiderSpawns == null)
            m_SpiderSpawns = new List<Vector3>();
        if (m_EnemyPointSet == null)
            m_EnemyPointSet = new List<EnemySetWrapper>();
        if (m_LostMothPoints == null)
            m_LostMothPoints = new List<Vector3>();
    }

    private void Start()
    {
        m_EndCollider = GetComponent<BoxCollider>();

        // Spawn all stalag prefabs
        m_SpawnedTileObjects = new List<GameObject>();
        foreach (Stalag stalag in m_Stalags)
        {
            // TODO: Use a difficulty coefficient to randomly spawn stalags
            Obstacle obstacle = Instantiate(m_StalagPrefab.StalagPrefab, transform.TransformPoint(stalag.m_Position), stalag.m_IsPointingUp ? Quaternion.identity : m_StalagPrefab.StalagPrefab.transform.rotation * Quaternion.Euler(Vector3.forward * 180));
            m_SpawnedTileObjects.Add(obstacle.gameObject);
        }
        foreach (Vector3 lostMoth in m_LostMothPoints)
        {
            LostMoth lostMothObj = Instantiate(m_LostMoth, transform.TransformPoint(lostMoth), Quaternion.identity);
            m_SpawnedTileObjects.Add(lostMothObj.gameObject);
        }
    }

    public void DeleteAllSpawned()
    {
        foreach (GameObject obj in m_SpawnedTileObjects)
        {
            Destroy(obj);
        }
        m_SpawnedTileObjects = new List<GameObject>();
    }

    public void Reset()
    {
        m_FollowPoint = new Vector3[] { transform.InverseTransformPoint(Vector3.zero) };
        m_StartPoint = transform.InverseTransformPoint(new Vector3(0, 0, 10));
        m_EndPoint = transform.InverseTransformPoint(new Vector3(0, 0, -10));
        m_EnemyPointSet = new List<EnemySetWrapper> { new EnemySetWrapper(new Vector3(0, 0, 0)) };
        m_SpiderSpawns = new List<Vector3>();
        m_Stalags = new List<Stalag>();
        m_LostMothPoints = new List<Vector3>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement m = other.GetComponent<PlayerMovement>();
        // if collided with PlayerMovement 
        if (m != null)
        {
            m_IsTraversedByPlayer = true;
        }
    }

    public void SetIsActive(bool IsActive)
    {
        gameObject.SetActive(IsActive);
        if (IsActive) m_IsTraversedByPlayer = false;
    }

    public float TileEndDistanceFromPlayer(CatmullWalker player)
    {
        return Vector3.Distance(transform.TransformPoint(m_EndPoint), player.transform.position);
    }

    public float TileStartDistanceFromPlayer(CatmullWalker player)
    {
        return Vector3.Distance(transform.TransformPoint(m_StartPoint), player.transform.position);
    }

    private Vector3 GetLocalPosInEditor()
    {
        return transform.InverseTransformPoint(SceneView.lastActiveSceneView.camera.transform.position);
    }

    public void AddFollowPoint()
    {
        Array.Resize(ref m_FollowPoint, PlayerFollowPointsCount + 1);
        m_FollowPoint[PlayerFollowPointsCount - 1] = GetLocalPosInEditor();
    }

    public void RemoveFollowPoint()
    {
        if (PlayerFollowPointsCount == 1)
            throw new Exception("Cannot have an empty follow points list");

        Array.Resize(ref m_FollowPoint, PlayerFollowPointsCount - 1);
    }

    public void AddEnemyPointSet()
    {
        m_EnemyPointSet.Add(new EnemySetWrapper(GetLocalPosInEditor()));
    }

    public void RemoveEnemyPointSet()
    {
        if (m_EnemyPointSet.Count <= 1)
            throw new Exception("Cannot have no enemy point sets");
        m_EnemyPointSet.RemoveAt(m_EnemyPointSet.Count - 1);
    }

    public void AddPointToEnemySet(int setIndex)
    {
        List<Vector3> set = m_EnemyPointSet[setIndex].EnemyPointSet;
        set.Add(set[set.Count - 1] + Vector3.left * 2);
    }

    public void RemovePointFromEnemySet(int setIndex)
    {
        List<Vector3> set = m_EnemyPointSet[setIndex].EnemyPointSet;
        if (set.Count <= 1)
            throw new Exception("Cannot have an empty enemy points set");
        set.RemoveAt(set.Count - 1);
    }

    public void SetPointInEnemySet(int setIndex, int pointIndex, Vector3 newPos)
    {
        List<Vector3> set = m_EnemyPointSet[setIndex].EnemyPointSet;
        set[pointIndex] = newPos;
    }

    public void UpdateAllPointsInSet(int setIndex, Vector3 translateDirection)
    {
        List<Vector3> set = m_EnemyPointSet[setIndex].EnemyPointSet;
        for (int i = 0; i < set.Count; i++)
        {
            set[i] += translateDirection;
        }
    }

    public void AddSpiderPoint()
    {
        m_SpiderSpawns.Add(GetLocalPosInEditor());
    }

    public void RemoveSpiderPoint()
    {
        if (m_SpiderSpawns.Count == 0)
            throw new System.Exception("Spider list already empty");
        m_SpiderSpawns.RemoveAt(m_SpiderSpawns.Count - 1);
    }
    public int GetSpiderSpawnCount()
    {
        return m_SpiderSpawns.Count;
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

        return m_EnemyPointSet[index].EnemyPointSet;
    }

    public List<Vector3> GetEnemyPointSetWorld(int index)
    {
        if (index >= m_EnemyPointSet.Count || index < 0)
            throw new Exception("Not a valid index in m_EnemyPointSet");

        EnemySetWrapper set = m_EnemyPointSet[index];
        List<Vector3> worldSet = new List<Vector3>();
        for (int i = 0; i < set.EnemyPointSet.Count; i++)
        {
            worldSet.Add(transform.TransformPoint(set.EnemyPointSet[i]));
        }
        return worldSet;
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

    public List<Vector3> SpiderSpawns { get { return m_SpiderSpawns; } }
    public Vector3 GetSpiderSpawn(int index)
    {
        if (index >= m_SpiderSpawns.Count || index < 0)
            throw new System.Exception("Not a valid index in m_SpiderSpawns");
        return m_SpiderSpawns[index];
    }
    public Vector3 GetSpiderSpawnWorld(int index)
    {
        if (index >= m_SpiderSpawns.Count || index < 0)
            throw new System.Exception("Not a valid index in m_SpiderSpawns");
        return transform.TransformPoint(m_SpiderSpawns[index]);
    }
    public void SetSpiderSpawn(int index, Vector3 point)
    {
        m_SpiderSpawns[index] = point;
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

    public bool IsTraversedByPlayer { get { return m_IsTraversedByPlayer; } }

    public int ID { get { return m_ID; } }

    // Stalags ***********

    public Stalag GetStalagSpawnPoint(int index)
    {
        return m_Stalags[index];
    }
    public Stalag GetStalagSpawnPointWorld(int index)
    {
        return m_Stalags[index];
    }
    public int GetStalagSpawnCount()
    {
        return m_Stalags.Count;
    }
    public void AddStalagSpawnPoint()
    {
        m_Stalags.Add(new Stalag(GetLocalPosInEditor()));
    }
    public void RemoveStalagPoint()
    {
        if (m_Stalags.Count == 0)
            throw new System.Exception("List of stalags already empty");
        m_Stalags.RemoveAt(m_Stalags.Count - 1);
    }
    public void UpdateStalgPoint(int index, Vector3 position)
    {
        m_Stalags[index].Position = position;
    }
    public void ChangeStalagOrientation(int index)
    {
        // flip the orientation of the stalag
        m_Stalags[index].ChangeStalgOrientation();
    }
    public bool GetIsStalagPointingUp(int index)
    {
        return m_Stalags[index].GetIsPointUp();
    }

    // Lost Moths ************

    public Vector3 GetLostMothPoint(int index)
    {
        return m_LostMothPoints[index];
    }
    public void UpdateLostMothPoint(int index, Vector3 position)
    {
        m_LostMothPoints[index] = position;
    }
    public int GetLostMothCount()
    {
        return m_LostMothPoints.Count;
    } 
    public void AddLostMothPoint()
    {
        m_LostMothPoints.Add(GetLocalPosInEditor());
    }
    public void RemoveLostMothPoint(int index)
    {
        if (index >= GetLostMothCount())
        {
            Debug.LogWarning("Lost moth index out of range");
            return;
        }
        m_LostMothPoints.RemoveAt(index);
    }

    public enum LOCATION_TYPES
    {
        FOLLOW,
        START, 
        END,
        SPIDER,
        STALAG,
        LOST_MOTH,
    }

    
}
