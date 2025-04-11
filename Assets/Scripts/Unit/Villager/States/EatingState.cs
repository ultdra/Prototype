using UnityEngine;
namespace VillagerStates
{
    /// <summary>
    /// Stub for future eating behavior. The villager will find food from a food store to eat.
    /// </summary>
    public class EatingState : IVillagerState
    {
        public string Name => "Eating";

        public void Enter(VillagerBehavior villager)
        {
            // TODO: Implement logic to find and move to food store
            // villager.MoveToFoodStore();
        }

        public void Update(VillagerBehavior villager)
        {
            // TODO: Implement eating logic
            // If finished eating or interrupted, transition to Idle
            villager.ChangeState("Idle");
        }

        public void Exit(VillagerBehavior villager)
        {
            // Cleanup if needed
        }
    }
}