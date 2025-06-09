using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(MapRadiusRender))]
public class MapRadiusEditor : Editor
{
    void OnSceneGUI()
    {
        MapRadiusRender line = target as MapRadiusRender;
        EditorGUI.BeginChangeCheck();
        // 绘制可拖拽的起点/终点手柄
        Vector3 newStart = Handles.PositionHandle(line.startPoint, Quaternion.identity);
        Vector3 newEnd = Handles.PositionHandle(line.endPoint, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(line, "Move Line Points");
            line.startPoint = newStart;
            line.endPoint = newEnd;
        }
        // 实时绘制线
        Handles.color = line.lineColor;
        Handles.DrawLine(line.startPoint, line.endPoint);
        // 显示长度标签
        Handles.Label((line.startPoint + line.endPoint) * 0.5f, $"Length: {line.LineLength:F2}");
    }
}
#endif