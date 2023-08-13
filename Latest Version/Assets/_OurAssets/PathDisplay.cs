using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class PathDisplay : MonoBehaviour
{
    public Vector3 destination; // The target destination 
    public GameObject pathHolder; // The GameObject to form the line
    public float yOffset = 0.2f; // Vertical offset for the path 

    private NavMeshPath path; // The  path
    private LineRenderer lineRenderer; // Component to render the path visualization
    private NavMeshSurface navMeshSurface; // Reference to the NavMeshSurface component

    private void Start()
    {
        path = new NavMeshPath();
        lineRenderer = pathHolder.GetComponent<LineRenderer>();
        navMeshSurface = FindObjectOfType<NavMeshSurface>(); // Find the NavMeshSurface in the scene

        // Initialize LineRenderer settings
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    private void Update()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = destination;

        // Calculate the path between the start and target positions
        NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, path);

        // Update the LineRenderer to visualize the path
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = path.corners.Length;

        for (int i = 0; i < path.corners.Length; i++)
        {
            Vector3 offsetPoint = path.corners[i] + Vector3.up * yOffset;
            lineRenderer.SetPosition(i, offsetPoint);
        }
    }

    // Draw a wire cube to visualize the NavMesh surface area in the editor
    private void OnDrawGizmosSelected()
    {
        if (navMeshSurface != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(navMeshSurface.transform.position + navMeshSurface.center, navMeshSurface.size);
        }
    }
}
