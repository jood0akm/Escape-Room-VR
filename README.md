# Escape Room VR

A VR puzzle escape room built in Unity. You are locked in a dark workshop. Complete the circuit on the workbench to open the door and escape.

![cover](cover.png)

## Gameplay

1. Press **NEW GAME** in the main menu.
2. Pick up the **battery**, the **resistor** and the **LED** from the workbench.
3. Place them in the sockets: the **resistor goes in the middle**, the battery and the LED go on the two sides.
4. If all three parts are placed in the wrong order you get a **TRY AGAIN** message and a buzzer sound.
5. Right order: the LED lights up and the metal door slides open. Walk up to the door (or grab it) to see **YOU ESCAPED THIS ROOM**.
6. Need help? Press the **HINT** button behind the table to see the correct circuit.

## Features

- World-space VR main menu, settings (volume, brightness, graphics) and end screen
- Socket-based circuit puzzle with right/wrong detection
- Hint popup that follows the player's gaze
- Ticking clock and eerie ambience, plus hint, wrong-order and success sounds
- Wall clock, workbench, battery, resistor and LED models

## Requirements

- Unity **6000.6.2f1** (Universal Render Pipeline)
- Packages: XR Interaction Toolkit 3.6.1, OpenXR 1.18, XR Hands, Input System (restored automatically from `Packages/manifest.json`)
- A VR headset, or the **XR Device Simulator** (included in the scene) to test with mouse and keyboard

## Run

1. Install Git LFS (`git lfs install`), then clone the repo.
2. Open the folder with Unity Hub (version 6000.6.2f1). The first import takes a while.
3. Open `Assets/Scenes/SampleScene.unity` and press **Play**.

## Build for Meta Quest (Android, experimental)

1. File > Build Profiles > Android.
2. Edit > Project Settings > XR Plug-in Management > Android: enable **OpenXR**.
3. In the OpenXR settings (Android tab) enable the **Meta Quest** feature group and a Touch controller interaction profile.
4. Disable the **XR Device Simulator** object in the scene for headset builds.
5. Build and install the APK (SideQuest or `adb install`).

## Project structure

| Path | What it is |
|---|---|
| `Assets/Scenes/SampleScene.unity` | The game scene |
| `Assets/Scripts/` | Menu, end screen, hint popup, audio, try-again banner |
| `Assets/CircuitManager.cs`, `Assets/CircuitSocket.cs` | Puzzle logic and sockets |
| `Assets/Editor/` | Editor tools that build the UI (`Tools > Escape Room`) |
| `Assets/sound/generated/` | Generated sound effects |

## AI disclosure

Parts of the code, the UI design and the hint illustration were created with the help of generative AI tools (Claude). Game design, scene setup and testing were done by the team.
