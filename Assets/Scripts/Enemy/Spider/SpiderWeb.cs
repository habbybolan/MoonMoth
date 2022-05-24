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

    private void Start()
    {
        m_LayerMask = 1 << 9;
        m_StartingPoint.transform.parent = null;
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

        for (int i = 0; i < count; i++)
        {
            GameObject tmp;

            tmp = Instantiate(m_PartPrefab, new Vector3 (transform.position.x, transform.position.y + m_partDistance * (i + 1), transform.position.z), Quaternion.identity, m_StartingPoint.transform);
            tmp.transform.eulerAngles = new Vector3(180, 0, 0);

            tmp.name = transform.childCount.ToString();

            if (i == 0)
            {
                // attach first to spider
                if (m_Spider != null)
                {
                    tmp.GetComponent<CharacterJoint>().connectedBody = m_Spider.GetComponent<Rigidbody>();
                } else
                {
                    Destroy(tmp.GetComponent<CharacterJoint>());
                }
            }
            else
            {
                tmp.GetComponent<CharacterJoint>().connectedBody = transform.Find((transform.childCount - 1).ToString()).GetComponent<Rigidbody>();
            }
        }

        if (snapLast)
        {
            transform.Find((transform.childCount).ToString()).GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
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
        d_WebDestroyedDelegate();
    }
}