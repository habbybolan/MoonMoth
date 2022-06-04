using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class Tile : MonoBehaviour
{
    // Manually set points
    public Vector3[] m_FollowPoint;
    public Vector3 m_StartPoint;
    public Vector3 m_EndPoint;
    public List<EnemySetWrapper> m_EnemyPointSet;
    public List<Vector3> m_LostMothPoints;

    private Vector3 m_VecStartToCenter;
    private Vector3 m_VecCenterToEnd;
    
    
    // prefabs
    public StalagScriptable m_StalagPrefab;
    public LostMoth m_LostMothPrefab;
    public MushroomObstacle m_MushroomPrefab;

    private bool m_IsTraversedByPlayer = false;   // If the tile has been traversed fully by the player
    private int m_ID;
    private BoxCollider m_EndCollider;

    private List<GameObject> m_SpawnedTileObjects;  // list of objects connected to tile for deletion

    private List<GameObject>[] m_SetObjectSpawns;

    private void Awake()
    {
        m_VecStartToCenter = transform.position - transform.TransformPoint(m_StartPoint);
        m_VecCenterToEnd = transform.TransformPoint(m_EndPoint) - transform.position;
        m_ID = TileManager.GetNewID();

        if (m_EnemyPointSet == null)
            m_EnemyPointSet = new List<EnemySetWrapper>();
        if (m_LostMothPoints == null)
            m_LostMothPoints = new List<Vector3>();
        
        // Store each dynamically spawned object into a list 
        m_SetObjectSpawns = new List<GameObject>[TileManager.PropertyInstance.NumSpawnTypes];
        for (int i = 0; i < m_SetObjectSpawns.Length; i++)
        {
            m_SetObjectSpawns[i] = new List<GameObject>();
        }
        foreach (Transform child in transform)
        {
            GameObject childObj = child.gameObject;
            switch(childObj.tag) {
                case "Stalag":
                    m_SetObjectSpawns[0].Add(childObj);
                    break;
                case "Spider":
                    m_SetObjectSpawns[1].Add(childObj);
                    break;
                case "Mushroom":
                    m_SetObjectSpawns[2].Add(childObj);
                    break;
            }
        }
    }

    private void Start()
    {
        m_EndCollider = GetComponent<BoxCollider>();
    }

    public void InitializeTile()
    {
        // Spawn all stalag prefabs
        m_SpawnedTileObjects = new List<GameObject>();
        foreach (Vector3 lostMoth in m_LostMothPoints)
        {
            LostMoth lostMothObj = Instantiate(m_LostMothPrefab, transform.TransformPoint(lostMoth), Quaternion.identity);
            m_SpawnedTileObjects.Add(lostMothObj.gameObject);
        }

        // Set as active/inactive based on the spawn percent of each objects defined in TileManager
        for (int i = 0; i < TileManager.PropertyInstance.NumSpawnTypes; i++)
        {
            List<GameObject> spawnsOfType = m_SetObjectSpawns[i];
            float spawnPercent = TileManager.PropertyInstance.SpawnRates[i];
            // Set as inactive/active
            foreach (GameObject spawnObject in spawnsOfType)
            {
                bool isSpawn = UnityEngine.Random.Range(0, 100) <= spawnPercent;
                spawnObject.SetActive(isSpawn);
            }
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

    public void AddFollowPoint(Vector3 spawnPoint)
    {
        Array.Resize(ref m_FollowPoint, PlayerFollowPointsCount + 1);
        m_FollowPoint[PlayerFollowPointsCount - 1] = spawnPoint;
    }

    public void RemoveFollowPoint()
    {
        if (PlayerFollowPointsCount == 1)
            throw new Exception("Cannot have an empty follow points list");

        Array.Resize(ref m_FollowPoint, PlayerFollowPointsCount - 1);
    }

    public void AddEnemyPointSet(Vector3 spawnPoint)
    {
        m_EnemyPointSet.Add(new EnemySetWrapper(spawnPoint));
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
    public void AddLostMothPoint(Vector3 spawnPoint)
    {
        m_LostMothPoints.Add(spawnPoint);
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
        LOST_MOTH,
    }

    
}
