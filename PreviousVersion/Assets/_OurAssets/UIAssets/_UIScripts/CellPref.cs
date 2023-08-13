using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CellPref : MonoBehaviour
{
    public string buttonText;

    public string infoText;

    private TextMeshProUGUI ButtonTextM;
    private TextMeshProUGUI InfoTextM;

    public void SetButtonText(string text)
    {
        ButtonTextM.SetText(text);
    }

    public void SetInfo(string text)
    {
        InfoTextM.SetText(text);
    }

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        ButtonTextM = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        InfoTextM = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        SetButtonText(buttonText);
        SetInfo(infoText);
    }
}
