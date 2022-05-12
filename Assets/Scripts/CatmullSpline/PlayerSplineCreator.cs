using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSplineCreator : SplineCreator
{

    public override void AddNewPoint()
    {
        base.AddNewPoint();
        
        Tile currTile = m_CurrTile.Value;
        Vector3 point = currTile.GetPlayerFollowPointWorld(0);
        
        AddPoint(point);
        GotoNextTile();
    }
}
