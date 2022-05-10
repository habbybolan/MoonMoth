using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineCreator : CatmullRomSpline
{
    LinkedListNode<Tile> m_CurrTile;
    private int m_CurrFollowPointInTile = 0; 

    public void AddNewPoint()
    {
        // TODO:
        // Check curr tile to see if any more follow points exists
        //      If so, add that to the spline and incremement index
        //      Otherwise find next tile and set their first follow point as the next one in the spline
    }
}
