using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Deals with Firefly's States and which methods to call each frame.
 * Acts as the central hub for interacting with all Firefly components
 */
public class FireflyController : MonoBehaviour
{
    [SerializeField] private FireflyCatmullWalker m_FireflyWalker;

    private void Update()
    {
        m_FireflyWalker.TryMove();
    }
}
