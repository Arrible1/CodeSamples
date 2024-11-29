using System;
using UnityEngine;
using ZergRush.ReactiveCore;
using ZergRush.ReactiveUI;

namespace Game
{
    public class GameView : ConnectableObject
    {
        [SerializeField] Transform root;
        [SerializeField] Transform targetingArrow;
        [SerializeField] Transform hudCanvas;
        [SerializeField] Transform camera;
        DistinctivePool<ReusableView, ReusableView> effectPool;

        void Awake()
        {
            effectPool = new DistinctivePool<ReusableView, ReusableView>(root, p => p, false);
        }

        public void Show(IReactiveCollection<Planet> planets, IReactiveCollection<RocketInstance> rockets, ICell<int> playerPlanetId)
        {
            // Present function is a very powerful tool
            // It allows to show any dynamic collection it can:
            //     * Automate loading and destruction
            //     * Automate object pooling
            //     * Show UI data with different layouts
            //     * Show UI data in scroll views with reusable cells (when only visible elements are loaded)
            //     * some other usefull features ...
            // Here is the simplest case of Present
            planets.Present(
                root, 
                prefabSelector: data => Resources.Load<PlanetView>(data.config.name),
                show: (data, view) => view.Show(data, playerPlanetId.value == data.id),
                delegates: ExplosionOnDeath<PlanetView>(GameResources.instance.planetDeathFx)
            );

            rockets.Present(
                root,
                prefabSelector: data => PrefabRef<RocketView>.ByName("Missile" + data.config.name),
                show: (data, view) => view.Show(data),
                delegates: ExplosionOnDeath<RocketView>(GameResources.instance.rocketExplosionFx)
            );
        }

        public void SetHudCanvasVisible(ICell<bool> visible) => hudCanvas.SetActive(visible);

        static Vector3 players2CamPos = new Vector3(0, 4.56f, -1.39f);
        static Vector3 players8CamPos = new Vector3(0, 10.5f, -3.4f);
        public void AdjustCamera(int playerCount)
        {
            camera.position = Vector3.Lerp(players2CamPos, players8CamPos, (playerCount - 2) / 6f);
        }

        public void UpdateTargetingArrow(Vector3 pos, Quaternion rot)
        {
            targetingArrow.SetPositionAndRotation(pos, rot);
        }

        PresentDelegates<T> ExplosionOnDeath<T>(ReusableView explosionEffectPrefab) where T : ReusableView
        {
            return new PresentDelegates<T>
            {
                onRemove = view =>
                {
                    // effects are also pooled and reused
                    var exp = effectPool.Get(explosionEffectPrefab);
                    exp.transform.position = view.transform.position;
                    effectPool.Recycle(exp, 2);
                    return 0.0f;
                },
            };
        }
    }
}