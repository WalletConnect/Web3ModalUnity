using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using Newtonsoft.Json;
using UnityEngine;
using WalletConnect.Web3Modal.WebGl.Modal;
using WalletConnect.Web3Modal.WebGl.Wagmi;
using WalletConnectSharp.Core;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal
{
#if UNITY_WEBGL
    public class WebGlConnector : Connector
    {

        [DllImport("__Internal")]
        private static extern void Initialize(string parameters, Action callback);

        private static TaskCompletionSource<bool> _initializationTaskCompletionSource;
        
        private string _lastAccountStatus;

        public WebGlConnector()
        {
            Type = ConnectorType.WebGl;
        }

        protected override async Task InitializeAsyncCore(Web3ModalConfig web3ModalConfig)
        {
            var walletConnectConfig = ProjectConfiguration.Load();

            var viemChainNames = web3ModalConfig.supportedChains
                .Where(c => !string.IsNullOrWhiteSpace(c.ViemName))
                .Select(c => c.ViemName)
                .ToArray();
            
            var parameters = new WebGlInitializeParameters
            {
                projectId = walletConnectConfig.Id,
                metadata = walletConnectConfig.Metadata,
                chains = viemChainNames,
                enableOnramp = web3ModalConfig.enableOnramp,
            };
            
            var parametersJson = JsonConvert.SerializeObject(parameters);
            
#pragma warning disable S2696
            _initializationTaskCompletionSource = new TaskCompletionSource<bool>();
#pragma warning restore S2696

            Initialize(parametersJson, InitializationCallback);

            await _initializationTaskCompletionSource.Task;
            
            WagmiInterop.InitializeEvents();
            ModalInterop.InitializeEvents();
            
            WagmiInterop.WatchAccountTriggered += WatchAccountTriggeredHandler;
            WagmiInterop.WatchChainIdTriggered += WatchChainIdTriggeredHandler;
        }

        protected override ConnectionProposal ConnectCore()
        {
            return new WebGlConnectionProposal(this);
        }

        protected override async Task<bool> TryResumeSessionAsyncCore()
        {
            var getAccountResult = await WagmiInterop.GetAccountAsync();

            if (getAccountResult.isConnected)
            {
                return true;
            }

            if (getAccountResult.isConnecting)
            {
                var tcs = new TaskCompletionSource<bool>();
                
                WagmiInterop.WatchAccountTriggered += WagmiInteropOnWatchAccountTriggered;
                
                void WagmiInteropOnWatchAccountTriggered(GetAccountReturnType arg)
                {
                    if (arg.isConnecting)
                        return;

                    tcs.SetResult(arg.isConnected);

                    WagmiInterop.WatchAccountTriggered -= WagmiInteropOnWatchAccountTriggered;
                }
                var result = await tcs.Task;

                return result;
            }
            else
            {
                return false;
            }
        }

        protected override Task DisconnectAsyncCore()
        {
            return WagmiInterop.DisconnectAsync();
        }

        protected override async Task ChangeActiveChainAsyncCore(Chain chain)
        {
            await WagmiInterop.SwitchChainAsync(int.Parse(chain.ChainReference)); // TODO: remove parsing
        }

        protected override async Task<Account> GetAccountAsyncCore()
        {
            var wagmiAccount = await WagmiInterop.GetAccountAsync();
            return new Account(wagmiAccount.address, $"eip155:{wagmiAccount.chainId}");
        }

        protected override async Task<Account[]> GetAccountsCore()
        {
            var wagmiAccount = await WagmiInterop.GetAccountAsync();
            var chainId = $"eip155:{wagmiAccount.chainId}";
            return wagmiAccount.addresses
                .Select(addr => new Account(addr, chainId))
                .ToArray();
        }
        
        private void WatchAccountTriggeredHandler(GetAccountReturnType arg)
        {
            var previousLastAccountStatus = _lastAccountStatus;
            _lastAccountStatus = arg.status;
            
            var account = new Account(arg.address, $"eip155:{arg.chainId}");

            if (_lastAccountStatus == "connected" && previousLastAccountStatus != "connected")
            {
                IsAccountConnected = true;
                var accountConnectedEventArgs = new AccountConnectedEventArgs(GetAccountAsync, GetAccounts);
                OnAccountConnected(accountConnectedEventArgs);
            }
            else if (_lastAccountStatus == "disconnected" && previousLastAccountStatus != "disconnected")
            {
                IsAccountConnected = false;
                OnAccountDisconnected(AccountDisconnectedEventArgs.Empty);
            }
            else
            {
                var accountChangedEventArgs = new AccountChangedEventArgs(account);
                OnAccountChanged(accountChangedEventArgs);
            }
        }

        private void WatchChainIdTriggeredHandler(int ethChainId)
        {
            if (ethChainId == default)
                return;
            
            var chainId = $"eip155:{ethChainId}";
            OnChainChanged(new ChainChangedEventArgs(chainId));
        }
        
        [MonoPInvokeCallback(typeof(Action))]
        public static void InitializationCallback()
        {
            _initializationTaskCompletionSource.SetResult(true);
        }
    }

    [Serializable]
    internal class WebGlInitializeParameters
    {
        public string projectId;
        public Metadata metadata;
        public string[] chains;
        
        public bool enableOnramp;
    }
#endif
}