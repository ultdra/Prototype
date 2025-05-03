using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class DungeonData 
    {
        private int m_Id;
        private Vector2Int m_DungeonCoord;
        private bool m_RoomCleared;
        private bool m_IsMainPath;
        private List<int> m_ConnectedDungeonIds;
        private DungeonType m_DungeonType;

        public int Id => m_Id;
        public Vector2Int DungeonCoord => m_DungeonCoord;
        public bool RoomCleared => m_RoomCleared;
        public DungeonType DungeonType => m_DungeonType;

        // Instantiating data
        public DungeonData(int id, bool isMainPath, Vector2Int coord, DungeonType type)
        {
            m_Id = id;
            m_IsMainPath = isMainPath;
            m_DungeonCoord = coord;
            m_DungeonType = type;

            m_RoomCleared = false;
            m_ConnectedDungeonIds = new List<int>();
        }

        public void ConnectToDungeon(int id)
        {
            m_ConnectedDungeonIds.Add(id);
        }
    }
}
