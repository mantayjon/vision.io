using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;

using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine;
using UnityEngine.UI;

public class TextBoardController : MonoBehaviour
{
    public HoloDebugger debug;
    public MQTTCommunication mqtt;

    public GameObject plane;

    public Material teamText;
    public Material communicationText;
    public Material interactionText;
    public Material logoText;


    public void teamButtonPressed()
    {
        ChangeMaterial(teamText);
    }

    public void communicationButtonPressed()
    {  
        ChangeMaterial(communicationText);
    }

    public void interactionButtonPressed()
    {
        ChangeMaterial(interactionText);
    }

    public void resetButtonPressed()
    {
        ChangeMaterial(logoText);
        mqtt.SetButtonState(false);
    }


    public void ChangeMaterial(Material colorText)
    {
        if (plane != null)
        {
           
            Renderer planeRenderer = plane.GetComponent<Renderer>();

            if ( planeRenderer != null)
            { 
                planeRenderer.material = colorText;
            }
        }
    }
}
