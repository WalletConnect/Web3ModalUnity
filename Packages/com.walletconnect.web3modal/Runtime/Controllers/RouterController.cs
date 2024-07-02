using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace WalletConnect.Web3Modal
{
    public class RouterController
    {
        private readonly Dictionary<ViewType, PresenterBase> _modalViews = new();
        private readonly Stack<ViewType> _history = new();

        public event EventHandler<ViewChangedEventArgs> ViewChanged;

        public PresenterBase CurrentPresenter
        {
            get => _modalViews[_history.Peek()];
        }

        private readonly VisualElement _routerVisualElement;

        public RouterController(VisualElement parent)
        {
            _routerVisualElement = new VisualElement
            {
                name = "router"
            };

            parent.Add(_routerVisualElement);

            RegisterDefaultModalViews();
        }

        public void OpenView(ViewType viewType)
        {
            var currentViewType = ViewType.None;
            if (_history.Count > 0)
            {
                CurrentPresenter.OnHide();
                CurrentPresenter.HideViewVisualElement();
                currentViewType = _history.Peek();
            }

            _history.Push(viewType);
            _modalViews[viewType].OnVisible();
            _modalViews[viewType].ShowViewVisualElement();
            ViewChanged?.Invoke(this, new ViewChangedEventArgs(currentViewType, viewType, _modalViews[viewType]));
        }

        public void GoBack()
        {
            if (_history.Count == 0)
                return;

            var oldViewType = _history.Pop();
            _modalViews[oldViewType].OnDisable();
            _modalViews[oldViewType].HideViewVisualElement();

            if (_history.Count > 0)
            {
                var nextViewType = _history.Peek();
                var nextView = _modalViews[nextViewType];
                nextView.OnVisible();
                nextView.ShowViewVisualElement();
                ViewChanged?.Invoke(this, new ViewChangedEventArgs(oldViewType, nextViewType, nextView));
            }
            else
            {
                ViewChanged?.Invoke(this, new ViewChangedEventArgs(oldViewType, ViewType.None, null));
            }
        }

        public void CloseAllViews()
        {
            while (_history.Count > 0)
            {
                var viewType = _history.Pop();
                _modalViews[viewType].OnDisable();
                _modalViews[viewType].HideViewVisualElement();
            }
        }

        public void RegisterModalView(ViewType viewType, PresenterBase modalView)
        {
            if (_modalViews.TryGetValue(viewType, out var oldModalView))
                oldModalView.Dispose();

            _modalViews[viewType] = modalView;
        }

        private void RegisterDefaultModalViews()
        {
            RegisterModalView(ViewType.Connect, new ConnectPresenter(this, _routerVisualElement));
            RegisterModalView(ViewType.QrCode, new QrCodePresenter(this, _routerVisualElement));
            RegisterModalView(ViewType.Wallet, new WalletPresenter(this, _routerVisualElement));
            RegisterModalView(ViewType.WalletSearch, new WalletSearchPresenter(this, _routerVisualElement));
            RegisterModalView(ViewType.Account, new AccountPresenter(this, _routerVisualElement));
            RegisterModalView(ViewType.NetworkSearch, new NetworkSearchPresenter(this, _routerVisualElement));
            RegisterModalView(ViewType.NetworkLoading, new NetworkLoadingPresenter(this, _routerVisualElement));
        }
    }

    public readonly struct ViewChangedEventArgs
    {
        public readonly ViewType oldViewType;
        public readonly ViewType newViewType;
        public readonly PresenterBase newPresenter;

        public ViewChangedEventArgs(ViewType oldViewType, ViewType newViewType, PresenterBase newPresenter)
        {
            this.oldViewType = oldViewType;
            this.newViewType = newViewType;
            this.newPresenter = newPresenter;
        }
    }
}