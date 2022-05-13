using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firefly : MonoBehaviour
{
   
    [SerializeField] private EnemySplineCreator m_EnemySplineCreator;
    [SerializeField] private float m_SensingRange = 50f;

    private bool m_IsActive;    // If the firefly has been within range of the player and starts moving along spline
    private FireflyCatmullWalker m_CatmullWalker;
    

    private void Start()
    {
        Debug.Log("Firefly spawned");
        m_IsActive = false;
        m_CatmullWalker = GetComponent<FireflyCatmullWalker>();
    }

    private void Update()
    {
        if (!m_IsActive)
        {
            CheckIsInRangeOfPlayer();
            return;
        }
        // TODO: DO other logic for shooting, animations, etc
    }

    private void CheckIsInRangeOfPlayer()
    {
        Vector3 playerPosition = PlayerManager.PropertyInstance.PlayerParent.transform.position;
        if (Vector3.Distance(playerPosition, transform.position) < 100)
        {
            m_IsActive = true;
            m_CatmullWalker.IsFollowSpline = true;
        }
    }


}
