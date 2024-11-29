#if CLIENT

namespace ZergRush.ReactiveUI
{
    public class ShowableView : ReusableView
    {
        public override void OnBeforeUsed()
        {
            base.OnBeforeUsed();
            gameObject.SetActive(true);
        }
        public override void OnRecycle()
        {
            base.OnRecycle();
            gameObject.SetActive(false);
        }
    }
}

#endif
