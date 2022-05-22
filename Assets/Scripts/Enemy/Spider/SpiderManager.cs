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
        
        TileManager.PropertyInstance.d_TileAddedDelegate += SpawnNewSpiders;
        TileManager.PropertyInstance.d_TileDeletedDelegate += DeleteSpidersAtTile;
        m_Spiders = new List<SpiderController>();
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

        // Delete all spiders assocuated with this tile
        while (m_Spiders.Count > 0 && m_Spiders[0].TileID == id)
        {
            DeleteSpiderAtIndex(0, m_Spiders[0].gameObject);
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
