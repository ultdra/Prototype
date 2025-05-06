using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


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

        private DungeonPathway[] m_Pathway; // This is for connecting with the gateways

        public DungeonData Data => m_DungeonData;

        private void Start()
        {
            m_Pathway = GetComponentsInChildren<DungeonPathway>();

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
            FadeController.Instance.FadeIn(0.25f, () =>
            {                
                //Improvement can be done with a additive scene load and handling
                FadeController.Instance.FadeOut(0.25f);
            });
        }

        public void AssignDungeonData(DungeonData data)
        {
            m_DungeonData = data;
        }
    }
}


