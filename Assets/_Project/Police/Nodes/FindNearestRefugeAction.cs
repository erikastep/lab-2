using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

namespace Lab2
{
    /// <summary>
    /// Picks the closest of the three refuges and writes it into NearestRefuge.
    /// Uses NavMesh path length when a path exists (real walking distance),
    /// otherwise falls back to straight-line distance.
    /// Success: NearestRefuge was set. Failure: no Self or no refuge at all.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Find Nearest Refuge",
        story: "[Self] picks nearest of [Refuge1] [Refuge2] [Refuge3] into [NearestRefuge]",
        description: "Writes the closest refuge (by NavMesh path length) into NearestRefuge.",
        category: "Action/Lab2",
        id: "6f3b2a9c4d1e4b7a9c0d2e3f4a5b6c71")]
    public partial class FindNearestRefugeAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<GameObject> Refuge1;
        [SerializeReference] public BlackboardVariable<GameObject> Refuge2;
        [SerializeReference] public BlackboardVariable<GameObject> Refuge3;
        [SerializeReference] public BlackboardVariable<GameObject> NearestRefuge;

        protected override Status OnStart()
        {
            if (Self?.Value == null) return Status.Failure;

            GameObject best = RefugeMath.Nearest(
                Self.Value.transform.position,
                Refuge1?.Value, Refuge2?.Value, Refuge3?.Value);

            if (best == null) return Status.Failure;

            NearestRefuge.Value = best;
            return Status.Success;
        }
    }

    /// <summary>Plain helper so the choice can be unit-tested without a graph.</summary>
    public static class RefugeMath
    {
        public static GameObject Nearest(Vector3 from, params GameObject[] refuges)
        {
            GameObject best = null;
            float bestDist = float.MaxValue;
            var path = new NavMeshPath();

            foreach (var r in refuges)
            {
                if (r == null || !r.activeInHierarchy) continue;
                float d = Distance(from, r.transform.position, path);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = r;
                }
            }
            return best;
        }

        static float Distance(Vector3 from, Vector3 to, NavMeshPath path)
        {
            if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path)
                && path.status == NavMeshPathStatus.PathComplete)
            {
                float len = 0f;
                var c = path.corners;
                for (int i = 1; i < c.Length; i++) len += Vector3.Distance(c[i - 1], c[i]);
                return len;
            }
            return Vector3.Distance(from, to);
        }
    }
}
