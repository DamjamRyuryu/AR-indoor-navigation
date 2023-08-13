using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

public class HumanGenerator : MonoBehaviour
{
    public GameObject humanModel; // ����ģ�͵�Ԥ����
    public NavMeshSurface navMeshSurface; // NavMeshSurface���

    public Vector3 position = Vector3.zero; // ��������ģ�͵�λ�ã�����Inspector����и��ģ�

    private List<GameObject> spawnedHumans = new List<GameObject>();
    private Vector3 lastPosition = Vector3.zero;

    private void Awake()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }
    void Start()
    {
        // ����Ƿ���NavMeshSurface���
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface���δָ����");
            return;
        }

        // ��ʼ����һ֡��λ��
        lastPosition = position;
    }

    void Update()
    {
        // ����Ƿ���λ����Ϣ������λ����NavMesh�ϣ���������һ֡��λ�ò�ͬ
        if (position != Vector3.zero && NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas) && position != lastPosition)
        {
            // ��λ����������ģ��
            GameObject human = Instantiate(humanModel, hit.position, Quaternion.identity);
            spawnedHumans.Add(human);

            // �������ɵ�����ģ��ΪNav Mesh Obstacle
            NavMeshObstacle navMeshObstacle = human.GetComponent<NavMeshObstacle>();
            if (navMeshObstacle == null)
            {
                navMeshObstacle = human.AddComponent<NavMeshObstacle>();
                navMeshObstacle.carving = true;
            }

            // �����ϰ������״�ʹ�С�����Ը���ʵ��������е���
            navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
            navMeshObstacle.center = new Vector3(0f, 0.5f, 0f);
            navMeshObstacle.radius = 0.3f;
            navMeshObstacle.height = 1f;

            // ��������Nav Mesh
            navMeshSurface.BuildNavMesh();

            // ������һ֡��λ��
            lastPosition = position;
        }
    }

    // ��Unity�༭���п��ӻ����ɵ�����ģ��λ��
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (var human in spawnedHumans)
        {
            Gizmos.DrawSphere(human.transform.position, 0.1f);
        }
    }
}
