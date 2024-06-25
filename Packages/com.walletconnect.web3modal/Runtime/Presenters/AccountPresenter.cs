using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnect.Web3Modal.Utils;
using WalletConnectUnity.Core;
using WalletConnectUnity.UI;

namespace WalletConnect.Web3Modal
{
    public class AccountPresenter : Presenter<AccountView>
    {
        public override bool HeaderBorder
        {
            get => false;
        }

        private readonly HashSet<ListItem> _items = new();

        private ListItem _networkButton;
        private RemoteSprite<Image> _networkIcon;
        private RemoteSprite<Image> _avatar;

        public AccountPresenter(RouterController router, VisualElement parent) : base(router)
        {
            View = new AccountView
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };
            parent.Add(View);

            CreateButtons(View.Buttons);

            Web3Modal.AccountController.PropertyChanged += AccountPropertyChangedHandler;
            Web3Modal.NetworkController.ChainChanged += ChainChangedHandler;
        }

        private void CreateButtons(VisualElement view)
        {
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
        }
        
        private void AccountPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AccountController.ProfileName):
                    UpdateProfileName();
                    break;
                case nameof(AccountController.Address):
                case nameof(AccountController.ProfileAvatar):
                    UpdateProfileAvatar();
                    break;
                case nameof(AccountController.Balance):
                    View.SetBalance(TrimToThreeDecimalPlaces(Web3Modal.AccountController.Balance));
                    break;
                case nameof(AccountController.BalanceSymbol):
                    View.SetBalanceSymbol(Web3Modal.AccountController.BalanceSymbol);
                    break;
            }
        }

        private void ChainChangedHandler(object sender, NetworkController.ChainChangedEventArgs e)
        {
            UpdateNetworkButton(e.Chain);
        }

        private void UpdateProfileName()
        {
            var profileName = Web3Modal.AccountController.ProfileName;
            profileName = profileName.Length > 15 
                ? profileName.Truncate(6) 
                : profileName;
            
            View.SetProfileName(profileName);
        }

        private void UpdateProfileAvatar()
        {
            var avatar = Web3Modal.AccountController.ProfileAvatar;

            if (avatar.IsEmpty || avatar.AvatarFormat != "png" && avatar.AvatarFormat != "jpg" && avatar.AvatarFormat != "jpeg")
            {
                var address = Web3Modal.AccountController.Address;
                var texture = UiUtils.GenerateAvatarTexture(address);
                View.ProfileAvatarImage.image = texture;
            }
            else
            {
                var remoteSprite = RemoteSpriteFactory.GetRemoteSprite<Image>(avatar.AvatarUrl);
                _avatar?.UnsubscribeImage(View.ProfileAvatarImage);
                _avatar = remoteSprite;
                _avatar.SubscribeImage(View.ProfileAvatarImage);
            }
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
        
        public static string TrimToThreeDecimalPlaces(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
            
            var dotIndex = input.IndexOf('.');
            if (dotIndex == -1 || input.Length <= dotIndex + 4)
                return input;
            return input[..(dotIndex + 4)];
        }
    }
}