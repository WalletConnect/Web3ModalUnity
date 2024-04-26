using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal
{
    public class ConnectorController : Connector
    {
        private readonly Dictionary<ConnectorType, Connector> _connectors = new();

        public override bool IsAccountConnected
        {
            get
            {
                if (ActiveConnector == null)
                    return false;

                return ActiveConnector.IsAccountConnected;
            }
        }

        public Connector ActiveConnector
        {
            get => _activeConnector;
            private set
            {
                if (_activeConnector == value)
                    return;

                _activeConnector = value;

                if (value != null)
                    Type = value.Type;
            }
        }

        private Connector _activeConnector;

        protected override async Task InitializeAsyncCore(IEnumerable<Chain> supportedChains)
        {
            DappSupportedChains = supportedChains;

            // --- WalletConnect Connector
            var walletConnectConnector = new WalletConnectConnector();
            walletConnectConnector.AccountConnected += (_, e) => ConnectorAccountConnected(walletConnectConnector, e);
            walletConnectConnector.AccountDisconnected += ConnectorAccountDisconnectedHandler;
            walletConnectConnector.AccountChanged += AccountChangedHandler;
            walletConnectConnector.ChainChanged += ChainChangedHandler;
            _connectors.Add(ConnectorType.WalletConnect, walletConnectConnector);

            await Task.WhenAll(_connectors.Values.Select(c => c.InitializeAsync(supportedChains)));
        }

        protected override async Task<bool> TryResumeSessionAsyncCore()
        {
            if (ActiveConnector != null)
                return await ActiveConnector.TryResumeSessionAsync();

            if (!TryGetLastConnector(out var connectorType))
                return false;

            var connector = _connectors[connectorType];
            var sessionResumed = await connector.TryResumeSessionAsync();

            if (sessionResumed)
                ActiveConnector = connector;

            return sessionResumed;
        }

        // ConnectorController creates WC connection. 
        // All other connections are created by their respective connectors.
        protected override ConnectionProposal ConnectCore()
        {
            if (!TryGetConnector<WalletConnectConnector>(ConnectorType.WalletConnect, out var wcConnector))
                throw new Exception("No WC connector"); // TODO: use custom exception

            return wcConnector.Connect();
        }

        protected override async Task DisconnectAsyncCore()
        {
            await ActiveConnector.DisconnectAsync();
        }

        protected override Task ChangeActiveChainAsyncCore(Chain chain)
        {
            return ActiveConnector.ChangeActiveChainAsync(chain);
        }

        protected override Account GetAccountCore()
        {
            return ActiveConnector.GetAccount();
        }

        protected override Account[] GetAccountsCore()
        {
            return ActiveConnector.GetAccounts();
        }

        public bool TryGetConnector<T>(ConnectorType connectorType, out T connector) where T : Connector
        {
            var ok = _connectors.TryGetValue(connectorType, out var uncasedConnector);
            connector = (T)uncasedConnector;
            return ok;
        }

        private static bool TryGetLastConnector(out ConnectorType connectorType)
        {
            const string key = "W3M_LAST_CONNECTOR_TYPE";

            if (PlayerPrefs.HasKey(key))
            {
                var connectorTypeInt = PlayerPrefs.GetInt(key);
                connectorType = (ConnectorType)connectorTypeInt;
                return connectorType != ConnectorType.None;
            }

            connectorType = ConnectorType.None;
            return false;
        }

        private void ConnectorAccountConnected(Connector connector, AccountConnectedEventArgs e)
        {
            PlayerPrefs.SetInt("W3M_LAST_CONNECTOR_TYPE", (int)connector.Type);
            ActiveConnector = connector;

            OnAccountConnected(e);
        }

        private void ConnectorAccountDisconnectedHandler(object sender, EventArgs e)
        {
            ActiveConnector = null;
            OnAccountDisconnected(AccountDisconnectedEventArgs.Empty);
        }

        private void AccountChangedHandler(object sender, AccountChangedEventArgs e)
        {
            OnAccountChanged(e);
        }

        private void ChainChangedHandler(object sender, ChainChangedEventArgs e)
        {
            OnChainChanged(e);
        }

        protected override void ConnectionConnectedHandler(ConnectionProposal connectionProposal)
        {
            PlayerPrefs.SetInt("W3M_LAST_CONNECTOR_TYPE", (int)connectionProposal.connector.Type);

            base.ConnectionConnectedHandler(connectionProposal);
        }
    }
}