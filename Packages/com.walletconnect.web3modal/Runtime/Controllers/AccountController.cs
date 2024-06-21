using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using WalletConnect.Web3Modal.Utils;

namespace WalletConnect.Web3Modal
{
    public class AccountController : INotifyPropertyChanged
    {
        public bool IsInitialized { get; set; }

        public bool IsConnected
        {
            get => _connectorController.IsAccountConnected;
        }
        
        public string Address
        {
            get => _address;
            set => SetField(ref _address, value);
        }
        
        public string AccountId
        {
            get => _accountId;
            set => SetField(ref _accountId, value);
        }

        public string ChainId
        {
            get => _chainId;
            set => SetField(ref _chainId, value);
        }
        
        public string ProfileName
        {
            get => _profileName;
            set => SetField(ref _profileName, value);
        }
        
        public string ProfileAvatar
        {
            get => _profileAvatar;
            set => SetField(ref _profileAvatar, value);
        }
        
        public string Balance
        {
            get => _balance;
            set => SetField(ref _balance, value);
        }
        
        public string BalanceSymbol
        {
            get => _balanceSymbol;
            set => SetField(ref _balanceSymbol, value);
        }

        private ConnectorController _connectorController;
        private NetworkController _networkController;
        private BlockchainApiController _blockchainApiController;
        
        private string _address;
        private string _accountId;
        private string _chainId;
        
        private string _profileName;
        private string _profileAvatar;
        
        private string _balance;
        private string _balanceSymbol;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public async Task InitializeAsync(ConnectorController connectorController, NetworkController networkController, BlockchainApiController blockchainApiController)
        {
            if (IsInitialized)
                throw new Exception("Already initialized"); // TODO: use custom ex type
            
            _connectorController = connectorController ?? throw new ArgumentNullException(nameof(connectorController));
            _networkController = networkController ?? throw new ArgumentNullException(nameof(networkController));
            _blockchainApiController = blockchainApiController ?? throw new ArgumentNullException(nameof(blockchainApiController));
            
            _connectorController.AccountConnected += ConnectorAccountConnectedHandler;
            _connectorController.AccountChanged += ConnectorAccountChangedHandler;
        }

        private async void ConnectorAccountConnectedHandler(object sender, Connector.AccountConnectedEventArgs e)
        {
            var account = await e.GetAccount();
            if (account.AccountId == AccountId)
                return;
            
            Address = account.Address;
            AccountId = account.AccountId;
            ChainId = account.ChainId;
            
            await Task.WhenAll(
                UpdateProfile(),
                UpdateBalance()
            );
        }

        private async void ConnectorAccountChangedHandler(object sender, Connector.AccountChangedEventArgs e)
        {
            if (e.Account.Address != Address)
            {
                Address = e.Account.Address;
                await UpdateProfile();
            }
      
            AccountId = e.Account.AccountId;
            ChainId = e.Account.ChainId;
            
            await UpdateBalance();
        }

        public async Task UpdateProfile()
        {
            var identity = await _blockchainApiController.GetIdentityAsync(Address);
            ProfileName = string.IsNullOrWhiteSpace(identity.Name)
                ? Address.Truncate()
                : identity.Name;

            ProfileAvatar = identity.Avatar ?? string.Empty;
        }

        public async Task UpdateBalance()
        {
            var response = await _blockchainApiController.GetBalanceAsync(Address);
            var balance = response.Balances.FirstOrDefault(x => x.chainId == ChainId && string.IsNullOrWhiteSpace(x.address));

            if (string.IsNullOrWhiteSpace(balance.quantity.numeric))
            {
                Balance = "0.000";
                BalanceSymbol = _networkController.ActiveChain.NativeCurrency.symbol;
            }
            else
            {
                Balance = balance.quantity.numeric;
                BalanceSymbol = balance.symbol;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}