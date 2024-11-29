using TWL;
using ZergRush;

namespace Game
{
    public partial class GameModel
    {
        static ZergRandom random = new ZergRandom();
        static PlanetConfig Sun = new PlanetConfig {name = "Sun", gravityPower = 1, orbitDistance = 0, radius = 1};

        static Planet NewPlanet(PlanetConfig config, int id)
        {
            return new Planet
            {
                config = config,
                id = id,
                rotationPhase = (float) random.NextDouble() * 10,
                rocketUsed = GameConfig.instance.rockets.RandomElement(random),
                hp = config.maxHp,
                currentRocketCooldown = random.NextFloat()
            };
        }

        static string PlanetPath(string category, int number) => $"{category}/{category}_Design{number}";

        static string[] PlanetNames = new string[]
            {PlanetPath("Frozen", 1), PlanetPath("Earth-like", 1), PlanetPath("Alien", 1)};

        public static GameModel New(RocketConfig playerRocket)
        {
            // this is kind of 'whatever' code
            // usually code like this is replaced as soon as possible with valid GD data and configs

            var c = GameConfig.instance;
            var model = new GameModel();
            // sun as planet impl )) right now it seems almost ok, but I surely wont do that in real project
            model.planets.Add(NewPlanet(Sun, 0));
            
            int planetCount = random.Range(c.minAiPlayers + 1, c.maxAiPlayers + 1);
            float basePlanetOrbit = 1;
            for (int i = 1; i <= planetCount; i++)
            {
                var orbit = 0.5f + basePlanetOrbit * i;
                var config = new PlanetConfig
                {
                    radius = random.Range(0.4f, 0.5f),
                    rotationSpeed = random.Sign() * 5 * random.Range(0.1f, 0.16f) / (orbit),
                    name = PlanetNames.RandomElement(random),
                    gravityPower = 1,
                    maxHp = 50,
                    orbitDistance = orbit
                };
                model.planets.Add(NewPlanet(config, i));
            }
            model.playerPlanetId = random.Range(1, planetCount + 1);
            if (playerRocket != null) model.PlanetWithId(model.playerPlanetId).rocketUsed = playerRocket;
            model.Update(0); // settings planet positions
            return model;
        }
    }
}