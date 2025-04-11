using UnityEngine;
using UnityEngine.AI;

public enum VillagerState
{
    Idle,
    Walking,
    Sleeping
}

public class BaseVillager : MonoBehaviour
{
    // Components
    protected NavMeshAgent m_NavMeshAgent;
    
    // State Management
    protected VillagerState m_CurrentState = VillagerState.Idle;
    protected float m_StateTimer = 0f;
    
    // Common properties
    public float WalkSpeed = 2.0f;
    public float RunSpeed = 4.0f;
    
    // State transition timers
    public float MinIdleTime = 2.0f;
    public float MaxIdleTime = 5.0f;
    public float MinSleepTime = 5.0f;
    public float MaxSleepTime = 10.0f;
    
    protected virtual void Awake()
    {
        // Get required components
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        
        // Add NavMeshAgent if not present
        if (m_NavMeshAgent == null)
        {
            m_NavMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            SetupNavMeshAgent();
        }
    }
    
    protected virtual void Start()
    {
        // Initialize with default state
        SetupNavMeshAgent();
        TransitionToState(VillagerState.Idle);

        m_NavMeshAgent.updateRotation = false;
        m_NavMeshAgent.updateUpAxis = false;
    }
    
    protected virtual void Update()
    {
        // Update current state
        UpdateCurrentState();
        
        // Check for state transitions
        CheckStateTransitions();
    }
    
    protected void SetupNavMeshAgent()
    {
        if (m_NavMeshAgent != null)
        {
            m_NavMeshAgent.speed = WalkSpeed;
            m_NavMeshAgent.angularSpeed = 120f;
            m_NavMeshAgent.acceleration = 8f;
            m_NavMeshAgent.stoppingDistance = 0.1f;
        }
    }
    
    protected virtual void UpdateCurrentState()
    {
        // Base behavior for each state
        switch (m_CurrentState)
        {
            case VillagerState.Idle:
                UpdateIdleState();
                break;
            case VillagerState.Walking:
                UpdateWalkingState();
                break;
            case VillagerState.Sleeping:
                UpdateSleepingState();
                break;
        }
        
        // Update timer
        m_StateTimer += Time.deltaTime;
    }
    
    protected virtual void CheckStateTransitions()
    {
        // Base transition logic - override in derived classes for specific behavior
    }
    
    // State Transition Method
    public virtual void TransitionToState(VillagerState newState)
    {
        // Exit previous state
        OnExitState(m_CurrentState);
        
        // Update state
        m_CurrentState = newState;
        m_StateTimer = 0f;
        
        // Enter new state
        OnEnterState(newState);
    }
    
    // State Enter/Exit Events
    protected virtual void OnEnterState(VillagerState state)
    {
        switch (state)
        {
            case VillagerState.Idle:
                if (m_NavMeshAgent != null)
                {
                    m_NavMeshAgent.isStopped = true;
                }
                Debug.Log($"{gameObject.name} entered Idle state");
                break;
            
            case VillagerState.Walking:
                if (m_NavMeshAgent != null)
                {
                    m_NavMeshAgent.isStopped = false;
                    m_NavMeshAgent.speed = WalkSpeed;
                }
                Debug.Log($"{gameObject.name} entered Walking state");
                break;
            
            case VillagerState.Sleeping:
                if (m_NavMeshAgent != null)
                {
                    m_NavMeshAgent.isStopped = true;
                }
                Debug.Log($"{gameObject.name} entered Sleeping state");
                break;
        }
    }
    
    protected virtual void OnExitState(VillagerState state)
    {
        // Clean up behavior when exiting states
    }
    
    // State Update Methods
    protected virtual void UpdateIdleState()
    {
        // Base idle behavior
    }
    
    protected virtual void UpdateWalkingState()
    {
        // Check if we've reached destination
        if (m_NavMeshAgent != null && !m_NavMeshAgent.pathPending)
        {
            if (m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
            {
                if (!m_NavMeshAgent.hasPath || m_NavMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    // We've reached our destination
                    OnReachedDestination();
                }
            }
        }
    }
    
    protected virtual void UpdateSleepingState()
    {
        // Base sleeping behavior
    }
    
    protected virtual void OnReachedDestination()
    {
        // Default behavior when reaching a destination
        TransitionToState(VillagerState.Idle);
    }
    
    // Navigation Helper Methods
    public virtual bool SetDestination(Vector3 target)
    {
        if (m_NavMeshAgent != null && m_NavMeshAgent.isOnNavMesh)
        {
            return m_NavMeshAgent.SetDestination(target);
        }
        return false;
    }
    
    // Get a random point within walkable distance
    protected virtual bool GetRandomDestination(float radius, out Vector3 result)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        
        result = Vector3.zero;
        return false;
    }
    
    // State Property
    public VillagerState CurrentState
    {
        get { return m_CurrentState; }
    }
}