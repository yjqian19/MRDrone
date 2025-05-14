# MRDrone: Spatial Instructions Using Object as a Reference

## Overview
MRDrone is a Mixed Reality application where users control a virtual drone through spatial instructions, leveraging real-world objects as reference points. This project was developed as the term project for MIT 6.8510: Multimodal User Interface.

- **Demo Video**: [YouTube Demo](https://youtu.be/vn8-CInzByg)
- **Project Report**: [Google Docs Report](https://docs.google.com/document/d/1BuWmL3Te1N_GTQz21ap-sFwEYuVOMiMqNWNhw17zYbE/edit?usp=sharing)
- **Final APK**: Find the installable version at `BuildSDKs/v1.1.apk` which can be directly installed on Meta Quest devices through the Meta Developer Center.

## Assets Directory
This section focuses on the files within the `Assets` directory, which are central to the application's functionality. Other folders contain files necessary for Unity's operation and project structure. Key components include:

#### 1. Drone
Contains drone models, animations, and control scripts specific to the drone functionality. This includes drone behavior states, movement patterns, and interaction capabilities.

#### 2. Models
3D models used in the application, including environmental objects, reference objects, and UI elements that support the spatial instruction system.

#### 3. Scripts
Core C# scripts that power the application:

- **APIConfig.cs**: Scriptable object that stores and manages the OpenAI API configuration, including the API key required for service access.

- **OpenAIRequest.cs**: Handles API communication with OpenAI services. Processes natural language instructions and translates them into drone commands using the OpenAI API integration.

- **SpeechToText.cs**: Implements speech recognition functionality, converting user's verbal commands into text that can be processed by the OpenAI API.

- **SceneInfo.cs**: Gathers and processes information about the scene environment, including real-world object detection and spatial mapping data via Meta MRUtilityKit.

- **UIController.cs**: Manages the user interface elements of the application, including displays, prompts, and visual feedback to user inputs and drone operations.

- **Functions.cs**: Contains core utility functions for the application, including drone movement calculation and spatial positioning relative to reference objects.

- **CubeController.cs**: Manages the behavior and properties of reference cube objects in the environment, which serve as spatial anchors for drone instructions.


#### 4. Scenes
Unity scenes that define the layout and environment of the application. The main scene is `SampleScene.unity`, which contains the drone, reference objects, and UI elements.

## Setup and Running Instructions

### Prerequisites
- Unity 2022.3.50f1
- Meta Quest development setup

### Steps to Run the Project
1. **Clone the Repository**
   - Clone the repository to your local machine using Git or download it as a ZIP file.
   - Extract the contents if downloaded as a ZIP.

2. **Open in Unity**
   - Open Unity Hub
   - Add the project and select Unity version 2022.3.50f1
   - Open the project

3. **Configure OpenAI API Key**
   - In the Assets root directory, locate the `APIConfig` asset
   - Select it in the Inspector window
   - Enter your OpenAI API key in the designated field

4. **Run and Test**
    - Connect your Meta Quest device using Meta Link
    - Ensure your device is in Developer Mode and connected to your PC via USB
    - Click the Play button in Unity to run the application

5. **Deploy to Device**
   - In Unity, go to `File > Build Settings`
   - Select `Android` as the platform and click `Switch Platform`
   - Click `Build and Run` to deploy the application to your Meta Quest device
