using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnectUnity.Core;
using WalletConnectUnity.UI;

namespace WalletConnect.Web3Modal
{
    public class AccountPresenter : Presenter<VisualElement>
    {
        public override bool HeaderBorder
        {
            get => false;
        }

        private readonly HashSet<ListItem> _items = new();

        private ListItem _networkButton;
        private RemoteSprite<Image> _networkIcon;

        public AccountPresenter(RouterController router, VisualElement parent) : base(router)
        {
            View = CreateVisualElement(parent);
            View.style.display = DisplayStyle.None;

            Web3Modal.NetworkController.ChainChanged += ChainChangedHandler;
        }

        private VisualElement CreateVisualElement(VisualElement parent)
        {
            var view = new VisualElement
            {
                name = "account-view"
            };

            // --- Network Button
            _networkButton = new ListItem("Network", OnNetwork, null, ListItem.IconType.Circle)
            {
                IconFallbackElement =
                {
                    vectorImage = Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_medium_info")
                }
            };
            _items.Add(_networkButton);
            view.Add(_networkButton);

            // --- Disconnect Button
            var disconnectIcon = Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_medium_disconnect");
            var disconnectButton = new ListItem("Disconnect", OnDisconnect, disconnectIcon, ListItem.IconType.Circle, ListItem.IconStyle.Accent);
            _items.Add(disconnectButton);
            view.Add(disconnectButton);

            parent.Add(view);
            return view;
        }

        private void ChainChangedHandler(object sender, NetworkController.ChainChangedEventArgs e)
        {
            UpdateNetworkButton(e.Chain);
        }

        protected override void OnVisibleCore()
        {
            base.OnVisibleCore();
            UpdateNetworkButton(Web3Modal.NetworkController.ActiveChain);
        }

        private void UpdateNetworkButton(Chain chain)
        {
            if (chain == null)
            {
                _networkButton.Label = "Network";
                _networkButton.IconImageElement.style.display = DisplayStyle.None;
                _networkButton.IconFallbackElement.style.display = DisplayStyle.Flex;
                _networkButton.ApplyIconStyle(ListItem.IconStyle.Error);
                return;
            }

            _networkButton.Label = chain.Name;

            var newNetworkIcon = RemoteSpriteFactory.GetRemoteSprite<Image>(chain.ImageUrl);

            _networkIcon?.UnsubscribeImage(_networkButton.IconImageElement);
            _networkIcon = newNetworkIcon;
            _networkIcon.SubscribeImage(_networkButton.IconImageElement);
            _networkButton.IconImageElement.style.display = DisplayStyle.Flex;
            _networkButton.IconFallbackElement.style.display = DisplayStyle.None;
            _networkButton.ApplyIconStyle(ListItem.IconStyle.Default);
        }

        private async void OnDisconnect()
        {
            try
            {
                ItemsSetEnabled(false);
                await Web3Modal.DisconnectAsync();
            }
            finally
            {
                ItemsSetEnabled(true);
            }
        }

        private void OnNetwork()
        {
            Router.OpenView(ViewType.NetworkSearch);
        }


        private void ItemsSetEnabled(bool value)
        {
            foreach (var item in _items)
                item.SetEnabled(value);
        }
    }
}