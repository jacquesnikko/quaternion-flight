# Quaternion Flight

A small Unity 6 assignment project demonstrating quaternion steering and homing. The scene is generated from C# when you press **Play**, so the project opens without needing to arrange objects manually.

## Play

1. Open this folder in Unity **6000.0.75f1** or a compatible Unity 6 release.
2. Open `Assets/Scenes/Main.unity`.
3. Press **Play**.
4. Steer with **A/D** or **Left/Right Arrow**. The aircraft continues forward automatically.

Missiles approach from ahead of the aircraft, entering just beyond the left or right edge of the camera view. They move faster than the aircraft and home in with `LookRotation` and turn-limited `Slerp`. A sharp turn can still evade them: hold A/D or the arrow keys to curve away from their approach. Waves are spaced 6.5 seconds apart, and no more than three missiles can be active at once. After every 10 seconds survived, the next wave requests one additional missile, up to three. A missile expires after 5 seconds or counts as a hit when it gets within 1.9 units of the player. Five hits restart the run.

## Quaternion concepts used

- The aircraft applies a small heading step with `Quaternion.AngleAxis` and quaternion multiplication.
- The aircraft banks using `Quaternion.Slerp`.
- Each missile computes its facing with `Quaternion.LookRotation` and steers with `Quaternion.Slerp`.
- All movement and hit checks use transforms and distance calculations. The scripts do not use rigidbodies, collision callbacks, or trigger callbacks.

## Demo video

Add the recorded gameplay clip at `Demo/QuaternionFlightDemo.mp4`; the embedded video below will display it in repository viewers that support MP4 embeds.

<!-- Replace this placeholder with the committed or hosted demo clip before submission. -->

<video src="Demo/QuaternionFlightDemo.mp4" controls width="800"></video>

The demo recording is not included yet. Record the clip and save it at `Demo/QuaternionFlightDemo.mp4` before submitting.

To record the demo, show the player steering, a missile tracking the aircraft, the hit counter, and the restart after five hits. Also capture the missile count increasing after the 10-second survival marks.

## Project structure

- `Assets/Scenes/Main.unity` — playable entry scene
- `Assets/Scripts/FlightPlayer.cs` — constant forward movement and banked steering
- `Assets/Scripts/HomingMissile.cs` — lifetime, tracking, and proximity hit detection
- `Assets/Scripts/MissileSpawner.cs` — off-screen waves and difficulty scaling
- `Assets/Scripts/GameController.cs` — procedural visuals, HUD, hit count, and restart
- `Assets/Scripts/GameBootstrap.cs` — runtime scene setup
