using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSplineCreator : SplineCreator
{
    public override void AddNewPoint()
    {
        base.AddNewPoint();

        // Add points forward in tutorial phase
        if (GameState.PropertyInstance.GameStateEnum == GameStateEnum.TUTORIAL)
        {
            Vector3 direction = PlayerManager.PropertyInstance.PlayerController.PlayerParent.transform.forward;
            if (m_Points == null)
            {
                AddPoint(PlayerManager.PropertyInstance.PlayerController.PlayerParent.RigidBody.transform.position + direction * 50);
            } else
            {
                AddPoint(EndOfSpline + direction * 50);
            }
        }
        // Otherwise create new spline points based on placed tiles
        else if (GameState.PropertyInstance.GameStateEnum == GameStateEnum.RUNNING)
        {
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
}
