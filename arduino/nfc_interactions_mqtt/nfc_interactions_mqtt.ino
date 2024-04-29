#include <ArduinoMqttClient.h>
#include <WiFiNINA.h>
#include <ArduinoJson.h>
#include <Wire.h>
#include <SPI.h>
#include <Adafruit_PN532.h>
#include "wifi_secrets.h"
#include "broker_conf.h"
#include "client_conf.h"

///////please enter your sensitive data in the Secret tab/arduino_secrets.h
char ssid[] = SECRET_SSID;  // your network SSID (name)
char pass[] = SECRET_PASS;  // your network password (use for WPA, or use as key for WEP)

WiFiClient wifiClient;
MqttClient mqttClient(wifiClient);

// MQTT Broker Setup
const char broker[] = BROKER_URL;
int port = BROKER_PORT;
const char topic[] = BROKER_TOPIC;

// LED Setup
int redpin = 11;
int greenpin = 10;

// NFC Setup
#define PN532_IRQ (2)
#define PN532_RESET (3)

bool nfcAvailable = false;
bool hasNoNfcCard = true;
unsigned long previousMillis = 0;
const long interval = 2000;

uint8_t lastCard[] = { 0, 0, 0, 0, 0, 0, 0 };

// Or use this line for a breakout or shield with an I2C connection:
Adafruit_PN532 nfc(PN532_IRQ, PN532_RESET);

// JSON buffer size
const size_t bufferSize = JSON_OBJECT_SIZE(3);

// Create a JSON buffer
StaticJsonDocument<bufferSize> jsonDocument;
JsonObject jsonObject = jsonDocument.to<JsonObject>();

void setup() {
  Serial.begin(115200);
  while (!Serial) {
    ;  // wait for serial port to connect. Needed for native USB port only
  }

  setup_wifi();
  setup_mqtt();
  setup_nfc();
  pinMode(redpin, OUTPUT);
	pinMode(greenpin, OUTPUT);
}

void loop() {
  unsigned long currentMillis = millis();
  if (currentMillis - previousMillis >= interval) {
    previousMillis = currentMillis;
    do_nfc_function();
  }
}

void setup_wifi() {
  // attempt to connect to WiFi network:
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

  // You can provide a unique client ID, if not set the library uses Arduino-millis()
  // Each client must have a unique client ID
  mqttClient.setId(CLIENT_ID);

  // You can provide a username and password for authentication
  // mqttClient.setUsernamePassword(CLIENT_USERNAME, CLIENT_PASSWORD);

  Serial.print("Attempting to connect to the MQTT broker: ");
  Serial.println(broker);

  while (!mqttClient.connect(broker, port)) {
    Serial.print("MQTT connection failed! Error code = ");
    Serial.println(mqttClient.connectError());
    delay(2000);
  }

  Serial.println("You're connected to the MQTT broker!");
}

void setup_nfc() {
  nfc.begin();

  uint32_t versiondata = nfc.getFirmwareVersion();
  if (!versiondata) {
    Serial.print("Didn't find PN53x board");
  } else {
    nfcAvailable = true;
    // Got ok data, print it out!
    Serial.print("Found chip PN5");
    Serial.println((versiondata >> 24) & 0xFF, HEX);
    // Serial.print("Firmware ver. "); Serial.print((versiondata>>16) & 0xFF, DEC);
    // Serial.print('.'); Serial.println((versiondata>>8) & 0xFF, DEC);

    Serial.println("Waiting for an ISO14443A Card ...");
  }
}

void do_nfc_function() {
  if (nfcAvailable) {
    uint8_t success;
    uint8_t uid[] = { 0, 0, 0, 0, 0, 0, 0 };  // Buffer to store the returned UID
    uint8_t uidLength;                        // Length of the UID (4 or 7 bytes depending on ISO14443A card type)
    success = nfc.readPassiveTargetID(PN532_MIFARE_ISO14443A, uid, &uidLength, 50);

    if (success) {
      /* for(int i = 0; i < sizeof(uid); i++){
        if (uid[i]<0x10) {Serial.print("0");}
        Serial.print(uid[i], HEX);
        Serial.print(":");
      } */
      if (memcmp(uid, lastCard, sizeof(uid))) {
        String uidString = byteArrayToString(uid, sizeof(uid));
        Serial.println("Card found");
        send_mqtt_message(3, uidString);
        memcpy(lastCard, uid, sizeof(uid));
      }
      hasNoNfcCard = false;
    } else {
      if (!hasNoNfcCard) {
        Serial.println("No Card found");
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
    // Convert each byte to a 2-digit hexadecimal string
    char hexString[3];
    sprintf(hexString, "%02X", byteArray[i]);

    // Append the hexadecimal string to the result
    result += hexString;
  }

  return result;
}

void send_mqtt_message(int id, String data) {
  Serial.println("Im alive!");

  jsonObject["id"] = id;
  jsonObject["timestamp"] = millis();
  jsonObject["data"] = data.c_str();
  // Convert JSON to a string
  String jsonString;
  serializeJson(jsonDocument, jsonString);

  checkConnection();

  if (mqttClient.connected()) {
    if (mqttClient.beginMessage(topic)) {
      mqttClient.print(jsonString.c_str());
      if (mqttClient.endMessage()) {
        Serial.println("MQTT message sent successfully");
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
