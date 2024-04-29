using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class HouseSectorController : MonoBehaviour
{
    public MQTTCommunication mqtt;
    public HoloDebugger debug;

    public GameObject oldHouseObject;
    public GameObject newHouseObject;
    public GameObject exclamationMarkObject;
    public GameObject nfcHouse;

    public List<GameObject> otherOldHouses;
    public List<GameObject> otherNewHouses;

    public GameObject houseText;

    public Material oldHouseText;
    public Material newHouseText;

    private bool nfcIdChanged = false;
    private bool CR_running = false;

    private string nfcId;

    private const string exclamationMarkId = "";
    private const string oldHouseId = "E3689115000000";
    private const string newHouseId = "235E970E000000";


    public float exclamationMarkOffset = 0.1f;

    Vector3 exMarkStartPos;
    Vector3 exMarkPos;
    float exclamationAmplitude = 0.1f;


    private void OnEnable()
    {
        if (mqtt != null)
        {
            mqtt.OnNfcIdChanged += HandleNfcIdChanged;
        }
    }
    private void OnDisable()
    {
        if (mqtt != null)
        {
            mqtt.OnNfcIdChanged -= HandleNfcIdChanged;
        }
    }

    private void HandleNfcIdChanged(string newId)
    {

        nfcId = newId;


        ChangePrefab();
        ChangeTextMaterial();
    }

    void Start()
    {
        oldHouseObject.SetActive(false);
        newHouseObject.SetActive(false);
        exclamationMarkObject.SetActive(true);

        exMarkStartPos = exclamationMarkObject.transform.localPosition;
    }

   
    void Update()
    {
        if(nfcId == exclamationMarkId)
        {
            animateExclamationMark();
        }
    }

    IEnumerator ChangeOtherHousesToNew()
    {
        for (int i = 0; i < otherNewHouses.Count; i++)
        {
            otherNewHouses[i].SetActive(true);
            otherOldHouses[i].SetActive(false);
            yield return new WaitForSeconds(0.3f);
        }
    }

    void ChangeOtherHousesToOld()
    {
        for(int i = 0; i < otherNewHouses.Count; i++)
        {
            otherNewHouses[i].SetActive(false);
            otherOldHouses[i].SetActive(true);
        }
        
    }

    private void animateExclamationMark()
    {
        exMarkPos = exMarkStartPos;
        exMarkPos.y = exMarkStartPos.y + exclamationAmplitude * Mathf.Sin(Time.time);
        exclamationMarkObject.transform.localPosition = exMarkPos;
    }

    void ChangePrefab()
    {
        exMarkPos = exMarkStartPos;
        switch (nfcId)
        {
            case exclamationMarkId:
                oldHouseObject.SetActive(false);
                newHouseObject.SetActive(false);
                exclamationMarkObject.SetActive(true);
                nfcHouse.SetActive(false);
                break;

            case oldHouseId:
                oldHouseObject.SetActive(true);
                newHouseObject.SetActive(false);
                exclamationMarkObject.SetActive(false);
                nfcHouse.SetActive(true);
                if (CR_running)
                {
                    StopCoroutine(ChangeOtherHousesToNew());
                }
                ChangeOtherHousesToOld();
                break;

            case newHouseId:
                oldHouseObject.SetActive(false);
                newHouseObject.SetActive(true);
                exclamationMarkObject.SetActive(false);
                nfcHouse.SetActive(true);
                StartCoroutine(ChangeOtherHousesToNew());
                break;
        }

    }

    private void ChangeTextMaterial()
    {
        Renderer textRenderer = houseText.GetComponent<Renderer>();

        if (textRenderer != null)
        {
            switch (nfcId)
            {
                case oldHouseId:
                    textRenderer.material = oldHouseText;
                    break;
                case newHouseId:
                    textRenderer.material = newHouseText;
                    break;

            }
        }
    }

}
