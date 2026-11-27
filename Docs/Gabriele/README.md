# Gabrielė's part

| Path | What |
|---|---|
| `Assets/_Project/Shared/Scenes/Lab2_Gabriele_Test.unity` | test scene: NavMesh, treasure room, door (DoorBlocker), key, tool, DoorPoint, 3 refuges, police, thief |
| `Assets/_Project/Shared/Models/`, `Materials/` | CC0 models (KayKit, Quaternius) and colours |
| `Assets/_Project/Police/PoliceController.cs` | police loop: guard → walk away → wait → come back. Play mode: **R** = come back now, **L** = leave now |
| `Assets/_Project/Police/Nodes/` | Behavior nodes *Action/Lab2*: **Find Nearest Refuge** and **Stay Hidden** |
| `Assets/_Project/Police/TestSetup.cs` | Inspector dropdown: Key / ToolOnly / DoorOpen / NoResources |
| `Docs/Gabriele/FSM_diagram.png` | FSM diagram for the report |

- DoorBlocker has a NavMeshObstacle with Carve on, so it blocks the path until it is turned off.
- The thief has an empty BehaviorGraphAgent: assign the thief graph after merging.
- If a test case has no key or no tool, TestSetup turns that object off, so the route check should be "HasKey or Key active" (same for Tool).

**Hiding:** Find Nearest Refuge → Navigate To Target (Target = NearestRefuge, Speed = RunSpeed) → go to Hidden.
**Hidden:** Stay Hidden.
