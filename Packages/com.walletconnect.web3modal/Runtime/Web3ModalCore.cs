using System;
using System.Collections.Generic;
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
            AccountController = new AccountController();
            ConnectorController = new ConnectorController();
            ApiController = new ApiController();
            BlockchainApiController = new BlockchainApiController();
            NotificationController = new NotificationController();
            NetworkController = new NetworkControllerCore();
            EventsController = new EventsController();

#if UNITY_WEBGL && !UNITY_EDITOR
            Evm = new WagmiEvmService();
#else
            Evm = new NethereumEvmService();
#endif

            await Task.WhenAll(
                ConnectorController.InitializeAsync(Config),
                ModalController.InitializeAsync(),
                EventsController.InitializeAsync(Config, ApiController),
                NetworkController.InitializeAsync(ConnectorController, Config.supportedChains),
                AccountController.InitializeAsync(ConnectorController, NetworkController, BlockchainApiController)
            );

            await Evm.InitializeAsync();

            ConnectorController.AccountConnected += AccountConnectedHandler;
            ConnectorController.AccountDisconnected += AccountDisconnectedHandler;

            EventsController.SendEvent(new Event
            {
                name = "MODAL_LOADED"
            });
        }

        protected override void OpenModalCore(ViewType viewType = ViewType.None)
        {
            if (viewType == ViewType.None)
            {
                ModalController.Open(IsAccountConnected ? ViewType.Account : ViewType.Connect);
            }
            else
            {
                if (IsAccountConnected && viewType == ViewType.Connect)
                    // TODO: use custom exception type
                    throw new Exception("Trying to open Connect view when account is already connected.");
                else
                    ModalController.Open(viewType);
            }
        }

        protected override void CloseModalCore()
        {
            ModalController.Close();
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