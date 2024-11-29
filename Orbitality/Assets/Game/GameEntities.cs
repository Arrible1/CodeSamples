using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class PlanetConfig
    {
        public string name;
        public float gravityPower;
        public float orbitDistance;
        public float radius;
        // I decided to do non realistic rotation speed that does not depend on orbit
        public float rotationSpeed;
        public float maxHp;

        public bool destructable => maxHp > 0;
    }

    // I often design game entities as public clean data
    [Serializable]
    public class Planet
    {
        public int id;
        [CanBeNull] public RocketConfig rocketUsed;
        [CanBeNull] public PlanetConfig config;

        public float rotationPhase;
        // data that is rarely changed i wrap into reactive property 
        public float hp;
        public Vector2 position;
        public float currentRocketCooldown;
        public bool onCooldown => currentRocketCooldown > 0;
        public float relativeCooldown => currentRocketCooldown / rocketUsed.cooldown;
        
        // In some simple cases its ok to store view pointer
        // But its makes a little less convenient code, so often this is not a production case
        [GenIgnore] public PlanetView currentView;

    }
    
    [Serializable]
    public class RocketConfig
    {
        public string name;
        public float startingSpeed;
        public float acceleration;
        public float damage;
        public float cooldown;
        public float gravityInfluence = 1;
        public string description;
        // to prevent rocket collision with its own planet
        public float invulTime = 0.5f;
    }

    public class RocketInstance
    {
        [CanBeNull]
        public RocketConfig config;
        public Vector2 position;
        public Vector2 speed;
        public float invulTime;
        public float lifeTime;
        public int parentId;
    }

    public enum GameResult
    {
        Undefined,
        Win,
        Lose
    }

}