using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyController : MonoBehaviour
{
    [SerializeField] private FireflyCatmullWalker m_FireflyWalker;

    private void Update()
    {
        m_FireflyWalker.TryMove();
    }
}
