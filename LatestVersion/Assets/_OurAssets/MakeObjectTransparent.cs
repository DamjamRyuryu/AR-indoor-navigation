using UnityEngine;

public class MakeObjectTransparent : MonoBehaviour
{
    public float transparency = 0.5f; // Desired transparency level for the object
    private Material transparentMaterial; // Reference to the transparent material

    void Start()
    {
        // Get the Renderer component
        Renderer renderer = GetComponent<Renderer>();

        // Create a new material 
        transparentMaterial = new Material(renderer.material);

        // Replace the object's material with the transparent material
        renderer.material = transparentMaterial;
    }

    void Update()
    {
        // Update the transparency value 
        Color color = transparentMaterial.color;
        color.a = transparency;
        transparentMaterial.color = color;
    }
}
