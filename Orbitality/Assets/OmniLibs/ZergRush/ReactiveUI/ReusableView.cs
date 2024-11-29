#if UNITY_5_3_OR_NEWER

using System;
using UnityEngine;

namespace ZergRush.ReactiveUI
{
    public class ReusableView : ConnectableObject
    {
        protected RectTransform _rect;
        public RectTransform rectTransform { get { return _rect ?? (_rect = GetComponent<RectTransform>()); } }
        private Transform _tr;
        public Transform tr { get { return _tr ?? (_tr = transform); } }
        public IDisposable currentMoveAnimation;
        [NonSerialized] public int indexInModel;
        // for some inner impl
        [NonSerialized] public ReusableView prefabRef;
        public virtual bool autoDisableOnRecycle => rectTransform == null;
        public virtual bool setImpossiblePositionOnRecycle => rectTransform != null;

        public virtual void OnRecycle()
        {
        }

        public virtual void OnBeforeUsed()
        {
        }
    }
}

#endif
