using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Wrappper class to allow Serializing a list of lists
[System.Serializable]
public class EnemySetWrapper
{
    public List<Vector3> EnemyPointSet; 

    public EnemySetWrapper(Vector3 newPoint)
    {
        EnemyPointSet = new List<Vector3> { newPoint };
    }
}
