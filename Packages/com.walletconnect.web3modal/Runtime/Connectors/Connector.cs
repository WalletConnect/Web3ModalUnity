using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal
{
    public abstract class Connector
    {
        public string ImageId { get; protected set; }

        public ConnectorType Type { get; protected set; }

        public bool IsInitialized { get; protected set; }

        public IEnumerable<Chain> DappSupportedChains { get; protected set; }

        public virtual bool IsAccountConnected { get; protected set; }

        public event EventHandler<AccountConnectedEventArgs> AccountConnected;
        public event EventHandler<AccountDisconnectedEventArgs> AccountDisconnected;
        public event EventHandler<AccountChangedEventArgs> AccountChanged;
        public event EventHandler<ChainChangedEventArgs> ChainChanged;

        private readonly HashSet<ConnectionProposal> _connectionProposals = new();

        protected Connector()
        {
        }

        public async Task InitializeAsync(IEnumerable<Chain> supportedChains)
        {
            if (IsInitialized)
                throw new Exception("Already initialized"); // TODO: use custom ex type

            await InitializeAsyncCore(supportedChains);
            IsInitialized = true;
        }

        public async Task<bool> TryResumeSessionAsync()
        {
            if (!IsInitialized)
                throw new Exception("Connector not initialized"); // TODO: use custom ex type

            if (IsAccountConnected)
                throw new Exception("Account already connected"); // TODO: use custom ex type

            var isResumed = await TryResumeSessionAsyncCore();

            if (isResumed)
            {
                IsAccountConnected = true;
                OnAccountConnected(new AccountConnectedEventArgs(GetAccount, GetAccounts));
            }

            return isResumed;
        }

        public ConnectionProposal Connect()
        {
            if (!IsInitialized)
                throw new Exception("Connector not initialized"); // TODO: use custom ex type

            var connection = ConnectCore();
            connection.Connected += ConnectionConnectedHandler;

            _connectionProposals.Add(connection);

            return connection;
        }

        public async Task DisconnectAsync()
        {
            if (!IsAccountConnected)
                throw new Exception("No account connected"); // TODO: use custom ex type

            await DisconnectAsyncCore();
        }

        public async Task ChangeActiveChainAsync(Chain chain)
        {
            if (!IsAccountConnected)
                throw new Exception("No account connected"); // TODO: use custom ex type

            await ChangeActiveChainAsyncCore(chain);
        }

        public Account GetAccount()
        {
            if (!IsAccountConnected)
                throw new Exception("No account connected"); // TODO: use custom ex type

            return GetAccountCore();
        }

        public Account[] GetAccounts()
        {
            if (!IsAccountConnected)
                throw new Exception("No account connected"); // TODO: use custom ex type

            return GetAccountsCore();
        }

        protected virtual void ConnectionConnectedHandler(ConnectionProposal connectionProposal)
        {
            foreach (var c in _connectionProposals)
                c.Dispose();

            _connectionProposals.Clear();
            OnAccountConnected(new AccountConnectedEventArgs(GetAccount, GetAccounts));
        }

        protected virtual void OnAccountConnected(AccountConnectedEventArgs e)
        {
            AccountConnected?.Invoke(this, e);
        }

        protected virtual void OnAccountDisconnected(AccountDisconnectedEventArgs e)
        {
            AccountDisconnected?.Invoke(this, e);
        }

        protected virtual void OnAccountChanged(AccountChangedEventArgs e)
        {
            AccountChanged?.Invoke(this, e);
        }

        protected virtual void OnChainChanged(ChainChangedEventArgs e)
        {
            ChainChanged?.Invoke(this, e);
        }

        protected abstract ConnectionProposal ConnectCore();

        protected abstract Task InitializeAsyncCore(IEnumerable<Chain> supportedChains);

        protected abstract Task<bool> TryResumeSessionAsyncCore();

        protected abstract Task DisconnectAsyncCore();

        protected abstract Task ChangeActiveChainAsyncCore(Chain chain);

        protected abstract Account GetAccountCore();

        protected abstract Account[] GetAccountsCore();

        public class AccountConnectedEventArgs : EventArgs
        {
            public Func<Account> GetAccount { get; }
            public Func<Account[]> GetAccounts { get; }

            public AccountConnectedEventArgs(Func<Account> getAccount, Func<Account[]> getAccounts)
            {
                GetAccount = getAccount;
                GetAccounts = getAccounts;
            }
        }

        public class AccountDisconnectedEventArgs : EventArgs
        {
            public static AccountDisconnectedEventArgs Empty { get; } = new();
        }

        public class AccountChangedEventArgs : EventArgs
        {
            public Account Account { get; }

            public AccountChangedEventArgs(Account account)
            {
                Account = account;
            }
        }

        public class ChainChangedEventArgs : EventArgs
        {
            public string ChainId { get; }

            public ChainChangedEventArgs(string chainId)
            {
                ChainId = chainId;
            }
        }
    }

    public enum ConnectorType
    {
        None,
        WalletConnect
    }
}