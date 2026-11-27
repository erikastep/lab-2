using Unity.Behavior;
using UnityEngine;

namespace Lab2
{
    /// <summary>
    /// Pick a test case in the Inspector, press Play. Turns the key / tool / door on or off
    /// and writes matching start values into the thief's Blackboard.
    ///   Key         - key in the scene, no tool, door locked
    ///   ToolOnly    - no key, tool in the scene, door locked
    ///   DoorOpen    - no key, no tool, door already open
    ///   NoResources - no key, no tool, door locked (robbery must fail -> back to Wander)
    ///   KeyAndTool  - both in the scene (extra: shows the key is preferred)
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class TestSetup : MonoBehaviour
    {
        public enum Scenario { Key, ToolOnly, DoorOpen, NoResources, KeyAndTool }

        public Scenario scenario = Scenario.Key;

        [Header("Scene objects")]
        public GameObject key;
        public GameObject tool;
        public GameObject doorBlocker;
        public GameObject treasure;

        [Header("Thief (optional, to sync the Blackboard)")]
        public BehaviorGraphAgent thiefAgent;

        void Awake()
        {
            Apply();
        }

        void Start()
        {
            SyncBlackboard();
        }

        /// <summary>Sets the objects for the chosen scenario. Also called from the Inspector (context menu).</summary>
        [ContextMenu("Apply scenario now")]
        public void Apply()
        {
            bool keyOn = scenario == Scenario.Key || scenario == Scenario.KeyAndTool;
            bool toolOn = scenario == Scenario.ToolOnly || scenario == Scenario.KeyAndTool;
            bool doorClosed = scenario != Scenario.DoorOpen;

            if (key != null) key.SetActive(keyOn);
            if (tool != null) tool.SetActive(toolOn);
            if (doorBlocker != null) doorBlocker.SetActive(doorClosed);
            if (treasure != null) treasure.SetActive(true);

            Debug.Log($"[TestSetup] Scenario {scenario}: key={keyOn}, tool={toolOn}, doorOpen={!doorClosed}");
        }

        void SyncBlackboard()
        {
            if (thiefAgent == null) return;
            bool doorOpen = scenario == Scenario.DoorOpen;
            // Names agreed in the team plan. Missing variables are simply ignored.
            thiefAgent.SetVariableValue("HasKey", false);
            thiefAgent.SetVariableValue("HasTool", false);
            thiefAgent.SetVariableValue("HasTreasure", false);
            thiefAgent.SetVariableValue("DoorOpen", doorOpen);
        }

        public bool IsDoorOpenAtStart => scenario == Scenario.DoorOpen;
    }
}
