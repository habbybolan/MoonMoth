using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWebPart : MonoBehaviour
{
    private void Awake()
    {
        health = gameObject.GetComponent<SpiderWebHealth>();
    }

    public Vector3 position;
    public Vector3 prevPosition;
    public bool bLocked;

    public SpiderWebHealth health;
}
