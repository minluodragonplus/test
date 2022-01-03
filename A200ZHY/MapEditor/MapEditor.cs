using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapEditor : MonoBehaviour
{
    private GameObject Order;
    private List<(string name, int)> OrderList = new List<(string name, int)>();

    private GameObject Path;
    private List<(int id, int layer, Transform obj)> PathPoint = new List<(int id, int layer, Transform obj)>();

    List<int> aiPoint = new List<int>();
    bool CanMove = false;

    public int NextPoint = 0;
    int lastPoint = 0;
    public GameObject Npc;
    public int Speed = 200;

    int NowDex = 0;

    Transform TargetPos;

    void Start()
    {
        ///////////////////////////////自动寻路初始化列表////////////////////////////
        Npc.AddComponent<Canvas>();
        Npc.GetComponent<Canvas>().overrideSorting = true;

        lastPoint = int.Parse(Npc.transform.parent.name.Split('_')[0]);

        Order = GameObject.Find("OrderInLayer");
        Transform[] obj = Order.GetComponentsInChildren<Transform>();
        foreach (var item in obj)
        {
            if (item.GetComponent<Canvas>() != null)
            {
                OrderList.Add((item.name, item.GetComponent<Canvas>().sortingOrder));
            }
        }

        Path = transform.Find("Path").gameObject;
        Transform[] point = Path.GetComponentsInChildren<Transform>();

        foreach (var item in point)
        {
            if (item.name.ToLower().Contains("_".ToLower()))
            {
                string id = item.name.Split('_')[0];
                string layer = item.name.Split('_')[1];
                PathPoint.Add((int.Parse(id), int.Parse(layer), item));
            }
        }
        /////////////////////////////////////////////////////////////////////////////
        //点击移动初始化
        foreach (var item in PathPoint)
        {
            item.obj.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
            item.obj.gameObject.AddComponent<Image>();
            item.obj.gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            item.obj.gameObject.AddComponent<Canvas>();
            item.obj.gameObject.GetComponent<Canvas>().overrideSorting = true;
            item.obj.gameObject.GetComponent<Canvas>().sortingOrder = 10000;
            item.obj.gameObject.AddComponent<GraphicRaycaster>();
            item.obj.gameObject.AddComponent<Button>();
            item.obj.gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                TargetPos = item.obj;
                aiPoint.Add(item.id);
                CanMove = true;
            });
        }
    }
    void Update()
    {
        //点击移动逻辑
        if (CanMove)
        {
            Npc.transform.position = Vector3.MoveTowards(Npc.transform.position, TargetPos.position, Time.deltaTime * Speed);
            if (Vector3.Distance(Npc.transform.position, TargetPos.position) <= 1)
            {
                int TargetLayer = int.Parse(TargetPos.gameObject.name.Split('_')[1]);
                for (int i = 0; i < OrderList.Count; i++)
                {
                    if(int.Parse(OrderList[i].name) == TargetLayer)
                    {
                        Npc.GetComponent<Canvas>().sortingOrder = OrderList[i].Item2;
                        CanMove = false;
                    }
                }
            }
        }
        //自动寻路逻辑
        //if (CanMove)
        //{
        //    int nextPoint = aiPoint[NowDex];
        //    foreach (var item in PathPoint)
        //    {
        //        if (item.id == nextPoint)
        //        {
        //            Npc.transform.position = Vector3.MoveTowards(Npc.transform.position, item.obj.transform.position, Time.deltaTime * Speed);
        //            if (Vector3.Distance(Npc.transform.position, item.obj.transform.position) <= 1)
        //            {
        //                Npc.GetComponent<Canvas>().sortingOrder = OrderList[item.layer];
        //                NowDex++;
        //                if (NowDex == aiPoint.Count)
        //                {
        //                    lastPoint = nextPoint;

        //                    CanMove = false;
        //                    aiPoint.Clear();
        //                    NowDex = 0;
        //                }
        //            }
        //        }
        //    }
        //}


        //自动寻路触发方式
        if (Input.GetKeyDown(KeyCode.Return))
        {
            AiPoint(NextPoint);
        }
    }
    //思维1：逆向寻路（半成品）
    private bool AiPoint(int Point)
    {
        foreach (var item in PathPoint)
        {
            if (CanMove) break;
            if (item.id == Point)
            {
                for (int i = 0; i < item.obj.childCount; i++)
                {
                    if (CanMove) break;

                    int nextPoint = int.Parse(item.obj.GetChild(i).name);

                    if (!aiPoint.Contains(nextPoint))
                    {
                        aiPoint.Add(nextPoint);
                        if (nextPoint == lastPoint)
                        {
                            List<int> newPoint = new List<int>();
                            for (int z = aiPoint.Count - 1; z >= 0; z--)
                            {
                                newPoint.Add(aiPoint[z]);
                            }
                            aiPoint.Clear();
                            aiPoint.AddRange(newPoint);
                            aiPoint.Add(NextPoint);
                            CanMove = true;
                            return CanMove;
                        }
                        else
                        {
                            AiPoint(nextPoint);
                        }
                    }
                }
            }
        }
        return CanMove;
    }
    //思维2：全局寻路（半成品）
    void AiPoint1(int Point)
    {
        for (int i = 0; i < PathPoint.Count; i++)
        {
            if (CanMove) break;
            string num = Point.ToString();
            if(PathPoint[i].obj.Find(num) != null)
            {
                if (!aiPoint.Contains(PathPoint[i].id))
                {
                    aiPoint.Add(PathPoint[i].id);
                }
                else
                {
                    //aiPoint.Clear();
                    return;
                }
                if (PathPoint[i].id == lastPoint)
                {
                    List<int> newPoint = new List<int>();
                    for (int z = aiPoint.Count - 1; z > 0; z--)
                    {
                        for (int s = 0; s < PathPoint.Count; s++)
                        {
                            if (aiPoint[z] == PathPoint[s].id)
                            {
                                Add();
                                void Add()
                                {
                                    int newZ = z - 1;
                                    string next = aiPoint[newZ].ToString();
                                    if (PathPoint[s].obj.Find(next))
                                    {
                                        newPoint.Add(aiPoint[z]);
                                    }
                                    else
                                    {
                                        z--;
                                        Add();
                                    }
                                }
                            }
                        }
                    }
                    aiPoint.Clear();
                    aiPoint.AddRange(newPoint);
                    aiPoint.Add(NextPoint);
                    CanMove = true;
                }
                else
                {
                    AiPoint1(PathPoint[i].id);
                }
            }
        }
    }
}
