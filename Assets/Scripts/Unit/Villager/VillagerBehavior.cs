using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class VillagerBehavior : BaseVillager
{
    // Idle behavior configuration
    public float IdleMovementRadius = 5f;
    public float IdleCheckFrequency = 1.5f;
    private float m_NextIdleCheckTime = 0f;
    private float m_IdleDuration = 0f;
    private float m_SleepDuration = 0f;
    
    // Sleep configuration
    [Header("Sleep Configuration")]
    [Tooltip("The transform representing the villager's sleeping spot.")]
    public Transform SleepingSpot;
    [Tooltip("Chance (0-1) that the villager will choose to sleep after idling.")]
    public float SleepChance = 0.2f;
    [Tooltip("Distance threshold to consider the villager has reached the sleeping spot.")]
    public float SleepingSpotProximity = 1.5f;
    
    // Debug visualization
    [Header("Debug Visualization")]
    [Tooltip("Show debug gizmos for villager behavior in the scene view.")]
    public bool ShowDebugGizmos = true;
    private Vector3 m_CurrentTarget;
    
    protected override void Start()
    {
        base.Start();
        
        // Start with idle behavior
        TransitionToState(VillagerState.Idle);
    }

    protected override void OnEnterState(VillagerState state)
    {
        base.OnEnterState(state);
        switch (state)
        {
            case VillagerState.Idle:
                m_IdleDuration = Random.Range(MinIdleTime, MaxIdleTime);
                break;
            case VillagerState.Sleeping:
                m_SleepDuration = Random.Range(MinSleepTime, MaxSleepTime);
                break;
        }
    }
    
    protected override void UpdateCurrentState()
    {
        base.UpdateCurrentState();
        
        // Additional state-specific behaviors
        switch (m_CurrentState)
        {
            case VillagerState.Idle:
                // Periodically check for random movement during idle
                if (Time.time >= m_NextIdleCheckTime)
                {
                    TryRandomIdleMovement();
                    m_NextIdleCheckTime = Time.time + IdleCheckFrequency;
                }
                break;
        }
    }
    
    protected override void CheckStateTransitions()
    {
        switch (m_CurrentState)
        {
            case VillagerState.Idle:
                // After idling for a while, determine next state
                if (m_StateTimer >= m_IdleDuration)
                {
                    // Chance to sleep if we have a sleeping spot
                    if (SleepingSpot != null && Random.value < SleepChance)
                    {
                        // Go to sleep spot
                        GoToSleepingSpot();
                    }
                    else
                    {
                        // Otherwise just wander around
                        TryRandomIdleMovement();
                    }
                }
                break;
                
            case VillagerState.Sleeping:
                // Wake up after sleeping for a while
                if (m_StateTimer >= m_SleepDuration)
                {
                    TransitionToState(VillagerState.Idle);
                }
                break;
        }
    }
    
    private void TryRandomIdleMovement()
    {
        Vector3 randomDest;
        if (GetRandomDestination(IdleMovementRadius, out randomDest))
        {
            m_CurrentTarget = randomDest;
            SetDestination(randomDest);
            TransitionToState(VillagerState.Walking);
        }
    }
    
    private void GoToSleepingSpot()
    {
        if (SleepingSpot != null)
        {
            m_CurrentTarget = SleepingSpot.position;
            if (SetDestination(SleepingSpot.position))
            {
                TransitionToState(VillagerState.Walking);
            }
        }
    }
    
    protected override void OnReachedDestination()
    {
        // If we arrived at sleeping spot, go to sleep
        if (m_CurrentState == VillagerState.Walking &&
            SleepingSpot != null &&
            Vector3.Distance(transform.position, SleepingSpot.position) < SleepingSpotProximity)
        {
            TransitionToState(VillagerState.Sleeping);
        }
        else
        {
            // Otherwise transition back to idle
            TransitionToState(VillagerState.Idle);
        }
    }
    
    // Debug drawing moved to VillagerDebugGizmos.cs
}