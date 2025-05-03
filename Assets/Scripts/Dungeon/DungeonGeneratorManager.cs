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

        [SerializeField]
        private int m_NumberOfEndRooms;

        [SerializeField]
        private int m_TotalDungeonCount;

        // These can be shifted to a scriptable object later on but for now this is okay
        [SerializeField]
        private int m_MainPathLength = 0;
        [SerializeField]
        private int m_BranchChance = 3000;
        [SerializeField]
        private int m_MaxBranchLength = 2;

        private DungeonGraph m_DungeonGraph = new DungeonGraph();
        private int m_CurrentAssignedId = 0;
        private HashSet<Vector2Int> m_AssignedDungeonPositions = new HashSet<Vector2Int>();
        private Dictionary<int, Dungeon> m_ExistingDungeons = new Dictionary<int, Dungeon>();

        private List<Dungeon> m_MainDungeonPath = new List<Dungeon>();

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
            if(m_TotalDungeonCount >= totalCount)
            {
                m_TotalDungeonCount = totalCount;
            }

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
            m_ExistingDungeons = new Dictionary<int, Dungeon>();
            m_MainDungeonPath = new List<Dungeon>();
            m_DungeonGraph = new DungeonGraph();
            m_CurrentAssignedId = 0;
            m_AssignedDungeonPositions.Clear();

            SpawnMainNodes();
            SpawnBranchNodes();
        }

        private void SpawnMainNodes()
        {
            Vector2Int currentPos = Vector2Int.zero;
            Dungeon startNode = GetRandomDungeon(DungeonType.START);
            startNode.SetupDungeon(m_CurrentAssignedId++, true, currentPos);
            m_AssignedDungeonPositions.Add(currentPos);
            m_MainDungeonPath.Add(startNode);

            Dungeon previousNode = startNode;

            // There will be 1 right path
            // -2 as we will will handle boss room seperately out of the loop for now.
            for(int i = 0; i < m_MainPathLength - 2; ++i)
            {
                Vector2Int? nextPos = GetEmptyNeighbour(currentPos);
                if(!nextPos.HasValue) break; // Cannot expand, but might be wrong for this

                currentPos = nextPos.Value;
                Dungeon newNode = GetRandomDungeon(DungeonType.COMBAT);
                newNode.SetupDungeon(m_CurrentAssignedId++, true, currentPos);
                newNode.transform.position = new Vector3{x = currentPos.x * 30f, y = 0, z = currentPos.y * 30f};
                previousNode.ConnectToDungeon(newNode.Id);
                newNode.ConnectToDungeon(previousNode.Id);
                m_MainDungeonPath.Add(newNode);
            }

            //this is to place the boss room at the end
            Vector2Int? bossPos = GetEmptyNeighbour(currentPos);
            if(!bossPos.HasValue)
            {
                Debug.LogError("No boss room");
            }
            Dungeon bossNode = GetRandomDungeon(DungeonType.BOSS);
            bossNode.SetupDungeon(m_CurrentAssignedId++, true, bossPos.Value);
            bossNode.transform.position = new Vector3{x = bossPos.Value.x * 30f, y = 0, z = bossPos.Value.y * 30f};
            bossNode.ConnectToDungeon(previousNode.Id);
            previousNode.ConnectToDungeon(bossNode.Id);
            m_MainDungeonPath.Add(bossNode);
        }

        private void SpawnBranchNodes()
        {
            foreach(Dungeon node in m_MainDungeonPath)
            {
                int branchchance = Random.Range(0, 10000);

                if(branchchance < m_BranchChance)
                {
                    //Means we will be branching a node.
                    Vector2Int? branchPos = GetEmptyNeighbour(node.DungeonCoord);
                    if(branchPos.HasValue)
                    {
                        Dungeon branchNode = GetRandomDungeon(DungeonType.COMBAT);
                        branchNode.SetupDungeon(m_CurrentAssignedId++, false, branchPos.Value);
                        branchNode.transform.position = new Vector3{x = branchPos.Value.x * 30f, y = 0, z = branchPos.Value.y * 30f};
                        branchNode.ConnectToDungeon(node.Id);
                        node.ConnectToDungeon(branchNode.Id);
                    }
                }
            }
        }

        private Dungeon GetRandomDungeon(DungeonType type)
        {
            Dungeon[] selectedType = null;
            switch(type)
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
            // Check if array is null or empty
            if (selectedType == null || selectedType.Length == 0)
            {
                Debug.LogError("No start dungeons available!");
                return null;
            }

            // Get random dungeon from array
            int randomIndex = Random.Range(0, selectedType.Length);
            
            // Create and return a copy
            return Instantiate(selectedType[randomIndex]);
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

