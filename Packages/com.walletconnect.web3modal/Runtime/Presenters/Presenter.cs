using System;
using UnityEngine.UIElements;

namespace WalletConnect.Web3Modal
{
    public abstract class PresenterBase : IDisposable
    {
        public virtual string Title { get; protected set; } = string.Empty;

        public virtual bool HeaderBorder { get; protected set; } = true;

        public RouterController Router { get; protected set; }

        public virtual VisualElement ViewVisualElement { get; protected set; }

        public bool IsVisible { get; private set; }

        private bool _disposed;

        public void OnVisible()
        {
            IsVisible = true;
            OnVisibleCore();
        }

        public void OnHide()
        {
            IsVisible = false;
            OnHideCore();
        }

        public void OnDisable()
        {
            IsVisible = false;
            OnDisableCore();
        }


        public void ShowViewVisualElement()
        {
            ViewVisualElement.style.display = DisplayStyle.Flex;
        }

        public void HideViewVisualElement()
        {
            ViewVisualElement.style.display = DisplayStyle.None;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                IsVisible = false;
            }

            _disposed = true;
        }

        protected abstract void OnVisibleCore();

        protected abstract void OnHideCore();

        protected abstract void OnDisableCore();
    }

    public class Presenter<TView> : PresenterBase where TView : VisualElement
    {
        protected TView View { get; set; }

        public override VisualElement ViewVisualElement
        {
            get => View;
        }

        public Presenter(RouterController router)
        {
            Router = router;
        }

        protected override void OnVisibleCore()
        {
        }

        protected override void OnHideCore()
        {
        }

        protected override void OnDisableCore()
        {
        }
    }
}