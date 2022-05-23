using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyContainer : MonoBehaviour
{
    [SerializeField] private FireflyCatmullWalker m_FireflyWalker;

    public FireflyCatmullWalker FireflyWalker { get { return m_FireflyWalker; } }
}
