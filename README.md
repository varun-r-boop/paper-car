# Paper Car

Local Unity prototype: blow into the microphone to push a paper car. Players alternate turns on one shared mic. Knock the other car outside the inner arena bounds to win.

## Open in Unity

1. Install **Unity 6 LTS** (or 2022.3 LTS) with **Universal Render Pipeline**.
2. Unity Hub → **Open** → this folder (`Documents/PaperCar`).
3. Let packages import. If URP 17 fails to resolve, create a new **3D (URP)** Hub project and copy the `Assets/Scripts` and `Assets/Editor` folders into it.
4. Press Play. The arena, cars, and UI are built automatically. Optional: menu **Paper Car → Setup Scene** to save `Assets/Scenes/PaperCar.unity`.
5. Allow microphone access when Windows asks.

## Play

- White arrow rotates on the active car (timing = aim). **A/D** or arrows also rotate aim.
- **Blow** into the mic, or press **Space**, to thrust.
- Cars use 3D Rigidbody physics on the XZ plane and bounce off each other and the walls.
- After motion settles, the other player aims.
- A car whose center leaves the inner yellow bounds loses. Restart appears on match over.

Tune `noiseGate`, `maxRms`, `maxImpulse`, `settleSpeed`, and `ArenaBounds` half extents in the Inspector after Setup Scene.
