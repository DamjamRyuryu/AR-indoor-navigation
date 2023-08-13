using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UserI;

/// <summary>
/// Sensor for the exhibit. Use trigger boxes to represent the interaction range. Send the name tag to UI and call the UI to handle the event.
/// </summary>
public class DetailTriggerSensor : MonoBehaviour
{
    private string exhibitName;     // Name tag
    
    // Needed component
    private PageView _UI;
    private BoxCollider _collider;

    // Awake is called when the object is created or activated, which is prior to Start()
    private void Awake()
    {
        // Get the objects and make sure the box collider is set to be a trigger box
        _UI = GameObject.Find("Canvas/PageView").GetComponent<PageView>();
        _collider = GetComponent<BoxCollider>();
        if (!_collider.isTrigger) Debug.LogError("Error: the BOX collision is not set to be a trigger box!");

        // Construct the url.The sname tag is the object name in the scene.
        string _url = "http://" + _UI.serverIP + ':' + _UI.serverPort + "/sensor_init?sname=" + gameObject.name;
        //Debug.Log(_url);
        StartCoroutine(MarkerInit(_url));
    }

    /// <summary>
    /// Get the name tag from the server.
    /// </summary>
    /// <param name="url"> The url of the API. </param>
    /// <returns></returns>
    IEnumerator MarkerInit(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    exhibitName = webRequest.downloadHandler.text[1..^1];   //get rid of \" and \"
                    break;
            }

        }
    }

    /*  
     *  The following three functions are built-in function in Unity. 
     *  They are called respectively when the user enter, stay or leave the trigger area.
     */
    private void OnTriggerEnter(Collider other)
    {
        // If the uesr enter the trigger area, call the trigger enter event handler in UI.
        if(other.gameObject.name == "PlayerCapsule")
        {
            Debug.Log("Detected: stepping on " + exhibitName);
            _UI.TriggerEnter(exhibitName);
        }
    }

    // It is the same to the function in ModelDestoryTrigger.cs
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "human(Clone)")
        {
            Debug.Log("Tricky human spawn point. The clone will be destroyed.");
            other.gameObject.SetActive(false);
            Destroy(other.gameObject);
            GameObject.Find("Environment/Ground_Mesh").GetComponent<HumanGenerator>().ModelDestroyed();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If the user leave the area, call the trigger exit event handler in UI.
        if (other.gameObject.name == "PlayerCapsule")
        {
            Debug.Log("Detected: leaving " + exhibitName);
            _UI.TriggerExit();
        }
    }
}
