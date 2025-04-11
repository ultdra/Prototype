using UnityEngine;
namespace VillagerStates
{
    /// <summary>
    /// Stub for future build behavior. The villager will move to the nearest in-progress building.
    /// </summary>
    public class BuildState : IVillagerState
    {
        public string Name => "Build";

        public void Enter(VillagerBehavior villager)
        {
            // TODO: Implement logic to find and move to nearest in-progress building
            // villager.MoveToNearestInProgressBuilding();
        }

        public void Update(VillagerBehavior villager)
        {
            // TODO: Implement build logic
            // If building is complete or interrupted, transition to Idle
            villager.ChangeState("Idle");
        }

        public void Exit(VillagerBehavior villager)
        {
            // Cleanup if needed
        }
    }
}