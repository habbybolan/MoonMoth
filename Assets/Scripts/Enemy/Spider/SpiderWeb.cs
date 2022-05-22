using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    [SerializeField] GameObject m_PartPrefab;

    [Range(1f, 1000f)]
    [SerializeField] private float m_Length = 1;

    [SerializeField] private float m_partDistance = 0.21f;

    [SerializeField] bool reset, spawn, snapFirst, snapLast;

    private void Update()
    {
        if (reset)
        {
            foreach(GameObject temp in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                Destroy(temp);
            }
            reset = false;
        }

        if (spawn)
        {
            Spawn();
            spawn = false;
        }
    }

    public void Spawn()
    {
        int count = (int)( m_Length / m_partDistance);

        for (int i = 0; i < count; i++)
        {
            GameObject tmp;

            tmp = Instantiate(m_PartPrefab, new Vector3 (transform.position.x, transform.position.y + m_partDistance * (i + 1), transform.position.z), Quaternion.identity, transform);
            tmp.transform.eulerAngles = new Vector3(180, 0, 0);

            tmp.name = transform.childCount.ToString();

            if (i == 0)
            {
                Destroy(tmp.GetComponent<CharacterJoint>());
                if (snapFirst)
                {
                    tmp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
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
}
