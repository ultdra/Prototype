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

    public Transform ShrineSpot;
    public float IdleMovementRadius = 5f;
    public float SleepingSpotProximity = 1.5f;
    private Bed m_AssignedBed;

    // Time/DayNightCycle integration
    public DayNightCycleManager dayNightCycleManager;

    // Debug UI
    public bool ShowDebugUI = false;
    public TMPro.TextMeshProUGUI debugText;
    private Canvas debugCanvas;

    // Misc
    public bool ShowDebugGizmos = true;

    // Getter
    public Bed AssignedBed => m_AssignedBed;

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
        // If arrived at bed, go to sleep
        if (m_AssignedBed != null &&
            Vector3.Distance(transform.position, m_AssignedBed.SleepPosition) < SleepingSpotProximity)
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

    private bool TryAssignBed()
    {
        if (BedManager.Instance == null)
        {
            Debug.LogWarning("BedManager instance not found");
            return false;
        }
        
        m_AssignedBed = BedManager.Instance.GetAvailableBed();
        if (m_AssignedBed == null)
        {
            Debug.Log("No available beds");
            return false;
        }
        
        if (!m_AssignedBed.TryAssignVillager(this))
        {
            m_AssignedBed = null;
            return false;
        }
        
        return true;
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
        if (dayNightCycleManager == null) return false;
        
        float hour = dayNightCycleManager.CurrentHour;
        bool shouldSleep = hour >= 22f || (hour < 8f && !IsSleeping());
        
        if (shouldSleep && m_AssignedBed == null)
        {
            shouldSleep = TryAssignBed();
            if (shouldSleep)
            {
                SetDestination(m_AssignedBed.SleepPosition);
                ChangeState("Walking");
            }
        }
        
        return shouldSleep;
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

    public void OnSleepStarted()
    {
        if (m_AssignedBed != null)
        {
            Debug.Log($"Started sleeping in bed {m_AssignedBed.name}");
        }
    }
    
    public void OnSleepEnded()
    {
        if (m_AssignedBed != null)
        {
            Debug.Log($"Finished sleeping in bed {m_AssignedBed.name}");
            m_AssignedBed.Release();
            m_AssignedBed = null;
        }
    }

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