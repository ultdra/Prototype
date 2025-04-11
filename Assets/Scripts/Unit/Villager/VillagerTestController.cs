using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class VillagerTestController : MonoBehaviour
{
    public VillagerBehavior m_Villager;
    public Transform m_SleepingSpot;
    
    // Optional test parameters
    public bool m_AutoGenerateNavMesh = true;
    public bool m_CreateTestPoints = true;
    public int m_NumberOfTestPoints = 5;
    public float m_TestPointDistance = 3.0f;
    
    // Test points we'll create
    private Transform[] m_TestPoints;
    
    private void Start()
    {
        // Create navigation mesh at runtime
        if (m_AutoGenerateNavMesh)
        {
            StartCoroutine(BuildNavMeshDelayed());
        }
        
        // Create test points for testing navigation
        if (m_CreateTestPoints)
        {
            CreateTestPoints();
        }
        
        // Initialize the villager
        InitializeVillager();
    }
    
    private IEnumerator BuildNavMeshDelayed()
    {
        // Give Unity a moment to initialize everything
        yield return new WaitForSeconds(0.5f);
        
        // Find the ground
        GameObject ground = GameObject.Find("Ground");
        
        if (ground != null)
        {
            // Ensure it's on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(ground.transform.position, out hit, 1.0f, NavMesh.AllAreas))
            {
                Debug.Log("NavMesh present under Ground object");
            }
            else
            {
                Debug.LogWarning("No NavMesh found under Ground object. Please bake a NavMesh in the Editor.");
            }
        }
    }
    
    private void CreateTestPoints()
    {
        m_TestPoints = new Transform[m_NumberOfTestPoints];
        
        // Create parent object for test points
        GameObject testPointsParent = new GameObject("TestPoints");
        testPointsParent.transform.position = Vector3.zero;
        
        // Create test points in a circle
        for (int i = 0; i < m_NumberOfTestPoints; i++)
        {
            float angle = i * (360f / m_NumberOfTestPoints);
            float radian = angle * Mathf.Deg2Rad;
            
            // Position in a circle
            Vector3 position = new Vector3(
                Mathf.Sin(radian) * m_TestPointDistance,
                0.1f, // slightly above ground
                Mathf.Cos(radian) * m_TestPointDistance
            );
            
            // Create the test point
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.name = "TestPoint_" + i;
            point.transform.position = position;
            point.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            point.transform.parent = testPointsParent.transform;
            
            // Store the reference
            m_TestPoints[i] = point.transform;
            
            // Optional: Add different colored materials to distinguish points
            Renderer renderer = point.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Create a new material with a random color
                Material material = new Material(Shader.Find("Standard"));
                Color randomColor = new Color(
                    Random.Range(0.3f, 1f), 
                    Random.Range(0.3f, 1f), 
                    Random.Range(0.3f, 1f)
                );
                material.color = randomColor;
                renderer.material = material;
            }
        }
    }
    
    private void InitializeVillager()
    {
        if (m_Villager != null)
        {
            // Connect the sleeping spot if assigned
            if (m_SleepingSpot != null)
            {
                m_Villager.SleepingSpot = m_SleepingSpot;
            }
            
            // Optional: Configure additional villager properties
            m_Villager.IdleMovementRadius = 5f;
            m_Villager.SleepChance = 0.2f;
            
            Debug.Log("Villager initialized with test controller");
        }
        else
        {
            Debug.LogWarning("No Villager assigned to VillagerTestController");
        }
    }
    
    // UI for testing
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 300));
        
        GUILayout.Label("Villager Test Controls");
        
        if (m_Villager != null)
        {
            // Display current state
            GUILayout.Label($"Current State: {m_Villager.CurrentState}");
            
            // Buttons for state transitions
            if (GUILayout.Button("Force Idle"))
            {
                m_Villager.TransitionToState(VillagerState.Idle);
            }
            
            if (GUILayout.Button("Go To Sleep"))
            {
                if (m_SleepingSpot != null)
                {
                    m_Villager.SetDestination(m_SleepingSpot.position);
                    m_Villager.TransitionToState(VillagerState.Walking);
                }
                else
                {
                    Debug.LogWarning("No sleeping spot assigned");
                }
            }
            
            // Button to go to random test point
            if (m_TestPoints != null && m_TestPoints.Length > 0)
            {
                if (GUILayout.Button("Go To Random Point"))
                {
                    Transform randomPoint = m_TestPoints[Random.Range(0, m_TestPoints.Length)];
                    m_Villager.SetDestination(randomPoint.position);
                    m_Villager.TransitionToState(VillagerState.Walking);
                }
            }
        }
        
        GUILayout.EndArea();
    }
}