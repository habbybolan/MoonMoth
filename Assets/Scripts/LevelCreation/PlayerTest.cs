using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position - Vector3.forward * Time.deltaTime * 35; 
    }
}
