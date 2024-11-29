using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using TWL;
using UnityEngine;
using ZergRush;
using ZergRush.ReactiveCore;

namespace Game
{
    [GenTask(GenTaskFlags.JsonSerialization | GenTaskFlags.DefaultConstructor)]
    public partial class GameModel : IJsonSerializable
    {
        // All game data so far
        public ReactiveCollection<RocketInstance> rockets;
        public ReactiveCollection<Planet> planets;
        public int playerPlanetId;

        
        public bool IsGameFinished(out GameResult result)
        {
            if (PlanetWithId(playerPlanetId) == null)
            {
                result = GameResult.Lose;
                return true;
            }
            // player and the sun
            else if (planets.Count == 2)
            {
                result = GameResult.Win;
                return true;
            }
            result = GameResult.Undefined;
            return false;
        }

        public Planet PlanetWithId(int id) => planets.FirstOrDefault(p => p.id == id);
        
        public void Shoot(Planet planet, Vector2 direction)
        {
            if (planet.currentRocketCooldown > 0) Debug.LogError("actually can't shoot at the moment");

            planet.currentRocketCooldown = planet.rocketUsed.cooldown;
            direction.Normalize();
            var rocket = planet.rocketUsed;
            // actually a derivative of planet movement formula
            var planetSpeed = planet.config.orbitDistance * planet.config.rotationSpeed *
                              new Vector2(-Mathf.Sin(planet.rotationPhase), 0.7f * Mathf.Cos(planet.rotationPhase));
            Debug.Log($"planet speed {planetSpeed}");
            
            rockets.Add(new RocketInstance
            {
                config = rocket,
                position = planet.position + direction * planet.config.radius,
                speed =  rocket.startingSpeed * direction + planetSpeed,
                invulTime = rocket.invulTime,
                lifeTime = 15,
                parentId = planet.id
            });
        }

        public void Update(float dt)
        {
            UpdatePhysicsAndLogic(dt);
            UpdateCollisions();
        }

        // I decided to go with custom physics because it is much cleaner, easier to save/load
        // and game logic is so simple it is not required complex physics engine at all
        void UpdatePhysicsAndLogic(float dt)
        {
            for (var i = rockets.Count - 1; i >= 0; i--)
            {
                var rocket = rockets[i];
                rocket.lifeTime -= dt;
                if (rocket.lifeTime <= 0) rockets.RemoveAt(i);
                
                rocket.speed += dt * rocket.config.gravityInfluence * GravityField(rocket.position);
                // Native rocket acceleration
                rocket.speed += rocket.speed.normalized * (rocket.config.acceleration * dt);
                rocket.position += rocket.speed * dt;
                rocket.invulTime -= dt;
            }

            foreach (var planet in planets)
            {
                planet.rotationPhase += dt * planet.config.rotationSpeed;
                planet.position = planet.config.orbitDistance *
                                  new Vector2(Mathf.Cos(planet.rotationPhase), 0.7f * Mathf.Sin(planet.rotationPhase));
                planet.currentRocketCooldown -= dt;
            }
        }

        // n^2 cause data volumes are too small
        void UpdateCollisions()
        {
            // removing entities right at the place with reverse iteration
            // it can be also done with some 'destroyed' flags with later iteration
            for (var i = rockets.Count - 1; i >= 0; i--)
            {
                var rocket = rockets[i];
                for (var j = planets.Count - 1; j >= 0; j--)
                {
                    var planet = planets[j];
                    // rocket owner is unvul to its own rockets for some time
                    if (rocket.invulTime > 0 && rocket.parentId == planet.id) continue;
                    if ((rocket.position - planet.position).magnitude <= planet.config.radius)
                    {
                        if (planet.config.destructable)
                        {
                            planet.hp -= rocket.config.damage;
                            if (planet.hp <= 0)
                            {
                                planets.RemoveAt(j);
                            }
                        }
                        rockets.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        // I actually does not need mass because actual acceleration does not depend on it
        Vector2 GravityField(Vector2 pos)
        {
            Vector2 field = Vector2.zero;
            foreach (var planet in planets)
            {
                var dir = planet.position - pos;
                var dist = dir.magnitude;
                dir.Normalize();
                field += dir * (planet.config.gravityPower / (dist * dist));
            }
            return field;
        }

    }
}