using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZergRush.ReactiveCore;

namespace Game
{
    public class MainMenu : MonoBehaviour
    {
        public Text title;
        public Button newGame;
        public Button resume;
        public Button exit;

        public RocketConfig rocketConfigSelected => selectedConfig.value;
        Cell<RocketConfig> selectedConfig = new Cell<RocketConfig>();
        public List<DebugTabButton> weaponButtons;

        void Awake()
        {
            for (int i = 0; i < 3; i++)
            {
                var rocket = GameConfig.instance.rockets[i];
                weaponButtons[i].Show(rocket.description.Replace("\\n", "\n"), selectedConfig.Is(rocket));
                weaponButtons[i].button.ClickStream().Subscribe(() => selectedConfig.value = rocket);
            }
        }
    }
}