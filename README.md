# TacticalFPS
Original Unity 6 single-player FPS prototype inspired by the supplied tactical district reference. Procedural geometry is stylized, not a reproduction of the reference artwork or its photorealistic assets.

## Run
Install the Unity version in ProjectSettings/ProjectVersion.txt using Unity Hub. Open this directory, open Assets/TacticalFPS/Scenes/Training.unity, and press Play. The runtime bootstrap creates the district, player, guards, lighting, and HUD. Use the Built-in render pipeline and legacy Input Manager.

## Controls and mission
WASD move, mouse look, Shift sprint, C or Ctrl crouch, Space jump. Left mouse fires, right mouse aims. 1/2/3 select carbine, sidearm, marksman rifle. R reloads, H uses a medkit, E collects intel within 3m. Escape pauses; the pause/end screen restarts the mission.
Find intel in the north courtyard, return to the south extraction area, and remain there for five seconds with no living enemy within 12m. Guards patrol, navigate an obstacle grid, detect line of sight, chase, and shoot. The HUD displays ammunition, health, stamina, objectives, and a map. Set WorldBuilder.Night in a scene component for night lighting; the automatic bootstrap defaults to day.

## Validation
Eight Unity EditMode tests cover ammunition, extraction, and pathfinding. Run Window > General > Test Runner > EditMode. Unity is unavailable in the authoring environment, so these tests, C# compilation, native builds, and gameplay are UNRUN. Static source/asset checks are recorded in docs/VALIDATION.md. No screenshots are represented as actual gameplay.

## Scope
Local single-player prototype. No networking, realistic animation, licensed art, audio assets, grenades, save system, or production performance claims. First-person weapon geometry is original primitive geometry. Runtime guard movement and collision behavior require evaluation in Unity before use.

## History
Implementation commits record actual stages of development, without backdating. artifacts/TacticalFPS.bundle preserves the original local commit identifiers; GitHub imports retain their order and messages.
