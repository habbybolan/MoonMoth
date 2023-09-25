using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class Stick
{
    public Stick(SpiderWebPart a, SpiderWebPart b)
    {
        partA = a;
        partB = b;
        length = Vector3.Distance(partA.position, partB.position);
    }
    public SpiderWebPart partA, partB;
    public float length;
}

public class SpiderWeb : MonoBehaviour
{
    [SerializeField] SpiderWebPart m_PartPrefab;
    [SerializeField] GameObject m_Spider;

    [Range(1f, 1000f)]
    [SerializeField] private float m_Length = 1;
    [SerializeField] private bool m_AutoLength = true;
    [SerializeField] private float m_partDistance = 0.21f;
    [SerializeField] public bool spawn = false, snapLast = true;
    [Min(1)]
    [SerializeField] public int numIterations = 5;

    private LayerMask m_LayerMask;

    public delegate void WebDestroyedDelegate();
    public WebDestroyedDelegate d_WebDestroyedDelegate;

    private List<SpiderWebPart> m_SpiderWebParts = null;
    private GameObject m_SpawnPointCopy;

    private List<Stick> m_Sticks;

    private void Start()
    {
        m_LayerMask = 1 << 9;
    }

    private void Update()
    {
        if (spawn)
        {
            m_SpiderWebParts = new List<SpiderWebPart>();
            m_Sticks = new List<Stick>();
            m_SpawnPointCopy = new GameObject();
            m_SpawnPointCopy.transform.position = transform.position;
            Spawn();
            spawn = false;
        }

        if (m_SpiderWebParts == null) return;

        for (int i = 0; i < m_SpiderWebParts.Count; i++)
        {
            if (i == 0)
            {
                if (m_Spider != null)
                {
                    m_Spider.transform.position = m_SpiderWebParts[i].position - new Vector3(0, 2, 0);
                }
            }
           

            SpiderWebPart part = m_SpiderWebParts[i];
            Vector3 vel = (part.position - part.prevPosition) * 0.99f;
            if (!part.bLocked)
            {
                Vector3 positionBeforeUpdate = part.position;
                part.position += vel;
                part.position += Vector3.down * 9.81f * Time.deltaTime * Time.deltaTime;
                part.prevPosition = positionBeforeUpdate;
            }
            part.transform.position = part.position;
            Vector3 rot = Vector3.zero;
            if (i > 0) rot += m_SpiderWebParts[i - 1].position - part.position;
            if (i < m_SpiderWebParts.Count - 1) rot += part.position - m_SpiderWebParts[i + 1].position;
            part.transform.rotation = Quaternion.LookRotation(rot, Vector3.up) * Quaternion.Euler(Vector3.right * -90);
        }

        for (int i = 0; i < numIterations; i++)
        {
            foreach (Stick stick in m_Sticks)
            {
                Vector3 stickCenter = (stick.partA.position + stick.partB.position) / 2;
                Vector3 stickDir = (stick.partA.position - stick.partB.position).normalized;
                if (!stick.partA.bLocked)
                    stick.partA.position = stickCenter + (stickDir * stick.length) / 2;
                if (!stick.partB.bLocked)
                    stick.partB.position = stickCenter - (stickDir * stick.length) / 2;
            }
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
                m_Length = 10;
            }
        }

        int count = (int)( m_Length / m_partDistance);

        // Create new part of the web and attach it to previous
        for (int i = 0; i < count; i++)
        {
            SpiderWebPart tmp;

            tmp = Instantiate(m_PartPrefab, new Vector3 (transform.position.x, transform.position.y + m_partDistance * (i + 1), transform.position.z), Quaternion.identity, m_SpawnPointCopy.transform);
            tmp.position = tmp.transform.position;
            tmp.prevPosition = tmp.transform.position;
            tmp.health.d_DeathDelegate += SpiderWebBroke;
            m_SpiderWebParts.Add(tmp);

            tmp.name = "Part" + m_SpawnPointCopy.transform.childCount.ToString();

            if (i == 0)
            {
                /*tmp.prevPosition = tmp.transform.position + Vector3.right * 0.2f;
                // attach first to spider
                if (m_Spider != null)
                {
                    m_Spider.transform.parent = tmp.transform;
                }*/
            }
            else
            {
                m_Sticks.Add(new Stick(m_SpiderWebParts[i], m_SpiderWebParts[i - 1]));
            }

            // lock last one created
            if (i == count - 1)
            {
                tmp.bLocked = true;
            }
        }

        //if (snapLast)
        //{
        //    //Rigidbody topRigidBody = m_SpawnPointCopy.transform.Find((m_SpawnPointCopy.transform.childCount).ToString()).GetComponent<Rigidbody>();
        //    //topRigidBody.constraints = RigidbodyConstraints.FreezeAll;

        //}
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
        foreach (SpiderWebPart webPart in m_SpiderWebParts)
        {
            if (webPart != null)
                Destroy(webPart.gameObject);
        }
    }
}
