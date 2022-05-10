using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySplineCreator : SplineCreator
{
    public override void AddNewPoint()
    {
        base.AddNewPoint();
        Tile currTile = m_CurrTile.Value;
        

        List<Vector3> followSet = currTile.GetEnemyPointSetWorld(m_CurrFollowPointInTile);
        int rand = Random.Range(0, followSet.Count);
        AddPoint(followSet[rand]);

        m_CurrFollowPointInTile++;
        // reached all of enemy follow point sets, goto next tile
        if (currTile.EnemyFollowSetCount <= m_CurrFollowPointInTile)
        {
            GotoNextTile();
        }
    }
}
