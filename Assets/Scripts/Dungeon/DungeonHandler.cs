using UnityEngine;


namespace Dungeon
{
    public enum DungeonDifficulty
    {
        EASY = 0,
        MEDIUM,
        HARD
    }

    public class DungeonHandler : MonoBehaviour
    {
        private Vector2 m_DungeonCoord;
        
        [SerializeField]
        private DungeonDifficulty m_Difficulty;

        [SerializeField]
        private DungeonPathway[] m_Pathway;

        private void Start()
        {
            foreach (var path in m_Pathway)
            {
                path.OnPathwayTriggered += HandlePathwayTriggered;
            }
        }

        private void OnDestroy()
        {
            foreach (var path in m_Pathway)
            {
                path.OnPathwayTriggered -= HandlePathwayTriggered;
            }
        }

        private void HandlePathwayTriggered(DungeonPathwayDirection direction)
        {
            // Handle pathway trigger event here
            Debug.Log("Pathway triggered with directions: " + string.Join(", ", direction));
        }
    }
}


