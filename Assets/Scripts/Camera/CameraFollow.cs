using UnityEngine;

/// <summary>
/// Makes the camera follow a target object with optional smoothing and offset.
/// Updates camera position in editor when offset values change.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The target the camera should follow")]
    [SerializeField] private Transform m_Target;
    
    [Tooltip("Offset from the target position")]
    [SerializeField] private Vector3 m_Offset = new Vector3(0, 10, -10);
    
    [Header("Follow Settings")]
    [Tooltip("How quickly the camera moves to follow the target")]
    [SerializeField] private float m_SmoothTime = 0.25f;
    
    // References
    private Transform m_Transform;
    
    // Smoothing
    private Vector3 m_CurrentVelocity = Vector3.zero;
    
    /// <summary>
    /// Initialize component references
    /// </summary>
    private void Awake()
    {
        m_Transform = transform;
        
        // If no target is assigned in inspector, try to find the BaseCharacter
        if (m_Target == null)
        {
            GameObject targetObj = GameObject.Find("BaseCharacter");
            if (targetObj != null)
            {
                m_Target = targetObj.transform;
                Debug.Log("CameraFollow automatically found and assigned BaseCharacter as target.");
            }
            else
            {
                Debug.LogWarning("CameraFollow has no target assigned and couldn't find BaseCharacter.");
            }
        }
    }
    
    /// <summary>
    /// Update camera position to follow target
    /// </summary>
    private void LateUpdate()
    {
        if (m_Target == null)
            return;
            
        // Calculate the desired position based on target position and offset
        Vector3 targetPosition = m_Target.position + m_Offset;
        
        // Smoothly move the camera towards the target position
        m_Transform.position = Vector3.SmoothDamp(
            m_Transform.position, 
            targetPosition, 
            ref m_CurrentVelocity, 
            m_SmoothTime
        );
        
    }
}
