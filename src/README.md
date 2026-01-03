# /src — Shareable Source (Original Work Only)

This `/src` directory contains **only the scripts and assets authored by me** that are safe to share publicly.

Why this exists:
- The full Unity project uses Unity Asset Store / paid packages.
- Asset licenses typically prohibit redistribution of those packages in a public GitHub repo.
- To stay compliant, I extracted and published my original components here, especially the rule detection system
  and the integration logic that ties vehicle control, AI traffic, UI, and scoring together.

If you are reviewing this project:
- You can understand the system design and logic from this folder, even without the full Unity project.
- Use the root README demo video + screenshots to see runtime behavior.

---

## What you should expect to find here

Common categories (your exact folder names may differ):
- `TrafficRuleDetection/` — CAP. 374B-based detection modules, scoring, and violation logging
- `UIFlow` — alert UI, result screen summaries, UI related script
- `Integrations/` — Logitech G29 input integration, gear logic, indicators, mirror hooks
- `Utils/` — Game flow logic utility, route progression, checkpoints, region-based enforcement, etc

---
