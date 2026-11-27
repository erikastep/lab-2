using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

// Writes the closest of the three refuges into NearestRefuge.
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Nearest Refuge", story: "[Self] picks nearest of [Refuge1] [Refuge2] [Refuge3] into [NearestRefuge]", category: "Action/Lab2", id: "6f3b2a9c4d1e4b7a9c0d2e3f4a5b6c71")]
public partial class FindNearestRefugeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Refuge1;
    [SerializeReference] public BlackboardVariable<GameObject> Refuge2;
    [SerializeReference] public BlackboardVariable<GameObject> Refuge3;
    [SerializeReference] public BlackboardVariable<GameObject> NearestRefuge;

    protected override Status OnStart()
    {
        if (Self.Value == null) return Status.Failure;

        GameObject best = null;
        float bestDistance = float.MaxValue;
        foreach (var refuge in new[] { Refuge1.Value, Refuge2.Value, Refuge3.Value })
        {
            if (refuge == null) continue;
            float distance = PathLength(Self.Value.transform.position, refuge.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = refuge;
            }
        }

        if (best == null) return Status.Failure;
        NearestRefuge.Value = best;
        return Status.Success;
    }

    // Walking distance on the NavMesh, so walls count (falls back to straight line).
    static float PathLength(Vector3 from, Vector3 to)
    {
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path)) return Vector3.Distance(from, to);
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++) length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        return length;
    }
}
