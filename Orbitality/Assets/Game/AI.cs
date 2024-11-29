using System.Linq;
using TWL;
using UnityEngine;
using ZergRush;

namespace Game
{
    public static class AI
    {
        static ZergRandom random = new ZergRandom();
        
        // My first AI guess is playable enough so did not transfer any attributes to GameConfig
        public static void DoSmth(GameModel model, Planet myPlanet)
        {
            // do nothing if rocket is on cooldown
            if (myPlanet.currentRocketCooldown > 0) return;
            var randomTarget = model.planets.Where(p => p.id != 0 && p.id != myPlanet.id).RandomElement(random);

            var dir = randomTarget.position - myPlanet.position;
            dir.Normalize();
            // Distor direction somehow, may be more complex logic later
            var distorAmpl = 0.2f;
            dir += new Vector2(random.NextFloat(), random.NextFloat()) * distorAmpl;
            model.Shoot(myPlanet, dir);
        }
    }
}