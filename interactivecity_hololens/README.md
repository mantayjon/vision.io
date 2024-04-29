# Unity Project Setup Guide

## Installation

1. **Unity Hub and Unity Installation:**
   - Begin by installing Unity Hub on a Windows PC running Windows 10 or 11. Unity Hub facilitates the management of various Unity versions. After installing Unity Hub, proceed to install Unity version 2022.3.13f1.

2. **Visual Studio and Required Components Installation:**
   - Visual Studio 2022 is required for development in Unity. During the setup of Visual Studio, certain components need to be selected and installed via the Visual Studio Installer. These include:
     - .NET Desktop Development
     - Desktop Development with C++
     - Game Development with Unity
     - Development for the Universal Windows Platform (UWP)
   - Additionally, the following components are necessary for installation:
     - Windows 10 SDK Version 10.0.19041.0 or 10.0.18362.0, or Windows 11 SDK
     - USB device connectivity (required for deployment/debugging on the HoloLens via USB)
     - C++ (v142) Universal Windows Platform Tools (required when using Unity)
   - A detailed installation guide can also be found on the official Microsoft documentation [here](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/install-the-tools).

## Configuration

1. **Unity and Visual Studio Configuration:**
   - After installation, open the project in Unity and change the platform to Universal Windows Platform in the build settings. Then, proceed to build the project into a designated folder.

2. **Visual Studio Build Configuration:**
   - Build configurations in Visual Studio need to be set to ARM or ARM64, Release, and Device. If the HoloLens 2 is connected to the computer via a cable, the application can be built using the start button.

## HoloLens 2 Setup

1. **Enabling Developer Mode on the HoloLens 2:**
   - Before deploying the project on the HoloLens 2, developer mode must be activated on the device. Navigate to "Update & Security" in the system settings and enable developer features.

For further assistance or troubleshooting, refer to the provided resources or contact support.

---

*Note: Additional configuration settings, such as IP address configuration and NFC chip integration, may be required based on project specifications.*
