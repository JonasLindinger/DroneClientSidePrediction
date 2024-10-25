using System;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public class TickSystem
    {
        public Action<uint> OnTick = delegate {  };
        
        public uint CurrentTick { get; private set; }
        public int TickRate { get; private set; }
        public float TimeBetweenTicks { get; private set; }
        
        private float _time;
        
        // Tick Adjustment
        private int _ticksToSkip;
        
        public TickSystem(int tickRate, uint startingTick = 0)
        {
            TickRate = tickRate;
            TimeBetweenTicks = 1f / tickRate;
            
            CurrentTick = startingTick;
        }

        public void Update(float deltaTime)
        {
            _time += deltaTime;

            if (_time >= TimeBetweenTicks)
            {
                _time -= TimeBetweenTicks;

                if (_ticksToSkip > 0)
                {
                    _ticksToSkip--;
                    return;
                }
                
                CurrentTick++;
                OnTick?.Invoke(CurrentTick);
            }
        }

        public void SkipTick(int ammount)
        {
            _ticksToSkip = ammount;
        }

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