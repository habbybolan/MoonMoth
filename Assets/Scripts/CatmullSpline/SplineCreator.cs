using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SplineCreator : CatmullRomSpline
{
    protected LinkedListNode<Tile> m_CurrTile;  // Tile holding the last added spline point
    protected int m_CurrFollowPointInTile;      // Index of the follow point inside the current tile

    private bool m_IsInitialized = false;
    protected bool m_IsActive = true;

    public virtual void AddNewPoint()
    {
        if (!m_IsInitialized)
            throw new System.Exception("Spline Creator must be initialized first");
    }

    public Vector3 GetClosestPointToCharacter(int currCurve, Vector3 characterPosition)
    {
        Vector3 closestPoint = Vector3.zero;
        float closestDistance = Mathf.Infinity;
        for (int i = 0; i < 10; i++)
        {
            Vector3 point = GetPoint(i / 10);
            float distance = Vector3.Distance(point, characterPosition);
            // if found a new closet point
            if (distance < closestDistance)
            {
                closestPoint = point;
                closestDistance = distance;
                // otherwise moveing past closest point, previous closest point must be closest
            }
            else break;
        }
        return closestPoint;
    }

    // Initialize some points on the spline
    public void InitializeSplineAtHead()
    {
        m_Points = null;
        m_CurrTile = TileManager.PropertyInstance.GetHead();
        m_CurrFollowPointInTile = 0;

        m_IsInitialized = true;
        InitializeStartingPoints();
    }

    // Initialize the spline at the tile tileStart
    public void InitializeSplineAtTile(LinkedListNode<Tile> tileStart)
    {
        m_CurrTile = tileStart;
        m_CurrFollowPointInTile = 0;
        m_IsInitialized = true;
        InitializeStartingPoints();
    }

    public bool IsInitialized
    {
        get { return m_IsInitialized; }
    }

    // Initialize spline with starting points to keep the walker sufficienctly behind the end of the spline
    private void InitializeStartingPoints()
    {
        for (int i = 0; i < 4; i++)
        {
            AddNewPoint();
        }
    }

    protected void GotoNextTile()
    {
        m_CurrTile = m_CurrTile.Next;
        m_CurrFollowPointInTile = 0;
    }

    public LinkedListNode<Tile> GetTileInfront(int index)
    {
        LinkedListNode<Tile> curr = m_CurrTile;
        for (int i = 0; i < index; i++)
        {
            curr = curr.Next;
        }
        return curr;
    }

    public void IntializeAtEndOfHead()
    {
        m_CurrTile = TileManager.PropertyInstance.GetHead();
        m_CurrFollowPointInTile = m_CurrTile.Value.EnemyFollowSetCount - 1;
        m_IsInitialized = true;
        InitializeStartingPoints();
    }
}
