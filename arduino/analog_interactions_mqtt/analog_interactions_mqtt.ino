#include <ArduinoMqttClient.h>
#include <WiFiNINA.h>
#include <ArduinoJson.h>
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

// Button Setup
const int buttonPin = 4;  // the number of the pushbutton pin
bool doButtonTask = false;

// Potenzimeter Setup
const int potPin = A0;
int oldPotValue;
const int hysteresisThreshold = 25;

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
  setup_button();
  pinMode(redpin, OUTPUT);
	pinMode(greenpin, OUTPUT);
}

void loop() {
  do_poti_function();
  do_button_function();
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
    delay(2500);
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

void setup_button() {
  pinMode(buttonPin, INPUT);
  attachInterrupt(digitalPinToInterrupt(buttonPin), doButtonInterrupt, FALLING);
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
  // print out the value you read:
  if (abs(sensorValue - oldPotValue) > hysteresisThreshold) {
    oldPotValue = sensorValue;  // save the changed value
    send_mqtt_message(2, String(sensorValue));
  }
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
