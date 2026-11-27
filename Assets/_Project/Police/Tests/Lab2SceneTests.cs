using System.Collections;
using Lab2;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Play-mode checks for Gabriele's part: scene, door blocking, police loop,
/// FindNearestRefuge and TestSetup. Run from Window > General > Test Runner > PlayMode.
/// </summary>
public class Lab2SceneTests
{
    const string ScenePath = "Assets/_Project/Shared/Scenes/Lab2_Gabriele_Test.unity";

    [UnitySetUp]
    public IEnumerator Load()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
            ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
        yield return null;
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator Reset()
    {
        Time.timeScale = 1f;
        yield return null;
    }

    static GameObject Find(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.name == name && go.scene.IsValid()) return go;
        return null;
    }

    static NavMeshPathStatus PathStatus(Vector3 from, Vector3 to)
    {
        NavMesh.SamplePosition(from, out var a, 2f, NavMesh.AllAreas);
        NavMesh.SamplePosition(to, out var b, 2f, NavMesh.AllAreas);
        var path = new NavMeshPath();
        NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path);
        return path.status;
    }

    [Test]
    public void SceneHasAllObjects()
    {
        foreach (var n in new[] { "Thief", "Police", "Treasure", "Key", "Tool", "DoorBlocker",
                                  "DoorPoint", "Refuge1", "Refuge2", "Refuge3", "TestSetup" })
            Assert.IsNotNull(Find(n), n + " missing");
    }

    [UnityTest]
    public IEnumerator ClosedDoorBlocksTreasure_OpenDoorAllowsIt()
    {
        var setup = Object.FindAnyObjectByType<TestSetup>();
        var door = Find("DoorBlocker");
        door.SetActive(true);
        for (int i = 0; i < 5; i++) yield return null;

        var from = Find("DoorPoint").transform.position;
        var to = Find("Treasure").transform.position;
        Assert.AreNotEqual(NavMeshPathStatus.PathComplete, PathStatus(from, to), "closed door should block");

        door.SetActive(false);
        for (int i = 0; i < 5; i++) yield return null;
        Assert.AreEqual(NavMeshPathStatus.PathComplete, PathStatus(from, to), "open door should allow path");
    }

    [Test]
    public void NearestRefugeIsPicked()
    {
        var r1 = Find("Refuge1"); var r2 = Find("Refuge2"); var r3 = Find("Refuge3");
        Assert.AreEqual(r1, RefugeMath.Nearest(new Vector3(-10, 0, -10), r1, r2, r3));
        Assert.AreEqual(r2, RefugeMath.Nearest(new Vector3(-10, 0, 10), r1, r2, r3));
        Assert.AreEqual(r3, RefugeMath.Nearest(new Vector3(10, 0, -10), r1, r2, r3));
        Assert.IsNull(RefugeMath.Nearest(Vector3.zero, null, null, null));
    }

    [UnityTest]
    public IEnumerator PoliceLoopsGuardAwayReturn()
    {
        var police = Object.FindAnyObjectByType<PoliceController>();
        police.guardTime = 0.3f;
        police.waitAwayTime = 0.3f;
        police.walkSpeed = 12f;
        police.GetComponent<NavMeshAgent>().acceleration = 100f;
        Time.timeScale = 4f;

        var seen = new System.Collections.Generic.List<PoliceController.PoliceState>();
        float t = 0f;
        while (t < 30f && seen.Count < 5)
        {
            if (seen.Count == 0 || seen[seen.Count - 1] != police.State) seen.Add(police.State);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        CollectionAssert.AreEqual(new[] {
            PoliceController.PoliceState.Guard, PoliceController.PoliceState.WalkAway,
            PoliceController.PoliceState.WaitAway, PoliceController.PoliceState.Return,
            PoliceController.PoliceState.Guard }, seen, string.Join(",", seen));
    }

    [UnityTest]
    public IEnumerator PoliceReturnNowInterrupts()
    {
        var police = Object.FindAnyObjectByType<PoliceController>();
        police.LeaveNow();
        yield return null;
        Assert.AreEqual(PoliceController.PoliceState.WalkAway, police.State);
        police.ReturnNow();
        Assert.AreEqual(PoliceController.PoliceState.Return, police.State);
    }

    [TestCase(TestSetup.Scenario.Key, true, false, true)]
    [TestCase(TestSetup.Scenario.ToolOnly, false, true, true)]
    [TestCase(TestSetup.Scenario.DoorOpen, false, false, false)]
    [TestCase(TestSetup.Scenario.NoResources, false, false, true)]
    [TestCase(TestSetup.Scenario.KeyAndTool, true, true, true)]
    public void TestSetupScenarios(TestSetup.Scenario s, bool key, bool tool, bool doorClosed)
    {
        var setup = Object.FindAnyObjectByType<TestSetup>();
        setup.scenario = s;
        setup.Apply();
        Assert.AreEqual(key, setup.key.activeSelf, "key");
        Assert.AreEqual(tool, setup.tool.activeSelf, "tool");
        Assert.AreEqual(doorClosed, setup.doorBlocker.activeSelf, "door");
        Assert.IsTrue(setup.treasure.activeSelf, "treasure");
    }

    /// <summary>Walks the thief along the key route with plain NavMeshAgent calls (what the graph will do).</summary>
    [UnityTest]
    public IEnumerator ThiefCanWalkKeyRouteThenHide()
    {
        Object.FindAnyObjectByType<PoliceController>().enabled = false;
        var thief = Find("Thief").GetComponent<NavMeshAgent>();
        thief.speed = 10f; thief.acceleration = 60f;
        Time.timeScale = 4f;

        yield return Walk(thief, Find("Key").transform.position, "key");
        yield return Walk(thief, Find("DoorPoint").transform.position, "door point");
        Find("DoorBlocker").SetActive(false); // what OpenDoor does
        yield return null; yield return null;
        yield return Walk(thief, Find("Treasure").transform.position, "treasure");

        var nearest = RefugeMath.Nearest(thief.transform.position, Find("Refuge1"), Find("Refuge2"), Find("Refuge3"));
        Assert.IsNotNull(nearest);
        yield return Walk(thief, nearest.transform.position, "refuge");
    }

    static IEnumerator Walk(NavMeshAgent a, Vector3 target, string label)
    {
        a.SetDestination(target);
        float t = 0f;
        while (t < 20f)
        {
            t += Time.unscaledDeltaTime;
            if (!a.pathPending && a.remainingDistance <= 0.8f
                && Vector3.Distance(Flat(a.transform.position), Flat(target)) < 1.2f) yield break;
            yield return null;
        }
        Assert.Fail($"thief did not reach {label}: at {a.transform.position}, path {a.pathStatus}");
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
}
