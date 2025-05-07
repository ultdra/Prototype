using System.Collections.Generic;
using UnityEngine;

public class BedManager : MonoBehaviour
{
    public static BedManager Instance { get; private set; }

    private List<Bed> availableBeds = new List<Bed>();
    private List<Bed> occupiedBeds = new List<Bed>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void RegisterBed(Bed bed)
    {
        if (bed == null) return;
        
        if (!availableBeds.Contains(bed) && !occupiedBeds.Contains(bed))
        {
            availableBeds.Add(bed);
        }
    }

    public void UnregisterBed(Bed bed)
    {
        if (bed == null) return;
        
        availableBeds.Remove(bed);
        occupiedBeds.Remove(bed);
    }

    public Bed GetAvailableBed()
    {
        if (availableBeds.Count == 0) return null;
        
        Bed bed = availableBeds[0];
        availableBeds.Remove(bed);
        occupiedBeds.Add(bed);
        return bed;
    }

    public void ReleaseBed(Bed bed)
    {
        if (bed == null) return;
        
        if (occupiedBeds.Contains(bed))
        {
            occupiedBeds.Remove(bed);
            availableBeds.Add(bed);
        }
    }
}