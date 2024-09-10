using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnect.Web3Modal
{
    public class WebAppPresenter : Presenter<WebAppView>
    {
        private WalletConnectConnectionProposal _connectionProposal;
        private Wallet _wallet;
        private bool _disposed;

        private const string ContinueInTextTemplate = "Continue in {0}";

        public WebAppPresenter(RouterController router, VisualElement parent, bool hideView = true) : base(router, parent, hideView)
        {
            View.style.display = DisplayStyle.Flex;

            View.OpenLinkClicked += OnOpenLinkClicked;
            View.CopyLinkClicked += OnCopyLinkClicked;

            Web3Modal.AccountConnected += AccountConnectedHandler;
        }

        protected override WebAppView CreateViewInstance()
        {
            return Parent.Q<WebAppView>();
        }

        protected override void OnVisibleCore()
        {
            base.OnVisibleCore();

            if (!Web3Modal.ConnectorController
                    .TryGetConnector<WalletConnectConnector>
                        (ConnectorType.WalletConnect, out var connector))
                throw new Exception("No WC connector"); // TODO: use custom exception

            _connectionProposal ??= (WalletConnectConnectionProposal)connector.Connect();

            if (WalletUtils.TryGetLastViewedWallet(out var wallet))
            {
                _wallet = wallet;
                var remoteSprite = RemoteSpriteFactory.GetRemoteSprite<Image>($"https://api.web3modal.com/getWalletImage/{wallet.ImageId}");
                View.SetWalletInfo(remoteSprite, string.Format(ContinueInTextTemplate, wallet.Name));
            }
        }

        private void OnOpenLinkClicked()
        {
            Application.OpenURL(Path.Combine(_wallet.WebappLink, $"wc?uri={_connectionProposal.Uri}"));
        }

        private void OnCopyLinkClicked()
        {
            GUIUtility.systemCopyBuffer = _connectionProposal.Uri;
            Web3Modal.NotificationController.Notify(NotificationType.Success, "Link copied to clipboard");
        }

        private void AccountConnectedHandler(object sender, Connector.AccountConnectedEventArgs e)
        {
            if (!IsVisible)
                return;

            Web3Modal.EventsController.SendEvent(new Event
            {
                name = "CONNECT_SUCCESS",
                properties = new Dictionary<string, object>
                {
                    { "method", "web" },
                    { "name", _wallet.Name },
                    { "explorer_id", _wallet.Id }
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Web3Modal.AccountConnected -= AccountConnectedHandler;
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}