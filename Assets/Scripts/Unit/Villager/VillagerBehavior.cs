using UnityEngine;
using System.Collections.Generic;
using VillagerStates;

/// <summary>
/// VillagerBehavior implements an extensible state machine for villager AI.
/// </summary>
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class VillagerBehavior : BaseVillager
{
    // State machine
    private Dictionary<string, IVillagerState> states;
    private IVillagerState currentState;

    // State references for convenience
    private IdleState idleState;
    private WalkingState walkingState;
    private SleepingState sleepingState;
    private BuildState buildState;
    private PrayingState prayingState;
    private EatingState eatingState;

    // State context
    public Vector3 CurrentTarget { get; private set; }
    public Transform SleepingSpot;
    public Transform ShrineSpot;
    public float IdleMovementRadius = 5f;
    public float SleepingSpotProximity = 1.5f;

    // Time/DayNightCycle integration
    public DayNightCycleManager dayNightCycleManager;

    // Debug UI
    public bool ShowDebugUI = false;
    public TMPro.TextMeshProUGUI debugText;
    private Canvas debugCanvas;

    // Misc
    public bool ShowDebugGizmos = true;

    protected override void Start()
    {
        base.Start();

        // Find DayNightCycleManager if not assigned
        if (dayNightCycleManager == null)
            dayNightCycleManager = FindFirstObjectByType<DayNightCycleManager>();

        // Initialize states
        idleState = new IdleState();
        walkingState = new WalkingState();
        sleepingState = new SleepingState();
        buildState = new BuildState();
        prayingState = new PrayingState();
        eatingState = new EatingState();

        states = new Dictionary<string, IVillagerState>
        {
            { idleState.Name, idleState },
            { walkingState.Name, walkingState },
            { sleepingState.Name, sleepingState },
            { buildState.Name, buildState },
            { prayingState.Name, prayingState },
            { eatingState.Name, eatingState }
        };

        ChangeState("Idle");
    }

    protected override void Update()
    {
        base.Update();
        if (currentState != null)
            currentState.Update(this);
    
        // Debug UI logic
        if (debugText == null)
        {
            // Try to find the debugText in children
            debugText = GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        }
        if (debugCanvas == null && debugText != null)
        {
            debugCanvas = debugText.GetComponentInParent<Canvas>(true);
        }
    
        if (debugText != null && debugCanvas != null)
        {
            if (ShowDebugUI)
            {
                debugCanvas.enabled = true;
                string stateName = currentState != null ? currentState.GetType().Name : "Unknown";
                string gameHour = dayNightCycleManager != null ? dayNightCycleManager.CurrentHour.ToString("F2") : "N/A";
                debugText.text = $"State: {stateName}\n" +
                                 $"Game Hour: {gameHour}\n" +
                                 $"Destination: {CurrentTarget}\n";
            }
            else
            {
                debugCanvas.enabled = false;
            }
        }
    }

    public void ChangeState(string stateName)
    {
        if (currentState != null)
            currentState.Exit(this);

        if (states.TryGetValue(stateName, out var nextState))
        {
            currentState = nextState;
            currentState.Enter(this);
        }
        else
        {
            Debug.LogError($"State '{stateName}' not found for villager.");
        }
    }

    // --- State transition helpers and context methods ---

    public void DecideNextAction()
    {
        // Example: randomly choose to walk, pray, eat, build, or idle again
        // For now, just walk to a random location
        TryRandomIdleMovement();
    }

    public void TryRandomIdleMovement()
    {
        Vector3 randomDest;
        if (GetRandomDestination(IdleMovementRadius, out randomDest))
        {
            CurrentTarget = randomDest;
            SetDestination(randomDest);
            ChangeState("Walking");
        }
        else
        {
            // If can't find a destination, stay idle
            ChangeState("Idle");
        }
    }

    public void OnArrivedAtDestination()
    {
        // If arrived at sleeping spot, go to sleep
        if (SleepingSpot != null && Vector3.Distance(transform.position, SleepingSpot.position) < SleepingSpotProximity)
        {
            ChangeState("Sleeping");
        }
        // If arrived at shrine, start praying
        else if (IsAtShrine())
        {
            ChangeState("Praying");
        }
        else
        {
            ChangeState("Idle");
        }
    }

    public bool HasReachedDestination()
    {
        return m_NavMeshAgent != null &&
               !m_NavMeshAgent.pathPending &&
               m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance &&
               (!m_NavMeshAgent.hasPath || m_NavMeshAgent.velocity.sqrMagnitude == 0f);
    }

    public void StopMoving()
    {
        if (m_NavMeshAgent != null)
            m_NavMeshAgent.isStopped = true;
    }

    public void ResumeMoving()
    {
        if (m_NavMeshAgent != null)
            m_NavMeshAgent.isStopped = false;
    }

    // --- Sleep/Time helpers ---

    public bool ShouldSleepNow()
    {
        // Sleep at 2200 (10pm) or later
        if (dayNightCycleManager != null)
        {
            float hour = dayNightCycleManager.CurrentHour;
            bool shouldSleep = hour >= 22f || (hour < 8f && !IsSleeping());
            return shouldSleep;
        }
        return false;
    }

    public bool ShouldWakeUpNow()
    {
        // Wake up at 0800 (8am)
        if (dayNightCycleManager != null)
        {
            float hour = dayNightCycleManager.CurrentHour;
            return hour >= 8f && hour < 22f;
        }
        return true;
    }

    public bool IsSleeping()
    {
        return currentState != null && currentState.Name == "Sleeping";
    }

    public void OnSleepStarted() { /* Optional: logic for when sleep starts */ }
    public void OnSleepEnded() { /* Optional: logic for when sleep ends */ }

    // --- Shrine/Praying helpers ---

    public bool IsAtShrine()
    {
        if (ShrineSpot == null) return false;
        return Vector3.Distance(transform.position, ShrineSpot.position) < 1.5f;
    }

    public void MoveToShrine()
    {
        if (ShrineSpot != null)
        {
            SetDestination(ShrineSpot.position);
            ChangeState("Walking");
        }
    }

    public float GetCurrentGameHour()
    {
        if (dayNightCycleManager != null)
            return dayNightCycleManager.CurrentHour;
        return 0f;
    }
}