using System;
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

        // List of buttons at the bottom of the account view.
        // This list is used to enable/disable all buttons at once when needed.
        protected readonly HashSet<ListItem> Buttons = new();

        private bool _disposed;
        private ListItem _networkButton;
        private RemoteSprite<Image> _networkIcon;
        private RemoteSprite<Image> _avatar;

        public AccountPresenter(RouterController router, VisualElement parent) : base(router, parent)
        {
            View.ExplorerButton.Clicked += OnBlockExplorerButtonClick;
            View.CopyLink.Clicked += OnCopyAddressButtonClick;

            InitializeButtons(View.Buttons);

            Web3Modal.AccountController.PropertyChanged += AccountPropertyChangedHandler;
            Web3Modal.NetworkController.ChainChanged += ChainChangedHandler;
        }

        private void InitializeButtons(VisualElement buttonsListView)
        {
            CreateButtons(buttonsListView);
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

        // Creates the buttons at the bottom of the account view.
        protected virtual void CreateButtons(VisualElement buttonsListView)
        {
            CreateNetworkButton(buttonsListView);
            CreateDisconnectButton(buttonsListView);
        }

        protected virtual void CreateNetworkButton(VisualElement buttonsListView)
        {
            _networkButton = new ListItem("Network", OnNetworkButtonClick, null, ListItem.IconType.Circle)
            {
                IconFallbackElement =
                {
                    vectorImage = Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_medium_info")
                }
            };
            Buttons.Add(_networkButton);
            buttonsListView.Add(_networkButton);
        }

        protected virtual void CreateDisconnectButton(VisualElement buttonsListView)
        {
            var disconnectIcon = Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_medium_disconnect");
            var disconnectButton = new ListItem("Disconnect", OnDisconnectButtonClick, disconnectIcon, ListItem.IconType.Circle, ListItem.IconStyle.Accent);
            Buttons.Add(disconnectButton);
            buttonsListView.Add(disconnectButton);
        }

        protected virtual void UpdateProfileName()
        {
            var profileName = Web3Modal.AccountController.ProfileName;
            if (profileName.Length > 15)
                profileName = profileName.Truncate(6);

            View.SetProfileName(profileName);
        }

        protected virtual void UpdateProfileAvatar()
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

        protected virtual async void OnDisconnectButtonClick()
        {
            try
            {
                ButtonsSetEnabled(false);
                await Web3Modal.DisconnectAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                ButtonsSetEnabled(true);
            }
        }

        protected virtual void OnNetworkButtonClick()
        {
            Router.OpenView(ViewType.NetworkSearch);

            Web3Modal.EventsController.SendEvent(new Event
            {
                name = "CLICK_NETWORKS"
            });
        }

        protected virtual void OnBlockExplorerButtonClick()
        {
            var chain = Web3Modal.NetworkController.ActiveChain;
            var blockExplorerUrl = chain.BlockExplorer.url;
            var address = Web3Modal.AccountController.Address;
            Application.OpenURL($"{blockExplorerUrl}/address/{address}");
        }

        protected virtual void OnCopyAddressButtonClick()
        {
            var address = Web3Modal.AccountController.Address;
            GUIUtility.systemCopyBuffer = address;
            Web3Modal.NotificationController.Notify(NotificationType.Success, "Address copied");
        }

        private void ButtonsSetEnabled(bool value)
        {
            foreach (var button in Buttons)
                button.SetEnabled(value);
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

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Web3Modal.AccountController.PropertyChanged -= AccountPropertyChangedHandler;
                Web3Modal.NetworkController.ChainChanged -= ChainChangedHandler;

                _networkIcon?.UnsubscribeImage(_networkButton.IconImageElement);
                _avatar?.UnsubscribeImage(View.ProfileAvatarImage);
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}