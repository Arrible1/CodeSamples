#if UNITY_5_3_OR_NEWER

using System;
using DG.Tweening;
using UnityEngine;

namespace ZergRush.ReactiveUI
{
    public class PresentDelegates<TView>
        where TView : ReusableView
    {
        // Used to animate dynamic insertion somehow
        public Action<TView> onInsert;

        // Used to animate dynamic remove. Returns time to delay recycle for proper animation.
        public Func<TView, float> onRemove;

        // Callback for proper view move animation if layout was changed.
        public Func<TView, Vector2, IDisposable> moveAnimation;

        public static PresentDelegates<TView> WithRemoveAnimation(Func<TView, float> nRemove) =>
            new PresentDelegates<TView> {onRemove = nRemove};

        public static PresentDelegates<TView> FadeInOut(float time)
        {
            return new PresentDelegates<TView>
            {
                onInsert = view =>
                {
                    var cg = view.gameObject.GetOrAddComponent<CanvasGroup>();
                    cg.alpha = 0;
                    cg.DOFade(1, time).SetDelay(time);
                },
                onRemove = view =>
                {
                    var cg = view.gameObject.GetOrAddComponent<CanvasGroup>();
                    cg.DOFade(0, time);
                    return time;
                },
                moveAnimation = (view, posTo) =>
                {
                    view.rectTransform.DOAnchorPos(posTo, 0.2f);
                    var cg = view.gameObject.GetOrAddComponent<CanvasGroup>();
                    cg.alpha = 1;
                    return new AnonymousDisposable(() => view.DOKill());
                }
            };
        }
        
        public static PresentDelegates<TView> ScaleInOut(float time, float minScale = 0.2f, float easeAmplitude = 0.1f)
        {
            return new PresentDelegates<TView>
            {
                onInsert = view =>
                {
                    var cg = view.gameObject.GetOrAddComponent<CanvasGroup>();
                    cg.alpha = 0;
                    cg.DOFade(1, time).SetDelay(time);
                    
                    var rt = view.gameObject.GetOrAddComponent<RectTransform>();
                    rt.localScale  = new Vector3(minScale, minScale);
                    rt.DOScale(Vector3.one, time).SetEase(Ease.OutBack, easeAmplitude).SetDelay(time);
                },
                onRemove = view =>
                {
                    var cg = view.gameObject.GetOrAddComponent<CanvasGroup>();
                    cg.alpha = 1;
                    cg.DOFade(0, time);
                    
                    var rt = view.gameObject.GetOrAddComponent<RectTransform>();
                    rt.localScale = new Vector3(1.0f, 1.0f);
                    rt.DOScale(new Vector3(minScale, minScale), time).SetEase(Ease.OutBack, easeAmplitude);
                    return time;
                },
                moveAnimation = (view, posTo) =>
                {
                    view.rectTransform.DOAnchorPos(posTo, 0.2f);
                    var cg = view.gameObject.GetOrAddComponent<CanvasGroup>();
                    cg.alpha = 1;
                    return new AnonymousDisposable(() => view.DOKill());
                }
            };
        }
    }
}
#endif