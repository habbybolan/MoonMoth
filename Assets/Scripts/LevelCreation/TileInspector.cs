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

    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;

    private int m_SelectedIndex = -1;
    private Tile.LOCATION_TYPES m_SelectedType = Tile.LOCATION_TYPES.FOLLOW;

    private void OnSceneGUI()
    {
        tile = target as Tile;
        handleTransform = tile.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        //for (int i = 0; i < tile.PlayerFollowPointsCount; i++)
        //{
        //    ShowPoint(i);
        //}

        //ShowPoint(true);
        //ShowPoint(false);
    }
    
    // Display all follow points in the tile
    private void ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(tile.GetPlayerFollowPoint(index));
        float size = HandleUtility.GetHandleSize(point);

        // allow node selection
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
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

    }

    private void ShowPoint(Vector3 position)
    {

    }

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
