using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

// Police loop: guard the door, walk away, wait, come back. R = come back now, L = leave now.
[RequireComponent(typeof(NavMeshAgent))]
public class PoliceController : MonoBehaviour
{
    public enum PoliceState { Guard, WalkAway, WaitAway, Return }

    public Transform guardPoint;
    public Transform[] awayPoints;
    public float guardTime = 6f;
    public float waitAwayTime = 10f;
    public float walkSpeed = 2.5f;

    public PoliceState State { get; private set; }

    NavMeshAgent agent;
    float timer;
    int awayIndex;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
    }

    void Start()
    {
        Enter(PoliceState.Guard);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame) Enter(PoliceState.Return);
        if (keyboard != null && keyboard.lKey.wasPressedThisFrame) Enter(PoliceState.WalkAway);

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

    void Enter(PoliceState next)
    {
        State = next;
        timer = 0f;

        if (next == PoliceState.WalkAway && awayPoints != null && awayPoints.Length > 0)
        {
            agent.SetDestination(awayPoints[awayIndex % awayPoints.Length].position);
            awayIndex++;
        }
        else if (next == PoliceState.Return && guardPoint != null)
        {
            agent.SetDestination(guardPoint.position);
        }
        else
        {
            agent.ResetPath();
        }
    }

    bool Arrived()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f;
    }
}
