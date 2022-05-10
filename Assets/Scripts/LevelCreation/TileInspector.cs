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

        for (int i = 0; i < tile.PlayerFollowPointsCount; i++)
        {
            ShowPlayerFollowPoint(i);
        }

        for (int i = 0; i < tile.EnemyFollowSetCount; i++)
        {
            ShowEnemyPointSet(i);
        }
        ShowPoint(true);
        ShowPoint(false);
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

    private void ShowEnemyPointSet(int index)
    {
        Handles.color = Color.yellow;
        List<Vector3> enemyPointSet = tile.GetEnemyPointSet(index);
        

        for (int i = 0; i < enemyPointSet.Count; i++)
        {
            Vector3 point = enemyPointSet[i];
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

        Vector3 centerPoint = GetCenterPointBetweenVectors(enemyPointSet);
        float centerSize = HandleUtility.GetHandleSize(centerPoint);
        if (Handles.Button(centerPoint, handleRotation, centerSize * handleSize, centerSize * pickSize, Handles.DotHandleCap))
        {
            m_EnemyPointSetSelectedIndex = index;
            Repaint();
        }

        // allow movement if button point selected
        if (m_EnemyPointSetSelectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            if (m_EnemyPointInSetSelected >= 0)
            {
                Vector3 point = enemyPointSet[m_EnemyPointInSetSelected];
                point = Handles.DoPositionHandle(point, handleRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(tile, "Move Point");
                    EditorUtility.SetDirty(tile);
                    tile.SetPointInEnemySet(index, handleTransform.InverseTransformPoint(point));
                }
            } 
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
        return new Vector3(xSum, ySum, zSum) / vectors.Count + Vector3.up * 1.5f;
    }

    // Displays either the starting or ending point of the tile
    private void ShowPoint(bool isStartPoint)
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
        if (GUILayout.Button("Add Follow point"))
        {
            Undo.RecordObject(tile, "Add Follow point");
            tile.AddFollowPoint();
            EditorUtility.SetDirty(tile);
        }

        if (GUILayout.Button("Remove Follow point"))
        {
            Undo.RecordObject(tile, "Remove Follow point");
            tile.RemoveFollowPoint();
            EditorUtility.SetDirty(tile);
        }

        if (GUILayout.Button("Add enemy follow point set"))
        {
            Undo.RecordObject(tile, "Add enemy follow point set");
            tile.AddEnemyPointSet();
            EditorUtility.SetDirty(tile);
        }

        if (GUILayout.Button("Remove enemy follow point set"))
        {
            Undo.RecordObject(tile, "Remove enemy follow point set");
            tile.RemoveEnemyPointSet();
            EditorUtility.SetDirty(tile);
        }

        if (m_EnemyPointSetSelectedIndex >= 0)
        {
            if (GUILayout.Button("Add enemy follow point to set"))
            {
                Undo.RecordObject(tile, "Add enemy follow point to set");
                tile.AddPointToEnemySet(m_EnemyPointSetSelectedIndex);
                EditorUtility.SetDirty(tile);
            }

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
