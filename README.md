# Lab 2 – FSM: Robbery Plans

Team: Erika Steponavičiūtė, Gabrielė Sabuckytė, Bianca Meknat
Unity version: 6000.2.9f1
Packages: Behavior, AI Navigation

## How to run
1. Clone the repo and open the folder in Unity Hub (Add project from disk).
2. Open `Assets/_Project/Scenes/Lab2_Main.unity` and press Play.

## What's inside
| Folder | Part | Made by |
| --- | --- | --- |
| `Thief/` | Thief FSM (Wander, Robbery, Hiding, Hidden), police distance condition | Erika |
| `Robbery/` | Robbery graph and `OpenDoor` node | Bianca |
| `Police/` | Police script, `FindNearestRefuge`, test setup | Gabrielė |
| `Shared/` | Scene objects, materials, models | Gabrielė |
| `Scenes/` | Main scene | Erika |

All folders are in `Assets/_Project/`.

## Workflow
- Don't push to `main` directly. Make a branch (e.g. `bianca-robbery`), push it and open a pull request.
- Work in your own test scene, so we don't get merge conflicts in the main scene.
