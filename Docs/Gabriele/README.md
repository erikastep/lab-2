# Gabrielė's part: scene, police, FindNearestRefuge, test setup

## Where things are
| Path | What |
|---|---|
| `Assets/_Project/Shared/Scenes/Lab2_Gabriele_Test.unity` | my test scene (ground + NavMesh, treasure room, door, key, tool, DoorPoint, 3 refuges, police, thief) |
| `Assets/_Project/Shared/Models/` | KayKit (rogue, knight, hammer) and Quaternius (gold chest) models, CC0 |
| `Assets/_Project/Shared/Materials/` | primitive colours |
| `Assets/_Project/Shared/Editor/Lab2SceneBuilder.cs` | menu **Lab2 → Build Gabriele Scene** rebuilds the test scene from scratch |
| `Assets/_Project/Police/PoliceController.cs` | police loop: Guard → WalkAway → WaitAway → Return. In Play mode: **R** = return now, **L** = leave now |
| `Assets/_Project/Police/Nodes/FindNearestRefugeAction.cs` | Behavior node *Action/Lab2/Find Nearest Refuge*: Self, Refuge1–3 → NearestRefuge |
| `Assets/_Project/Police/Nodes/StayHiddenAction.cs` | Behavior node *Action/Lab2/Stay Hidden*: stops the agent, always Running |
| `Assets/_Project/Police/TestSetup.cs` | Inspector dropdown: Key / ToolOnly / DoorOpen / NoResources / KeyAndTool |
| `Assets/_Project/Police/Tests/` | 11 PlayMode tests (Window → General → Test Runner → PlayMode), all passing |
| `Docs/Gabriele/FSM_diagram.png` | FSM diagram for the report |

## Scene details
- **DoorBlocker** has a NavMeshObstacle with **Carve** on and is excluded from the bake, so it really blocks the path. Deactivating it opens the path (a test checks this).
- Items (Key, Tool, Treasure) have trigger colliders and are excluded from the bake.
- The thief has a NavMeshAgent and an empty **BehaviorGraphAgent**. Assign the thief graph after merging.
- Police points (guard + 2 away points) live in `PoliceController`, not in the thief Blackboard.

## For Erika (merging into Lab2_Main)
- Copy the scene objects from my test scene, or open it and assign the thief graph there.
- The police guard point is about 3 m from the door, and the away points are in the far corners (about 20 m). `PoliceSafeDistance = 8` works with this layout.
- `TestSetup` writes `HasKey`, `HasTool` and `HasTreasure` = false and sets `DoorOpen` for the chosen scenario into the thief Blackboard (via `SetVariableValue`).

## For Bianca (Robbery routes)
- When a scenario has no key or no tool, `TestSetup` **deactivates** that object. A route condition should be "`HasKey` **or** Key object active" (same for Tool), otherwise in NoResources the thief would walk to a missing key.

## Hiding / Hidden in the thief graph
- **Hiding:** Find Nearest Refuge (Self, Refuge1–3 → NearestRefuge) → Navigate To Target (Agent = Self, Target = NearestRefuge, Speed = RunSpeed) → switch to Hidden.
- **Hidden:** Stay Hidden (Self).

## Report: Q4 test cases (fill in Result after merging)
| Case | Setup | Expected | Result |
|---|---|---|---|
| Key | key on, tool off, door locked | key → DoorPoint → OpenDoor → treasure → nearest refuge → Hidden | |
| Tool only | key off, tool on, door locked | tool → DoorPoint → force door → treasure → refuge → Hidden | |
| Door open | no key, no tool, door open | straight to treasure → refuge → Hidden | |
| No resources | no key, no tool, door locked | no route → Wander, wait 2 s, retry; never gets the treasure | |
| Key + tool (extra) | both on | takes the key (priority) | |

Checked in this repo with **Unity 6000.2.9f1** and Behavior 1.0.16: 11/11 PlayMode tests pass. The scene, NavMesh and materials are saved with 6000.2.9f1.
