using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyCatmullWalker : CatmullWalker
{
    override protected void Start()
    {
        IsFollowSpline = false;
        m_Spline.InitializeSplineAtTile(PlayerManager.PropertyInstance.PlayerParent.spline.GetTileInfront(0));
        base.Start();
    }
}
