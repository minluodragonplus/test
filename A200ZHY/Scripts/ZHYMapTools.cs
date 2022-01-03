using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZHYMapTools : MonoBehaviour
{

    Dictionary<Transform, List<Transform>> m_Nodes = new Dictionary<Transform, List<Transform>>();
    Dictionary<string, Transform> m_NodesNames = new Dictionary<string, Transform>();
    string[] mapDefaultName = new string[9] { "Path", "BackgroundLayer", "Hotel", "VisibilityControl", "MaskControl", "ResponseArea", "KeyPoints", "KeyLayers", "Npc" };
    [Header("总是执行/焦点执行")]
    public bool m_DrawGizmos;

    void OnDrawGizmosSelected()
    {
        if (!m_DrawGizmos)
            return;

        Init(transform);
    }
    private void OnDrawGizmos()
    {
        if (m_DrawGizmos)
            return;
        Init(transform);
    }

    private void Init(Transform _target)
    {
        if (!_target)
        {
            Debug.LogError("parent is null");
            return;
        }
        if (CompareMapName(transform.name))
        {
            return;
        }
        else
        {
            m_Nodes = GetNodes(transform.Find("Path"));
            foreach (var node in m_Nodes.Keys)
            {
                foreach (var nodechild in m_Nodes[node])
                {

                    if (m_NodesNames.ContainsKey(nodechild.name))
                    {
                        if (m_NodesNames[nodechild.name].Find(node.name.Split('_')[0]))//从目标路径点打回来
                        {
                            Debug.DrawLine(m_NodesNames[nodechild.name].transform.position, node.transform.position, Color.green);
                        }
                        else
                        {
                            Debug.DrawLine(m_NodesNames[nodechild.name].transform.position, node.transform.position, Color.red);
                        }
                    }

                }
            }


            Check1(_target);
            Check2(_target);
            Check3(_target);
            Check4(_target);
        }
    }

    public bool CompareMapName(string name)
    {
        name = name.Substring(0, 3);
        if (name != "Map")
        {
            Debug.LogErrorFormat("Error! Map is null");
            return true;
        }
        else
        {
            return false;
        }
    }
    //取所有路径点
    public Dictionary<Transform, List<Transform>> GetNodes(Transform _parent)
    {
        Dictionary<Transform, List<Transform>> PathNodes = new Dictionary<Transform, List<Transform>>();
        m_NodesNames.Clear();

        if (_parent.childCount >= 1)
        {
            for (int i = 0; i < _parent.childCount; i++)
            {
                var pathNode = _parent.GetChild(i).transform;
                if (pathNode.name != "OrderInLayer")
                {
                    PathNodes.Add(pathNode, GetChildren(pathNode));

                    m_NodesNames.Add(pathNode.name.Split('_')[0], pathNode);
                }
            }
            if (PathNodes.Count == 0) return null;
        }
        return PathNodes;
    }

    /// <summary>
    /// 获取子物体
    /// </summary>
    public List<Transform> GetChildren(Transform _target)
    {
        List<Transform> children = new List<Transform>();
        if (_target.childCount >= 1)
        {
            for (int i = 0; i < _target.childCount; i++)
            {
                var pathNode = _target.GetChild(i).transform;

                if (pathNode.name != "OrderInLayer")//特殊处理
                    children.Add(pathNode);
            }
        }
        return children;
    }
    /// <summary>
    /// 获取子物体名字
    /// </summary>
    public List<string> GetChildrenName(Transform _target)
    {
        List<string> childrenName = new List<string>();
        if (_target.childCount >= 1)
        {
            for (int i = 0; i < _target.childCount; i++)
            {
                var pathNode = _target.GetChild(i).transform;

                if (pathNode.name != "OrderInLayer")//特殊处理
                    childrenName.Add(pathNode.name);
            }
        }
        return childrenName;
    }



    /// <summary>
    /// 检查节点是否完整及节点名字对不对
    /// </summary>
    public void Check1(Transform _target)
    {
        if (_target.childCount < 9)
        {
            LogOrange("节点不足,请检查节点是否完整");
        }
        else
        {
            List<string> mapChildren = GetChildrenName(_target);
            for (int i = 0; i < mapDefaultName.Length; i++)
            {
                if (!mapChildren.Contains(mapDefaultName[i]))
                    LogOrange(mapDefaultName[i] + "节点不存在,请检查节点是否完整");
            }
        }

    }
    /// <summary>
    /// 检查节点坐标情况
    /// </summary>
    public void Check2(Transform _target)
    {
        List<Transform> mapChildren = GetChildren(_target);
        for (int i = 0; i < mapChildren.Count; i++)
        {
            RectTransform temp = mapChildren[i].GetComponent<RectTransform>();
            if (temp)
            {
                if (temp.anchoredPosition != Vector2.zero)
                {
                    Debug.Log(temp.name + "节点坐标不为零");
                }
            }
        }
    }
    /// <summary>
    /// 检查节点挂载组件情况
    /// </summary>
    public void Check3(Transform _target)
    {
        if (_target.Find("Hotel") == null) return;
        List<Transform> hotelChildren = GetChildren(_target.Find("Hotel"));
        for (int i = 0; i < hotelChildren.Count; i++)
        {
            if (hotelChildren[i].name != "BackgroundLayer" && hotelChildren[i].name != "UILayer")
            {

                Animator anitemp = hotelChildren[i].GetComponent<Animator>();
                if (!anitemp)
                {
                    LogFuchsia(hotelChildren[i].name + "的升级动画组件不存在,请检查!");
                }
                else
                {
                    if (!anitemp.runtimeAnimatorController)
                    {
                        LogFuchsia(hotelChildren[i].name + "的升级动画丢失引用,请检查!");
                    }
                }
            }
        }
    }
    /// <summary>
    /// 检查节点下升级特效情况
    /// </summary>
    public void Check4(Transform _target)
    {
        if (_target.Find("Hotel") == null) return;
        List<Transform> hotelChildren = GetChildren(_target.Find("Hotel"));
        for (int i = 0; i < hotelChildren.Count; i++)
        {
            if (hotelChildren[i].name != "BackgroundLayer" && hotelChildren[i].name != "UILayer")
            {
                if (!hotelChildren[i].Find("vfx_level"))
                {
                    LogFuchsia(hotelChildren[i].name + "的升级特效(vfx_level)不存在,请检查!");
                }
            }
        }
    }
   
    /// <summary>
    /// 橙色打印
    /// </summary>
    public void LogOrange(object _Msg)
    {
        Debug.Log("<color=" + "orange" + ">" + _Msg + "</color>");
    }
    /// <summary>
    /// 樱红色打印
    /// </summary>
    public void LogFuchsia(object _Msg)
    {
        Debug.Log("<color=" + "fuchsia" + ">" + _Msg + "</color>");
    }
}
