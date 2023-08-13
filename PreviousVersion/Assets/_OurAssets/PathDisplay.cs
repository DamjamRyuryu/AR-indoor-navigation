using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class PathDisplay : MonoBehaviour
{
    public Vector3 destination; // 目的地位置
    public GameObject pathHolder; // 存放路径线段的空物体
    public float yOffset = 0.2f; // 路径线段的高度偏移量

    private NavMeshPath path;
    private LineRenderer lineRenderer;
    private NavMeshSurface navMeshSurface;

    private void Start()
    {
        path = new NavMeshPath();
        lineRenderer = pathHolder.GetComponent<LineRenderer>();
        navMeshSurface = FindObjectOfType<NavMeshSurface>(); // 获取NavMeshSurface组件

        // 初始化 LineRenderer
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    private void Update()
    {

        // 获取物体当前位置和目的地位置
        Vector3 startPos = transform.position;
        Vector3 targetPos = destination;

        // 计算最短路径
        NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, path);

        // 更新 LineRenderer 的顶点
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        // 设置 LineRenderer 的顶点数和位置
        lineRenderer.positionCount = path.corners.Length;

        for (int i = 0; i < path.corners.Length; i++)
        {
            // 对路径线段的每个顶点进行高度偏移
            Vector3 offsetPoint = path.corners[i] + Vector3.up * yOffset;
            lineRenderer.SetPosition(i, offsetPoint);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 在Scene视图绘制NavMeshSurface的边界框
        if (navMeshSurface != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(navMeshSurface.transform.position + navMeshSurface.center, navMeshSurface.size);
        }
    }
}
