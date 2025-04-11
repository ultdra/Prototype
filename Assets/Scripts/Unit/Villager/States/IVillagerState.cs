using System;

namespace VillagerStates
{
    public interface IVillagerState
    {
        /// <summary>
        /// Called when entering the state.
        /// </summary>
        void Enter(VillagerBehavior villager);

        /// <summary>
        /// Called every frame while in this state.
        /// </summary>
        void Update(VillagerBehavior villager);

        /// <summary>
        /// Called when exiting the state.
        /// </summary>
        void Exit(VillagerBehavior villager);

        /// <summary>
        /// The name of the state (for debugging/logging).
        /// </summary>
        string Name { get; }
    }
}