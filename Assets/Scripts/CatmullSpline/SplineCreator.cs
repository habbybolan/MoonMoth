using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SplineCreator : CatmullRomSpline
{
    protected LinkedListNode<Tile> m_CurrTile;  // Tile holding the last added spline point
    protected int m_CurrFollowPointInTile;      // Index of the follow point inside the current tile

    private bool m_IsInitialized = false;

    public virtual void AddNewPoint()
    {
        if (!m_IsInitialized)
            throw new System.Exception("Spline Creator must be initialized first");
    }

    // Initialize some points on the spline
    public void InitializeSpline()
    {
        m_CurrTile = TileManager.PropertyInstance.GetHead();
        m_CurrFollowPointInTile = 0;

        m_IsInitialized = true;
        // Initialize with 3 curves
        for (int i = 0; i < 3; i++)
        {
            AddNewPoint();
        }
    }

    protected void GotoNextTile()
    {
        m_CurrTile = m_CurrTile.Next;
        m_CurrFollowPointInTile = 0;
    }
}
