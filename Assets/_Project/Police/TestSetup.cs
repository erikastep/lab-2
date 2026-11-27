using Unity.Behavior;
using UnityEngine;

// Pick a test case in the Inspector, then press Play.
public class TestSetup : MonoBehaviour
{
    public enum Scenario { Key, ToolOnly, DoorOpen, NoResources }

    public Scenario scenario = Scenario.Key;
    public GameObject key;
    public GameObject tool;
    public GameObject doorBlocker;
    public BehaviorGraphAgent thiefAgent;

    void Awake()
    {
        if (key != null) key.SetActive(scenario == Scenario.Key);
        if (tool != null) tool.SetActive(scenario == Scenario.ToolOnly);
        if (doorBlocker != null) doorBlocker.SetActive(scenario != Scenario.DoorOpen);
    }

    void Start()
    {
        if (thiefAgent != null) thiefAgent.SetVariableValue("DoorOpen", scenario == Scenario.DoorOpen);
    }
}
