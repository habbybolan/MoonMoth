using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR
[CustomEditor(typeof(Tile))]
public class TileInspector : Editor
{
    [SerializeField] private Tile tile;

    protected Transform handleTransform;
    protected Quaternion handleRotation;

    private const float handleSize = 0.15f;
    private const float pickSize = 0.2f;

    private int m_SelectedIndex = -1;
    private Tile.LOCATION_TYPES m_SelectedType = Tile.LOCATION_TYPES.FOLLOW;

    private void OnSceneGUI()
    {
        tile = target as Tile;
        handleTransform = tile.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        for (int i = 0; i < tile.PlayerFollowPointsCount; i++)
        {
            ShowPoint(i);
        }

        ShowPoint(true);
        ShowPoint(false);
    }
    
    // Display all follow points in the tile
    private void ShowPoint(int index)
    {
        Handles.color = Color.blue;
        Vector3 point = handleTransform.TransformPoint(tile.GetPlayerFollowPoint(index));
        float size = HandleUtility.GetHandleSize(point);

        // Display button and allow selection
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            m_SelectedType = Tile.LOCATION_TYPES.FOLLOW;
            m_SelectedIndex = index;
            Repaint();
        }

        // allow movement if button point selected
        if (m_SelectedIndex == index)
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
    }
}
#endif
