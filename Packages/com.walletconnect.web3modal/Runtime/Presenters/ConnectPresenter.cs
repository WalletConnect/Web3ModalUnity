using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;
using DeviceType = WalletConnectUnity.Core.Utils.DeviceType;

namespace WalletConnect.Web3Modal
{
    public class ConnectPresenter : Presenter<VisualElement>
    {
        public override string Title
        {
            get => "Connect wallet";
        }

        public ConnectPresenter(RouterController router, VisualElement parent) : base(router, parent)
        {
            Build();

            // Rebuild UI after wallet disconnects
            Web3Modal.Initialized += (_, _) =>
                Web3Modal.AccountDisconnected += async (_, _) =>
                    await RebuildAsync();
        }

        private async void Build()
        {
            try
            {
                await BuildAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected virtual async Task RebuildAsync()
        {
            foreach (var visualElement in View.Children().ToArray())
                View.Remove(visualElement);

            await BuildAsync();
        }

        protected virtual async Task BuildAsync()
        {
            ShowWalletConnectButton();

            var recentWalletExists = WalletUtils.TryGetRecentWallet(out var recentWallet);
            if (recentWalletExists)
                ShowRecentWalletButton(recentWallet);

            int count = DeviceUtils.GetDeviceType() is DeviceType.Phone
                ? Web3Modal.Config.connectViewWalletsCountMobile
                : Web3Modal.Config.connectViewWalletsCountDesktop;

            if (recentWalletExists)
                count++;

            var response = await Web3Modal.ApiController.GetWallets(1, count);

            foreach (var wallet in response.Data)
            {
                // Skip recent wallet to avoid duplicates
                if (recentWalletExists && recentWallet.Id == wallet.Id)
                    continue;

                var remoteSprite =
                    RemoteSpriteFactory.GetRemoteSprite<Image>(
                        $"https://api.web3modal.com/getWalletImage/{wallet.ImageId}");

                var walletClosure = wallet;
                var isWalletInstalled = WalletUtils.IsWalletInstalled(wallet);
                var walletStatusIcon = isWalletInstalled ? StatusIconType.Success : StatusIconType.None;
                View.Add(new ListItem(wallet.Name, remoteSprite, () => OnWalletListItemClick(walletClosure), statusIconType: walletStatusIcon));
            }

            var allWalletsListItem = new ListItem("All wallets", (Sprite)null, () => Router.OpenView(ViewType.WalletSearch));
            var roundedCount = MathF.Round((float)response.Count / 10) * 10;
            allWalletsListItem.RightSlot.Add(new Tag($"{roundedCount}+", Tag.TagType.Info));
            View.Add(allWalletsListItem);
        }

        protected virtual void ShowWalletConnectButton()
        {
            var deviceType = DeviceUtils.GetDeviceType();

            if (deviceType is DeviceType.Phone)
                return;
            var wcLogo =
                RemoteSpriteFactory.GetRemoteSprite<Image>(
                    $"https://api.web3modal.com/public/getAssetImage/ef1a1fcf-7fe8-4d69-bd6d-fda1345b4400");
            var listItem = new ListItem("WalletConnect", wcLogo, () =>
            {
                WalletUtils.RemoveLastViewedWallet();
                Router.OpenView(ViewType.QrCode);
            });
            listItem.RightSlot.Add(new Tag("QR CODE", Tag.TagType.Accent));
            View.Add(listItem);
        }

        protected virtual void ShowRecentWalletButton(Wallet recentWallet)
        {
            var remoteSprite =
                RemoteSpriteFactory.GetRemoteSprite<Image>(
                    $"https://api.web3modal.com/getWalletImage/{recentWallet.ImageId}");
            var listItem = new ListItem(recentWallet.Name, remoteSprite, () => OnWalletListItemClick(recentWallet));
            listItem.RightSlot.Add(new Tag("RECENT", Tag.TagType.Info));
            View.Add(listItem);
        }

        protected virtual void OnWalletListItemClick(Wallet wallet)
        {
            WalletUtils.SetLastViewedWallet(wallet);
            Router.OpenView(ViewType.Wallet);
        }
    }
}