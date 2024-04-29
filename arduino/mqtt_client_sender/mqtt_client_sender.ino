#include <ArduinoMqttClient.h>
#include <WiFiNINA.h>
#include <ArduinoJson.h>
#include <Wire.h>
#include <SPI.h>
#include <Adafruit_PN532.h>
#include "wifi_secrets.h"
#include "broker_conf.h"
#include "client_conf.h"

char ssid[] = SECRET_SSID;
char pass[] = SECRET_PASS;

WiFiClient wifiClient;
MqttClient mqttClient(wifiClient);

// MQTT Broker Setup
const char broker[] = BROKER_URL;
int port = BROKER_PORT;
const char topic[] = BROKER_TOPIC;

// Wi-Fi Status LED Setup
int redpin = 11;
int greenpin = 10;

// Button Setup
const int buttonPin = 4;
bool doButtonTask = false;

// Potenzimeter Setup
const int potPin = A0;
int oldPotValue;
const int hysteresisThreshold = 25;

// NFC Setup
#define PN532_IRQ (2)
#define PN532_RESET (3)

bool nfcAvailable = false;
bool hasNoNfcCard = true;
unsigned long previousMillis = 0;
const long interval = 5000;

uint8_t lastCard[] = { 0, 0, 0, 0, 0, 0, 0 };

Adafruit_PN532 nfc(PN532_IRQ, PN532_RESET);

// Create a JSON buffer
const size_t bufferSize = JSON_OBJECT_SIZE(3);
StaticJsonDocument<bufferSize> jsonDocument;
JsonObject jsonObject = jsonDocument.to<JsonObject>();

void setup() {
  Serial.begin(115200);
  while (!Serial) {
    ;  // wait for serial port to connect. Needed for native USB port only
  }

  setup_wifi();
  setup_mqtt();
  setup_button();
  setup_nfc();
  pinMode(redpin, OUTPUT);
  pinMode(greenpin, OUTPUT);
}

void loop() {
  unsigned long currentMillis = millis();
  do_poti_function();
  do_button_function();
  if (currentMillis - previousMillis >= interval) {
    previousMillis = currentMillis;
    do_nfc_function();
  }
}

void setup_wifi() {
  Serial.print("Attempting to connect to WPA SSID: ");
  Serial.println(ssid);
  analogWrite(redpin, 255);
  analogWrite(greenpin, 0);
  while (WiFi.begin(ssid, pass) != WL_CONNECTED) {
    // failed, retry
    Serial.print(".");
    delay(5000);
  }
  analogWrite(redpin, 0);
  analogWrite(greenpin, 255);
  Serial.println("You're connected to the network");
}

void setup_mqtt() {
  mqttClient.setId(CLIENT_ID);
  Serial.print("Attempting to connect to the MQTT broker: ");
  Serial.println(broker);

  while (!mqttClient.connect(broker, port)) {
    Serial.print("MQTT connection failed! Error code = ");
    Serial.println(mqttClient.connectError());
    delay(2000);
  }
  Serial.println("You're connected to the MQTT broker!");
}

void setup_button() {
  pinMode(buttonPin, INPUT);
  attachInterrupt(digitalPinToInterrupt(buttonPin), doButtonInterrupt, FALLING);
}

void setup_nfc() {
  nfc.begin();

  uint32_t versiondata = nfc.getFirmwareVersion();
  if (!versiondata) {
    Serial.print("Didn't find PN53x board");
  } else {
    nfcAvailable = true;
  }
}

void doButtonInterrupt() {
  doButtonTask = true;
}

void do_button_function() {
  if (doButtonTask) {
    send_mqtt_message(1, "");
    doButtonTask = false;
  }
}

void do_poti_function() {
  int sensorValue = analogRead(potPin);
  if (abs(sensorValue - oldPotValue) > hysteresisThreshold) {
    oldPotValue = sensorValue;
    send_mqtt_message(2, String(sensorValue));
  }
}

void do_nfc_function() {
  if (nfcAvailable) {
    uint8_t success;
    uint8_t uid[] = { 0, 0, 0, 0, 0, 0, 0 };
    uint8_t uidLength;
    success = nfc.readPassiveTargetID(PN532_MIFARE_ISO14443A, uid, &uidLength, 50);

    if (success) {
      if (memcmp(uid, lastCard, sizeof(uid))) {
        String uidString = byteArrayToString(uid, sizeof(uid));
        send_mqtt_message(3, uidString);
        memcpy(lastCard, uid, sizeof(uid));
      }
      hasNoNfcCard = false;
    } else {
      if (!hasNoNfcCard) {
        send_mqtt_message(3, "");
        hasNoNfcCard = true;
        memcpy(lastCard, uid, sizeof(uid));
      }
    }
  }
}

String byteArrayToString(uint8_t* byteArray, size_t size) {
  String result = "";

  for (size_t i = 0; i < size; i++) {
    char hexString[3];
    sprintf(hexString, "%02X", byteArray[i]);

    result += hexString;
  }

  return result;
}

void send_mqtt_message(int id, String data) {
  jsonObject["id"] = id;
  jsonObject["timestamp"] = millis();
  jsonObject["data"] = data.c_str();

  String jsonString;
  serializeJson(jsonDocument, jsonString);

  checkConnection();

  if (mqttClient.connected()) {
    if (mqttClient.beginMessage(topic)) {
      mqttClient.print(jsonString.c_str());
      if (mqttClient.endMessage()) {
      } else {
        Serial.print("Error in MQTT endMessage: ");
        Serial.println(mqttClient.connectError());
      }
    } else {
      Serial.print("Error in MQTT beginMessage: ");
      Serial.println(mqttClient.connectError());
    }
  } else {
    Serial.println("error");
    checkConnection();
    send_mqtt_message(id, data);
  }
}

void checkConnection() {
  if (WiFi.status() != WL_CONNECTED) {
    setup_wifi();
  }
  if (!mqttClient.connected()) {
    setup_mqtt();
  }
}
