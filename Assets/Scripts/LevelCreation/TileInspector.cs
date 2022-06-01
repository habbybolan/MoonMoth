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

    private Tile.LOCATION_TYPES m_SelectedType = Tile.LOCATION_TYPES.FOLLOW;
    private int m_PlayerPointSelectedIndex = -1;
    private int m_SpiderSpawnSelected = -1;
    private int m_StalagSpawnSelected = -1;
    private int m_FireflyPointSetSelectedIndex = -1;
    private int m_FireflyPointInSetSelected = -1;
    private int m_LostMothPointSelected = -1;
    private int m_MushroomPointSelected = -1;
   

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

        // Create buttons for all Spider spawn points
        for (int i = 0; i < tile.GetSpiderSpawnCount(); i++)
        {
            ShowSpiderSpawns(i);
        }
        for (int i = 0; i < tile.GetStalagSpawnCount(); i++)
        {
            ShowStalagSpawnPoint(i);
        }
        for (int i = 0; i < tile.GetLostMothCount(); i++)
        {
            ShowLostMothPoint(i);
        } 
        for (int i = 0; i < tile.GetMushroomCount(); i++)
        {
            ShowMushroomPoint(i);
        }
    }

    Transform objectTouched = null; //the reference of the last object hit

    private Vector3 GetLocalPosInEditor()
    {
        return tile.transform.InverseTransformPoint(SceneView.lastActiveSceneView.camera.transform.position);
    }

    private void InputPressed()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (m_StalagSpawnSelected != 0)
            {
                // TODO:

                //Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                //RaycastHit hitInfo;
                //if (Physics.Raycast(worldRay, out hitInfo, Mathf.Infinity))
                //{
                //    if (hitInfo.collider.gameObject != null)
                //    {
                //        objectTouched = hitInfo.collider.gameObject;
                //        tile.
                //        objectPreview.position = hitInfo.point;
                //        objectPreview.rotation = hitInfo.normal;
                //    }
                //}
            }
        }
    }

    private void ShowSpiderSpawns(int index)
    {
        Handles.color = new Color(115, 43, 204);
        Vector3 point = handleTransform.TransformPoint(tile.GetSpiderSpawn(index));
        float size = HandleUtility.GetHandleSize(point);

        // Display button and allow selection
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            UnselectAll();
            m_SelectedType = Tile.LOCATION_TYPES.SPIDER;
            m_SpiderSpawnSelected = index;
            Repaint();
        }

        // allow movement if button point selected
        if (m_SpiderSpawnSelected == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tile, "Move Point");
                EditorUtility.SetDirty(tile);
                tile.SetSpiderSpawn(index, handleTransform.InverseTransformPoint(point));
            }
        }
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

    // Display lost moth point
    private void ShowLostMothPoint(int index)
    {
        Handles.color = Color.black;
        Vector3 point = handleTransform.TransformPoint(tile.GetLostMothPoint(index));
        float size = HandleUtility.GetHandleSize(point);

        // Display button and allow selection
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            UnselectAll();
            m_SelectedType = Tile.LOCATION_TYPES.LOST_MOTH;
            m_LostMothPointSelected = index;
            Repaint();
        }

        // allow movement if button point selected
        if (m_LostMothPointSelected == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tile, "Move Point");
                EditorUtility.SetDirty(tile);
                tile.UpdateLostMothPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
    }

    // Display mushroom point
    private void ShowMushroomPoint(int index)
    { 
        Handles.color = new Color(0, .14f, 0);
        Vector3 point = handleTransform.TransformPoint(tile.GetMushroomPoint(index));
        float size = HandleUtility.GetHandleSize(point);

        // Display button and allow selection
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            UnselectAll();
            m_SelectedType = Tile.LOCATION_TYPES.MUSHROOM;
            m_MushroomPointSelected = index;
            Repaint();
        }

        // allow movement if button point selected
        if (m_MushroomPointSelected == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tile, "Move Point");
                EditorUtility.SetDirty(tile);
                tile.UpdateMushroomPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
    }

    // Display all follow points in the tile
    private void ShowStalagSpawnPoint(int index) 
    {
        Handles.color = Color.grey;
        Vector3 point = handleTransform.TransformPoint(tile.GetStalagSpawnPoint(index).Position);
        float size = HandleUtility.GetHandleSize(point);

        // Display button and allow selection
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            UnselectAll();
            m_SelectedType = Tile.LOCATION_TYPES.STALAG;
            m_StalagSpawnSelected = index;
            Repaint();
        }

        // allow movement if button point selected
        if (m_StalagSpawnSelected == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tile, "Move Point");
                EditorUtility.SetDirty(tile);
                tile.UpdateStalgPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }

        Handles.ArrowHandleCap(
            0,
            point,
            tile.GetIsStalagPointingUp(index) ? Quaternion.LookRotation(Vector3.up) : Quaternion.LookRotation(Vector3.down),
            size,
            EventType.Repaint
        );
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
                m_FireflyPointInSetSelected = i;
                m_FireflyPointSetSelectedIndex = index;
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
            m_FireflyPointSetSelectedIndex = index;
            Repaint();
        }

        // allow movement if button point selected
        if (m_FireflyPointSetSelectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            // If an enemy point in the set is selected
            if (m_FireflyPointInSetSelected >= 0)
            {
                Vector3 point = handleTransform.TransformPoint(enemyPointSet[m_FireflyPointInSetSelected]);
                point = Handles.DoPositionHandle(point, handleRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(tile, "Move Point");
                    EditorUtility.SetDirty(tile);
                    tile.SetPointInEnemySet(index, m_FireflyPointInSetSelected, handleTransform.InverseTransformPoint(point));
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
        m_FireflyPointSetSelectedIndex = -1;
        m_FireflyPointInSetSelected = -1;
        m_SpiderSpawnSelected = -1;
        m_StalagSpawnSelected = -1;
        m_LostMothPointSelected = -1;
        m_MushroomPointSelected = -1;
        m_SelectedType = Tile.LOCATION_TYPES.FOLLOW;
}

    // Buttons in inspector for adding/deleting follow points inside tile
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Label("Player follow points");
        // Butotn for adding a player follow point
        if (GUILayout.Button("Add Follow point"))
        {
            Undo.RecordObject(tile, "Add Follow point");
            tile.AddFollowPoint(GetLocalPosInEditor());
            EditorUtility.SetDirty(tile);
        }

        // Button for removing last created player follow point
        if (GUILayout.Button("Remove Follow point"))
        {
            Undo.RecordObject(tile, "Remove Follow point");
            tile.RemoveFollowPoint();
            EditorUtility.SetDirty(tile);
        }

        GUILayout.Label("Spider spawn points");
        // Button for adding new Spider spawn point
        if (GUILayout.Button("Add spider spawn"))
        {
            Undo.RecordObject(tile, "Add Spider spawn");
            tile.AddSpiderPoint(GetLocalPosInEditor());
            EditorUtility.SetDirty(tile);
        }

        // Button for adding new Spider spawn point
        if (GUILayout.Button("Remove spider spawn"))
        {
            Undo.RecordObject(tile, "Remove Spider spawn");
            tile.RemoveSpiderPoint();
            EditorUtility.SetDirty(tile);
        }

        GUILayout.Label("Stalag Spawn point");
        // Button for adding new stalag spawn point
        if (GUILayout.Button("Add stalag spawn"))
        {
            Undo.RecordObject(tile, "Add stalag spawn");
            tile.AddStalagSpawnPoint(GetLocalPosInEditor());
            EditorUtility.SetDirty(tile);
        }

        // Button for removing last created stalag point
        if (GUILayout.Button("Remove stalag spawn"))
        {
            Undo.RecordObject(tile, "Remove stalag spawn");
            tile.RemoveStalagPoint();
            EditorUtility.SetDirty(tile);
        }

        if (m_StalagSpawnSelected >= 0)
        {
            // Button for changing if the the stalg is point up or down
            if (GUILayout.Button("Change stalag orientation"))
            {
                Undo.RecordObject(tile, "Change stalag orientation");
                tile.ChangeStalagOrientation(m_StalagSpawnSelected);
                EditorUtility.SetDirty(tile);
            }
        }

        GUILayout.Label("Enemy point sets");
        // button for adding a new set of enemy follow points
        if (GUILayout.Button("Add enemy follow point set"))
        {
            Undo.RecordObject(tile, "Add enemy follow point set");
            tile.AddEnemyPointSet(GetLocalPosInEditor());
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
        if (m_FireflyPointSetSelectedIndex >= 0)
        {
            GUILayout.Label("Enemy points in selected set");
            // add a new point to the enemy set
            if (GUILayout.Button("Add enemy follow point to set"))
            {
                Undo.RecordObject(tile, "Add enemy follow point to set");
                tile.AddPointToEnemySet(m_FireflyPointSetSelectedIndex);
                EditorUtility.SetDirty(tile);
            }

            // remove the last point of the enemy set
            if (GUILayout.Button("Remove enemy follow point from set"))
            {
                Undo.RecordObject(tile, "Remove enemy follow point from set");
                tile.RemovePointFromEnemySet(m_FireflyPointSetSelectedIndex);
                EditorUtility.SetDirty(tile);
            }
        }

        GUILayout.Label("Lost Moth Point");
        // button for adding a new set of enemy follow points
        if (GUILayout.Button("Add lost moth point"))
        {
            Undo.RecordObject(tile, "Add lost moth point");
            tile.AddLostMothPoint(GetLocalPosInEditor()); 
            EditorUtility.SetDirty(tile);
        }

        if (m_LostMothPointSelected >= 0)
        {
             // button for removing last created set of follow points
            if (GUILayout.Button("Remove lost moth point"))
            {
                Undo.RecordObject(tile, "Remove lost moth point");
                
                tile.RemoveLostMothPoint(m_LostMothPointSelected);
                EditorUtility.SetDirty(tile);
                UnselectAll();
            }
        }

        GUILayout.Label("Mushroom Point");
        // button for adding a new set of enemy follow points
        if (GUILayout.Button("Add mushroom point"))
        {
            Undo.RecordObject(tile, "Add mushroom point");
            tile.AddMushroomPoint(GetLocalPosInEditor());
            EditorUtility.SetDirty(tile);
        }

        if (m_MushroomPointSelected >= 0)
        {
            // button for removing last created set of follow points
            if (GUILayout.Button("Remove mushroom point"))
            {
                Undo.RecordObject(tile, "Remove mushroom point");

                tile.RemoveMushroomPoint(m_MushroomPointSelected);
                EditorUtility.SetDirty(tile);
                UnselectAll();
            }
        }
    }  
}
#endif
