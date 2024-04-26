using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnect.Web3Modal
{
    public class DeepLinkPresenter : Presenter<DeepLinkView>
    {
        private readonly WaitForSecondsRealtime _waitForSeconds5;
        private readonly WaitForSecondsRealtime _waitForSeconds05;

        private WalletConnectConnectionProposal _connectionProposal;
        private Wallet _wallet;
        private string _continueInText;
        private bool _isLoadingDeepLink;
        private Coroutine _loadingCoroutine;

        private const string ContinueInTextTemplate = "Continue in {0}";

        public DeepLinkPresenter(RouterController router, DeepLinkView deepLinkView) : base(router)
        {
            View = deepLinkView;
            View.CopyLinkClicked += OnCopyLinkClicked;
            View.TryAgainLinkClicked += OnTryAgainLinkClicked;

            _waitForSeconds5 = new WaitForSecondsRealtime(5f);
            _waitForSeconds05 = new WaitForSecondsRealtime(0.5f);

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            UnityEventsDispatcher.Instance.ApplicationFocus += OnApplicationHandler;
#endif
        }

        protected override void OnVisibleCore()
        {
            base.OnVisibleCore();

            if (!Web3Modal.ConnectorController
                    .TryGetConnector<WalletConnectConnector>
                        (ConnectorType.WalletConnect, out var connector))
                throw new System.Exception("No WC connector"); // TODO: use custom exception

            if (_connectionProposal == null || _connectionProposal.IsConnected)
                _connectionProposal = (WalletConnectConnectionProposal)connector.Connect();

            if (WalletUtils.TryGetLastViewedWallet(out var wallet))
            {
                _wallet = wallet;
                var remoteSprite = RemoteSpriteFactory.GetRemoteSprite<Image>($"https://api.web3modal.com/getWalletImage/{wallet.ImageId}");
                _continueInText = string.Format(ContinueInTextTemplate, wallet.Name);
                View.SetWalletInfo(remoteSprite, _continueInText);
            }

            UnityEventsDispatcher.Instance.StartCoroutine(OpenDeepLinkWhenReady());
        }

        private IEnumerator OpenDeepLinkWhenReady()
        {
            // Wait for transition to finish
            yield return _waitForSeconds05;

            if (string.IsNullOrWhiteSpace(_connectionProposal.Uri))
                yield return new WaitUntil(() => !string.IsNullOrWhiteSpace(_connectionProposal.Uri) || !IsVisible);

            if (IsVisible)
                OpenDeepLink();
        }

        private void OpenDeepLink()
        {
            Linker.OpenSessionProposalDeepLink(_connectionProposal.Uri, _wallet);
        }

        private void OnTryAgainLinkClicked()
        {
            OpenDeepLink();
        }

        private void OnCopyLinkClicked()
        {
            GUIUtility.systemCopyBuffer = _connectionProposal.Uri;
            Web3Modal.NotificationController.Notify(NotificationType.Success, "Link copied to clipboard");
        }

#if UNITY_IOS || UNITY_ANDROID
        private void OnApplicationHandler(bool hasFocus)
        {
            if (IsVisible || !hasFocus)
                return;

            if (_isLoadingDeepLink)
            {
                StopLoadingCoroutine();
            }

            _loadingCoroutine = UnityEventsDispatcher.Instance.StartCoroutine(LoadingRoutine());
        }

        private void StopLoadingCoroutine()
        {
            UnityEventsDispatcher.Instance.StopCoroutine(_loadingCoroutine);

            View.CopyLink.SetEnabled(true);
            View.TryAgainLink.SetEnabled(true);
            View.ContinueInText = _continueInText;

            _isLoadingDeepLink = false;
        }

        private IEnumerator LoadingRoutine()
        {
            _isLoadingDeepLink = true;

            View.CopyLink.SetEnabled(false);
            View.TryAgainLink.SetEnabled(false);
            View.ContinueInText = "Loading...";

            yield return _waitForSeconds5;

            StopLoadingCoroutine();
        }
#endif
    }
}