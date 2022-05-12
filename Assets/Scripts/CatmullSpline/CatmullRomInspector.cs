using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(EnemySplineCreator))]
public class CatmullRomInspector : Editor 
{
	protected const int stepsPerCurve = 10;
	private const float directionScale = 0.5f;

	protected EnemySplineCreator spline;
	protected Transform handleTransform;
	protected Quaternion handleRotation;

	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	private int selectedIndex = -1;

	// Draw the GUI
	private void OnSceneGUI()
	{
		spline = target as EnemySplineCreator;
		handleTransform = spline.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;

		if (spline.PointCount > 2)
        {
			Vector3 p0 = ShowPoint(0);

			for (int i = 1; i < spline.PointCount; i++)
			{
				Vector3 p1 = ShowPoint(i);
				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.color = Color.white;
				ShowCurve(i);
				p0 = p1;
			}

			ShowDirections();
        }
	}

	// Show a specific curve by the the curve index, drawing a curve between the 2 points
	private void ShowCurve(int curve)
	{
		Handles.color = Color.white;
		int start = (curve - 1) * stepsPerCurve;
		int steps = stepsPerCurve * spline.CurveCount;
		Vector3 lineStart = spline.GetPoint(start / (float)steps);
		// Get time t along the curve, using the number of steps and the curve index
		for (int i = start + 1; i <= stepsPerCurve * (curve); i++)
		{
			Vector3 lineEnd = spline.GetPoint(i / (float)steps);

			Handles.DrawLine(lineStart, lineEnd);
			lineStart = lineEnd;
		}
	}

	// Show the tangents of the spline
	protected void ShowDirections()
	{
		Handles.color = Color.green;
		Vector3 point = spline.GetPoint(0f);
		Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);

		int steps = stepsPerCurve * spline.CurveCount;
		for (int i = 1; i <= steps; i++)
		{
			point = spline.GetPoint(i / (float)steps);
			Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
		}
	}


	// Show a point on the spline by index of the point
	protected Vector3 ShowPoint(int index)
	{
		Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);

		// allow node selection
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
		{
			selectedIndex = index;
			Repaint();
		}

		// allow movement if button point selected
		if (selectedIndex == index)
		{
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(spline, "Move Point");
				EditorUtility.SetDirty(spline);
				spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}

	// Add button to inspector for adding new curve
	public override void OnInspectorGUI()
	{
		spline = target as EnemySplineCreator;
		if (selectedIndex >= 0 && selectedIndex < spline.PointCount)
		{
			DrawSelectedPointInspector();
		}
		if (GUILayout.Button("Add Curve"))
		{
			Undo.RecordObject(spline, "Add Curve");
			spline.AddCurve();
			EditorUtility.SetDirty(spline);
		}
	}

	private void DrawSelectedPointInspector()
	{
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(spline, "Move Point");
			EditorUtility.SetDirty(spline);
		}
	}

}
#endif
