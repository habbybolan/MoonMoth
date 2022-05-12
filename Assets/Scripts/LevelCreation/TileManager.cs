using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    static TileManager s_PropertyInstance;

    [SerializeField] private Tile[] m_TilePrefabs;
    [SerializeField] private float m_DistanceToPlaceTile = 200f;
    [SerializeField] private float m_DistanceToDeleteTile = 50f;

    private Tile[] m_PoolTiles = new Tile[20];
    private LinkedList<Tile> m_VisibleTiles = new LinkedList<Tile>();

    public static TileManager PropertyInstance
    { 
        get { return s_PropertyInstance; }
    }

    private void Awake()
    {
        // Singleton
        if (s_PropertyInstance != null && s_PropertyInstance != this)
            Destroy(this);
        else
            s_PropertyInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // create uniform distribution of all prefab tiles
        int numEachTile = m_PoolTiles.Length / m_TilePrefabs.Length;
        int indexPrefab = 0;
        int indexPool = 0;
        int numEachCurrTile = 0;

        // Initialize pool of GameObjects
        while (indexPool < m_PoolTiles.Length)
        {
            // Reached max number of current prefab inside pool and not at last prefab
            if (numEachCurrTile == numEachTile && indexPrefab != m_TilePrefabs.Length - 1)
            {
                indexPrefab++;
                numEachCurrTile = 0;
            }
            Tile newTile = Instantiate(m_TilePrefabs[indexPrefab], Vector3.zero, Quaternion.identity, transform);
            m_PoolTiles[indexPool] = newTile;
            newTile.gameObject.SetActive(false);
            indexPool++;
            numEachCurrTile++;
        }

        for (int i = 0; i < 5; i++)
        {
            InstantiateTile();
        }
    }

    private void InstantiateTile()
    {
        // Find and add new tile to end of path
        var newTile = FindAvailableTile();

        Tile lastCreated = null;
        if (m_VisibleTiles.Count > 0)
            lastCreated = m_VisibleTiles.Last.Value;

        int visibleCount = m_VisibleTiles.Count;

        m_VisibleTiles.AddLast(newTile);
        newTile.gameObject.SetActive(true);

        // If first created, dont do anything
        if (visibleCount == 0)
            return;
        
        if (lastCreated != null)
        {
            Vector3 newPos = lastCreated.EndPointWorld + newTile.VecStartToCenter;
            newTile.transform.position = newPos;
        }
    }

    private Tile FindAvailableTile()
    {
        int rand = Random.Range(0, m_PoolTiles.Length);
        int numIterations = 0;
        // loop until non-active gameobject found
        while (m_PoolTiles[rand].gameObject.activeSelf)
        {
            rand = (rand + 1) % (m_PoolTiles.Length - 1);
            numIterations++;
            if (numIterations == m_PoolTiles.Length)
                throw new System.Exception("Stopped infinite loop when searching for available tile");
        }
        return m_PoolTiles[rand];
    }

    private void CheckRemoveTile()
    {
        Tile firstTile = m_VisibleTiles.First.Value;

        float distanceFromEnd = firstTile.TileEndDistanceFromPlayer(PlayerManager.PropertyInstance.Player);
        float distanceFromStart = firstTile.TileStartDistanceFromPlayer(PlayerManager.PropertyInstance.Player);
        if (distanceFromEnd > m_DistanceToDeleteTile && distanceFromStart > distanceFromEnd + m_DistanceToDeleteTile)
        {
            firstTile.gameObject.SetActive(false);
            m_VisibleTiles.RemoveFirst();
        }
    }

    private void CheckAddTile()
    {
        // TODO: Make reliant on distance rather then z-position
        Tile lastTile = m_VisibleTiles.Last.Value;
        // add tile if player is within a certain distance from last tile
        if (lastTile.TileEndDistanceFromPlayer(PlayerManager.PropertyInstance.Player) < m_DistanceToPlaceTile)
        {
            InstantiateTile();
        }
    }

    public LinkedListNode<Tile> GetHead()
    {
        return m_VisibleTiles.First;
    }

    // Update is called once per frame
    void Update()
    {
        CheckRemoveTile();
        CheckAddTile();
    }
}
