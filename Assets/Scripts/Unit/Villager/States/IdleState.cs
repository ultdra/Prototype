using UnityEngine;
namespace VillagerStates
{
    public class IdleState : IVillagerState
    {
        private float idleDuration;
        private float timer;

        public string Name => "Idle";

        public void Enter(VillagerBehavior villager)
        {
            // Randomize idle duration between 2-4 seconds
            idleDuration = Random.Range(2f, 4f);
            timer = 0f;
            villager.StopMoving();
            // Optionally: play idle animation
            // villager.Animator.SetTrigger("Idle");
        }

        public void Update(VillagerBehavior villager)
        {
            timer += Time.deltaTime;

            // If it's time to sleep (handled by VillagerBehavior), let it transition
            if (villager.ShouldSleepNow())
            {
                Debug.Log($"[IdleState] Villager transitioning to Sleeping at hour={villager.GetCurrentGameHour()}");
                villager.ChangeState("Sleeping");
                return;
            }

            // After idle duration, decide next action
            if (timer >= idleDuration)
            {
                villager.DecideNextAction();
            }
        }

        public void Exit(VillagerBehavior villager)
        {
            // Optionally: clean up idle state
        }
    }
}