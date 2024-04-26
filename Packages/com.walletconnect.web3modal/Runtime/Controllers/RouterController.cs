using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace WalletConnect.Web3Modal
{
    public class RouterController
    {
        private readonly Dictionary<ViewType, PresenterBase> _viewsControllers = new();
        private readonly Stack<ViewType> _history = new();

        public event EventHandler<ViewChangedEventArgs> ViewChanged;

        public PresenterBase CurrentPresenter
        {
            get => _viewsControllers[_history.Peek()];
        }

        private readonly VisualElement _routerVisualElement;

        public RouterController(VisualElement parent)
        {
            _routerVisualElement = new VisualElement
            {
                name = "router"
            };

            parent.Add(_routerVisualElement);

            BuildViewsDictionary();
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
            _viewsControllers[viewType].OnVisible();
            _viewsControllers[viewType].ShowViewVisualElement();
            ViewChanged?.Invoke(this, new ViewChangedEventArgs(currentViewType, viewType, _viewsControllers[viewType]));
        }

        public void GoBack()
        {
            if (_history.Count == 0)
                return;

            var oldViewType = _history.Pop();
            _viewsControllers[oldViewType].OnDisable();
            _viewsControllers[oldViewType].HideViewVisualElement();

            if (_history.Count > 0)
            {
                var nextViewType = _history.Peek();
                var nextView = _viewsControllers[nextViewType];
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
                _viewsControllers[viewType].OnDisable();
                _viewsControllers[viewType].HideViewVisualElement();
            }
        }

        private void BuildViewsDictionary()
        {
            _viewsControllers.Add(ViewType.Connect, new ConnectPresenter(this, _routerVisualElement));
            _viewsControllers.Add(ViewType.QrCode, new QrCodePresenter(this, _routerVisualElement));
            _viewsControllers.Add(ViewType.Wallet, new WalletPresenter(this, _routerVisualElement));
            _viewsControllers.Add(ViewType.WalletSearch, new WalletSearchPresenter(this, _routerVisualElement));
            _viewsControllers.Add(ViewType.Account, new AccountPresenter(this, _routerVisualElement));
            _viewsControllers.Add(ViewType.NetworkSearch, new NetworkSearchPresenter(this, _routerVisualElement));
            _viewsControllers.Add(ViewType.NetworkLoading, new NetworkLoadingPresenter(this, _routerVisualElement));
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