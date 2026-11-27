using UnityEngine;
using UnityEngine.AI;

namespace Lab2
{
    /// <summary>
    /// Police officer loop: Guard (at the treasure door) -> WalkAway -> WaitAway -> Return -> Guard ...
    /// Patrol points live here (team rule), not in the thief Blackboard.
    /// Keys in Play mode (for the interruption test): R = return now, L = leave now.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class PoliceController : MonoBehaviour
    {
        public enum PoliceState { Guard, WalkAway, WaitAway, Return }

        [Header("Points")]
        [Tooltip("Where the police stands while guarding the treasure.")]
        public Transform guardPoint;
        [Tooltip("Points the police walks to when leaving. One is used per trip, in order.")]
        public Transform[] awayPoints;

        [Header("Timing (seconds)")]
        public float guardTime = 6f;
        public float waitAwayTime = 10f;

        [Header("Speed")]
        public float walkSpeed = 2.5f;

        [Header("Debug")]
        [Tooltip("Allow R (return now) and L (leave now) keys in Play mode.")]
        public bool debugKeys = true;

        public PoliceState State { get; private set; } = PoliceState.Guard;

        NavMeshAgent agent;
        float timer;
        int awayIndex;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            agent.speed = walkSpeed;
            if (guardPoint != null)
            {
                agent.Warp(guardPoint.position);
            }
            Enter(PoliceState.Guard);
        }

        void Update()
        {
            HandleDebugKeys();
            timer += Time.deltaTime;

            switch (State)
            {
                case PoliceState.Guard:
                    if (timer >= guardTime) Enter(PoliceState.WalkAway);
                    break;
                case PoliceState.WalkAway:
                    if (Arrived()) Enter(PoliceState.WaitAway);
                    break;
                case PoliceState.WaitAway:
                    if (timer >= waitAwayTime) Enter(PoliceState.Return);
                    break;
                case PoliceState.Return:
                    if (Arrived()) Enter(PoliceState.Guard);
                    break;
            }
        }

        /// <summary>Send the police back to the guard point right now (used for the interruption test).</summary>
        public void ReturnNow() => Enter(PoliceState.Return);

        /// <summary>Make the police leave right now.</summary>
        public void LeaveNow() => Enter(PoliceState.WalkAway);

        void Enter(PoliceState next)
        {
            State = next;
            timer = 0f;
            agent.speed = walkSpeed;

            switch (next)
            {
                case PoliceState.Guard:
                case PoliceState.WaitAway:
                    agent.ResetPath();
                    break;
                case PoliceState.WalkAway:
                    if (awayPoints != null && awayPoints.Length > 0)
                    {
                        Transform p = awayPoints[awayIndex % awayPoints.Length];
                        awayIndex++;
                        if (p != null) agent.SetDestination(p.position);
                    }
                    break;
                case PoliceState.Return:
                    if (guardPoint != null) agent.SetDestination(guardPoint.position);
                    break;
            }
        }

        bool Arrived()
        {
            if (agent.pathPending) return false;
            return agent.remainingDistance <= agent.stoppingDistance + 0.2f;
        }

        void HandleDebugKeys()
        {
#if ENABLE_INPUT_SYSTEM
            if (!debugKeys || UnityEngine.InputSystem.Keyboard.current == null) return;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb.rKey.wasPressedThisFrame) ReturnNow();
            if (kb.lKey.wasPressedThisFrame) LeaveNow();
#else
            if (!debugKeys) return;
            if (Input.GetKeyDown(KeyCode.R)) ReturnNow();
            if (Input.GetKeyDown(KeyCode.L)) LeaveNow();
#endif
        }

        void OnDrawGizmos()
        {
            if (guardPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(guardPoint.position, 0.5f);
            }
            if (awayPoints == null) return;
            Gizmos.color = Color.cyan;
            foreach (var p in awayPoints)
            {
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.4f);
            }
        }
    }
}
