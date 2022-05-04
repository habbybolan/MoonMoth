using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.ProBuilder;

public class SaveValidation : MonoBehaviour
{
    private static string _badDrive = "H:/";

    [MenuItem("File/Save and Validate %#&s", priority = 0)]
    public static void SaveAndValidate()
    {
        // clear log hack
        Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.LogEntries").GetMethod("Clear").Invoke(new object(), null);

        int errorCount = 0;

        Debug.Log("<b>Saving and validating scene/assets...</b>");

        // project location
        string path = Application.dataPath;
        if(path.Contains(_badDrive))
        {
            Debug.LogError($"Project location: {path} is on network drive, move to local (C:/) location.");
            errorCount++;
        }

        // camera settings
        GameObject cameraGo = GameObject.FindGameObjectWithTag("MainCamera");
        if(cameraGo != null && cameraGo.TryGetComponent(out UniversalAdditionalCameraData camera))
        {
            if(camera.renderPostProcessing == false)
            {
                Debug.LogError("Rendering => Post Processing is disabled on Main Camera");
                errorCount++;
            }
        }

        // lighting settings
        if(!Lightmapping.TryGetLightingSettings(out LightingSettings lightingSettings))
        {
            Debug.LogWarning("Lighting Settings asset is null, recommend applying NeutralLightSettings asset");
            errorCount++;
        }

        if(Lightmapping.lightingDataAsset == null)
        {
            Debug.LogWarning("Light bake needs to be generated");
            errorCount++;
        }

        if(FindObjectOfType<LightProbeGroup>() == null)
        {
            Debug.LogWarning("No lightprobes found in scene");
            errorCount++;
        }

        // level setup
        ProBuilderMesh[] pbMeshes = FindObjectsOfType<ProBuilderMesh>();
        int pbCount = pbMeshes.Length;
        if (pbCount > 0)
        {
            Debug.LogWarning($"{pbCount} ProBuilder mesh(es) found in scene, they should be replaced after prototyping");
            errorCount++;
        }

        // save scene(s)
        EditorSceneManager.SaveOpenScenes();

        // save assets
        AssetDatabase.SaveAssets();

        if(errorCount > 0)
        {
            Debug.Log($"{errorCount} above issue(s) found while validating.");
        }
        else
        {
            Debug.Log("Project validated with 0 issues.");
        }
    }
}