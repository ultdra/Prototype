using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{

    public class DungeonGraph
    {
        public Dictionary<int, Dungeon> DungeonNodes = new Dictionary<int, Dungeon>();
        public int StartId = -1;
        public int BossId = -1;
    }

    public class DungeonGeneratorManager : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int m_DungeonSize;

        private Dungeon[] m_DungeonObjects;

        [SerializeField]
        private int m_NumberOfEndRooms;

        [SerializeField]
        private int m_TotalDungeonCount;


         
       
        void Start()
        {
            // Load all dungeon prefabs from the specified folders
            var startDungeons = Resources.LoadAll<Dungeon>("Prefabs/Dungeons/Start");
            var combatDungeons = Resources.LoadAll<Dungeon>("Prefabs/Dungeons/Combat");
            var bossDungeons = Resources.LoadAll<Dungeon>("Prefabs/Dungeons/Boss");
            
            // Combine all loaded prefabs using List
            var combinedList = new System.Collections.Generic.List<Dungeon>();
            combinedList.AddRange(startDungeons);
            combinedList.AddRange(combatDungeons);
            combinedList.AddRange(bossDungeons);
            m_DungeonObjects = combinedList.ToArray();
            
            // Log loaded prefab names for debugging
            foreach (var dungeonObj in m_DungeonObjects)
            {
                Debug.Log($"Loaded dungeon prefab: {dungeonObj.name}");
            }
            
            if (m_DungeonObjects == null || m_DungeonObjects.Length == 0)
            {
                Debug.LogWarning("No dungeon prefabs found in Resources/Prefabs/Dungeons/Easy");
            }

            int totalCount = m_DungeonSize.x * m_DungeonSize.y;
            if(m_TotalDungeonCount >= totalCount)
            {
                m_TotalDungeonCount = totalCount;
            }
        }

        /// <summary>
        /// This will handle the generation of the dungeons.
        /// This will consider the rules set by the user and generate accordingly to the dungeon size and bounds
        /// 
        /// Must achieve:
        ///     1) Fufil dungeon count
        ///     2) Fufil number of end rooms
        /// </summary>
        void GenerateDungeon()
        {

        }
    }


}

