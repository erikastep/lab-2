using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

namespace Lab2
{
    /// <summary>
    /// Hidden state: stop the NavMeshAgent and stay still forever (always Running).
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Stay Hidden",
        story: "[Self] stays still",
        description: "Stops the NavMeshAgent and keeps running so the thief never moves again.",
        category: "Action/Lab2",
        id: "9a8b7c6d5e4f40312a1b2c3d4e5f6a72")]
    public partial class StayHiddenAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;

        protected override Status OnStart()
        {
            if (Self?.Value == null) return Status.Failure;
            var agent = Self.Value.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
            return Status.Running;
        }

        protected override Status OnUpdate() => Status.Running;
    }
}
