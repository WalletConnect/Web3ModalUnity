using System;
using System.Threading.Tasks;
using UnityEngine;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Utils;

namespace WalletConnect.Web3Modal
{
    public class Web3ModalCore : Web3Modal
    {
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogError("[Web3Modal] Instance already exists. Destroying...");
                Destroy(gameObject);
            }
        }

        protected override async Task InitializeAsyncCore()
        {
            ModalController = CreateModalController();
            ConnectorController = new ConnectorController();
            ApiController = new ApiController();
            NotificationController = new NotificationController();
            NetworkController = new NetworkControllerCore();
            
#if UNITY_WEBGL && !UNITY_EDITOR
            Evm = new WagmiEvmService();
#else
            Evm = new NethereumEvmService();
#endif

            await Task.WhenAll(
                ConnectorController.InitializeAsync(Config),
                ModalController.InitializeAsync(),
                NetworkController.InitializeAsync(ConnectorController, Config.supportedChains)
            );

            await Evm.InitializeAsync();

            ConnectorController.AccountConnected += AccountConnectedHandler;
            ConnectorController.AccountDisconnected += AccountDisconnectedHandler;
        }

        protected override void OpenModalCore(ViewType viewType = ViewType.None)
        {
            if (viewType == ViewType.None)
            {
                ModalController.OpenCore(IsAccountConnected ? ViewType.Account : ViewType.Connect);
            }
            else
            {
                if (IsAccountConnected && viewType == ViewType.Connect)
                    // TODO: use custom exception type
                    throw new Exception("Trying to open Connect view when account is already connected.");
                else
                    ModalController.OpenCore(viewType);
            }
        }

        protected override void CloseModalCore()
        {
            ModalController.CloseCore();
        }

        protected override Task DisconnectAsyncCore()
        {
            return ConnectorController.DisconnectAsync();
        }

        protected virtual ModalController CreateModalController()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new WalletConnect.Web3Modal.WebGl.ModalControllerWebGl();
#else
            return new ModalControllerUtk();
#endif
        }

        private static async void AccountConnectedHandler(object sender, Connector.AccountConnectedEventArgs e)
        {
            if (WalletUtils.TryGetLastViewedWallet(out var lastViewedWallet))
                WalletUtils.SetRecentWallet(lastViewedWallet);

            CloseModal();
        }

        private static void AccountDisconnectedHandler(object sender, Connector.AccountDisconnectedEventArgs e)
        {
            CloseModal();
        }
    }
}