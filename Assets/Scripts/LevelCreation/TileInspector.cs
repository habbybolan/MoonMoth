using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


#if UNITY_EDITOR
[CustomEditor(typeof(Tile))]
public class TileInspector : Editor
{
    [SerializeField] private Tile tile;

    protected Transform handleTransform;
    protected Quaternion handleRotation;

    private const float handleSize = 0.15f;
    private const float pickSize = 0.2f;

    private int m_PlayerPointSelectedIndex = -1;

    private int m_EnemyPointSetSelectedIndex = -1;
    private int m_EnemyPointInSetSelected = -1;
    private Tile.LOCATION_TYPES m_SelectedType = Tile.LOCATION_TYPES.FOLLOW;

    private void OnSceneGUI()
    {
        tile = target as Tile;
        handleTransform = tile.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        // Create buttons for all player follow points
        for (int i = 0; i < tile.PlayerFollowPointsCount; i++)
        {
            ShowPlayerFollowPoint(i);
        }

        // Create buttons for all enemy follows sets and their points
        for (int i = 0; i < tile.EnemyFollowSetCount; i++)
        {
            ShowEnemyPointSet(i);
        }
        // Create button for Start point
        ShowEndPoint(true);
        // Create button for end point
        ShowEndPoint(false);
    }
    
    // Display all follow points in the tile
    private void ShowPlayerFollowPoint(int index) 
    {
        Handles.color = Color.blue;
        Vector3 point = handleTransform.TransformPoint(tile.GetPlayerFollowPoint(index));
        float size = HandleUtility.GetHandleSize(point);

        // Display button and allow selection
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            UnselectAll();
            m_SelectedType = Tile.LOCATION_TYPES.FOLLOW;
            m_PlayerPointSelectedIndex = index;
            Repaint();
        }

