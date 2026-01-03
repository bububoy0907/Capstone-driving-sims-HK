# Hong Kong Driving Simulator (G29 Edition) — Capstone Project (COMP4913)

A Unity-based private car driving simulator tailored to Hong Kong’s driving test context, featuring:
- A 1:1 driving-test route (Ho Man Tin: Chung Yee Street ↔ Carmel Village Street Small Loop)
- Logitech G29 steering wheel + pedals driving (desktop setup)
- AI traffic using waypoint-based navigation and traffic-light logic
- A CAP. 374B-based rule detection + scoring system with real-time alerts and a final result report

> Important: This repository is intentionally **partial-source** due to Unity Asset Store / paid package licensing.
> It contains **my original code and documentation**, plus an executable build (if included in your repo),
> but it does **not** redistribute paid assets/packages.

---

## Demo

- Demo video: https://youtu.be/oZaKbXssdSM?si=HHuj-rQUMfqlvria
- Screenshots: [Screenshots Folder]
- Build (Windows): [Build Folder or GitHub Releases]

If you are reviewing this project and need a walkthrough of how the systems work (rule detection, AI traffic),
please use the demo video and screenshots, or contact me for a guided demo.

---

## Project Context

This simulator was developed as a PolyU COMP4913 capstone project to explore an interactive training tool
for driving skills and road safety awareness in Hong Kong. The simulator includes real-time feedback
(rule violations) and AI traffic scenarios to better approximate practical road conditions.

---

## Key Features

### 1) Driving Route & Environment (Hong Kong: Ho Man Tin)
- 1:1 scale route layout based on OpenStreetMap geographic data
- Visual references from Google Street View
- Modeled road geometry (slopes, turnouts, junctions, crossings) and environment objects for realism

### 2) Vehicle Mechanics (G29 Driving)
- Automatic transmission support: **P / R / N / D**
- Steering wheel + pedal driving via Logitech G29
- Driver aids and realism features (examples):
  - Mirror system (rear / left / right)
  - Turn indicators and required checks
  - Speedometer / gear UI

> Note: I used third-party vehicle/traffic packages as foundations, then extended and integrated them
> with my own systems (especially rule detection, scoring flow, UI flow, route logic).

### 3) AI Traffic System (Waypoint-Based)
- NPC vehicles navigate using waypoints with speed/direction constraints
- Give-way logic and lane/merge behavior via conditional waypoint rules
- Traffic light cycle logic and intersection behavior
- NPC “cleanup” / removal logic when far away (performance-minded)

### 4) Traffic Rule Detection + Scoring (CAP. 374B-Based)
- Driving test form rules were mapped into practical detection modules for simulation
- Real-time violation alerts + logging
- Final results screen summarizing:
  - pass/fail
  - violations per category
  - most frequent mistakes and targeted feedback

Pass/fail rule used in this project:
- **Pass** if total violations **< 3**
- **Fail** if total violations **≥ 3**

---

## What’s Included vs Not Included (Licensing Transparency)

### Included
- My original scripts (see `/src`)
- Documentation for architecture and setup
- Screenshots / media (if you add them)
- Windows build (if you included it in the repo)

### Not Included (by design)
- Unity Asset Store paid assets / packages (vehicle controller, traffic packages, environment packs, etc.)
- Any third-party content that cannot be redistributed legally

If you have licenses for the same assets/packages, you can recreate the full Unity project by importing them
and then integrating the scripts from this repository.

---

## Repository Structure (Recommended)

If your repo already has a different layout, update this section to match it.

- `/build` — Windows executable build (optional)
- `/media` — screenshots, GIFs, short clips
- `/docs` — reports, diagrams, development notes
- `/src` — my original code only (safe to share)
  - `/src/rule-detection`
  - `/src/ai-traffic`
  - `/src/vehicle-control`
  - `/src/route-checkpoints`
  - `/src/ui-reporting`
  - `/src/analysis` (Python/MATLAB notes or scripts, if any)

---

## Requirements (For Running the Build)

### Hardware
- Windows 10/11 (the platform used/tested)
- Logitech G29 steering wheel + pedals (required)
- Recommended: 16GB RAM and a DX11/DX12/Vulkan-capable GPU

### Software
- Logitech G HUB (used for wheel calibration and tuning)

---

## How to Run (Windows Build)

1. Install **Logitech G HUB** and confirm the G29 is recognized.
2. Connect the wheel + pedals before launching the simulator.
3. Launch the executable: `[DrivingSimulator.exe]`
4. Interact using the G29 controls (keyboard/mouse support may be limited depending on your build setup).

---

## Development Notes (For Rebuilding from Partial Source)

Because paid assets are excluded, a full “open and run” Unity project is not provided here.

If you want to rebuild:
1. Install Unity (use the same major version used in development if possible).
2. Import your licensed packages:
   - Vehicle controller package
   - Traffic / NPC package
   - Road/building/terrain packages (if you used them)
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

Details are available in the final report/presentation (see `/docs` if included).

---

## Known Limitations / Future Improvements

- VR mode was removed during development due to comfort and interaction issues (wheel/button usability).
- Rule alert UI can become noisy when multiple violations trigger quickly.
- AI traffic realism can be further improved for complex merges and edge cases.

Planned improvements:
- Cleaner alert UX (priority/stacking, better placement, clearer cues)
- More NPC event variety and “human-like” behavior
- Better onboarding/tutorial flow for first-time users

---

## Credits / Third-Party

This project uses third-party Unity packages and assets (not redistributed here).  
See: [THIRD_PARTY.md] or list them here if you want reviewers to understand dependencies.

---

## Author

Jason Wong (Wong Tsz Yeung)  
PolyU BSc in Computing (Computer Science), Second-Class Honours (May 2025)  
GitHub: https://github.com/bububoy0907  
LinkedIn: [LinkedIn Profile Link]
Email: [Your Email]
