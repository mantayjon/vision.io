using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class IndustrySectorController : MonoBehaviour
{
    public MQTTCommunication mqtt;
    public HoloDebugger debug;
    public GameObject oldIndustryObject;
    public GameObject newIndustryObject;

    private GameObject industrySector;
    public GameObject industrytext;

    public Material oldIndustryText;
    public Material newIndustryText;

    bool isActive = false;

    void OnEnable()
    {
        mqtt.OnButtonStateChanged += HandleButtonStateChanged;
    }

    private void OnDisable()
    {
        mqtt.OnButtonStateChanged -= HandleButtonStateChanged;
    }

    void Start()
    {
        industrySector = GameObject.Find("IndustryBlock Button");
    }

    public bool GetButtonState()
    {
        return isActive;
    }

    void HandleButtonStateChanged(bool newState)
    {
        isActive = newState;
        oldIndustryObject.SetActive(!isActive);
        newIndustryObject.SetActive(isActive);
        ChangeMaterial();
    }

    public void ChangeMaterial()
    {
        if (industrytext != null)
        {
            Renderer textRenderer = industrytext.GetComponent<Renderer>();

            if (textRenderer != null)
            {   
                if(isActive) textRenderer.material = newIndustryText;
                else textRenderer.material = oldIndustryText;
            }
        }
       
    }
}
