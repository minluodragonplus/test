using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZHYUtility : MonoBehaviour
{

    public struct GameObjectRelate
    {
        /// <summary>
        /// 在父物体下创建子物体
        /// </summary>
        public static GameObject InstantiateGameObject(GameObject parent, string name)
        {
            GameObject go = new GameObject(name);

            if (parent != null)
            {
                //根据父物体的Transform组件确定子物体组件
                if (parent.GetComponent<RectTransform>())
                    go.AddComponent<RectTransform>();

                Transform t = go.transform;
                t.SetParent(parent.transform);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;

                RectTransform rect = go.transform as RectTransform;
                if (rect != null)
                {
                    rect.anchoredPosition = Vector3.zero;
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;

                    //判斷anchor是否在同一點
                    if (rect.anchorMin.x != rect.anchorMax.x && rect.anchorMin.y != rect.anchorMax.y)
                    {
                        rect.offsetMin = Vector2.zero;
                        rect.offsetMax = Vector2.zero;
                    }
                }

                go.layer = parent.layer;
            }
            return go;
        }

        /// <summary>
        /// 查询子物件
        /// </summary>
        public static Transform SearchChild(Transform target, string name)
        {
            if (target.name == name) return target;

            for (int i = 0; i < target.childCount; ++i)
            {
                var result = SearchChild(target.GetChild(i), name);

                if (result != null) return result;
            }

            return null;
        }

        /// <summary>
        /// 查询多个子物体
        /// </summary>
        public static List<Transform> SearchChildsPartName(Transform target, string name)
        {
            List<Transform> objs = new List<Transform>();
            Transform child = null;

            for (int i = 0; i < target.childCount; ++i)
            {
                child = target.GetChild(i);

                if (child != null)
                {
                    if (child.name.IndexOf(name, 0) >= 0)
                        objs.Add(child);
                }
            }

            return objs;
        }

        /// <summary>
        /// GameObject Array 排序
        /// </summary>
        public static void SortGameObjectArray(ref GameObject[] gos)
        {
            System.Array.Sort(gos, (a, b) => a.name.CompareTo(b.name));
        }

        /// <summary>
        /// GameObject Child 排序
        /// </summary>
        public static void SortHierarchyObjectChildByName(Transform parent)
        {
            List<Transform> children = new List<Transform>();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                {
                    Transform child = parent.GetChild(i);
                    children.Add(child);
                    child.parent = null;
                }
            }

            children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });
            foreach (Transform child in children)
            {
                child.parent = parent;
            }
        }

        /// <summary>
        /// 获取所有子物体
        /// </summary>
        /// <returns></returns>
        public List<Transform> GetChildren(Transform _parent)
        {
            List<Transform> children = new List<Transform>();
            if (_parent.childCount >= 1)
            {
                for (int i = 0; i < _parent.childCount; i++)
                {
                    var pathNode = _parent.GetChild(i).transform;

                    if (pathNode.name != "OrderInLayer")//特殊处理
                        children.Add(pathNode);
                }
            }
            return children;
        }

    }

}
