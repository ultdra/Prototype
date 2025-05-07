using UnityEngine;
namespace VillagerStates
{
    public class SleepingState : IVillagerState
    {
        public string Name => "Sleeping";

        public void Enter(VillagerBehavior villager)
        {
            // If not at bed, move to bed first
            if (villager.AssignedBed != null && Vector3.Distance(villager.transform.position, villager.AssignedBed.SleepPosition) > villager.SleepingSpotProximity)
            {
                villager.SetDestination(villager.AssignedBed.SleepPosition);
                villager.ChangeState("Walking");
                return;
            }
            villager.StopMoving();
            // Optionally: play sleeping animation
            // villager.Animator.SetTrigger("Sleep");
            villager.OnSleepStarted();
        }

        public void Update(VillagerBehavior villager)
        {
            // Only sleep if at bed
            if (villager.AssignedBed != null && Vector3.Distance(villager.transform.position, villager.AssignedBed.SleepPosition) > villager.SleepingSpotProximity)
            {
                // If not at bed, move to bed
                villager.SetDestination(villager.AssignedBed.SleepPosition);
                villager.ChangeState("Walking");
                return;
            }

            // Wake up at 0800
            if (villager.ShouldWakeUpNow())
            {
                villager.ChangeState("Idle");
            }
        }

        public void Exit(VillagerBehavior villager)
        {
            // Optionally: clean up sleeping state
            villager.OnSleepEnded();
        }
    }
}