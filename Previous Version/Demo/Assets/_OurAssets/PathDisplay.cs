using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class PathDisplay : MonoBehaviour
{
    public Vector3 destination; // Ŀ�ĵ�λ��
    public GameObject pathHolder; // ���·���߶εĿ�����
    public float yOffset = 0.2f; // ·���߶εĸ߶�ƫ����

    private NavMeshPath path;
    private LineRenderer lineRenderer;
    private NavMeshSurface navMeshSurface;

    private void Start()
    {
        path = new NavMeshPath();
        lineRenderer = pathHolder.GetComponent<LineRenderer>();
        navMeshSurface = FindObjectOfType<NavMeshSurface>(); // ��ȡNavMeshSurface���

        // ��ʼ�� LineRenderer
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    private void Update()
    {

        // ��ȡ���嵱ǰλ�ú�Ŀ�ĵ�λ��
        Vector3 startPos = transform.position;
        Vector3 targetPos = destination;

        // �������·��
        NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, path);

        // ���� LineRenderer �Ķ���
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        // ���� LineRenderer �Ķ�������λ��
        lineRenderer.positionCount = path.corners.Length;

        for (int i = 0; i < path.corners.Length; i++)
        {
            // ��·���߶ε�ÿ��������и߶�ƫ��
            Vector3 offsetPoint = path.corners[i] + Vector3.up * yOffset;
            lineRenderer.SetPosition(i, offsetPoint);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // ��Scene��ͼ����NavMeshSurface�ı߽��
        if (navMeshSurface != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(navMeshSurface.transform.position + navMeshSurface.center, navMeshSurface.size);
        }
    }
}
