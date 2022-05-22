using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderManager : MonoBehaviour
{
    [SerializeField] private SpiderController m_SpiderPrefab;

    private List<SpiderController> m_Spiders;

    static SpiderManager s_PropertyInstance;
    public static SpiderManager PropertyInstance
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

    private void Start()
    {
        m_Spiders = new List<SpiderController>();
        TileManager.PropertyInstance.d_TileAddedDelegate += SpawnNewSpiders;
        TileManager.PropertyInstance.d_TileDeletedDelegate += DeleteSpidersAtTile;
    }

    private void SpawnNewSpiders(Tile tile)
    {
        for (int i = 0; i < tile.GetSpiderSpawnCount(); i++)
        {
            Vector3 position = tile.GetSpiderSpawnWorld(i);
            SpiderController controller = Instantiate(m_SpiderPrefab, position, Quaternion.identity);
            // set TIleID of spider to know when to remove from world
            controller.TileID = tile.ID;
            m_Spiders.Add(controller);
        }
    }

    private void DeleteSpidersAtTile(Tile tile)
    {
        int id = tile.ID;
        for (int i = 0; i < m_Spiders.Count; i++)
        {
            // delete spider if it contains the tile id that was deleted
            if (m_Spiders[i].TileID == id)
                DeleteSpiderAtIndex(i, m_Spiders[i].gameObject);
            // otherwise, no other spider has that tileID since only the last spawned tile is deleted at a time
            else
                break;
        }
    }

    public void OnSpiderDeath(GameObject spiderToDelete)
    {
        // find the spider to delete and remove from list and game
        for (int i= 0; i < m_Spiders.Count; i++)
        {
            if (GameObject.ReferenceEquals(m_Spiders[i].gameObject, spiderToDelete))
            {
                DeleteSpiderAtIndex(i, spiderToDelete);
                break;
            }
        }
    }

    private void DeleteSpiderAtIndex(int index, GameObject spiderToDelete)
    {
        Destroy(spiderToDelete);
        m_Spiders.RemoveAt(index);
    }
}
