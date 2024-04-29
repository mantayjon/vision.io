using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnergySectorController : MonoBehaviour
{

    public MQTTCommunication mqtt;
    public HoloDebugger debug;

    public GameObject energyText;

    public Material oldEnergyText;
    public Material newEnergyText;


    public List<GameObject> pollutingObjects;
    public List<GameObject> renewableObjects;
    public List<Vector3> pollutingObjectSizes;
    public List<Vector3> renewableObjectSizes;

    double sliderValue = 0.0;
    double normalizedSliderValue = 0.0;
    double oldState = 0.0;
    float rotationSpeed = 10f;
    bool isActive = true;


    void OnEnable()
    {
        mqtt.OnSliderValueChanged += HandleSliderValueChanged;
    }

    private void OnDisable()
    {
        mqtt.OnSliderValueChanged -= HandleSliderValueChanged;
    }


    void Start()
    {
        for (int i = 0; i < pollutingObjects.Count; i++)
        {
            pollutingObjects[i].SetActive(false);
        }
        pollutingObjectSizes = new List<Vector3>();
        renewableObjectSizes = new List<Vector3>();
        for (int i = 0; i < pollutingObjects.Count; i++)
        {
            pollutingObjectSizes.Add(pollutingObjects[i].transform.localScale);
            renewableObjectSizes.Add(renewableObjects[i].transform.localScale);

        }

    }
    
    void HandleSliderValueChanged(string value)
    {
        if (!value.Equals(""))
        {
            if (double.TryParse(value, out sliderValue))
            {
                normalizedSliderValue = sliderValue / 900.0;
            }
        }
    }

    void Update()
    {
        RotateWindmills();
        if (oldState != normalizedSliderValue)
        {
            if (normalizedSliderValue < 0.5) ChangeMaterial(oldEnergyText);
            else ChangeMaterial(newEnergyText);
            changeEnergyLevel();
            oldState = normalizedSliderValue;
        }
    }

    public void ChangeMaterial(Material material)
    {
        if (energyText != null)
        {
            Renderer textRenderer = energyText.GetComponent<Renderer>();

            if (textRenderer != null)
            {
               
               textRenderer.material = material;
            }
        }

    }
    void RotateWindmills()
    {
        foreach (GameObject windmill in renewableObjects)
        {
            Transform rotaryParent = windmill.transform.Find("Rotary_parent");
            if (rotaryParent != null)
            {
                rotaryParent.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed);
            }
        }
    }

    public double GetSliderValue()
    {
        return normalizedSliderValue;
    }

    void changeEnergyLevel()
    {
        float percentage = (float)normalizedSliderValue;
        if (percentage <= 0.15) { percentage = 0; }
        else if (percentage >= 0.85) { percentage = 1; };
        float threshold = percentage * pollutingObjects.Count;
        for (int i = 0; i < renewableObjects.Count; i++)
        {
            if (i < Mathf.Floor(threshold))
            {
                renewableObjects[i].SetActive(true);
                renewableObjects[i].transform.localScale = renewableObjectSizes[i];
                pollutingObjects[i].SetActive(false);

            }
            else if (i < Mathf.Ceil(threshold))
            {
                pollutingObjects[i].SetActive(true);
                float scalar = (threshold - i) * 0.5f + 0.5f;
                float half = 1.0f - scalar;
                pollutingObjects[i].transform.localScale = pollutingObjectSizes[i] * half;
                renewableObjects[i].transform.localScale = renewableObjectSizes[i] * scalar;

            }
            else if (i > threshold)
            {
                pollutingObjects[i].SetActive(true);
                pollutingObjects[i].transform.localScale = pollutingObjectSizes[i];
                renewableObjects[i].transform.localScale = renewableObjectSizes[i];
                renewableObjects[i].SetActive(false);
            }
        }
    }


}
