using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Store the basic functions of the cell unit. Provide easy access for the main UI to edit the children of the cell
/// </summary>
public class CellPref : MonoBehaviour
{
    public string buttonText;   // Text on button, replaced by image now
    public string callname;     // Name tag of the cell
    public string infoText;     // Text shown in info text bar

    // Corresponding component
    private TextMeshProUGUI _buttonTextM;
    private TextMeshProUGUI _infoTextM;
    private RawImage _buttonImage;

    /// <summary>
    /// Set the text of the button with the given string. Replaced by image display now.
    /// </summary>
    /// <param name="text"> The text to be shown in the button. </param>
    public void SetButtonText(string text)
    {
        // The text component is set to be disabled as default, so it needs to be enabled again.
        _buttonTextM.enabled = true;
        _buttonTextM.SetText(text);
    }

    /// <summary>
    /// Set the text on the info bar (right side of the button) with the given string
    /// </summary>
    /// <param name="text"> The text to be shown in the info bar. </param>
    public void SetInfo(string text)
    {
        _infoTextM.SetText(text);
    }

    /// <summary>
    /// Create the image with a base64 string and display it in the button.
    /// </summary>
    /// <param name="imagestr"> A base64 string representing the image </param>
    public void SetButtonImage(string imagestr)
    {
        // Make sure that the text component is disabled.
        if (imagestr.Length != 0) _buttonTextM.enabled = false;

        // Decode the string and then display it.
        _buttonImage.texture = GetTextureByString(imagestr);
    }

    // Awake is called when the object is created or activated, which is prior to Start()
    protected virtual void Awake()
    {
        // Complete the component finding.
        _buttonTextM = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        _infoTextM = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        _buttonImage = transform.GetChild(0).GetComponent<RawImage>();
        SetButtonText(buttonText);
        SetInfo(infoText);
    }

    /// <summary>
    /// Decode the base64 string and reconstruct the image as a 2D texture.
    /// </summary>
    /// <param name="texturestr"> A base64 string representing the image </param>
    /// <returns> Decoded 2D texture </returns>
    private Texture2D GetTextureByString(string texturestr)
    {
        // Decode the string into a byte array.
        Texture2D _tex = new Texture2D(1, 1);
        byte[] _arr = Convert.FromBase64String(texturestr);

        // Create the texture
        _tex.LoadImage(_arr);
        _tex.Apply();
        return _tex;
    }
}
