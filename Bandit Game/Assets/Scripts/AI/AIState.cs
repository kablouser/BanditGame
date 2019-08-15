using UnityEngine;
using System;

namespace AI
{
    public abstract class AIState
    {
        public AIMovement movement;

        protected AIState(AIMovement movement)
        {
            this.movement = movement;
        }

        public abstract Type Tick();
    }

    public struct RandomRange
    {
        public float average, variation;

        public RandomRange(float average, float variation)
        {
            this.average = average;
            this.variation = variation;
        }

        public float GenerateRandom()
        {
            return UnityEngine.Random.Range(average - variation / 2.0f, average + variation / 2.0f);
        }
    }
}