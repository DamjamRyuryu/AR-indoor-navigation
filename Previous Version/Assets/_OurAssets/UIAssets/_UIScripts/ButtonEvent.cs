using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ScrollList.Basic;
using UserI;

public class ButtonEvent : MonoBehaviour
{
    private Button _btn;
    private PageView _UI;
    private string text = "NULL";
    private bool _hasTxt = false;
    // Start is called before the first frame update
    void Awake()
    {
        _UI = GameObject.Find("Canvas/PageView").GetComponent<PageView>();

        if (GetComponentsInChildren<Transform>(true).Length <= 1) text = "crowdctrl";
        else _hasTxt = true;

        _btn = GetComponent<Button>();

        _btn.onClick.AddListener(OnClick);
    }
    public void OnClick()
    {
        if (_hasTxt) text = transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
        _UI.StringEventListener(text);
    }
}
