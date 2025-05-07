using UnityEngine;

[RequireComponent(typeof(VillagerBehavior))]
public class VillagerDebugGizmos : MonoBehaviour
{
    private VillagerBehavior villager;

    private void Awake()
    {
        villager = GetComponent<VillagerBehavior>();
    }

    private void OnDrawGizmos()
    {
        if (villager == null || !villager.ShowDebugGizmos)
            return;

        // Draw idle radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, villager.IdleMovementRadius);

        // Draw current target
        if (villager.CurrentState == VillagerState.Walking)
        {
            Gizmos.color = Color.red;
            var target = typeof(VillagerBehavior)
                .GetField("m_CurrentTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(villager);
            Gizmos.DrawSphere((Vector3)target, 0.2f);
            Gizmos.DrawLine(transform.position, (Vector3)target);
        }

        // Draw sleeping spot
        if (villager.AssignedBed != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(villager.AssignedBed.SleepPosition, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}