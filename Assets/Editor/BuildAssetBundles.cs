using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildAssetBundles
{
    [MenuItem("Tools/Build AssetBundles (Current Platform)")]
    public static void Build()
    {
        string outDir = Path.Combine("AssetBundles", EditorUserBuildSettings.activeBuildTarget.ToString());
        if (!Directory.Exists(outDir))
            Directory.CreateDirectory(outDir);

        BuildPipeline.BuildAssetBundles(
            outDir,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget
        );

        Debug.Log("AssetBundles built to: " + outDir);
    }
}
