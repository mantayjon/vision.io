# RaspberryPiSetupWithMQTT

## 1. Startup Raspberry Pi 3 
Install the latest Debian Raspberry Pi Os Distribution.
Once installed, connect to the internet.

Execute the following commands:
````
sudo apt-get update
sudo apt-get upgrade
sudo apt install -y mosquitto mosquitto-clients
sudo apt install network-manager 
````

Internet Connection is no longer necessary after this step.

## 2. Starting up the Wifi Hotspot

### 2.1 Switching to Network-Manager

We use the "Network-Manager" service for our Wifi Hotspot.
Since Raspberry Pi Os 2023-12-05 uses the "dhcpcd" service by default, we need to switch and clean up.

To do this, enter:
```
sudo raspi-config
```
1. Go to _6. Advanced Options_.
2. Then go to _AA Network Config_.
3. Choose _2. NetworkManager_ and confirm.
4. Click finish and reboot.

After that, we can clean up everything by removing the now no longer needed components:
```
apt purge openresolv dhcpcd5
```

### 2.2 Setting up the WiFi-Hotspot via GUI

1. Click on the WiFi symbol in the upper right corner.
2. Then click on "Advanced Options."
3. "Create a WIFI Hotspot"
4. Choose the appropriate SSID.

    ```
    Network name: HOLOHUB
    Wi-Fi Security: WPA & WPA2 Personal
    Password: b7ws2324
    ```
This generates a good config basis. It can be found using:

```
sudo nano /etc/NetworkManager/system-connections/HoloHub.nmconnection
```
Additional settings can be made here. For the local WiFi Hotspot, this should not be necessary for now.

To make the WiFi Hotspot start automatically, do the following:

1. Click on the WiFi symbol in the upper right corner.
2. Then click on "Advanced Options."
3. "Edit Connections..."
4. In the new window, go to the "General" tab.
5. Select the option "Connect Automatically with priority."

Later, the Mosquitto Broker will be reachable by default at the IP address 10.42.0.1.

## 3. Starting up MQTT 

MQTT is the protocol that creates a broker on which messages can be sent and retrieved under various topics. Topics can be arbitrarily defined by subscribers and clients and do not need to be predefined by the MQTT server or MQTT broker.

Mosquitto has a default configuration under Linux, located at "/etc/mosquitto/mosquitto.conf." In this default configuration, there is an include statement pointing to additional files that are loaded as configuration. These additional configs are located in the "/etc/mosquitto/conf.d/local.conf" folder. Further information can be found in the [Manpage](https://mosquitto.org/man/mosquitto-conf-5.html), under include_dir.

In this folder, we create our broker config:
```
sudo nano /etc/mosquitto/conf.d/local.conf
```
In the config file, we add 2 lines:
```
listener 1883
allow_anonymous true
```
We save using `CTRL+O` and exit the config with `CTRL+X`.

### 3.1 Testing if the MQTT Broker is functioning.

For testing, use:
```
mosquitto -v -c /etc/mosquitto/conf.d/local.conf
```
By starting in verbose mode, we can precisely observe what happens in the broker.

Now open 2 additional terminal tabs and type:
```
mosquitto_sub -t test
```
In the other tab, type:
```
mosquitto_pub -t test -m "Hallo, Welt!"
```

You can also log into HoloHub from another computer. After that, you should be able to send messages using the following command:

```
mosquitto_pub -h 10.42.0.1 -p 1883 -t "test" -m "Hallo, Welt!"
```
Keep in mind that you will now only listen to messages on the "test" topic.

### 3.2 Setting up Autostart

To automatically start the broker at system startup, use:

```
sudo systemctl enable mosquitto
sudo systemctl start mosquitto
```

**Optional**: 
To start Mosquitto with additional optional flags, you can modify this file:
```
sudo nano /lib/systemd/system/mosquitto.service
````
Afterwards, use this to make sure changes are applied:
```
sudo systemctl daemon-reload
```
Now the mqtt broker should function as soon as it is connected to power.
As soon as the HoloLens 2 and Arduino are connected to the HoloHub Wifi, communication will function.

### 4.0 Troubleshooting

If the broker will only start in local mode, there must be an issue with the config. Make sure to kill any instances of the broker running, and run the broker with the -c <config-path> parameter.
