using System;
using UnityEngine;
using ZergRush;
using ZergRush.ReactiveCore;

namespace Game
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] MainMenu menu;
        [SerializeField] GameView view;
        [SerializeField] Camera gameCamera;

        public GameModel game => gameCell.value;
        readonly Cell<GameModel> gameCell = new Cell<GameModel>();
        readonly Cell<bool> paused = new Cell<bool>();

        static string saveFileName = "saveData";

        public void Start()
        {
            gameCell.value = Utils.LoadFromJsonFile<GameModel>(saveFileName, () => null, false);
            
            // game starts paused
            paused.value = true;
            
            // This is reactive binding, so menu will be active when game paused,
            // This approach allows to write cleaner, easier to change code
            menu.SetActive(paused);
            menu.newGame.Subscribe(() =>
            {
                gameCell.value = GameModel.New(menu.rocketConfigSelected);
                paused.value = false;
                ResetTitleToDefault();
                view.AdjustCamera(game.planets.Count - 1);
            });
            // Also a reactive binding shortcut
            menu.resume.SetActive(gameCell.IsNot(null));
            menu.resume.Subscribe(() => paused.value = false);
            menu.exit.Subscribe(Application.Quit);
            
            // Here is the reactive library operators example 
            // game model is changing over time, and collections in game model are changing over time
            // Join() allows to collapse any complex dependant changes into simple IReactiveCollection or ICell
            // Its very cool because view actually do not care when and how game model is changed, it just shows dynamic collection of items
            // MapWithDefaultIfNull(...) allows to choose empty collection when game model is null
            var planets = gameCell.MapWithDefaultIfNull(m => m.planets, StaticCollection<Planet>.Empty()).Join();
            var rockets = gameCell.MapWithDefaultIfNull(m => m.rockets, StaticCollection<RocketInstance>.Empty()).Join();
            view.Show(planets, rockets, gameCell.Map(g => g.playerPlanetId));
            
            view.SetHudCanvasVisible(paused.Not());
            ResetTitleToDefault();
        }

        public void OnApplicationQuit()
        {
            if (gameCell.value == null) FileWrapper.RemoveIfExists(saveFileName);
            else gameCell.value.SaveToJsonFile(saveFileName);
        }

        void FinishGame(GameResult result)
        {
            gameCell.value = null;
            paused.value = true;
            if (result == GameResult.Lose)
                menu.title.text = $"You {result}!";
            else 
                menu.title.text = $"Orbitality!!!";
        }

        void ResetTitleToDefault()
        {
            menu.title.text = "Orbitality?";
        }

        public void Update()
        {
            if (gameCell.value == null || paused.value) return;
            if (gameCell.value.IsGameFinished(out var result))
            {
                FinishGame(result);
                return;
            }
            
            gameCell.value.Update(Time.deltaTime);
            
            // Input and AI
            foreach (var gamePlanet in game.planets)
            {
                if (gamePlanet.id == game.playerPlanetId)
                {
                    var planetPos = gameCamera.WorldToScreenPoint(gamePlanet.currentView.transform.position);
                    var dir = Input.mousePosition - planetPos;
                    
                    // updating arrow
                    var arrowCenterShift = dir.SwapYZ().normalized * gamePlanet.config.radius;
                    view.UpdateTargetingArrow(
                        gamePlanet.position.HorizontalToVec3() + arrowCenterShift,
                        Quaternion.LookRotation(dir.SwapYZ())
                        );

                    if (gamePlanet.currentRocketCooldown <= 0)
                    {
                        if (Input.GetMouseButtonDown(0)) game.Shoot(gamePlanet, dir);
                    }


                    // Updating rotation hack
                    var rotCorrection = 0;
                    if (Input.GetKey(KeyCode.W)) rotCorrection = 1;
                    if (Input.GetKey(KeyCode.S)) rotCorrection = -1;
                    gamePlanet.rotationPhase += 0.5f * gamePlanet.config.rotationSpeed * rotCorrection * Time.deltaTime;
                }
                else if (gamePlanet.id != 0)
                {
                    AI.DoSmth(game, gamePlanet);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) paused.value = true;
        }
    }
}