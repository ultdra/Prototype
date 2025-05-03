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
        private int m_Id;
        private Vector2Int m_DungeonCoord;
        private bool m_RoomCleared;

        private bool m_IsMainPath;
        private List<int> m_ConnectedDungeonIds = new List<int>();

        [SerializeField]
        private DungeonDifficulty m_Difficulty;

        [SerializeField]
        private DungeonType m_RoomType;

        [SerializeField]
        private DungeonPathway[] m_Pathway;

        public int Id => m_Id;
        public Vector2Int DungeonCoord => m_DungeonCoord;
        public bool RoomCleared => m_RoomCleared;


        
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
            Vector2Int nextRoom = m_DungeonCoord;
            switch(direction)
            {
                case DungeonPathwayDirection.UP:
                    nextRoom = new Vector2Int(m_DungeonCoord.x, m_DungeonCoord.y + 1);
                break;

                case DungeonPathwayDirection.DOWN:
                    nextRoom = new Vector2Int(m_DungeonCoord.x, m_DungeonCoord.y - 1);

                break;

                case DungeonPathwayDirection.LEFT:
                    nextRoom = new Vector2Int(m_DungeonCoord.x + 1, m_DungeonCoord.y);

                break;

                case DungeonPathwayDirection.RIGHT:
                    nextRoom = new Vector2Int(m_DungeonCoord.x - 1, m_DungeonCoord.y);
                break;
            }

            // Trigger transition to the next room
        }


        public void SetupDungeon(int id, bool isMainPath, Vector2Int coord)
        {
            m_Id = id;
            m_IsMainPath = isMainPath;
            m_DungeonCoord = coord;
            string prefix = isMainPath? "" : "BRANCH_";
            gameObject.name = $"{prefix}{m_RoomType}_{id}";
        }

        public void ConnectToDungeon(int id)
        {
            m_ConnectedDungeonIds.Add(id);
        }

    }
}


