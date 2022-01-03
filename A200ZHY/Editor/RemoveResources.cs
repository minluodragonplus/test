using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class RemoveResources
{
    #region 一键移除所有引用(图片、动画控制器）
    [MenuItem("GameObject/资源引用/RemoveAll[慎用]", false, 12)]
    public static void RemoveAll()
    {
        var Select = Selection.activeGameObject;
        List<Transform> childs = new List<Transform>();
        childs.Add(Selection.activeGameObject.transform);
        GetChilds(Selection.activeGameObject.transform, childs);
        foreach (var item in childs)
        {
            if (item.GetComponent<Image>() != null)
            {
                item.GetComponent<Image>().sprite = null;
                Select = item.gameObject;
            }
            if (item.GetComponent<Animator>() != null)
            {
                item.GetComponent<Animator>().runtimeAnimatorController = null;
            }
        }
        Selection.activeGameObject = Select;
    }
    #endregion
    #region 一件移除Image引用
    [MenuItem("GameObject/资源引用/RemoveImage[慎用]", false, 12)]
    public static void RemoveImage()
    {
        var Select = Selection.activeGameObject;
        List<Transform> childs = new List<Transform>();
        childs.Add(Selection.activeGameObject.transform);
        GetChilds(Selection.activeGameObject.transform, childs);
        foreach (var item in childs)
        {
            if (item.GetComponent<Image>() != null)
            {
                item.GetComponent<Image>().sprite = null;
                Select = item.gameObject;
            }
        }
        Selection.activeGameObject = Select;
    }
    #endregion
    #region 一件移除Animator引用
    [MenuItem("GameObject/资源引用/RemoveAnimator[慎用]", false, 12)]
    public static void RemoveAnimator()
    {
        List<Transform> childs = new List<Transform>();
        childs.Add(Selection.activeGameObject.transform);
        GetChilds(Selection.activeGameObject.transform, childs);
        foreach (var item in childs)
        {
            if (item.GetComponent<Animator>() != null)
            {
                item.GetComponent<Animator>().runtimeAnimatorController = null;
            }
        }
    }
    #endregion
    #region 一键匹配所有资源引用(对文件夹操作）
    //------------------扩展Project面板--------------------
    //-----------------拓展右键菜单-----------------
    [MenuItem("Assets/匹配对应美术资源引用", false, 2)]
    public static void GetCurrentAssetDirectory()
    {
        string Path = "";
        foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            Path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(Path))
                continue;
            if (System.IO.Directory.Exists(Path))
                continue;
            else if (System.IO.File.Exists(Path))
                Path = System.IO.Path.GetDirectoryName(Path);
        }
        string[] allPath = AssetDatabase.FindAssets("t:Prefab", new string[] { Path });
        for (int i = 0; i < allPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allPath[i]);
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (obj != null)
            {
                ChangeResourcesQuote(obj);
            }
        }
    }
    /// <summary>
    /// 替换动画控制器和图片资源
    /// </summary>
    /// <param name="obj"></param>
    public static void ChangeResourcesQuote(GameObject obj)
    {
        //美术资源目录
        string ResPath = GetPrefabAssetPath(obj, "Assets/Res/HotelUI/");
        //预制体资源目录
        string PrefabPath = GetPrefabAssetPath(obj);
        List<Transform> childs = new List<Transform>();
        childs.Add(obj.transform);
        GetChilds(obj.transform, childs);
        foreach (var item in childs)
        {
            if (item.GetComponent<Animator>() != null && item.GetComponent<Animator>().runtimeAnimatorController!= null)
            {
                string Path = PrefabPath + item.GetComponent<Animator>().runtimeAnimatorController.name + ".controller";
                if (AssetDatabase.LoadAssetAtPath<AnimatorController>(Path) != null)
                    item.GetComponent<Animator>().runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(Path);
            }
            if (item.GetComponent<Image>() != null && item.GetComponent<Image>().sprite != null)
            {
                string Path = ResPath + item.GetComponent<Image>().sprite.name + ".png";
                if (AssetDatabase.LoadAssetAtPath<Sprite>(Path) != null)
                    item.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Path);
            }
        }
        EditorUtility.SetDirty(obj);
        AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();
    }
    #endregion
    #region 单个预制体匹配资源引用方法（对预制体操作）
    /// <summary>
    /// 一键修改资源引用
    /// </summary>
    [MenuItem("GameObject/资源引用/一键匹配资源引用", false, 12)]
    public static void ChangeResourcesQuote()
    {
        //美术资源目录
        string ResPath = GetPrefabAssetPath(Selection.activeGameObject, "Assets/Res/HotelUI/");
        //预制体资源目录
        string PrefabPath = GetPrefabAssetPath(Selection.activeGameObject);
        List<Transform> childs = new List<Transform>();
        childs.Add(Selection.activeGameObject.transform);
        GetChilds(Selection.activeGameObject.transform, childs);
        foreach (var item in childs)
        {
            if (item.GetComponent<Animator>() != null)
            {
                string Path = PrefabPath + item.GetComponent<Animator>().runtimeAnimatorController.name + ".controller";
                if (AssetDatabase.LoadAssetAtPath<AnimatorController>(Path) != null)
                    item.GetComponent<Animator>().runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(Path);
            }
            if (item.GetComponent<Image>() != null && item.GetComponent<Image>().sprite != null)
            {
                string Path = ResPath + item.GetComponent<Image>().sprite.name + ".png";
                if (AssetDatabase.LoadAssetAtPath<Sprite>(Path) != null)
                    item.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Path);
            }
        }
        EditorUtility.SetDirty(Selection.activeGameObject);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //PrefabUtility.SavePrefabAsset(Selection.activeGameObject);
    }
    #endregion
    #region 通用方法
    /// <summary>
    /// 获取预制体资源路径。
    /// </summary>
    /// <param name="gameObject">选中的物体</param>
    /// <param name="Root">"Asset/xxx/xxx/"</param>
    /// <returns></returns>
    public static string GetPrefabAssetPath(GameObject gameObject, string Root = null)
    {
#if UNITY_EDITOR
        // Project中的Prefab是Asset不是Instance
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            // 预制体资源就是自身
            string Path = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
            if (Root != null)
                Path = Path.Substring(17, Path.LastIndexOf('/') - 17);
            else Path = Path.Substring(0, Path.LastIndexOf('/'));
            return (Root + Path + "/");
        }

        // Scene中的Prefab Instance是Instance不是Asset
        if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(gameObject))
        {
            // 获取预制体资源
            var prefabAsset = UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
            string Path = UnityEditor.AssetDatabase.GetAssetPath(prefabAsset);
            if (Root != null)
                Path = Path.Substring(17, Path.LastIndexOf('/') - 17);
            else Path = Path.Substring(0, Path.LastIndexOf('/'));
            return (Root + Path + "/");
        }

        // PrefabMode中的GameObject既不是Instance也不是Asset
        var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);
        if (prefabStage != null)
        {
            // 预制体资源：prefabAsset = prefabStage.prefabContentsRoot
            string Path = prefabStage.prefabAssetPath; ;
            if (Root != null)
                Path = Path.Substring(17, Path.LastIndexOf('/') - 17);
            else Path = Path.Substring(0, Path.LastIndexOf('/'));
            return (Root + Path + "/");
        }
#endif
        // 不是预制体
        return null;
    }
    /// <summary>
    /// 获取所有子节点
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="transforms"></param>
    static void GetChilds(Transform obj, List<Transform> transforms)
    {
        for (int i = 0; i < obj.childCount; i++)
        {
            transforms.Add(obj.GetChild(i));
            if (obj.GetChild(i).childCount > 0)
            {
                GetChilds(obj.GetChild(i), transforms);
            }
        }
    }
    #endregion
}