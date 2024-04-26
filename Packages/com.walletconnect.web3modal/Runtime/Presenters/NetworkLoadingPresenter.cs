using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnectUnity.UI;

namespace WalletConnect.Web3Modal
{
    public class NetworkLoadingPresenter : Presenter<NetworkLoadingView>
    {
        public NetworkLoadingPresenter(RouterController router, VisualElement parent) : base(router)
        {
            View = new NetworkLoadingView();
            parent.Add(View);

            View.style.display = DisplayStyle.None;

            Web3Modal.NetworkController.ChainChanged += ChainChangedHandler;
        }

        protected override void OnVisibleCore()
        {
            base.OnVisibleCore();

            var chainId = PlayerPrefs.GetString("WC_SELECTED_CHAIN_ID");
            var chain = Web3Modal.NetworkController.Chains[chainId];

            Title = chain.Name;
            var remoteSprite = RemoteSpriteFactory.GetRemoteSprite<Image>(chain.ImageUrl);
            View.SetNetworkIcon(remoteSprite);
        }

        private void ChainChangedHandler(object sender, NetworkController.ChainChangedEventArgs e)
        {
            if (!IsVisible)
                return;

            if (e.Chain == null)
                Router.GoBack();
            else
                Web3Modal.CloseModal();
        }
    }
}