        // allow movement if button point selected
        if (m_PlayerPointSelectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tile, "Move Point");
                EditorUtility.SetDirty(tile);
                tile.SetPlayerFollowPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
    }

    // Display the enemy point set and a center point to control them all
    private void ShowEnemyPointSet(int index)
    {
        Handles.color = Color.yellow;
        List<Vector3> enemyPointSet = tile.GetEnemyPointSet(index);
        
        // Loop through all points in the set, display and create select functionality
        for (int i = 0; i < enemyPointSet.Count; i++)
        {
            Vector3 point = handleTransform.TransformPoint(enemyPointSet[i]);
            float size = HandleUtility.GetHandleSize(point);
            // Display button and allow selection
            if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
            {
                UnselectAll();
                m_EnemyPointInSetSelected = i;
                m_EnemyPointSetSelectedIndex = index;
                Repaint();
            }
        }

        Handles.color = Color.cyan;
        // Create a point that's the center of all points in the set to be able to move them all together
        Vector3 centerPoint = GetCenterPointBetweenVectors(enemyPointSet);
        float centerSize = HandleUtility.GetHandleSize(centerPoint);
        if (Handles.Button(centerPoint, handleRotation, centerSize * handleSize, centerSize * pickSize, Handles.DotHandleCap))
        {
            UnselectAll();
            m_EnemyPointSetSelectedIndex = index;
            Repaint();
        }

        // allow movement if button point selected
        if (m_EnemyPointSetSelectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            // If an enemy point in the set is selected
            if (m_EnemyPointInSetSelected >= 0)
            {
                Vector3 point = handleTransform.TransformPoint(enemyPointSet[m_EnemyPointInSetSelected]);
                point = Handles.DoPositionHandle(point, handleRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(tile, "Move Point");
                    EditorUtility.SetDirty(tile);
                    tile.SetPointInEnemySet(index, m_EnemyPointInSetSelected, handleTransform.InverseTransformPoint(point));
                }
            } 
            // otherwise, the center point of the set was selected
            else
            {
                Vector3 point = centerPoint;
                point = Handles.DoPositionHandle(point, handleRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(tile, "Move Point");
                    EditorUtility.SetDirty(tile);
                    tile.UpdateAllPointsInSet(index, point - centerPoint);
                }
            }
            
        }
    }

    // Get the vector center point between all points inside a set of enemy points
    private Vector3 GetCenterPointBetweenVectors(List<Vector3> vectors)
    {
        float xSum = 0;
        float ySum = 0;
        float zSum = 0;
        for (int i = 0; i < vectors.Count; i++)
        {
            xSum += vectors[i].x;
            ySum += vectors[i].y;
            zSum += vectors[i].z;
        }
        return handleTransform.TransformPoint(new Vector3(xSum, ySum, zSum) / vectors.Count + Vector3.up * 1.5f);
    }

    // Displays either the starting or ending point of the tile
    private void ShowEndPoint(bool isStartPoint)
    {
        Handles.color = isStartPoint ? Color.green : Color.red;
        Vector3 point = handleTransform.TransformPoint(isStartPoint ? tile.StartPoint : tile.EndPoint);
        float size = HandleUtility.GetHandleSize(point);

        // Display button and allow selection
        Handles.DrawWireDisc(point, Vector3.forward, 10);
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            UnselectAll();
            m_SelectedType = isStartPoint ? Tile.LOCATION_TYPES.START : Tile.LOCATION_TYPES.END;
            Repaint();
        }

        // allow movement if button point selected
        if (isStartPoint && m_SelectedType == Tile.LOCATION_TYPES.START ||
            !isStartPoint && m_SelectedType == Tile.LOCATION_TYPES.END)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tile, "Move Point");
                EditorUtility.SetDirty(tile);
                Vector3 newPoint = handleTransform.InverseTransformPoint(point);
                if (isStartPoint)
                    tile.StartPoint = newPoint;
                else
                    tile.EndPoint = newPoint;
            }
        }
    }

    // Reset all selected points, called each time a gizmo is selected
    private void UnselectAll()
    {
        m_PlayerPointSelectedIndex = -1;
        m_EnemyPointSetSelectedIndex = -1;
        m_EnemyPointInSetSelected = -1;
        m_SelectedType = Tile.LOCATION_TYPES.FOLLOW;
}

    // Buttons in inspector for adding/deleting follow points inside tile
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Player follow points");
        // Butotn for adding a player follow point
        if (GUILayout.Button("Add Follow point"))
        {
            Undo.RecordObject(tile, "Add Follow point");
            tile.AddFollowPoint();
            EditorUtility.SetDirty(tile);
        }

        // Button for removing last created player follow point
        if (GUILayout.Button("Remove Follow point"))
        {
            Undo.RecordObject(tile, "Remove Follow point");
            tile.RemoveFollowPoint();
            EditorUtility.SetDirty(tile);
        }

        GUILayout.Label("Enemy point sets");
        // button for adding a new set of enemy follow points
        if (GUILayout.Button("Add enemy follow point set"))
        {
            Undo.RecordObject(tile, "Add enemy follow point set");
            tile.AddEnemyPointSet();
            EditorUtility.SetDirty(tile);
        }

        // button for removing last created set of follow points
        if (GUILayout.Button("Remove enemy follow point set"))
        {
            Undo.RecordObject(tile, "Remove enemy follow point set");
            tile.RemoveEnemyPointSet();
            EditorUtility.SetDirty(tile);
        }

        // if a point in the enemy point set is selected
        if (m_EnemyPointSetSelectedIndex >= 0)
        {
            GUILayout.Label("Enemy points in selected set");
            // add a new point to the enemy set
            if (GUILayout.Button("Add enemy follow point to set"))
            {
                Undo.RecordObject(tile, "Add enemy follow point to set");
                tile.AddPointToEnemySet(m_EnemyPointSetSelectedIndex);
                EditorUtility.SetDirty(tile);
            }

            // remove the last point of the enemy set
            if (GUILayout.Button("Remove enemy follow point from set"))
            {
                Undo.RecordObject(tile, "Remove enemy follow point from set");
                tile.RemovePointFromEnemySet(m_EnemyPointSetSelectedIndex);
                EditorUtility.SetDirty(tile);
            }
        }
    }
}
#endif
