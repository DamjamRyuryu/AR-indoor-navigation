using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class HumanGenerator : MonoBehaviour
{
    public GameObject humanModel; //The human model
    public NavMeshSurface navMeshSurface; // The NavMeshSurface component

    public Vector3 position = Vector3.zero; // The position where the human will be generated

    private List<GameObject> spawnedHumans = new List<GameObject>(); // List to store human objects
    private Vector3 lastPosition = Vector3.zero; // Store the last generated position

    private void Awake()
    {
        // Get the NavMeshSurface component attached to this object and build the NavMesh
        navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }

    void Start()
    {
        // Check if a NavMeshSurface component is assigned
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface component is not assigned!");
            return;
        }

        lastPosition = position; // Initialize lastPosition with the provided position
    }

    void Update()
    {
       
    }

    public void SingleGenerate()
    {
        // Generate a human model if the provided position is valid and different from the last position
        if (position != Vector3.zero && NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas) && position != lastPosition)
        {
            // Instantiate a human model at the sampled position
            GameObject human = Instantiate(humanModel, hit.position, Quaternion.identity);
            // Add the human object to the spawnedHumans list
            spawnedHumans.Add(human); 
            // Add or get the NavMeshObstacle component and set its properties
            NavMeshObstacle navMeshObstacle = human.GetComponent<NavMeshObstacle>();
            if (navMeshObstacle == null)
            {
                navMeshObstacle = human.AddComponent<NavMeshObstacle>();
                navMeshObstacle.carving = true;
            }
            navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
            navMeshObstacle.center = new Vector3(0f, 0.5f, 0f);
            navMeshObstacle.radius = 0.3f;
            navMeshObstacle.height = 1f;

            // Rebuild the NavMesh
            navMeshSurface.BuildNavMesh();

            lastPosition = position; // Update the last position
        }
    }

    public void ModelDestroyed()
    {
        // Rebuild the NavMesh after a model is destroyed
        navMeshSurface.BuildNavMesh();
    }

    void OnDrawGizmos()
    {
        // Draw blue spheres at the positions of spawned human models
        Gizmos.color = Color.blue;
        foreach (var human in spawnedHumans)
        {
            if (human != null) Gizmos.DrawSphere(human.transform.position, 0.1f);
        }
    }
}
