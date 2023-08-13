using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

public class HumanGenerator : MonoBehaviour
{
    public GameObject humanModel; // 人类模型的预制体
    public NavMeshSurface navMeshSurface; // NavMeshSurface组件

    public Vector3 position = Vector3.zero; // 生成人类模型的位置（可在Inspector面板中更改）

    private List<GameObject> spawnedHumans = new List<GameObject>();
    private Vector3 lastPosition = Vector3.zero;

    private void Awake()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }
    void Start()
    {
        // 检查是否有NavMeshSurface组件
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface组件未指定！");
            return;
        }

        // 初始化上一帧的位置
        lastPosition = position;
    }

    void Update()
    {
        // 检查是否有位置信息，并且位置在NavMesh上，并且与上一帧的位置不同
        if (position != Vector3.zero && NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas) && position != lastPosition)
        {
            // 在位置生成人类模型
            GameObject human = Instantiate(humanModel, hit.position, Quaternion.identity);
            spawnedHumans.Add(human);

            // 设置生成的人类模型为Nav Mesh Obstacle
            NavMeshObstacle navMeshObstacle = human.GetComponent<NavMeshObstacle>();
            if (navMeshObstacle == null)
            {
                navMeshObstacle = human.AddComponent<NavMeshObstacle>();
                navMeshObstacle.carving = true;
            }

            // 设置障碍物的形状和大小，可以根据实际需求进行调整
            navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
            navMeshObstacle.center = new Vector3(0f, 0.5f, 0f);
            navMeshObstacle.radius = 0.3f;
            navMeshObstacle.height = 1f;

            // 重新生成Nav Mesh
            navMeshSurface.BuildNavMesh();

            // 更新上一帧的位置
            lastPosition = position;
        }
    }

    // 在Unity编辑器中可视化生成的人类模型位置
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (var human in spawnedHumans)
        {
            Gizmos.DrawSphere(human.transform.position, 0.1f);
        }
    }
}
