using System;
using System.Collections.Generic;
using System.Text;
using ZergRush.Alive;
using System.IO;
using Newtonsoft.Json;
#if !INCLUDE_ONLY_CODE_GENERATION
namespace Game {

    public partial class GameModel : IJsonSerializable
    {
        public  GameModel() 
        {
            rockets = new ZergRush.ReactiveCore.ReactiveCollection<Game.RocketInstance>();
            planets = new ZergRush.ReactiveCore.ReactiveCollection<Game.Planet>();
        }
        public virtual void ReadFromJsonField(JsonTextReader reader, string name) 
        {
            switch(name)
            {
                case "rockets":
                rockets.ReadFromJson(reader);
                break;
                case "planets":
                planets.ReadFromJson(reader);
                break;
                case "playerPlanetId":
                playerPlanetId = (int)(Int64)reader.Value;
                break;
            }
        }
        public virtual void WriteJsonFields(JsonTextWriter writer) 
        {
            writer.WritePropertyName("rockets");
            rockets.WriteJson(writer);
            writer.WritePropertyName("planets");
            planets.WriteJson(writer);
            writer.WritePropertyName("playerPlanetId");
            writer.WriteValue(playerPlanetId);
        }
    }
}
#endif
