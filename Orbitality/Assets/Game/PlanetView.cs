using UnityEngine;
using ZergRush.ReactiveUI;

namespace Game
{
    public class PlanetView : ReusableView
    {
        public Planet planet;
        public HpBarCanvas hpBar;
        float rotation;

        void Awake()
        {
            rotation = UnityEngine.Random.value * 10;
            hpBar = Instantiate(GameResources.instance.hpBar, transform, false);
        }
        
        public void Show(Planet planet, bool isPlayerPlanet)
        {
            gameObject.SetActive(true);
            this.planet = planet;
            planet.currentView = this;
            hpBar.gameObject.SetActive(this.planet.config.destructable);
            transform.localScale = planet.config.radius * 1.8f * Vector3.one;
            hpBar.hpBarFill.color = isPlayerPlanet ? Color.green : Color.red;

        }
        void Update()
        {
            rotation += Time.deltaTime;
            transform.position = planet.position.HorizontalToVec3();
            transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
            hpBar.hpBarFill.fillAmount = planet.hp / planet.config.maxHp;
            hpBar.shootCooldownRoot.gameObject.SetActiveSafe(planet.onCooldown);
            hpBar.shootCooldownFill.fillAmount = planet.relativeCooldown;
        }
    }
}