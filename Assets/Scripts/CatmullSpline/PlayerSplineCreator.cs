using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSplineCreator : SplineCreator
{

    public override void AddNewPoint()
    {
        base.AddNewPoint();
        
        Tile currTile = m_CurrTile.Value;
        Vector3 point = currTile.GetPlayerFollowPointWorld(m_CurrFollowPointInTile);
        m_CurrFollowPointInTile++;

        AddPoint(point);
        // reached all of player follow point sets, goto next tile
        if (currTile.PlayerFollowPointsCount <= m_CurrFollowPointInTile)
        {
            GotoNextTile();
        }
    }
}
