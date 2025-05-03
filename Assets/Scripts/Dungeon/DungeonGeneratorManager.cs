using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeon
{

    public class DungeonGraph
    {
        public Dictionary<int, Dungeon> DungeonNodes = new Dictionary<int, Dungeon>();
        public int BossId = -1;
    }

    public class DungeonGeneratorManager : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int m_DungeonSize;

        private Dungeon[] m_StartDungeons;
        private Dungeon[] m_CombatDungeons;
        private Dungeon[] m_BossDungeons;

        // These can be shifted to a scriptable object later on but for now this is okay
        [SerializeField]
        private int m_MainPathLength = 0;

        [SerializeField]
        private int m_BranchChance = 3000;

        [SerializeField]
        private int m_MaxBranchLength = 2;

        [SerializeField]
        private float m_SpacingBetweenDungeons = 30f;

        private DungeonGraph m_DungeonGraph = new DungeonGraph();
        private int m_CurrentAssignedId = 0;
        private HashSet<Vector2Int> m_AssignedDungeonPositions = new HashSet<Vector2Int>();

        private List<DungeonData> m_MainDungeonPath = new List<DungeonData>();
        private List<DungeonData> m_AllDungeons = new List<DungeonData>();

        private List<Vector2Int> m_Directions = new List<Vector2Int>{
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        

        void Start()
        {
            // Load all dungeon prefabs from the specified folders
            m_StartDungeons = Resources.LoadAll<Dungeon>("Prefabs/Dungeons/Start");
            m_CombatDungeons = Resources.LoadAll<Dungeon>("Prefabs/Dungeons/Combat");
            m_BossDungeons = Resources.LoadAll<Dungeon>("Prefabs/Dungeons/Boss");

            int totalCount = m_DungeonSize.x * m_DungeonSize.y;

            GenerateDungeon();
        }

        /// <summary>
        /// This will handle the generation of the dungeons.
        /// This will consider the rules set by the user and generate accordingly to the dungeon size and bounds
        /// 
        /// Must achieve:
        ///     1) Fufil dungeon count
        ///     2) Fufil number of end rooms
        /// </summary>
        private void GenerateDungeon()
        {
            m_MainDungeonPath = new List<DungeonData>();
            m_AllDungeons = new List<DungeonData>();
            m_DungeonGraph = new DungeonGraph();
            m_CurrentAssignedId = 0;
            m_AssignedDungeonPositions.Clear();

            SpawnMainNodes();
            SpawnBranchNodes();
        }

        private void SpawnMainNodes()
        {
            Vector2Int currentPos = Vector2Int.zero;
            DungeonData startData = new DungeonData(m_CurrentAssignedId++, true, currentPos, DungeonType.START);
            m_AssignedDungeonPositions.Add(currentPos);
            m_MainDungeonPath.Add(startData);
            m_AllDungeons.Add(startData);

            DungeonData previousData = startData;

            // There will be 1 right path
            // -2 as we will will handle boss room seperately out of the loop for now.
            for(int i = 0; i < m_MainPathLength - 2; ++i)
            {
                Vector2Int? nextPos = GetEmptyNeighbour(currentPos);
                if(!nextPos.HasValue) break; // Cannot expand, but might be wrong for this

                currentPos = nextPos.Value;
                DungeonData nextData = new DungeonData(m_CurrentAssignedId++, true, currentPos, DungeonType.COMBAT);
                
                ConnectDungeons(nextData, previousData);

                previousData = nextData;

                m_AssignedDungeonPositions.Add(currentPos);
                m_MainDungeonPath.Add(nextData);
                m_AllDungeons.Add(nextData);
            }

            //this is to place the boss room at the end
            Vector2Int? bossPos = GetEmptyNeighbour(currentPos);
            if(!bossPos.HasValue)
            {
                Debug.LogError("No boss room");
            }

            DungeonData bossData = new DungeonData(m_CurrentAssignedId++, true, bossPos.Value, DungeonType.BOSS);
            m_AssignedDungeonPositions.Add(bossPos.Value);

            ConnectDungeons(bossData, previousData);
            
            m_AssignedDungeonPositions.Add(bossPos.Value);
            m_MainDungeonPath.Add(bossData);
            m_AllDungeons.Add(bossData);
        }

        private void SpawnBranchNodes()
        {
            foreach(DungeonData node in m_MainDungeonPath)
            {
                int branchchance = Random.Range(0, 10000);

                if(branchchance < m_BranchChance)
                {
                    //Means we will be branching a node.
                    Vector2Int? branchPos = GetEmptyNeighbour(node.DungeonCoord);
                    if(branchPos.HasValue)
                    {
                        DungeonData branchData = new DungeonData(m_CurrentAssignedId++, false, branchPos.Value, DungeonType.COMBAT);
                        
                        ConnectDungeons(branchData, node);
                        m_AssignedDungeonPositions.Add(branchPos.Value);
                        m_AllDungeons.Add(branchData);
                    }
                }
            }
        }

        private void ConnectDungeons(DungeonData currentNode, DungeonData previousNode)
        {
            currentNode.ConnectToDungeon(previousNode.Id);
            previousNode.ConnectToDungeon(currentNode.Id);
        }

        private void InstantiateDungeons()
        {
            Dungeon[] selectedType = null;

            foreach(DungeonData data in m_AllDungeons)
            {
                switch(data.DungeonType)
                {
                    case DungeonType.START:
                    selectedType = m_StartDungeons;
                    break;
                    case DungeonType.COMBAT:
                    selectedType = m_CombatDungeons;
                    break;  
                    case DungeonType.BOSS:
                    selectedType = m_BossDungeons;
                    break;
                }
            }

            
            // Check if array is null or empty
            if (selectedType == null || selectedType.Length == 0)
            {
                Debug.LogError("No start dungeons available!");
                return;
            }

            // Get random dungeon from array
            int randomIndex = Random.Range(0, selectedType.Length);
            
            // Create and return a copy
            Instantiate(selectedType[randomIndex]);
        }

        private Vector2Int? GetEmptyNeighbour(Vector2Int pos)
        {
            m_Directions = m_Directions.OrderBy(x => Random.value).ToList(); //Random

            foreach(Vector2Int dir in m_Directions)
            {
                Vector2Int neighbourPos = pos + dir;
                if(!m_AssignedDungeonPositions.Contains(neighbourPos))
                {
                    return neighbourPos;
                }
            }

            return null;
        }
    }


}

