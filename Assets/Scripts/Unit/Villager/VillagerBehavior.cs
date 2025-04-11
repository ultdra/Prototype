using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class VillagerBehavior : BaseVillager
{
    // Idle behavior configuration
    public float IdleMovementRadius = 5f;
    public float IdleCheckFrequency = 1.5f;
    private float m_NextIdleCheckTime = 0f;
    
    // Sleep configuration
    public Transform SleepingSpot;
    public float SleepChance = 0.2f;
    
    // Debug visualization
    public bool ShowDebugGizmos = true;
    private Vector3 m_CurrentTarget;
    
    protected override void Start()
    {
        base.Start();
        
        // Start with idle behavior
        TransitionToState(VillagerState.Idle);
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
                if (m_StateTimer >= Random.Range(MinIdleTime, MaxIdleTime))
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
                if (m_StateTimer >= Random.Range(MinSleepTime, MaxSleepTime))
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
            Vector3.Distance(transform.position, SleepingSpot.position) < 1.5f)
        {
            TransitionToState(VillagerState.Sleeping);
        }
        else
        {
            // Otherwise transition back to idle
            TransitionToState(VillagerState.Idle);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!ShowDebugGizmos) return;
        
        // Draw idle radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, IdleMovementRadius);
        
        // Draw current target
        if (m_CurrentState == VillagerState.Walking)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_CurrentTarget, 0.2f);
            Gizmos.DrawLine(transform.position, m_CurrentTarget);
        }
        
        // Draw sleeping spot
        if (SleepingSpot != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(SleepingSpot.position, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}