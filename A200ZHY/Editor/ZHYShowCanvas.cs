using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ZHYShowCanvas
{
    private static bool m_IsShowSortingOrder = false;

    [MenuItem("中华原/ShowCanvas")]
    static void ShowCanvas_Open()
    {
        if (m_IsShowSortingOrder)
        {
            m_IsShowSortingOrder = false;
            EditorApplication.hierarchyWindowItemOnGUI -= ShowCanvas;
        }
        else
        {
            m_IsShowSortingOrder = true;
            EditorApplication.hierarchyWindowItemOnGUI += ShowCanvas;
        }
        Menu.SetChecked("中华原/ShowCanvas", m_IsShowSortingOrder);
        EditorApplication.RepaintHierarchyWindow();
    }

    static ZHYShowCanvas()
    {
        Menu.SetChecked("中华原/ShowCanvas", m_IsShowSortingOrder);
    }

    private const int _LableWidth = 22;
    private const int _BtnWidth = 32;
    private const string _LablePlus = "+";
    private const string _LableLess = "-";

    private static void ShowCanvas(int instanceID, Rect selectionRect)
    {
        var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (go == null) return;

        Component objSpriteRenderer = go.GetComponent<SpriteRenderer>();
        Component objCanvas = go.GetComponent<Canvas>();
        Component obj = (objCanvas == null) ? objSpriteRenderer : objCanvas;

        if (obj == null) return;

        if (obj.GetType() == typeof(SpriteRenderer))
        {
            Show((SpriteRenderer) obj, selectionRect);
        }
        else if (obj.GetType() == typeof(Canvas))
        {
            Show((Canvas) obj, selectionRect);
        }
    }

    /// <summary>
    /// 绘制含有SpriteRenderer组件的物体
    /// </summary>
    static void Show(SpriteRenderer objSpriteRenderer, Rect selectionRect)
    {
        var pos = selectionRect;
        pos.x = pos.xMax - _LableWidth;
        pos.width = _LableWidth;
        pos.height = 14;

        if (GUI.Button(pos, _LablePlus))
        {
            objSpriteRenderer.sortingOrder++;
        }

        pos.x -= _BtnWidth;
        pos.width = _BtnWidth;


        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        EditorGUI.LabelField(pos, objSpriteRenderer.sortingOrder.ToString(), centeredStyle);
        EditorUtility.SetDirty(objSpriteRenderer);

        pos.x -= _LableWidth;
        pos.width = _LableWidth;

        if (GUI.Button(pos, _LableLess))
        {
            objSpriteRenderer.sortingOrder--;
        }
    }

    /// <summary>
    /// 绘制含有Canvas组件的物体
    /// </summary>
    static void Show(Canvas objCanvas, Rect selectionRect)
    {
        var pos = selectionRect;
        pos.x = pos.xMax - _LableWidth;
        pos.width = _LableWidth;
        pos.height = 14;

        if (GUI.Button(pos, _LablePlus))
        {
            objCanvas.sortingOrder++;
        }

        pos.x -= _BtnWidth;
        pos.width = _BtnWidth;

        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        EditorGUI.LabelField(pos, objCanvas.sortingOrder.ToString(), centeredStyle);
        EditorUtility.SetDirty(objCanvas);

        pos.x -= _LableWidth;
        pos.width = _LableWidth;

        if (GUI.Button(pos, _LableLess))
        {
            objCanvas.sortingOrder--;
        }
    }
}