using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    [SerializeField] GameObject m_PartPrefab;
    [SerializeField] GameObject m_StartingPoint;
    [SerializeField] GameObject m_Spider;

    [Range(1f, 1000f)]
    [SerializeField] private float m_Length = 1;
    [SerializeField] private bool m_AutoLength = true;
    [SerializeField] private float m_partDistance = 0.21f;
    [SerializeField] bool spawn = true, snapLast = true;

    private LayerMask m_LayerMask;

    public delegate void WebDestroyedDelegate();
    public WebDestroyedDelegate d_WebDestroyedDelegate;

    private List<GameObject> m_SpiderWebParts;

    private void Start()
    {
        m_LayerMask = 1 << 9;
        m_StartingPoint.transform.parent = null;
        m_SpiderWebParts = new List<GameObject>();
    }

    private void Update()
    {
        if (spawn)
        {
            Spawn();
            spawn = false;
        }
    }

    public void Spawn()
    {
        if (m_AutoLength)
        {
            RaycastHit hit;
            if (Physics.Raycast(m_StartingPoint.transform.position, Vector3.up, out hit, m_LayerMask))
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
            GameObject tmp;

            tmp = Instantiate(m_PartPrefab, new Vector3 (transform.position.x, transform.position.y + m_partDistance * (i + 1), transform.position.z), Quaternion.identity, m_StartingPoint.transform);
            m_SpiderWebParts.Add(tmp);
            tmp.transform.eulerAngles = new Vector3(180, 0, 0);

            tmp.name = transform.childCount.ToString();

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
                tmp.GetComponent<Joint>().connectedBody = transform.Find((transform.childCount - 1).ToString()).GetComponent<Rigidbody>();
            }
        }

        if (snapLast)
        {
            Rigidbody topRigidBody = transform.Find((transform.childCount).ToString()).GetComponent<Rigidbody>();
            topRigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    public void DeleteAll()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
        Destroy(gameObject);
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
            Destroy(m_SpiderWebParts[0].GetComponent<Joint>());
        }
        
    }
    public void DestroyWeb()
    {
        // destroy each spider web that was created
        foreach (GameObject webPart in m_SpiderWebParts)
        {
            Destroy(webPart);
        }
    }
}
