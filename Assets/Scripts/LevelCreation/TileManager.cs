using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    static TileManager s_PropertyInstance;

    [Header("Tiles")]
    [SerializeField] private TileListWrapper[] m_TileSets;
    [Tooltip("Empty tile that's used as a transition piece between tile set 1 and 2")]
    [SerializeField] private Tile m_TileTransition1;
    [Tooltip("Empty tile that's used as a transition piece between tile set 2 and 3")]
    [SerializeField] private Tile m_TileTransition2;

    [SerializeField] private float m_DistanceToPlaceTile = 200f;
    [SerializeField] private int m_TilePoolSize = 40;

    private int m_CurrentTileSet = 0;

    // Delegate for every time a new tile is added
    public delegate void TileAddedDelegate(Tile addedTile); 
    public TileAddedDelegate d_TileAddedDelegate;

    // Delegate for every time the oldest visible tile is deleted
    public delegate void TileDeletedDelegate(Tile deletedTile); 
    public TileDeletedDelegate d_TileDeletedDelegate; 

    private Tile[] m_PoolTiles;
    private LinkedList<Tile> m_VisibleTiles = new LinkedList<Tile>();

    private static int m_IDCount = 0;
    private bool m_IsInitialized = false;   // If all starting tiles have been initialize

    private float m_TileLevel = 0;  // tile levels are from 0-2

    public static TileManager PropertyInstance
    { 
        get { return s_PropertyInstance; }
    }

    private void Awake()
    {
        m_PoolTiles = new Tile[m_TilePoolSize];
        // Singleton
        if (s_PropertyInstance != null && s_PropertyInstance != this)
            Destroy(this);
        else
            s_PropertyInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeStartTile();
        m_IsInitialized = true;
    }

    private void InitializeStartTile() 
    {
        // create uniform distribution of all prefab tiles
        int numEachTile = m_PoolTiles.Length / m_TileSets[m_CurrentTileSet].tiles.Length;
        int indexPrefab = 0;
        int indexPool = 0;
        int numEachCurrTile = 0;

        // Initialize pool of Tiles
        while (indexPool < m_PoolTiles.Length)
        {
            // Reached max number of current prefab inside pool and not at last prefab
            if (numEachCurrTile == numEachTile && indexPrefab != m_TileSets[m_CurrentTileSet].tiles.Length - 1)
            {
                indexPrefab++;
                numEachCurrTile = 0;
            }
            Tile newTile = Instantiate(m_TileSets[m_CurrentTileSet].tiles[indexPrefab], Vector3.zero, Quaternion.identity, transform);
            m_PoolTiles[indexPool] = newTile;
            newTile.gameObject.SetActive(false);
            indexPool++;
            numEachCurrTile++;
        }

        // Initialize level with some tiles
        for (int i = 0; i < 5; i++)
            InstantiateTile();
    }

    public static int GetNewID()
    {
        return m_IDCount++;
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
        newTile.SetIsActive(true);

        // First tile being created
        if (visibleCount == 0)
        {
            // notify delegate a tile was created
            if (d_TileAddedDelegate != null)
                d_TileAddedDelegate(newTile);
            return;
        }

        if (lastCreated != null)
        {
            Vector3 newPos = lastCreated.EndPointWorld + newTile.VecStartToCenter;
            newTile.transform.position = newPos;
        }

        if (d_TileAddedDelegate != null)
            d_TileAddedDelegate(newTile);
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

    // Delete 2 tiles back once player has traversed the back 2 tiles
    private void CheckRemoveTile()
    {
        Tile firstTile = m_VisibleTiles.First.Value;
        Tile secondTile = m_VisibleTiles.First.Next.Value;
        if (firstTile.IsTraversedByPlayer && secondTile.IsTraversedByPlayer)
        {
            if (d_TileDeletedDelegate != null)
                d_TileDeletedDelegate(firstTile);
            firstTile.DeleteAllSpawned();
            firstTile.SetIsActive(false);
            m_VisibleTiles.RemoveFirst();
        }
    }

    private void CheckAddTile()
    {
        // TODO: Make reliant on distance rather then z-position
        Tile lastTile = m_VisibleTiles.Last.Value;
        // add tile if player is within a certain distance from last tile
        if (lastTile.TileEndDistanceFromPlayer(PlayerManager.PropertyInstance.PlayerController.PlayerParent) < m_DistanceToPlaceTile)
        {
            InstantiateTile();
        }
    }

    // On conditions of tile set being fulfilled, goto next set or player won the game
    // returns true if the game was won
    public bool TileSetFinished()
    {
        // only delete set if game was running so there's a set to delete
        if (GameState.m_GameState == GameStateEnum.RUNNING)
        {
            DeleteSet();
        }
        return false;
    }

    private void DeleteSet()
    {
        StartCoroutine(TransitionToNextSet());
    }

    // Asynchronously 
    IEnumerator TransitionToNextSet()
    {
        foreach (Tile tile in m_PoolTiles)
        {
            Destroy(tile.gameObject);
            yield return new WaitForSeconds(.02f);
        }
        m_PoolTiles = new Tile[m_TilePoolSize];
        m_VisibleTiles.Clear();
        m_CurrentTileSet++;

        // TODO: Make asynchronous instead of all in one frame
        InitializeStartTile();
        GameState.m_GameState = GameStateEnum.RUNNING;
    }

    public bool IsInitialized { get { return m_IsInitialized; } }

    public LinkedListNode<Tile> GetHead()
    {
        return m_VisibleTiles.First;
    }

    // Update is called once per frame
    void Update()
    {
        // only update if game is in normal running state
        if (GameState.m_GameState != GameStateEnum.RUNNING) return;

        CheckRemoveTile();
        CheckAddTile();
    }
}
