using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyContainer : MonoBehaviour
{
    [SerializeField] private FireflyCatmullWalker m_FireflyWalker;
    [SerializeField] private FireflyController m_Firefly;

    public FireflyCatmullWalker FireflyWalker { get { return m_FireflyWalker; } }
    public FireflyController Firefly { get { return m_Firefly; } }
}
