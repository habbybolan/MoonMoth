using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct PossiblePoint
{
    public Vector3 Point;
    public float Dot;

    public static int SortByDot(PossiblePoint p1, PossiblePoint p2)
    {
        return p1.Dot < p2.Dot ? 1 : -1;
    }
}

public class EnemySplineCreator : SplineCreator
{
    public int NextPointBlockingMask;
    
    private void Start()
    {
        m_IsActive = false;

        NextPointBlockingMask = 1 << LayerMask.NameToLayer("Terrain");
        NextPointBlockingMask = 1 << LayerMask.NameToLayer("Obstacle");
    }

    public override void AddNewPoint()
    {
        base.AddNewPoint();
        Tile currTile = m_CurrTile.Value;

        // If at least 2 points on spline, find best next point for smooth movement
        if (PointCount > 1)
        {
            AddBestPoint();
        } 
        // otherwise just choose random follow point
        else
        {
            AddRandPoint();
        }

        m_CurrFollowPointInTile++;
        // reached all of enemy follow point sets, goto next tile
        if (currTile.EnemyFollowSetCount <= m_CurrFollowPointInTile)
        {
            GotoNextTile();
        }
    }

    private void AddRandPoint()
    {
        Tile currTile = m_CurrTile.Value;
        List<Vector3> followSet = currTile.GetEnemyPointSetWorld(m_CurrFollowPointInTile);

        int rand = Random.Range(0, followSet.Count);
        AddPoint(followSet[rand]);
    }

    // Find the best point, using last point to check for obstructions and angle to next point
    private void AddBestPoint()
    {
        Tile currTile = m_CurrTile.Value;
        List<Vector3> followSet = currTile.GetEnemyPointSetWorld(m_CurrFollowPointInTile);

        Vector3 EndingPoint = GetPoint(1);
        // Filter out any points that are blocked by terrain or obstacles
        for (int i = followSet.Count - 1; i >= 0; i--)
        {
            if (Physics.Raycast(EndingPoint, followSet[i] - EndingPoint, 10000, NextPointBlockingMask))
            {
                followSet.RemoveAt(i);
                Debug.Log("Point removed");
            }
        }

        // If all points obstructed, No best point, then choose random
        if (followSet.Count == 0)
        {
            AddRandPoint();
        }
        // Otherwise choose best point based on angle to last point and its spline tangent
        else
        {
            Vector3 EndingDirection = GetDirection(1);
            List<PossiblePoint> possiblePoints = new List<PossiblePoint>();
            for (int i = 0; i < followSet.Count; i++)
            {
                float dotProd = Vector3.Dot(EndingDirection.normalized, (followSet[i] - EndingPoint).normalized);
                PossiblePoint point = new PossiblePoint();
                point.Point = followSet[i];
                point.Dot = dotProd;
                possiblePoints.Add(point);
            }

            possiblePoints.Sort(PossiblePoint.SortByDot);
            AddPoint(possiblePoints[0].Point);
        }
    }
}
