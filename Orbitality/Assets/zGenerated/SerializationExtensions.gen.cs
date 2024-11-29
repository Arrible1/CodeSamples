using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ZergRush.Alive;
using Newtonsoft.Json;
#if !INCLUDE_ONLY_CODE_GENERATION

public static partial class SerializationExtensions
{
    public static void ReadFromJson(this ZergRush.ReactiveCore.ReactiveCollection<Game.Planet> self, JsonTextReader reader) 
    {
        if (reader.TokenType != JsonToken.StartArray) throw new JsonSerializationException("Bad Json Format");
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndArray) { break; }
            Game.Planet val = default;
            val = new Game.Planet();
            val.ReadFromJson(reader);
            self.Add(val);
        }
    }
    public static void WriteJson(this ZergRush.ReactiveCore.ReactiveCollection<Game.Planet> self, JsonTextWriter writer) 
    {
        writer.WriteStartArray();
        for (int i = 0; i < self.Count; i++)
        {
            self[i].WriteJson(writer);
        }
        writer.WriteEndArray();
    }
    public static void ReadFromJson(this Game.Planet self, JsonTextReader reader) 
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var name = (string) reader.Value;
                reader.Read();
                switch(name)
                {
                    case "id":
                    self.id = (int)(Int64)reader.Value;
                    break;
                    case "rocketUsed":
                    if (reader.TokenType == JsonToken.Null) {
                        self.rocketUsed = null;
                    }
                    else { 
                        if (self.rocketUsed == null) {
                            self.rocketUsed = new Game.RocketConfig();
                        }
                        self.rocketUsed.ReadFromJson(reader);
                    }
                    break;
                    case "config":
                    if (reader.TokenType == JsonToken.Null) {
                        self.config = null;
                    }
                    else { 
                        if (self.config == null) {
                            self.config = new Game.PlanetConfig();
                        }
                        self.config.ReadFromJson(reader);
                    }
                    break;
                    case "rotationPhase":
                    self.rotationPhase = (System.Single)(double)reader.Value;
                    break;
                    case "hp":
                    self.hp = (System.Single)(double)reader.Value;
                    break;
                    case "position":
                    self.position = (UnityEngine.Vector2)reader.ReadFromJsonUnityEngine_Vector2();
                    break;
                    case "currentRocketCooldown":
                    self.currentRocketCooldown = (System.Single)(double)reader.Value;
                    break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject) { break; }
        }
    }
    public static void WriteJson(this Game.Planet self, JsonTextWriter writer) 
    {
        writer.WriteStartObject();
        writer.WritePropertyName("id");
        writer.WriteValue(self.id);
        if (self.rocketUsed == null)
        {
            writer.WritePropertyName("rocketUsed");
            writer.WriteNull();
        }
        else
        {
            writer.WritePropertyName("rocketUsed");
            self.rocketUsed.WriteJson(writer);
        }
        if (self.config == null)
        {
            writer.WritePropertyName("config");
            writer.WriteNull();
        }
        else
        {
            writer.WritePropertyName("config");
            self.config.WriteJson(writer);
        }
        writer.WritePropertyName("rotationPhase");
        writer.WriteValue(self.rotationPhase);
        writer.WritePropertyName("hp");
        writer.WriteValue(self.hp);
        writer.WritePropertyName("position");
        self.position.WriteJson(writer);
        writer.WritePropertyName("currentRocketCooldown");
        writer.WriteValue(self.currentRocketCooldown);
        writer.WriteEndObject();
    }
    public static UnityEngine.Vector2 ReadFromJsonUnityEngine_Vector2(this JsonTextReader reader) 
    {
        var self = new UnityEngine.Vector2();
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var name = (string) reader.Value;
                reader.Read();
                switch(name)
                {
                    case "x":
                    self.x = (System.Single)(double)reader.Value;
                    break;
                    case "y":
                    self.y = (System.Single)(double)reader.Value;
                    break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject) { break; }
        }
        return self;
    }
    public static void WriteJson(this UnityEngine.Vector2 self, JsonTextWriter writer) 
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(self.x);
        writer.WritePropertyName("y");
        writer.WriteValue(self.y);
        writer.WriteEndObject();
    }
    public static void ReadFromJson(this Game.PlanetConfig self, JsonTextReader reader) 
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var name = (string) reader.Value;
                reader.Read();
                switch(name)
                {
                    case "name":
                    self.name = (string) reader.Value;
                    break;
                    case "gravityPower":
                    self.gravityPower = (System.Single)(double)reader.Value;
                    break;
                    case "orbitDistance":
                    self.orbitDistance = (System.Single)(double)reader.Value;
                    break;
                    case "radius":
                    self.radius = (System.Single)(double)reader.Value;
                    break;
                    case "rotationSpeed":
                    self.rotationSpeed = (System.Single)(double)reader.Value;
                    break;
                    case "maxHp":
                    self.maxHp = (System.Single)(double)reader.Value;
                    break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject) { break; }
        }
    }
    public static void WriteJson(this Game.PlanetConfig self, JsonTextWriter writer) 
    {
        writer.WriteStartObject();
        writer.WritePropertyName("name");
        writer.WriteValue(self.name);
        writer.WritePropertyName("gravityPower");
        writer.WriteValue(self.gravityPower);
        writer.WritePropertyName("orbitDistance");
        writer.WriteValue(self.orbitDistance);
        writer.WritePropertyName("radius");
        writer.WriteValue(self.radius);
        writer.WritePropertyName("rotationSpeed");
        writer.WriteValue(self.rotationSpeed);
        writer.WritePropertyName("maxHp");
        writer.WriteValue(self.maxHp);
        writer.WriteEndObject();
    }
    public static void ReadFromJson(this Game.RocketConfig self, JsonTextReader reader) 
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var name = (string) reader.Value;
                reader.Read();
                switch(name)
                {
                    case "name":
                    self.name = (string) reader.Value;
                    break;
                    case "startingSpeed":
                    self.startingSpeed = (System.Single)(double)reader.Value;
                    break;
                    case "acceleration":
                    self.acceleration = (System.Single)(double)reader.Value;
                    break;
                    case "damage":
                    self.damage = (System.Single)(double)reader.Value;
                    break;
                    case "cooldown":
                    self.cooldown = (System.Single)(double)reader.Value;
                    break;
                    case "gravityInfluence":
                    self.gravityInfluence = (System.Single)(double)reader.Value;
                    break;
                    case "description":
                    self.description = (string) reader.Value;
                    break;
                    case "invulTime":
                    self.invulTime = (System.Single)(double)reader.Value;
                    break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject) { break; }
        }
    }
    public static void WriteJson(this Game.RocketConfig self, JsonTextWriter writer) 
    {
        writer.WriteStartObject();
        writer.WritePropertyName("name");
        writer.WriteValue(self.name);
        writer.WritePropertyName("startingSpeed");
        writer.WriteValue(self.startingSpeed);
        writer.WritePropertyName("acceleration");
        writer.WriteValue(self.acceleration);
        writer.WritePropertyName("damage");
        writer.WriteValue(self.damage);
        writer.WritePropertyName("cooldown");
        writer.WriteValue(self.cooldown);
        writer.WritePropertyName("gravityInfluence");
        writer.WriteValue(self.gravityInfluence);
        writer.WritePropertyName("description");
        writer.WriteValue(self.description);
        writer.WritePropertyName("invulTime");
        writer.WriteValue(self.invulTime);
        writer.WriteEndObject();
    }
    public static void ReadFromJson(this ZergRush.ReactiveCore.ReactiveCollection<Game.RocketInstance> self, JsonTextReader reader) 
    {
        if (reader.TokenType != JsonToken.StartArray) throw new JsonSerializationException("Bad Json Format");
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndArray) { break; }
            Game.RocketInstance val = default;
            val = new Game.RocketInstance();
            val.ReadFromJson(reader);
            self.Add(val);
        }
    }
    public static void WriteJson(this ZergRush.ReactiveCore.ReactiveCollection<Game.RocketInstance> self, JsonTextWriter writer) 
    {
        writer.WriteStartArray();
        for (int i = 0; i < self.Count; i++)
        {
            self[i].WriteJson(writer);
        }
        writer.WriteEndArray();
    }
    public static void ReadFromJson(this Game.RocketInstance self, JsonTextReader reader) 
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var name = (string) reader.Value;
                reader.Read();
                switch(name)
                {
                    case "config":
                    if (reader.TokenType == JsonToken.Null) {
                        self.config = null;
                    }
                    else { 
                        if (self.config == null) {
                            self.config = new Game.RocketConfig();
                        }
                        self.config.ReadFromJson(reader);
                    }
                    break;
                    case "position":
                    self.position = (UnityEngine.Vector2)reader.ReadFromJsonUnityEngine_Vector2();
                    break;
                    case "speed":
                    self.speed = (UnityEngine.Vector2)reader.ReadFromJsonUnityEngine_Vector2();
                    break;
                    case "invulTime":
                    self.invulTime = (System.Single)(double)reader.Value;
                    break;
                    case "lifeTime":
                    self.lifeTime = (System.Single)(double)reader.Value;
                    break;
                    case "parentId":
                    self.parentId = (int)(Int64)reader.Value;
                    break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject) { break; }
        }
    }
    public static void WriteJson(this Game.RocketInstance self, JsonTextWriter writer) 
    {
        writer.WriteStartObject();
        if (self.config == null)
        {
            writer.WritePropertyName("config");
            writer.WriteNull();
        }
        else
        {
            writer.WritePropertyName("config");
            self.config.WriteJson(writer);
        }
        writer.WritePropertyName("position");
        self.position.WriteJson(writer);
        writer.WritePropertyName("speed");
        self.speed.WriteJson(writer);
        writer.WritePropertyName("invulTime");
        writer.WriteValue(self.invulTime);
        writer.WritePropertyName("lifeTime");
        writer.WriteValue(self.lifeTime);
        writer.WritePropertyName("parentId");
        writer.WriteValue(self.parentId);
        writer.WriteEndObject();
    }
    public static void ReadFromJson<T>(this ZergRush.ReactiveCore.ReactiveCollection<T> self, JsonTextReader reader) where T : ZergRush.Alive.Livable, ILivableModification, IHashable
    {
        if (reader.TokenType != JsonToken.StartArray) throw new JsonSerializationException("Bad Json Format");
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndArray) { break; }
            self.Add(null);
            self[self.Count - 1] = (T)ZergRush.Alive.Livable.CreatePolymorphic(reader.ReadJsonClassId());
            self[self.Count - 1].ReadFromJson(reader);
        }
    }
    public static void WriteJson<T>(this ZergRush.ReactiveCore.ReactiveCollection<T> self, JsonTextWriter writer) where T : ZergRush.Alive.Livable, ILivableModification, IHashable
    {
        writer.WriteStartArray();
        for (int i = 0; i < self.Count; i++)
        {
            self[i].WriteJson(writer);
        }
        writer.WriteEndArray();
    }
    public static void CompareCheck<T>(this ZergRush.ReactiveCore.ReactiveCollection<T> self, ZergRush.ReactiveCore.ReactiveCollection<T> other, Stack<string> path) where T : ZergRush.Alive.Livable, ILivableModification, IHashable
    {
        if (self.Count != other.Count) SerializationTools.LogCompError(path, "Count", other.Count, self.Count);
        var count = Math.Min(self.Count, other.Count);
        for (int i = 0; i < count; i++)
        {
            if (SerializationTools.CompareClassId(path, i.ToString(), self[i], other[i])) {
                path.Push(i.ToString());
                self[i].CompareCheck(other[i], path);
                path.Pop();
            }
        }
    }
    public static ulong CalculateHash<T>(this ZergRush.ReactiveCore.ReactiveCollection<T> self) where T : ZergRush.Alive.Livable, ILivableModification, IHashable
    {
        System.UInt64 hash = 345093625;
        hash += (ulong)590981122;
        hash += hash << 11; hash ^= hash >> 7;
        var size = self.Count;
        for (int i = 0; i < size; i++)
        {
            hash += self[i].CalculateHash();
            hash += hash << 11; hash ^= hash >> 7;
        }
        return hash;
    }
    public static System.Int32[] ReadFromJson(this System.Int32[] self, JsonTextReader reader) 
    {
        if (reader.TokenType != JsonToken.StartArray) throw new JsonSerializationException("Bad Json Format");
        if(self == null || self.Length > 0) self = Array.Empty<int>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndArray) { break; }
            int val = default;
            val = (int)(Int64)reader.Value;
            Array.Resize(ref self, self.Length + 1);
            self[self.Length - 1] = val;
        }
        return self;
    }
    public static void WriteJson(this System.Int32[] self, JsonTextWriter writer) 
    {
        writer.WriteStartArray();
        for (int i = 0; i < self.Length; i++)
        {
            writer.WriteValue(self[i]);
        }
        writer.WriteEndArray();
    }
    public static void CompareCheck(this System.Int32[] self, System.Int32[] other, Stack<string> path) 
    {
        if (self.Length != other.Length) SerializationTools.LogCompError(path, "Length", other.Length, self.Length);
        var count = Math.Min(self.Length, other.Length);
        for (int i = 0; i < count; i++)
        {
            if (self[i] != other[i]) SerializationTools.LogCompError(path, i.ToString(), other[i], self[i]);
        }
    }
    public static ulong CalculateHash(this System.Int32[] self) 
    {
        System.UInt64 hash = 345093625;
        hash += (ulong)546861222;
        hash += hash << 11; hash ^= hash >> 7;
        var size = self.Length;
        for (int i = 0; i < size; i++)
        {
            hash += (System.UInt64)self[i];
            hash += hash << 11; hash ^= hash >> 7;
        }
        return hash;
    }
    public static void Serialize(this System.Int32[] self, BinaryWriter writer) 
    {
        writer.Write(self.Length);
        for (int i = 0; i < self.Length; i++)
        {
            writer.Write(self[i]);
        }
    }
    public static System.Int32[] ReadSystem_Int32Array(this BinaryReader reader) 
    {
        var size = reader.ReadInt32();
        var array = new int[size];
        for (int i = 0; i < size; i++)
        {
            array[i] = reader.ReadInt32();
        }
        return array;
    }
    public static void UpdateFrom(this System.Int32[] self, System.Int32[] other) 
    {
        for (int i = 0; i < self.Length; i++)
        {
            self[i] = other[i];
        }
    }
}
#endif
