using System;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEngine.AI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class ZHY : EditorWindow
{
    [MenuItem("中华原/MapTools %#Q")]
    static void ZHYTools()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow<ZHY>("ZHY");
        editorWindow.position =
            new Rect(editorWindow.position.xMin + 100f, editorWindow.position.yMin + 100f, 420, 600);
        editorWindow.titleContent = new GUIContent("ZHY");
        editorWindow.Show();
    }

    void OnHierarchyChange() => Repaint();
    void OnSelectionChange() => Repaint();

    [SerializeField] protected List<UnityEngine.Object> _mGameObjectList = new List<UnityEngine.Object>();
    private SerializedObject _serializedObject;
    private SerializedProperty _mAssetListProperty;

    private readonly int _mSpace = 10;
    private GUIStyle _btnStyle;

    private Transform _mapRoot;
    private Transform _pathRoot;

    private bool _mIsCreateEmptyGo = true;
    private Transform _anyGoParent;
    private int _mCreateEmptyGoIndex1;
    private int _mCreateEmptyGoIndex2;
    private readonly List<GameObject> _mCreateEmptyGos = new List<GameObject>();

    private bool _mIsRenameGo = true;
    private string _mRenameGoString1 = "";

    private bool _mIsConnectPath1;
    private bool _mIsConnectPath2;
    private string _mConnectPathNode1 = "";
    private string _mConnectPathNode2 = "";
    private readonly Dictionary<string, GameObject> _mConnectPathNodes = new Dictionary<string, GameObject>();
    private readonly List<GameObject> _mConnectPathGos = new List<GameObject>();

    private bool _mIsAddManToPath;
    private Transform _mAddManToPathGo;
    private readonly List<GameObject> _mAddManToPathGos = new List<GameObject>();

    private Transform _mMachine;
    private bool _mIsMachineEditor;
    private string _mMachineEditorStr1 = "";
    private string _mMachineEditorStr2 = "";
    private List<GameObject> _mMachineEditorGos = new List<GameObject>();

    private bool Ismap()
    {
        if (_mapRoot == null) return false;

        if (_mapRoot.Find("Path") != null) return true;


        return false;
    }

    private void OnEnable()
    {
        _serializedObject = new SerializedObject(this);
        _mAssetListProperty = _serializedObject.FindProperty("_mGameObjectList");

        _pathRoot = GameObject.Find("Path").transform;
    }

    private void OnGUI()
    {
        GUILayout.Label("Map根节点");
        _mapRoot = EditorGUILayout.ObjectField(_mapRoot, typeof(Transform), GUILayout.Width(180)) as Transform;

        if (!Ismap()) EditorGUILayout.HelpBox(GetHelpString(), MessageType.Error);


        _btnStyle = new GUIStyle(GUI.skin.button)
            {fontSize = 14, alignment = TextAnchor.MiddleCenter, fixedWidth = 120};


        //HelpBoxTips();
        AddMapTools();

        GUILayout.Space(_mSpace);
        GUILayout.Button("", GUILayout.Height(2));
        CreateEmptyGo();

        GUILayout.Space(_mSpace);
        GUILayout.Button("", GUILayout.Height(2));
        RenameGo();

        GUILayout.Space(_mSpace);
        GUILayout.Button("", GUILayout.Height(2));
        ConnectPath();

        GUILayout.Space(_mSpace);
        GUILayout.Button("", GUILayout.Height(2));
        AddManToPath();

        GUILayout.Space(_mSpace);
        GUILayout.Button("", GUILayout.Height(2));
        MachineEditor();


        GUILayout.Space(_mSpace);
        GUILayout.Button("", GUILayout.Height(2));


        _serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_mAssetListProperty, true);
        if (EditorGUI.EndChangeCheck())
            _serializedObject.ApplyModifiedProperties();
    }

    private void AddMapTools()
    {
        if (!Ismap()) return;
        GUILayout.Space(10);
        GUILayout.Label("添加/移除Map画线工具");
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("AddLine", _btnStyle))
        {
            var zhyMapTools = _mapRoot.transform.GetComponent<ZHYMapTools>();
            if (zhyMapTools) return;
            Undo.AddComponent<ZHYMapTools>(_mapRoot.gameObject);
        }

        if (GUILayout.Button("RemoveLine", _btnStyle))
        {
            var zhyMapTools = _mapRoot.transform.GetComponent<ZHYMapTools>();
            if (zhyMapTools == null) return;
            Undo.DestroyObjectImmediate(zhyMapTools);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void CreateEmptyGo()
    {
        _mIsCreateEmptyGo = EditorGUILayout.Foldout(_mIsCreateEmptyGo, "创建从编号1到编号2的N个空节点");
        if (!_mIsCreateEmptyGo) return;

        GUILayout.BeginHorizontal();
        _anyGoParent = EditorGUILayout.ObjectField(_anyGoParent, typeof(Transform), GUILayout.Width(180)) as Transform;
        if (_anyGoParent == null)
        {
            GUILayout.Label("请拽入父节点！");
            GUILayout.EndHorizontal();
            return;
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("起始编号");
        _mCreateEmptyGoIndex1 = EditorGUILayout.IntField(_mCreateEmptyGoIndex1);
        GUILayout.Space(20);
        GUILayout.Label("截止编号");
        _mCreateEmptyGoIndex2 = EditorGUILayout.IntField(_mCreateEmptyGoIndex2);
        EditorGUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create", GUILayout.MinHeight(16)))
        {
            int minIndex, maxIndex;
            if (_mCreateEmptyGoIndex1 > _mCreateEmptyGoIndex2)
            {
                minIndex = _mCreateEmptyGoIndex2;
                maxIndex = _mCreateEmptyGoIndex1;
            }
            else
            {
                minIndex = _mCreateEmptyGoIndex1;
                maxIndex = _mCreateEmptyGoIndex2;
            }

            if (_anyGoParent.Find(minIndex.ToString()) && _anyGoParent.Find(maxIndex.ToString()))
            {
                ShowNotification(new GUIContent("节点已存在！"));
                return;
            }

            _mCreateEmptyGos.Clear();

            for (int index = minIndex; index < maxIndex + 1; index++)
            {
                var go = ZHYUtility.GameObjectRelate.InstantiateGameObject(_anyGoParent.gameObject, index.ToString());
                _mCreateEmptyGos.Add(go);
            }
        }

        if (_mCreateEmptyGos.Count < 1)
        {
            EditorGUILayout.EndHorizontal();
            return;
        }

        if (GUILayout.Button("Undo", GUILayout.MinHeight(16)))
        {
            for (int i = _mCreateEmptyGos.Count - 1; i > -1; i--)
                Undo.DestroyObjectImmediate(_mCreateEmptyGos[i]);

            _mCreateEmptyGos.Clear();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void RenameGo()
    {
        _mIsRenameGo = EditorGUILayout.Foldout(_mIsRenameGo, "添加/替换后缀");
        if (!_mIsRenameGo) return;

        GUILayout.Label("请输入后缀名称");
        GUILayout.BeginHorizontal();
        _mRenameGoString1 = EditorGUILayout.TextField(_mRenameGoString1, GUILayout.Width(180));
        if (GUILayout.Button("Rename", _btnStyle))
        {
            var selects = Selection.gameObjects;
            for (int i = 0; i < selects.Length; i++)
            {
                string name = selects[i].name;
                if (name.IndexOf('_') != -1)
                {
                    string name1 = name.Split('_')[0];
                    selects[i].name = $"{name1}_{_mRenameGoString1}";
                }
                else
                {
                    selects[i].name = $"{name}_{_mRenameGoString1}";
                }
            }
        }

        GUILayout.EndVertical();
    }

    private void ConnectPath()
    {
        _mIsConnectPath1 = EditorGUILayout.Foldout(_mIsConnectPath1, "连接、解除连接路径点");
        if (!_mIsConnectPath1) return;

        GUILayout.Label("Path根节点");
        GUILayout.BeginHorizontal();
        _pathRoot = EditorGUILayout.ObjectField(_pathRoot, typeof(Transform), GUILayout.Width(180)) as Transform;
        if (_pathRoot == null)
        {
            GUILayout.EndHorizontal();
            return;
        }

        GUILayout.Label("选中任意两个路径点即可连接。");
        GUILayout.EndHorizontal();

        if (Selection.gameObjects.Length == 2
            && Selection.gameObjects[0].name != "OrderInLayer"
            && Selection.gameObjects[1].name != "OrderInLayer"
            && Selection.gameObjects[0].transform.parent.name == "Path"
            && Selection.gameObjects[1].transform.parent.name == "Path")
        {
            GUILayout.Space(10);
            GUILayout.Label("Map路径点相连", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Node1:" + Selection.gameObjects[0].name, EditorStyles.boldLabel);
            GUILayout.Label("Node2:" + Selection.gameObjects[1].name, EditorStyles.boldLabel);
            if (GUILayout.Button("Connect", _btnStyle))
            {
                Transform node1 = Selection.gameObjects[0].transform;
                for (int i = 0; i < Selection.gameObjects.Length; i++)
                {
                    Transform node2 = Selection.gameObjects[1].transform;
                    if (node2.Find(node1.name.Split('_')[0]) == null)
                    {
                        ZHYUtility.GameObjectRelate.InstantiateGameObject(node2.gameObject, node1.name.Split('_')[0]);
                        Undo.RegisterCreatedObjectUndo(node2.gameObject, "InstantiateGameObject");
                    }

                    if (node1.Find(node2.name.Split('_')[0]) == null)
                    {
                        ZHYUtility.GameObjectRelate.InstantiateGameObject(node1.gameObject, node2.name.Split('_')[0]);
                        Undo.RegisterCreatedObjectUndo(node1.gameObject, "InstantiateGameObject");
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Map路径点解除连接关系", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Node1:" + Selection.gameObjects[0].name, EditorStyles.boldLabel);
            GUILayout.Label("Node2:" + Selection.gameObjects[1].name, EditorStyles.boldLabel);
            if (GUILayout.Button("Decoupling", _btnStyle))
            {
                Transform node1 = Selection.gameObjects[0].transform;
                for (int i = 0; i < Selection.gameObjects.Length; i++)
                {
                    Transform node2 = Selection.gameObjects[1].transform;
                    if (node2.Find(node1.name.Split('_')[0]) != null)
                    {
                        Transform node1Child = node2.Find(node1.name.Split('_')[0]);
                        node1Child.name = "xxxxxxxxxx";
                        if (!PrefabUtility.IsPartOfAnyPrefab(Selection.gameObjects[0]))
                            Undo.DestroyObjectImmediate(node1Child.gameObject);
                    }

                    if (node1.Find(node2.name.Split('_')[0]) != null)
                    {
                        Transform node2Child = node1.Find(node2.name.Split('_')[0]);
                        node2Child.name = "xxxxxxxxxx";
                        if (!PrefabUtility.IsPartOfAnyPrefab(Selection.gameObjects[0]))
                            Undo.DestroyObjectImmediate(node2Child.gameObject);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        _mIsConnectPath2 = EditorGUILayout.Toggle("按名称连接", _mIsConnectPath2);
        // GUILayout.Label("Map路径点相连(节点名要正确)");
        if (!_mIsConnectPath2) return;

        _mConnectPathNode1 = EditorGUILayout.TextField("Node1 只能填一个", _mConnectPathNode1, GUILayout.Width(240));
        _mConnectPathNode2 = EditorGUILayout.TextField("Node2 可多个 用 , 分隔", _mConnectPathNode2, GUILayout.Width(360));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Connect", _btnStyle))
        {
            if (string.IsNullOrEmpty(_mConnectPathNode1) || string.IsNullOrEmpty(_mConnectPathNode2))
            {
                Debug.LogError("请输入路径点名称");
                return;
            }

            _mConnectPathGos.Clear();
            _mConnectPathNodes.Clear();
            var pathRootChildCount = _pathRoot.childCount;
            for (int i = 1; i < pathRootChildCount; i++)
            {
                var child = _pathRoot.GetChild(i);
                _mConnectPathNodes.Add(child.name.Split('_')[0], child.gameObject);
            }

            var isContainsnode1 = _mConnectPathNodes.TryGetValue(_mConnectPathNode1.Split('_')[0], out var node1);
            if (!isContainsnode1)
            {
                ShowNotification(new GUIContent("node1不存在,请检查名称是否正确！"));
                return;
            }

            string[] str = _mConnectPathNode2.Split(',');
            for (int i = 0; i < str.Length; i++)
            {
                var isContainsnode2 = _mConnectPathNodes.TryGetValue(str[i].Split('_')[0], out var node2);
                if (!isContainsnode2)
                {
                    ShowNotification(new GUIContent("node2第" + (i + 1) + "个参数不存在,请检查名称是否正确"));
                    return;
                }

                if (node2.transform.Find(node1.name.Split('_')[0]) == null)
                {
                    var go = ZHYUtility.GameObjectRelate.InstantiateGameObject(node2, node1.name.Split('_')[0]);
                    _mConnectPathGos.Add(go);
                    Undo.RegisterCreatedObjectUndo(go, "InstantiateGameObject");

                }

                if (node1.transform.Find(node2.name.Split('_')[0]) == null)
                {
                    var go = ZHYUtility.GameObjectRelate.InstantiateGameObject(node1, node2.name.Split('_')[0]);
                    _mConnectPathGos.Add(go);
                    Undo.RegisterCreatedObjectUndo(go, "InstantiateGameObject");

                }
            }
        }

        if (_mConnectPathGos.Count < 1)
        {
            GUILayout.EndHorizontal();
            return;
        }

        if (GUILayout.Button("Undo", _btnStyle))
        {
            for (int i = _mConnectPathGos.Count - 1; i > -1; i--)
                Undo.DestroyObjectImmediate(_mConnectPathGos[i]);
            _mConnectPathGos.Clear();
        }

        GUILayout.EndHorizontal();
    }

    private void AddManToPath()
    {
        _mIsAddManToPath = EditorGUILayout.Foldout(_mIsAddManToPath, "为每个路径点添加NPC");
        if (!_mIsAddManToPath) return;
        GUILayout.Label("Path根节点");
        _pathRoot = EditorGUILayout.ObjectField(_pathRoot, typeof(Transform), GUILayout.Width(180)) as Transform;

        GUILayout.Label("NPC（需含有Canvas组件）");
        GUILayout.BeginHorizontal();
        _mAddManToPathGo =
            EditorGUILayout.ObjectField(_mAddManToPathGo, typeof(Transform), GUILayout.Width(180)) as Transform;
        if (_mAddManToPathGo == null)
        {
            EditorGUILayout.EndHorizontal();
            return;
        }

        if (GUILayout.Button("AddNPC", _btnStyle))
        {
            var man = _mAddManToPathGo;
            var parent = _pathRoot;

            var canvases = new Dictionary<string, Canvas>();
            if (!(parent is null))
                for (int i = 0; i < parent.GetChild(0).childCount; i++)
                {
                    canvases.Add(parent.GetChild(0).GetChild(i).name,
                        parent.GetChild(0).GetChild(i).GetComponent<Canvas>());
                }

            if (!(parent is null) && parent.GetChild(2).Find("xxxxxxxxxx") != null &&
                parent.GetChild(parent.childCount).Find("xxxxxxxxxx") != null) return;

            _mAddManToPathGos.Clear();

            for (int i = 1; i < parent.childCount; i++)
            {
                var go = Instantiate(man);
                var child = parent.GetChild(i);
                go.transform.SetParent(parent.GetChild(i));
                go.name = "xxxxxxxxxx";
                go.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                var addComponent = go.gameObject.AddComponent<Canvas>();
                addComponent.overrideSorting = true;
                addComponent.sortingOrder = canvases[child.name.Split('_')[1]].sortingOrder;
                Undo.RegisterCreatedObjectUndo(go, "Instantiate");
                _mAddManToPathGos.Add(go.gameObject);
            }
        }

        if (_mAddManToPathGos.Count < 1)
        {
            EditorGUILayout.EndHorizontal();
            return;
        }

        if (GUILayout.Button("Undo", _btnStyle))
        {
            for (int i = _mAddManToPathGos.Count - 1; i > -1; i--)
                Undo.DestroyObjectImmediate(_mAddManToPathGos[i]);
            _mAddManToPathGos.Clear();
        }

        EditorGUILayout.EndHorizontal();
    }

    private string GetHelpString()
    {
        var sb = new StringBuilder();
        sb.Append("    No Source." + "\n");
        sb.Append("    请将Map拖拽到此处");
        return sb.ToString();
    }

    private void MachineEditor()
    {
        _mIsMachineEditor = EditorGUILayout.Foldout(_mIsMachineEditor, "Map物件子节点批量选中");
        if (!_mIsMachineEditor) return;

        GUILayout.BeginHorizontal();
        _mMachine = EditorGUILayout.ObjectField(_mMachine, typeof(Transform), GUILayout.Width(180)) as Transform;

        if (_mMachine == null)
        {
            GUILayout.Label("拽入Room或者物件以使用");
            GUILayout.EndHorizontal();
            return;
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("批量选择节点");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Level下的节点名");
        _mMachineEditorStr1 = GUILayout.TextField(_mMachineEditorStr1, GUILayout.Width(180));

        if (GUILayout.Button("Select", _btnStyle))
        {
            if (_mMachine.GetChild(1).Find(_mMachineEditorStr1) == null)
            {
                ShowNotification(new GUIContent("节点不存在,请检查名称是否正确"));
                return;
            }

            _mMachineEditorGos.Clear();
            for (int i = 0; i < _mMachine.childCount; i++)
            {
                var levelnode = _mMachine.Find("Level" + i);
                if (levelnode == null) break;
                var go = levelnode.Find(_mMachineEditorStr1);
                if (go == null) break;
                _mMachineEditorGos.Add(go.gameObject);
            }

            Object[] Gos = _mMachineEditorGos.ToArray();
            Selection.objects = Gos;
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("批量创建节点");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Level下的节点名");
        _mMachineEditorStr2 = GUILayout.TextField(_mMachineEditorStr2, GUILayout.Width(180));

        if (GUILayout.Button("Create", _btnStyle))
        {
            if (_mMachine.GetChild(1).Find(_mMachineEditorStr2) != null)
            {
                ShowNotification(new GUIContent("节点已存在,请检查名称是否正确"));
                return;
            }

            _mMachineEditorGos.Clear();
            for (int i = 0; i < _mMachine.childCount; i++)
            {
                var levelnode = _mMachine.Find("Level" + i);
                if (levelnode == null) break;
                var go = ZHYUtility.GameObjectRelate.InstantiateGameObject(levelnode.gameObject, _mMachineEditorStr2);
                Undo.RegisterCreatedObjectUndo(go, "InstantiateGameObject");
            }
        }

        GUILayout.EndHorizontal();
    }
}