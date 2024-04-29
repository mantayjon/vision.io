using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HoloDebugger : MonoBehaviour
{
    public TextMeshPro debugText;

    void Start()
    {
        GameObject debugTextObject = GameObject.Find("debugText");

        if (debugTextObject != null)
        {
            debugText = debugTextObject.GetComponent<TextMeshPro>();
            
        }
    }

    public void LogMessage(string message)
    {
        debugText.text += message + "\n";
    }
}
