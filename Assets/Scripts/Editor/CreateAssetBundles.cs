using UnityEditor;
using UnityEngine;

public class CreateAssetBundles : MonoBehaviour
{
    [MenuItem("Assets/BuildAssets")]
    static void BuildAssetBundles()
    {
        var selected = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
        var assetNames = new string[selected.Length];
        for (int i = 0; i < selected.Length; i++)
        {
            assetNames[i] = AssetDatabase.GetAssetPath(selected[i]);
        }

        var build = new AssetBundleBuild
        {
            assetBundleName = "luafile",
            assetNames = assetNames
        };
        BuildPipeline.BuildAssetBundles(
            Application.streamingAssetsPath + "/Lua", 
            new AssetBundleBuild[] { build }, 
            BuildAssetBundleOptions.None, 
            BuildTarget.Android);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
