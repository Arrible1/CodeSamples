using UnityEngine;
using ZergRush.ReactiveUI;

namespace Game
{
    public class RocketView : ReusableView
    {
        RocketInstance rocket;
        public void Show(RocketInstance rocket)
        {
            this.rocket = rocket;
            this.transform.localScale = Vector3.one * 0.1f; 
        }
        void Update()
        {
            transform.position = rocket.position.HorizontalToVec3();
            transform.rotation = Quaternion.LookRotation(rocket.speed.HorizontalToVec3());
        }
    }
}