using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using UnityEngine;
using UnityEngine.Scripting;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Nethereum;

namespace WalletConnect.Web3Modal
{
    public abstract class Web3Modal : MonoBehaviour
    {
        public static Web3Modal Instance { get; protected set; }

        public static ModalController ModalController { get; protected set; }
        public static ConnectorController ConnectorController { get; protected set; }
        public static ApiController ApiController { get; protected set; }
        public static NotificationController NotificationController { get; protected set; }
        public static NetworkController NetworkController { get; protected set; }
        
        public static EvmService Evm { get; protected set; }

        public static Web3ModalConfig Config { get; private set; }
        public static bool IsInitialized { get; private set; }

        public static bool IsAccountConnected
        {
            get => ConnectorController.IsAccountConnected;
        }

        public static bool IsModalOpen
        {
            get => ModalController.IsOpen;
        }

        public static event EventHandler<InitializeEventArgs> Initialized;

        public static event EventHandler<Connector.AccountConnectedEventArgs> AccountConnected
        {
            add => ConnectorController.AccountConnected += value;
            remove => ConnectorController.AccountConnected -= value;
        }

        public static event EventHandler<Connector.AccountDisconnectedEventArgs> AccountDisconnected
        {
            add => ConnectorController.AccountDisconnected += value;
            remove => ConnectorController.AccountDisconnected -= value;
        }

        public static event EventHandler<Connector.AccountChangedEventArgs> AccountChanged
        {
            add => ConnectorController.AccountChanged += value;
            remove => ConnectorController.AccountChanged -= value;
        }

        public static event EventHandler<NetworkController.ChainChangedEventArgs> ChainChanged
        {
            add => NetworkController.ChainChanged += value;
            remove => NetworkController.ChainChanged -= value;
        }

        public static Task InitializeAsync()
        {
            return InitializeAsync(new Web3ModalConfig());
        }

        public static async Task InitializeAsync(Web3ModalConfig config)
        {
            if (Instance == null)
                throw new Exception("Instance not set");
            if (IsInitialized)
                throw new Exception("Already initialized"); // TODO: use custom ex type

            UnityWebRequestExtensions.sdkType = "w3m";
            UnityWebRequestExtensions.sdkVersion = "unity-w3m-v0.3.0"; // TODO: update this from CI

            Config = config ?? throw new ArgumentNullException(nameof(config));

            await Instance.InitializeAsyncCore();

            IsInitialized = true;
            Initialized?.Invoke(null, new InitializeEventArgs());
        }

        public static void OpenModal(ViewType viewType = ViewType.None)
        {
            if (!IsInitialized)
                throw new Exception("Web3Modal not initialized"); // TODO: use custom ex type

            if (IsModalOpen)
                throw new Exception("Web3Modal already open"); // TODO: use custom ex type

            Instance.OpenModalCore(viewType);
        }

        public static void CloseModal()
        {
            if (!IsModalOpen)
                return;

            Instance.CloseModalCore();
        }

        public static Task<Account> GetAccountAsync()
        {
            return ConnectorController.GetAccountAsync();
        }

        public static Task DisconnectAsync()
        {
            if (!IsInitialized)
                throw new Exception("Web3Modal not initialized"); // TODO: use custom ex type

            if (!IsAccountConnected)
                throw new Exception("No account connected"); // TODO: use custom ex type

            return Instance.DisconnectAsyncCore();
        }

        protected abstract Task InitializeAsyncCore();

        protected abstract void OpenModalCore(ViewType viewType = ViewType.None);

        protected abstract void CloseModalCore();

        protected abstract Task DisconnectAsyncCore();

        public class InitializeEventArgs : EventArgs
        {
            [Preserve]
            public InitializeEventArgs()
            {
            }
        }
    }
}