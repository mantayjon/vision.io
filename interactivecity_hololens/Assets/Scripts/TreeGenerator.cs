using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    public List<GameObject> objects;
    public float sliderPercentage;
    public bool industryIsOnGreenState;
    public float maximumSize = 1.0f;
    private float oldSliderPercentage = 0;
    private List<Vector3> objectSizes;
    private EnergySectorController energySector;
    private IndustrySectorController industrySector;
    private HoloDebugger debug;

    void Start()
    {
        if ((objects.Count > 0))
        {
            objectSizes = new List<Vector3>();
            for(int i = 0; i < objects.Count; i++)
            {
                objectSizes.Add(objects[i].transform.localScale);
            }
        }
    }

    void Update()
    {
        if (energySector == null || industrySector == null || debug == null || objectSizes == null)
        {
            if (GameObject.FindWithTag("CityTemplate"))
            {
                debug = GameObject.FindWithTag("CityTemplate").GetComponent<HoloDebugger>();
                energySector = GameObject.FindWithTag("CityTemplate").GetComponent<EnergySectorController>();
                industrySector = GameObject.FindWithTag("CityTemplate").GetComponent<IndustrySectorController>();

                objectSizes = new List<Vector3>();
                for (int i = 0; i < objects.Count; i++)
                {
                    objectSizes.Add(objects[i].transform.localScale);
                }
            }
        }
        else
        {
            if (shouldResize())
            {
                ResizeObjects();
            }
        }
    }

    bool shouldResize()
    {
        sliderPercentage = (float)energySector.GetSliderValue();
        if (sliderPercentage < 0.1f) sliderPercentage = 0.0f;
        if (sliderPercentage > 0.9f) sliderPercentage = 1.0f;
        oldSliderPercentage = sliderPercentage;
        industryIsOnGreenState = industrySector.GetButtonState();
        return true;
    }

    void ResizeObjects()
    {
        adjustMaxSize();
        float threshold = sliderPercentage * objects.Count;
        for(int i = 0; i < objects.Count; i++)
        {
            if (i < Mathf.Floor(threshold))
            {
                objects[i].SetActive(true);
                objects[i].transform.localScale = objectSizes[i] * maximumSize;

            }
            else if (i < Mathf.Ceil(threshold))
            {
                objects[i].SetActive(true);
                float scalar = (threshold - i) * 0.5f + 0.5f;
                objects[i].transform.localScale = objectSizes[i] * maximumSize * scalar;

            }
            else if (i > threshold)
            {
                objects[i].SetActive(false);
            }
        }
    }
    void adjustMaxSize()
    {
        if (industryIsOnGreenState)
        {
            maximumSize = 1.2f;
        }
        else
        {
            maximumSize = 1.0f;
        }
    }
}