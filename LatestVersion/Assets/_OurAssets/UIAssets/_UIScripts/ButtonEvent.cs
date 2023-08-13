using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserI;

/// <summary>
/// Create the listener to trigger click event when the button is clicked.
/// </summary>
public class ButtonEvent : MonoBehaviour
{
    private Button _btn;            // Button component
    private PageView _UI;           // Main UI script
    private string text = "NULL";   // The name tag to call
    private bool _hasTxt = false;
    // Awake is called when the object is created or activated, which is prior to Start()
    void Awake()
    {
        // Find the UI in game
        _UI = GameObject.Find("Canvas/PageView").GetComponent<PageView>();

        // Determine whether the button belongs to a menu cell or the crowd control
        if (GetComponentsInChildren<Transform>(true).Length <= 1) text = "crowdctrl";
        else _hasTxt = true;

        // Get the button and add listener, the listener will call the selected function when the button is clicked.
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// Get the name tag in the parent menu cell and sent it to the UI button event handler.
    /// </summary>
    public void OnClick()
    {
        if (_hasTxt) text = GetComponentInParent<CellPref>().callname;
        _UI.StringEventListener(text);
    }
}
