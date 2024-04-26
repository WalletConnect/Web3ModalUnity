using System;
using System.Threading.Tasks;
using Nethereum.Web3;
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
            ModalController = Instance.GetComponentInChildren<ModalController>(true);
            ConnectorController = new ConnectorController();
            ApiController = new ApiController();
            NotificationController = new NotificationController();
            NetworkController = new NetworkControllerCore();

            await Task.WhenAll(
                ModalController.InitializeAsync(),
                ConnectorController.InitializeAsync(Config.supportedChains),
                NetworkController.InitializeAsync(ConnectorController, Config.supportedChains)
            );

            ConnectorController.AccountConnected += AccountConnectedHandler;
            ConnectorController.AccountDisconnected += AccountDisconnectedHandler;

            NetworkController.ChainChanged += ChainChangedHandler;
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

        private static void AccountConnectedHandler(object sender, Connector.AccountConnectedEventArgs e)
        {
            if (WalletUtils.TryGetLastViewedWallet(out var lastViewedWallet))
                WalletUtils.SetRecentWallet(lastViewedWallet);

            CloseModal();

            if (NetworkController.ActiveChain == default)
                OpenModal(ViewType.NetworkSearch);
            else
                UpdateWeb3Instance(GetAccount().ChainId);
        }

        private static void AccountDisconnectedHandler(object sender, Connector.AccountDisconnectedEventArgs e)
        {
            CloseModal();
        }

        private void ChainChangedHandler(object sender, NetworkController.ChainChangedEventArgs e)
        {
            if (e.Chain != null) UpdateWeb3Instance(e.Chain.ChainId);
        }

        private static void UpdateWeb3Instance(string chainId)
        {
            Web3 = new Web3(CreateRpcUrl(chainId))
            {
                Client =
                {
                    OverridingRequestInterceptor = Interceptor
                }
            };
        }

        private static string CreateRpcUrl(string chainId)
        {
            return $"https://rpc.walletconnect.com/v1?chainId={chainId}&projectId={ProjectConfiguration.Load().Id}";
        }
    }
}