# Hong Kong Driving Simulator — Capstone Project

A Unity-based private car driving simulator tailored to Hong Kong’s driving test context, featuring:
- A 1:1 driving-test route (Ho Man Tin: Chung Yee Street ↔ Carmel Village Street Small Loop)
- Logitech G29 steering wheel + pedals driving (desktop setup)
- AI traffic using waypoint-based navigation and traffic-light logic
- A CAP. 374B-based rule detection + scoring system with real-time alerts and a final result report

It is designed for **enhancing driving skills and promoting road safety awareness** within the **realistic traffic conditions of Hong Kong**.

> Important: This repository is intentionally **partial-source** due to Unity Asset Store / paid package licensing.
> It contains **my original code and documentation**, plus an executable build,
> but it does **not** redistribute paid assets/packages.

---

## Demo

- Demo video: https://youtu.be/oZaKbXssdSM?si=HHuj-rQUMfqlvria
- Screenshots: [See below showcases](https://github.com/bububoy0907/Capstone-driving-sims-HK?tab=readme-ov-file#project-showcases-screenshots)
- Build (Windows): [GitHub Releases](https://github.com/bububoy0907/Capstone-driving-sims-HK/releases/tag/v2025.04-demo)

If you are reviewing this project and need a walkthrough of how the systems work (rule detection, AI traffic),
Please use the demo video and screenshots, or contact me for a guided demo.

---

## Project Context

This simulator was developed as a PolyU COMP4913 capstone project to explore an interactive training tool
for driving skills and road safety awareness in Hong Kong. The simulator includes real-time feedback
(rule violations) and AI traffic scenarios to better approximate practical road conditions.

---

## Purpose and Goal

The goal of the simulator is to address critical issues in Hong Kong's driver education system, such as:
- Low pass rates (approx. 30%)
- Long waiting time for practical tests (up to 294 days)
- Poor real-world driving preparedness

Through simulation, this project aims to:
- Enhance foundational driving skills
- Improve rule awareness and road safety
- Provide cost- and time-effective alternatives to traditional lessons

---

## Key Features

### Realistic Vehicle Mechanics
- Full **vehicle physics** with RCC system
- Supported gear shift modes:
  - `P` (Park), `R` (Reverse), `N` (Neutral), `D` (Drive)
- Support for **Logitech G29 steering wheel + pedals**
- Functioning **turn indicators**, **look left/right**, **brake/throttle logic**
- Simulated **camera tilt** when turning for realism
- Route guidance system with on-road checkpoints and visual indicators

### Rule Detection & Scoring System
- Implements real-time feedback based on the **Hong Kong Transport Department’s Driving Test Form (CAP. 374B)**
- Real-time traffic rule violation detection with visual alerts
- Tracks total number and categories of all violations
- Supports **pass/fail** logic based on number of violations (>=3 = fail)
- Final result includes:
  - Violation breakdown per category
  - Most violated rule with improvement recommendations

####  Detection Modules:
1. **Fail to Check Traffic Conditions**
2. **Unintended Rolling (on slopes)**
3. **Striking or Colliding with Objects**
4. **Improper Parking or Stopping**
5. **Signaling Errors**
6. **Gear/Handbrake Misuse**
7. **Speeding / Poor Speed Control**
8. **Lane Discipline Violations**
9. **Traffic Light / Right-of-Way / Pedestrian Crossings**

### NPC AI Traffic Simulation
- AI-controlled vehicles follow pre-configured traffic routes
- Behave according to traffic lights and obey signs
- Interact with player realistically (e.g., collisions, spacing)
- Used for overtaking checks and following-distance evaluation

### Custom Hong Kong 3D Road Map
- **"Chun Yee Street – Carmel Village Street"** developed with accurate data from OpenStreetMap
- Realistic environmental elements such as sidewalks, fences, road textures
- Map expanded beyond test route to support immersive urban realism&#8203;:contentReference[oaicite:1]{index=1}

---

## Performance Metrics

Performance is evaluated based on the **traffic rule violation system**:
- The simulator records each violation and classifies it (e.g., minor or major)
- Repeated minor violations may escalate to major, mirroring real-world rules
- Users can review performance through logs for training feedback

---

## What’s this repo Included vs Not Included

### Included
- My original scripts (see `/src`)
- Screenshots/media
- Executable build file (Can be downloaded from the Release)

### Not Included (by design)
- Unity Asset Store paid assets / packages (vehicle controller, traffic packages, environment packs, etc.)
- Any third-party content that cannot be redistributed legally

---

## Repository Structure

- `/Executable` — Windows executable build (optional)
- `/media` — screenshots, GIFs, short clips
- `/src` — my original script only
  - `/src/TrafficRuleDetection`
  - `/src/Utils/`
  - `/src/Integrations`
  - `/src/Utils`

---

## Requirements (For Running the Build)

### Hardware
- Windows 10/11 (the platform used/tested)
- Logitech G29 steering wheel + pedals (required)
- Recommended: 16GB RAM and a DX11/DX12/Vulkan-capable GPU

### Software
- Logitech G HUB (used for wheel calibration and tuning)

> **Note:** This simulator has been tested and verified only with **Logitech G29 Steering wheel controller with pedals**. Other devices may not be fully supported or stable.
---

## How to Run (Windows Build)

1. Install **Logitech G HUB** and confirm the G29 is recognized.
2. Connect the wheel + pedals before launching the simulator.
3. Launch the executable: `[DrivingSimulator.exe]`
4. Interact using the G29 controls (keyboard/mouse support may be limited depending on your build setup).
---

## Project Showcases (Screenshots)

### Title Screen <br />
![alt text](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/title_screen.png?raw=true)

### Route Design:
  The terrain(map) is constructed:
  1. Using OpenStreetMap for geographic data
  2. Using Google Street View from Google Earth for image reference, e.g.: building, road object, signs...
  3. Modelling the layout and building detail in Blender
  4. Using EzRoadPro to construct the road

### Map Development Progress on Blender <br />
![alt text](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/map_development.png)

Map Comparison with the actual Google Street View <br />
![Map comparison](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/Map%20comparison_1.png?raw=1)
![Map comparison](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/Map%20comparison_2.png?raw=1)
![Map comparison](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/Map%20comparison_3.png?raw=1)
![Map comparison](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/Map%20comparison_4.png?raw=1)
![Map comparison](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/Map%20comparison_5.png?raw=1)
![Map comparison](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/Map%20comparison_7.png?raw=1)
![Map comparison](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/Map%20comparison_8.png?raw=1)
![Map comparison](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/Map%20comparison_9.png?raw=1)
![Map comparison](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/Map%20comparison_10.png?raw=1)

### Vehicle View <br />
![Map comparison](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/Map%20comparison_11.png?raw=1)

### AI-Driven Dynamic Traffic <br />
![alt text](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/NPC_vehicle.png)
![alt text](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/NPC_vehicle_nav.jpg)

### NPC Vehicle Waypoint System <br />
![alt text](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/npc_waypoint.png)

### Traffic Intersection <br />
![alt text](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/traffic_intersection.png)

### Traffic Rule Detection Alert Example <br />
![alt text](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/vehicle_mechanic.png)

### Result Report <br />
![alt text](https://github.com/bububoy0907/Capstone-driving-sims-HK/blob/main/media/result_screen.png)

---

## Development Notes (For Rebuilding from Partial Source)

Because paid assets are excluded, a full “open and run” Unity project is not provided here.

If you want to rebuild:
1. Install Unity (use the same major version used in development if possible).
2. Import your licensed packages:
   - Realistic Car Controller (RCC)
   - Mobile Traffic System v2
   - EzRoadPro
3. Import the `/src` scripts from this repo into your Unity project.
4. Wire up references in scenes/prefabs:
   - G29 input mappings
   - Waypoint graphs + intersections
   - Rule detection triggers/sensors
   - UI alert canvas + result screen data flow

---

## Evaluation Summary (High Level)

This project was evaluated using quantitative methods:
- Participants (N = 60) grouped into learner / novice / experienced drivers
- Post-simulation surveys and simulator performance metrics were collected
- Descriptive statistics and correlation analysis were used to interpret outcomes

Details are available in the presentation (demo video).

---

## Known Limitations / Future Improvements

- VR mode was removed during development due to comfort and interaction issues (wheel/button usability).
- Rule alert UI can become noisy when multiple violations trigger quickly.
- AI traffic realism can be further improved for complex merges and edge cases.

Planned improvements:
- Cleaner alert UX (priority/stacking, better placement, clearer cues)
- More NPC event variety and “human-like” behaviour
- Better onboarding/tutorial flow for first-time users

---

## Credits / Third-Party

This project uses third-party Unity packages and assets (not redistributed here). 
