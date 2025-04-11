using UnityEngine;
namespace VillagerStates
{
    public class PrayingState : IVillagerState
    {
        private float prayStartTime;
        private const float PrayDurationHours = 1f;

        public string Name => "Praying";

        public void Enter(VillagerBehavior villager)
        {
            // Move to shrine if not already there
            if (!villager.IsAtShrine())
            {
                villager.MoveToShrine();
            }
            prayStartTime = villager.GetCurrentGameHour();
            // Optionally: play praying animation
            // villager.Animator.SetTrigger("Pray");
        }

        public void Update(VillagerBehavior villager)
        {
            // Wait until at shrine, then start praying
            if (!villager.IsAtShrine())
                return;

            float currentHour = villager.GetCurrentGameHour();
            if (currentHour - prayStartTime >= PrayDurationHours)
            {
                villager.ChangeState("Idle");
            }
        }

        public void Exit(VillagerBehavior villager)
        {
            // Optionally: clean up praying state
        }
    }
}