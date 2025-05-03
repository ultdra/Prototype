using System.Collections.Generic;
using UnityEngine;


namespace Dungeon
{
    public enum DungeonDifficulty
    {
        EASY = 0,
        MEDIUM,
        HARD
    }

    public enum DungeonType
    {
        START = 0,
        COMBAT,
        BOSS
    }

    public class Dungeon : MonoBehaviour
    {
        DungeonData m_DungeonData;

        [SerializeField]
        private DungeonPathway[] m_Pathway; // This is for connecting with the gateways

        public DungeonData Data => m_DungeonData;

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


