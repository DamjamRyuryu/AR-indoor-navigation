using UnityEngine;

public class ToggleEnableDisableObjects : MonoBehaviour
{
    public GameObject[] objectsToToggle; // Array of GameObjects to toggle

    public float minToggleTime = 5f; // Minimum time period
    public float maxToggleTime = 10f; // Maximum time period

    private float[] nextToggleTimes; // Array to store next toggle times for each object

    private void OnEnable()
    {
        // Initialize next toggle times 
        nextToggleTimes = new float[objectsToToggle.Length];
        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            nextToggleTimes[i] = Time.time + Random.Range(minToggleTime, maxToggleTime);
        }
    }

    private void Update()
    {
        //Check objects if they should be toggled
        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            if (Time.time >= nextToggleTimes[i])
            {
                // Toggle the active state of the object
                objectsToToggle[i].SetActive(!objectsToToggle[i].activeSelf);

                // Set the next toggle time
                nextToggleTimes[i] = Time.time + Random.Range(minToggleTime, maxToggleTime);
            }
        }
    }
}




