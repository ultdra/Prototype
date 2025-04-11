using UnityEngine;
namespace VillagerStates
{
    public class WalkingState : IVillagerState
    {
        public string Name => "Walking";

        public void Enter(VillagerBehavior villager)
        {
            // Optionally: play walking animation
            // villager.Animator.SetTrigger("Walk");
            villager.ResumeMoving();
        }

        public void Update(VillagerBehavior villager)
        {
            // If it's time to sleep, interrupt walking and go to sleep
            if (villager.ShouldSleepNow())
            {
                villager.ChangeState("Sleeping");
                return;
            }

            // Check if reached destination
            if (villager.HasReachedDestination())
            {
                villager.OnArrivedAtDestination();
            }
        }

        public void Exit(VillagerBehavior villager)
        {
            // Optionally: clean up walking state
            villager.StopMoving();
        }
    }
}