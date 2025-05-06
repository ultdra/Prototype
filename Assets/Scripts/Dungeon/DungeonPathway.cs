using UnityEngine;
using System;

namespace Dungeon
{
    public enum DungeonPathwayDirection
    {
        NONE = -1,
        UP = 0,
        DOWN,
        LEFT,
        RIGHT
    }

    public class DungeonPathway : MonoBehaviour
    {
        [SerializeField]
        private DungeonPathwayDirection m_Direction;
        public event Action<DungeonPathwayDirection> OnPathwayTriggered;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnPathwayTriggered?.Invoke(m_Direction);
            }
        }
    }
}

