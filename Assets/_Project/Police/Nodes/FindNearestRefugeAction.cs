using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
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
            float distance = Vector3.Distance(Self.Value.transform.position, refuge.transform.position);
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
}
