# Arduino Setup

Group B7 - interactive city

## Requirements

The following packages must be installed for development and modification:

- ArduinoMqttClient by Arduino
- WiFiNINA by Arduino
- ArduinoJson by Benoit Blanchon
- Adafruit PN532 by Adafruit

To modify settings regarding the pin layout, Wi-Fi and MQTT settings, it's best to use the Arduino IDE.

## Pin Layout

### Button

| Button | Arduino |
| ------ | ------- |
| 5V     | 5V      |
| GND    | GND     |
| Out    | 4       |

### Potentiometer

| Potentiometer | Arduino |
| ------------- | ------- |
| Right-pin     | 5V      |
| Middle-Pin    | A0      |
| Left-pin      | GND     |

### NFC-Reader

| NFC  | Arduino |
| ---- | ------- |
| VCC  | 5V      |
| GND  | GND     |
| SDA  | SDA     |
| SCL  | SCL     |
| IRQ  | 2       |
| RSTC | 3       |

### Wi-Fi Status LED

| LED | Arduino     |
| --- | ----------- |
| GND | Digital GND |
| R1  | 11          |
| R2  | 10          |

## Wi-Fi Settings

The network to be used can be added in the "wifi_secrets.h" file. To do this, the data for the network (SSID and password) must be entered in the corresponding fields. Please ensure that this network is also the corresponding network in which the MQTT broker is running, as this is the only way to establish a connection to the broker.

```
#define SECRET_SSID "Add Wifi SSID here"
#define SECRET_PASS "Add Wifi Password here"
```

## MQTT Broker Settings

The MQTT broker data can be changed in the "broker_conf.h" file. You can get the data for the MQTT broker on the device on which the broker is running. The broker IP is usually the IP of the host and the port is 1883 by default.

```
#define BROKER_URL "Add Broker IP Here"
#define BROKER_PORT Add Broker Port here (No Quotation marks!)
#define BROKER_TOPIC "Add Broker Topic here"
```

**Notice:** Please make sure that the given data is correct to ensure a smooth functioning of the system. The use of the Arduino IDE is recommended for development.
