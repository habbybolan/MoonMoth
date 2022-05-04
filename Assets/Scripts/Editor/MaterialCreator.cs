using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class MaterialCreator
{
    private static string _shaderName = "Shader Graphs/Uber";
    private static string[] _texurePatterns = new[] { "basecolor", "normaldx", "aorough" };
    private static string[] _textureNames = new[] { "Base Color", "Normal Map", "Pack Map" };
    private static string[] _parameterNames = new[] { "_PrimaryColor", "_PrimaryNormal", "_PrimaryMask" };

    [MenuItem("Assets/Create/Art Pipeline/Create Uber material", false, 1)]
    public static void CreateUberMaterial()
    {
        Texture[] selectedTextures = Selection.GetFiltered<Texture>(SelectionMode.Assets);

        if (selectedTextures.Length != 3)
        {
            Debug.LogWarning("Select 3 PBR maps before creating material.");
            return;
        }

        Texture[] textures = new Texture[3];
        for (int i = 0; i < _texurePatterns.Length; i++)
        {
            for (int j = 0; j < selectedTextures.Length; j++)
            {
                if (selectedTextures[j].name.ToLower().Contains(_texurePatterns[i])) textures[i] = selectedTextures[j];
            }
        }

        string output =  $"Textures found:{Environment.NewLine}";
        for (int i = 0; i < _textureNames.Length; i++)
        {
            string result = "Failed";
            if(textures[i] != null)
            {
                result = textures[i].name;
                TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(textures[i]));
                switch (i)
                {
                    case 0:
                        importer.textureType = TextureImporterType.Default;
                        importer.sRGBTexture = true;
                        break;
                    case 1:
                        importer.textureType = TextureImporterType.NormalMap;
                        break;
                    case 2:
                        importer.textureType = TextureImporterType.Default;
                        importer.sRGBTexture = false;
                        break;
                }

                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(textures[i]), ImportAssetOptions.ForceUpdate);
            }
            output += ($"{_textureNames[i]}: {result}{Environment.NewLine}");
        }

        Debug.Log(output);

        for (int i = 0; i < textures.Length; i++)
        {
            if(textures[i] == null)
            {
                Debug.LogWarning("Select 3 PBR maps before creating material.");
                return;
            }
        }

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        Regex pathPattern = new Regex(@".+[/]");
        path = pathPattern.Match(path).Value;

        Regex fileNamePattern = new Regex(@".+[_]");
        string fileName = fileNamePattern.Match(Selection.activeObject.name).Value.Replace("_", "");

        Material material = new Material(Shader.Find(_shaderName));
        AssetDatabase.CreateAsset(material, $"{path}/{fileName}.mat");

        for (int i = 0; i < _parameterNames.Length; i++)
        {
            material.SetTexture(_parameterNames[i], textures[i]);
        }

        Debug.Log($"Material created: {material}", material);
    }
}