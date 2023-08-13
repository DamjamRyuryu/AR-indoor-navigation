using UnityEngine;

/// <summary>
/// A simple script aimed at resolving a pre-existing issue where the navigation line wouldn't display
/// if the destination was set to a marker occupied by a generated human model.
/// </summary>
public class ModelDestroyTrigger : MonoBehaviour
{
    // This function is triggered when another object with collider and rigid body enters the trigger 
    private void OnTriggerStay(Collider other)
    {
        // Check if the colliding object is a human model clone
        if (other.gameObject.name == "human(Clone)")
        {
            Debug.Log("Tricky human spawn point. The clone will be destroyed.");

            // Deactivate and destroy the human model clone
            // If the clone is not deactivated first, area it blocks on the NavMesh will remain 
            other.gameObject.SetActive(false);
            Destroy(other.gameObject);

            // Trigger the re-building of the NavMeshSurface
            GameObject.Find("Environment/Ground_Mesh").GetComponent<HumanGenerator>().ModelDestroyed();
        }
    }
}

