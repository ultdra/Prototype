using UnityEngine;

public class Bed : MonoBehaviour
{
    [SerializeField] private Transform sleepPosition;
    private BaseVillager assignedVillager;

    public bool IsOccupied => assignedVillager != null;
    public Vector3 SleepPosition => sleepPosition != null ? sleepPosition.position : transform.position;

    private void Start()
    {
        if (BedManager.Instance == null)
        {
            Debug.LogError("BedManager instance not found!");
            return;
        }
        BedManager.Instance.RegisterBed(this);
    }

    public bool TryAssignVillager(BaseVillager villager)
    {
        if (villager == null || IsOccupied || BedManager.Instance == null) 
        {
            Debug.LogWarning("Failed to assign villager to bed");
            return false;
        }
        
        assignedVillager = villager;
        BedManager.Instance.ReleaseBed(this);
        Debug.Log($"Assigned {villager.name} to bed {name}");
        return true;
    }

    public void Release()
    {
        if (assignedVillager == null || BedManager.Instance == null) return;
        
        Debug.Log($"Released {assignedVillager.name} from bed {name}");
        assignedVillager = null;
        BedManager.Instance.ReleaseBed(this);
    }

    private void OnDestroy()
    {
        if (BedManager.Instance != null)
        {
            BedManager.Instance.UnregisterBed(this);
        }
    }
}
