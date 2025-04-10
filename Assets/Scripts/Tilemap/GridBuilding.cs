using UnityEngine;

public class GridBuilding : MonoBehaviour
{
    private bool m_Placed = false;
    public BoundsInt Area;

    public bool Placed => m_Placed;

    public void SetBuilding()
    {
        m_Placed = true;
    }
}
