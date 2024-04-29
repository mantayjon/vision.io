using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Random = UnityEngine.Random;
using TMPro;

public class MQTTCommunication : MonoBehaviour
{
    MqttClient client;
    public HoloDebugger debug;
    string id;
    string timestamp;
    string data;
    private Queue<Action> mainThreadActions = new Queue<Action>();


    public delegate void SliderValueChangedHandler(string value);
    public delegate void ButtonStateChangedHandler(bool state);
    public delegate void NfcIdChangedHandler(string id);

    public event SliderValueChangedHandler OnSliderValueChanged;
    public event ButtonStateChangedHandler OnButtonStateChanged;
    public event NfcIdChangedHandler OnNfcIdChanged;


    private string sliderValue = "1023";


    private bool buttonState = false;


    private string nfcId = "";

    // Private setter methods to update the values
    private void SetSliderValue(string value)
    {
        sliderValue = value;
        OnSliderValueChanged?.Invoke(sliderValue);
    }

    public void SetButtonState(bool state)
    {
        buttonState = state;
        OnButtonStateChanged?.Invoke(buttonState);
    }

    private void SetNfcId(string id)
    {
        nfcId = id;
        OnNfcIdChanged?.Invoke(nfcId);
    }

    // Public methods to retrieve values (optional)
    public string GetSliderValue()
    {
        return sliderValue;
    }

    public bool GetButtonState()
    {
        return buttonState;
    }

    public string GetNfcId()
    {
        return nfcId;
    }


    void Start()
    {

        debug = gameObject.GetComponentInChildren<HoloDebugger>();

        string brokerAddress = "192.168.178.34";

        int brokerPort = 1883;

        bool isEncrypted = false;

        try
        {
            client = new MqttClient(brokerAddress, brokerPort, isEncrypted, null, null, isEncrypted ? MqttSslProtocols.SSLv3 : MqttSslProtocols.None); // Instantiate MqttClient with the combined broker address
           
            String clientID = "hololensClient" + generateClientID().ToString();
            client.Connect(clientID);
        }
        catch (Exception e)
        {
            debug.LogMessage("b7-debug: ConnectionError:" + e.Message);
        }

        // the MqttMsgPublishReceived is an event of the MqttClient. here we want to add ReactToMessage as
        // an event handler when a message is received

        client.MqttMsgPublishReceived += ReactToMessage;

        // Subscribe to the MQTT topic
        client.Subscribe(new string[] { "arduino/data" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
    }



    private float generateClientID()
    {
        int seed = System.DateTime.Now.Millisecond;
        Random.InitState(seed);
        float randomNumber = Random.Range(0f, 1f);
        return randomNumber;
    }

    void Update()
    {
        // Execute actions queued to run on the main thread
        while (mainThreadActions.Count > 0)
        {
            Action action = null;

            lock (mainThreadActions)
            {
                if (mainThreadActions.Count > 0)
                {
                    action = mainThreadActions.Dequeue();
                }
            }
            action?.Invoke();
        }
    }

    void ReactToMessage(object sender, MqttMsgPublishEventArgs e)
    {
        QueueMainThreadAction(() =>
        {

            string topic = e.Topic;
            string jsonString = System.Text.Encoding.UTF8.GetString(e.Message);
            parseData(jsonString);
            int idN = int.Parse(id);
            switch (idN)
            {
                case 1:
                   
                    HandleButtonInteraction(data);
                    break;
                case 2:
                    
                    HandleRotaryKnobInteraction(data);
                    break;
                case 3:
                   
                    HandleNFCInteraction(data);
                    break;

            }

        });
    }

    private void QueueMainThreadAction(Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    void parseData(string jsonString)
    {

        string[] keyValuePairs = jsonString.Split(',');

        // Extract and display parameter names and values
        int i = 0;
        foreach (string pair in keyValuePairs)
        {
            string[] parts = pair.Split(':');

            if (parts.Length == 2)
            {
                string paramName = parts[0];
                object paramValue = parts[1];
                if (i == 0) { id = (string)paramValue; }
                else if (i == 1) { timestamp = (string)paramValue; }
                else
                {
                    data = (string)paramValue;
                    data = data.Substring(0, data.Length - 1);
                    data = data.Trim('"');
                }
                i++;

            }
        }

    }


   

    void HandleButtonInteraction(string data)
    {
        SetButtonState(!buttonState);
        
    }
    void HandleRotaryKnobInteraction(string data)
    {
        SetSliderValue(data);
      
    }
    void HandleNFCInteraction(string data)
    {
        SetNfcId(data);
    }
}

