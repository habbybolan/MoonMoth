using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    static TileManager s_PropertyInstance;

    [Header("Tiles")]
    [Header("List of all the sets of tiles. Each set corresponds to a level that starts at the first and ends at the last set")]
    [SerializeField] private TileListWrapper[] m_TileSets;
    [Tooltip("Empty tile that's used as a transition piece between set at index i and i+1. Size should be length of m_TileSets - 1")]
    [SerializeField] private Tile[] m_TileTransitions;

    [SerializeField] private float m_NumTilesToSpawn = 8f;  
    [SerializeField] private int m_TilePoolSize = 40;

    [Header("Difficulty")]
    [Tooltip("The percent the difficulty increases each second player is in a set that spawns that specific obstacle/enemy, with a max of 100% difficulty where everything is spawning." +
        "The highest percent is limited by the max spawn percent set for each obstacle/enemy")]
    [Range(0, 100)]
    [SerializeField] private float m_PercentDifficultyIncreasePerSecond = 1f;
    [Tooltip("Max spawn percent of stalags in a level. Represents the max difficulty of stalags")]
    [Range(0, 100)]
    [SerializeField] private float m_StalagMaxSpawnPercent = 50f;
    [Tooltip("Starting spawn percent of stalags")]
    [Range(0, 100)]
    [SerializeField] private float m_StalagStartingSpawnPercent = 10; 
    [Tooltip("Max spawn percent of spiders in a level. Represents the max difficulty of spiders")]
    [Range(0, 100)]
    [SerializeField] private float m_SpiderMaxSpawnPercent = 50f;
    [Tooltip("Starting spawn percent of Spiders")]
    [Range(0, 100)]
    [SerializeField] private float m_SpiderStartingSpawnPercent = 10;
    [Tooltip("Max spawn percent of mushrooms in a level. Represents the max difficulty of mushrooms")]
    [Range(0, 100)]
    [SerializeField] private float m_MushroomMaxSpawnPercent = 50f;
    [Tooltip("Starting spawn percent of mushrooms")]
    [Range(0, 100)]
    [SerializeField] private float m_MushroomStartingSpawnPercent = 10;
    [SerializeField] private bool m_IsDifficultyValuesLogged = false;

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
    private float m_currDifficultyPercent = 0;

    private float m_StalagCurrentSpawnPercent;
    private float m_SpiderCurrentSpawnPercent;
    private float m_MushroomCurrentSpawnPercent;

    private Tile m_transitionTile;

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

        m_StalagCurrentSpawnPercent = m_StalagStartingSpawnPercent;
        m_SpiderCurrentSpawnPercent = m_SpiderStartingSpawnPercent;
        m_MushroomCurrentSpawnPercent = m_MushroomStartingSpawnPercent;
}

    // Start is called before the first frame update
    void Start()
    {
        GameState.PropertyInstance.d_GameRunningDelegate += InitializeStartTile;
        
        if (m_TileTransitions.Length < m_TileSets.Length - 1)
        {
            Debug.LogWarning("There should be a transition tile m_TileTransitions for each tile transition between sets m_TileSets in TileManager");
        }

        if (m_IsDifficultyValuesLogged)
        {
            StartCoroutine(SpawnPercentagesPrint());
        }
    }

    // For debugging percentage values
    IEnumerator SpawnPercentagesPrint()
    {
        while (true)
        {
            Debug.Log("------------------------------------------");
            Debug.Log("Stalag: " + m_StalagCurrentSpawnPercent.ToString());
            Debug.Log("Spiders: " + m_SpiderCurrentSpawnPercent.ToString());
            Debug.Log("Mushrooms: " + m_MushroomCurrentSpawnPercent.ToString());
            yield return new WaitForSeconds(1);
        }
    }

    private void InitializeStartTile()
    {
        m_IsInitialized = true;
        // create uniform distribution of all prefab tiles
        int numEachTile = m_PoolTiles.Length / m_TileSets[GameManager.PropertyInstance.CurrLevel].tiles.Length;
        int indexPrefab = 0;
        int indexPool = 0;
        int numEachCurrTile = 0;

        // Initialize pool of Tiles
        while (indexPool < m_PoolTiles.Length)
        {
            // Reached max number of current prefab inside pool and not at last prefab
            if (numEachCurrTile == numEachTile && indexPrefab != m_TileSets[GameManager.PropertyInstance.CurrLevel].tiles.Length - 1)
            {
                indexPrefab++;
                numEachCurrTile = 0;
            }
            Tile newTile = Instantiate(m_TileSets[GameManager.PropertyInstance.CurrLevel].tiles[indexPrefab], Vector3.zero, Quaternion.identity, transform);
            m_PoolTiles[indexPool] = newTile;
            newTile.gameObject.SetActive(false);
            indexPool++;
            numEachCurrTile++;
        }

        // Initialize level with some tiles
        for (int i = 0; i < m_NumTilesToSpawn; i++)
            InstantiateTile();
    }

    public static int GetNewID()
    {
        return m_IDCount++;
    }

    private void InstantiateTile()
    {
        Tile newTile;
        // if adding the first tile on transitions to new tileset, start with designated tile transition
        if (m_VisibleTiles.Count == 0 && m_TileTransitions.Length > GameManager.PropertyInstance.CurrLevel - 1)
        {
            newTile = Instantiate(m_TileTransitions[GameManager.PropertyInstance.CurrLevel], Vector3.zero, Quaternion.identity, transform);
            m_transitionTile = newTile;
        } else
        {
            // Find and add new tile to end of path
            newTile = FindAvailableTile();
        }

        Tile lastCreated = null;
        if (m_VisibleTiles.Count > 0)
            lastCreated = m_VisibleTiles.Last.Value;

        int visibleCount = m_VisibleTiles.Count;

        m_VisibleTiles.AddLast(newTile);
        newTile.gameObject.SetActive(true);
        newTile.InitializeTile();

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
            newTile.transform.rotation = lastCreated.transform.rotation * lastCreated.EndPointRotation;
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
            firstTile.gameObject.SetActive(false);
            m_VisibleTiles.RemoveFirst();
            InstantiateTile();
        }
    }

    private void IncrementSpawnPercents()
    {
        switch(GameManager.PropertyInstance.CurrLevel)
        {
            case 0:
                m_StalagCurrentSpawnPercent += m_PercentDifficultyIncreasePerSecond * Time.deltaTime;
                break;
            case 1:
                m_StalagCurrentSpawnPercent += m_PercentDifficultyIncreasePerSecond * Time.deltaTime;
                m_SpiderCurrentSpawnPercent += m_PercentDifficultyIncreasePerSecond * Time.deltaTime;
                break;
            case 2:
                m_StalagCurrentSpawnPercent += m_PercentDifficultyIncreasePerSecond * Time.deltaTime;
                m_SpiderCurrentSpawnPercent += m_PercentDifficultyIncreasePerSecond * Time.deltaTime;
                m_MushroomCurrentSpawnPercent += m_PercentDifficultyIncreasePerSecond * Time.deltaTime;
                break;
        }
        if (m_StalagCurrentSpawnPercent > m_StalagMaxSpawnPercent) m_StalagCurrentSpawnPercent = m_StalagMaxSpawnPercent;
        if (m_SpiderCurrentSpawnPercent > m_SpiderMaxSpawnPercent) m_SpiderCurrentSpawnPercent = m_SpiderMaxSpawnPercent;
        if (m_MushroomCurrentSpawnPercent > m_MushroomMaxSpawnPercent) m_MushroomCurrentSpawnPercent = m_MushroomMaxSpawnPercent;
    }

    // On conditions of tile set being fulfilled, goto next set or player won the game
    public void TileSetFinished()
    {
        DeleteSet();
    }

    private void DeleteSet()
    {
        StartCoroutine(TransitionToNextSet());
    }

    // Checks if it is valid to spawn a firefly by checking if a tile exists before the player's current tile
    public bool IsValidToSpawnFirefly()
    {
        return GetHead() != null && GetHead().Value.IsTraversedByPlayer;
    }

    // Asynchronously 
    IEnumerator TransitionToNextSet()
    {
        Destroy(m_transitionTile.gameObject);

        foreach (Tile tile in m_PoolTiles)
        {
            Destroy(tile.gameObject);
            yield return new WaitForSeconds(.02f);
        }
        m_PoolTiles = new Tile[m_TilePoolSize];
        m_VisibleTiles.Clear();


        // TODO: Make asynchronous instead of all in one frame
        GameManager.PropertyInstance.UpdateState(GameStateEnum.RUNNING);
        
    }

    public bool IsInitialized { get { return m_IsInitialized; } }

    public LinkedListNode<Tile> GetHead()
    {
        return m_VisibleTiles.First;
    }

    // [stalags, spiders, mushrooms]
    public float[] SpawnRates => new float[] { m_StalagCurrentSpawnPercent, m_SpiderCurrentSpawnPercent, m_MushroomCurrentSpawnPercent };
    public int NumSpawnTypes => 3; 

    // Update is called once per frame
    void Update()
    {
        // only update if game is in normal running state
        if (GameState.PropertyInstance.GameStateEnum != GameStateEnum.RUNNING) return;

        CheckRemoveTile();
        IncrementSpawnPercents();
    }
}
