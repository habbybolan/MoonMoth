using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToggleProbuilderDebug : Editor
{
    private static string _keyword = "_PROBUILDERDEBUG";

    [MenuItem("Level Design/Toggle Probuilder Debug")]
    public static void Toggle()
    {
        float _current = Shader.GetGlobalFloat(_keyword);
        Shader.SetGlobalFloat(_keyword, 1f - _current);
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}