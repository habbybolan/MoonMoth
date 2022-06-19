using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    [SerializeField] SpiderWebHealth m_PartPrefab;
    [SerializeField] GameObject m_Spider;

    [Range(1f, 1000f)]
    [SerializeField] private float m_Length = 1;
    [SerializeField] private bool m_AutoLength = true;
    [SerializeField] private float m_partDistance = 0.21f;
    [SerializeField] public bool spawn = false, snapLast = true;

    private LayerMask m_LayerMask;

    public delegate void WebDestroyedDelegate();
    public WebDestroyedDelegate d_WebDestroyedDelegate;

    private List<SpiderWebHealth> m_SpiderWebParts;
    private GameObject m_SpawnPointCopy;

    private void Start()
    {
        m_LayerMask = 1 << 9;
    }

    private void Update()
    { 
        if (spawn)
        {
            m_SpawnPointCopy = new GameObject();
            m_SpawnPointCopy.transform.position = transform.position;
            m_SpiderWebParts = new List<SpiderWebHealth>();
            Spawn();
            spawn = false;
        }
    }

    public void Spawn()
    {
        if (m_AutoLength)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.up, out hit, m_LayerMask))
            {
                m_Length = hit.distance;
            } else
            {
                Debug.LogWarning("Raycast for spider web could not find a ceiling");
            }
        }

        int count = (int)( m_Length / m_partDistance);

        // Create new part of the web and attach it to previous
        for (int i = 0; i < count; i++)
        {
            SpiderWebHealth tmp;

            tmp = Instantiate(m_PartPrefab, new Vector3 (transform.position.x, transform.position.y + m_partDistance * (i + 1), transform.position.z), Quaternion.identity, m_SpawnPointCopy.transform);
            tmp.d_DeathDelegate += SpiderWebBroke;
            m_SpiderWebParts.Add(tmp);
            tmp.transform.eulerAngles = new Vector3(180, 0, 0);

            tmp.name = m_SpawnPointCopy.transform.childCount.ToString();

            if (i == 0)
            {
                // attach first to spider
                if (m_Spider != null)
                {
                    tmp.GetComponent<Joint>().connectedBody = m_Spider.GetComponent<Rigidbody>();
                } else
                {
                    Destroy(tmp.GetComponent<Joint>());
                }
            }
            else
            {
                Joint joint = tmp.GetComponent<Joint>();
                Transform webPieceTransform = m_SpawnPointCopy.transform.Find((m_SpawnPointCopy.transform.childCount - 1).ToString());
                joint.connectedBody = webPieceTransform.GetComponent<Rigidbody>();
            }
        }

        if (snapLast)
        {
            Rigidbody topRigidBody = m_SpawnPointCopy.transform.Find((m_SpawnPointCopy.transform.childCount).ToString()).GetComponent<Rigidbody>();
            topRigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    public void DeleteAll()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject == gameObject) return;
            Destroy(child.gameObject);
        }
        Destroy(m_SpawnPointCopy);
    }

    public void SpiderWebBroke()
    {
        RemoveSpiderFromWeb();
        d_WebDestroyedDelegate();
    }

    public void RemoveSpiderFromWeb()
    {
        Joint joint = m_SpiderWebParts[0].GetComponent<Joint>();
        if (joint != null)
        {
            joint.connectedBody = null;
        }
        
    }
    public void DestroyWeb()
    {
        if (m_SpiderWebParts == null) return;
        // destroy each spider web that was created
        foreach (SpiderWebHealth webPart in m_SpiderWebParts)
        {
            Destroy(webPart.gameObject);
        }
    }
}
