using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    
    // Config class for all gameplay settings and adjustments, right now only rockets are here
    [CreateAssetMenu(fileName = "config", menuName = "Create Game Config", order = 1)]
    public class GameConfig : ScriptableObject
    {
        static GameConfig _instance;
        public static GameConfig instance => _instance ?? (_instance = Resources.Load<GameConfig>("config"));
        
        public List<RocketConfig> rockets;
        
        [Range(1, 8)] public int minAiPlayers;
        [Range(1, 8)] public int maxAiPlayers;
    }
}