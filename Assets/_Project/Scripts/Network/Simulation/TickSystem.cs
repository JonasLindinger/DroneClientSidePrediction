using System;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public class TickSystem
    {
        // C# Event. Get's called every tick and contains the current Tick
        public Action<uint> OnTick = delegate {  };
        
        // Current Tick we are at.
        public uint CurrentTick { get; private set; }
        // The Tick rate we run at.
        public int TickRate { get; private set; }
        // The Time Between Ticks in miliseconds
        public float TimeBetweenTicks { get; private set; }
        
        // The Time parsed between the last tick and now
        private float _time;
        
        // Tick Adjustment
        private int _ticksToSkip;
        
        /// <summary>
        /// Please parse in the TickRate the System should run at and if you want, you can also parse the Starting Tick (Offset)
        /// </summary>
        /// <param name="tickRate"></param>
        /// <param name="startingTick"></param>
        public TickSystem(int tickRate, uint startingTick = 0)
        {
            // Setting the TickRate and calculating the Time Between Ticks.
            TickRate = tickRate;
            TimeBetweenTicks = 1f / tickRate;
            
            // Setting the starting Tick (default 0)
            CurrentTick = startingTick;
        }

        /// <summary>
        /// Updates the Tick System. This method has to be called as often as possible for presice calculations.
        /// </summary>
        /// <param name="deltaTime">The Time Betwwen the last Update</param>
        public void Update(float deltaTime)
        {
            // Increase the Time between last Tick and now
            _time += deltaTime;

            // If the Time betwwen last Tick and now is greater than the Time Between Ticks,
            // we should run a tick and decrease the time between the last Tick by the Time Between Ticks.
            if (_time >= TimeBetweenTicks)
            {
                _time -= TimeBetweenTicks;

                // Check if we should skip ticks
                if (_ticksToSkip > 0)
                {
                    _ticksToSkip--;
                    return;
                }
                
                // Increase tick and run the tick
                CurrentTick++;
                OnTick?.Invoke(CurrentTick);
            }
        }

        /// <summary>
        /// Set the Ammount of ticks we should skip
        /// </summary>
        /// <param name="ammount"></param>
        public void SkipTick(int ammount)
        {
            _ticksToSkip = ammount;
        }

        /// <summary>
        /// When running this, we instantly run the next few ticks and set the ticks to skip to zero.
        /// </summary>
        /// <param name="ammount"></param>
        public void CalculateExtraTicks(int ammount)
        {
            _ticksToSkip = 0;

            for (int i = 0; i < ammount; i++)
            {
                CurrentTick++;
                OnTick?.Invoke(CurrentTick);
            }
        }
    }
}