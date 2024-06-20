using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Evm;

namespace WalletConnect.Web3Modal
{
    public class WalletConnectConnector : Connector
    {
        public static IWalletConnect WalletConnectInstance
        {
            get => WalletConnectUnity.Core.WalletConnect.Instance;
        }

        private ConnectionProposal _connectionProposal;

        public WalletConnectConnector()
        {
            ImageId = "ef1a1fcf-7fe8-4d69-bd6d-fda1345b4400";
            Type = ConnectorType.WalletConnect;
        }

        protected override async Task InitializeAsyncCore(Web3ModalConfig config)
        {
            DappSupportedChains = config.supportedChains;

            await WalletConnectInstance.InitializeAsync();

            WalletConnectInstance.ActiveSessionChanged += ActiveSessionChangedHandler;
            WalletConnectInstance.ActiveChainIdChanged += ActiveChainIdChangedHandler;
            WalletConnectInstance.SessionConnected += SessionConnectedHandler;
            WalletConnectInstance.SessionDisconnected += SessionDeletedHandler;
        }

        private void ActiveSessionChangedHandler(object sender, SessionStruct e)
        {
            if (string.IsNullOrWhiteSpace(e.Topic))
                return;

            var currentAccount = GetCurrentAccount();
            OnAccountChanged(new AccountChangedEventArgs(currentAccount));
        }

        private void ActiveChainIdChangedHandler(object sender, string chainId)
        {
            OnChainChanged(new ChainChangedEventArgs(chainId));
            OnAccountChanged(new AccountChangedEventArgs(GetCurrentAccount()));
        }

        private void SessionConnectedHandler(object sender, SessionStruct e)
        {
            Web3Modal.NotificationController.Notify(NotificationType.Success, "Session connected");
            IsAccountConnected = true;
        }

        private void SessionDeletedHandler(object sender, EventArgs e)
        {
            IsAccountConnected = false;
            OnAccountDisconnected(AccountDisconnectedEventArgs.Empty);
        }

        protected override async Task<bool> TryResumeSessionAsyncCore()
        {
            var result = await WalletConnectInstance.TryResumeSessionAsync();

            return result;
        }

        protected override ConnectionProposal ConnectCore()
        {
            if (_connectionProposal is { IsConnected: false })
                return _connectionProposal;

            var connectOptions = new ConnectOptions
            {
                OptionalNamespaces = DappSupportedChains
                    .GroupBy(chainEntry => chainEntry.ChainNamespace)
                    .ToDictionary(
                        group => group.Key,
                        group => new ProposedNamespace
                        {
                            Methods = new[]
                            {
                                "eth_accounts",
                                "eth_requestAccounts",
                                "eth_sendRawTransaction",
                                "eth_sign",
                                "eth_signTransaction",
                                "eth_signTypedData",
                                "eth_signTypedData_v3",
                                "eth_signTypedData_v4",
                                "eth_sendTransaction",
                                "personal_sign",
                                "wallet_switchEthereumChain",
                                "wallet_addEthereumChain",
                                "wallet_getPermissions",
                                "wallet_requestPermissions",
                                "wallet_registerOnboarding",
                                "wallet_watchAsset",
                                "wallet_scanQRCode"
                            },
                            Chains = group.Select(chainEntry => chainEntry.ChainId).ToArray(),
                            Events = new[]
                            {
                                "chainChanged",
                                "accountsChanged",
                                "message",
                                "disconnect",
                                "connect"
                            }
                        }
                    )
            };
            _connectionProposal = new WalletConnectConnectionProposal(this, WalletConnectInstance.SignClient, connectOptions);
            return _connectionProposal;
        }

        protected override async Task DisconnectAsyncCore()
        {
            await WalletConnectInstance.DisconnectAsync();
        }

        protected override async Task ChangeActiveChainAsyncCore(Chain chain)
        {
            if (ActiveSessionSupportsMethod("wallet_switchEthereumChain") && !ActiveSessionIncludesChain(chain.ChainId))
            {
                var ethereumChain = new EthereumChain(chain);
                await WalletConnectInstance.SwitchEthereumChainAsync(ethereumChain);
            }
            else
            {
                if (!ActiveSessionIncludesChain(chain.ChainId))
                    throw new Exception("Chain is not supported"); // TODO: use custom ex type

                await WalletConnectInstance.SignClient.AddressProvider.SetDefaultChainIdAsync(chain.ChainId);
                OnChainChanged(new ChainChangedEventArgs(chain.ChainId));
                OnAccountChanged(new AccountChangedEventArgs(GetCurrentAccount()));
            }
        }

        protected override Task<Account> GetAccountAsyncCore()
        {
            return Task.FromResult(GetCurrentAccount());
        }

        protected override Task<Account[]> GetAccountsCore()
        {
            var ciapAddresses = WalletConnectInstance.SignClient.AddressProvider.AllAddresses();
            return Task.FromResult(ciapAddresses.Select(ciapAddress => new Account(ciapAddress.Address, ciapAddress.ChainId)).ToArray());
        }
        
        private Account GetCurrentAccount()
        {
            var ciapAddress = WalletConnectInstance.SignClient.AddressProvider.CurrentAddress();
            return new Account(ciapAddress.Address, ciapAddress.ChainId);
        }

        private static bool ActiveSessionSupportsMethod(string method)
        { 
            var @namespace = WalletConnectInstance.SignClient.AddressProvider.DefaultNamespace;
            var activeSession = WalletConnectInstance.ActiveSession;
            return activeSession.Namespaces[@namespace].Methods.Contains(method);
        }

        private static bool ActiveSessionIncludesChain(string chainId)
        {
            var @namespace = WalletConnectInstance.SignClient.AddressProvider.DefaultNamespace;
            var activeSession = WalletConnectInstance.ActiveSession;
            var activeNamespace = activeSession.Namespaces[@namespace];

            var chainsOk = activeNamespace.TryGetChains(out var approvedChains);
            return chainsOk && approvedChains.Contains(chainId);
        }
    }
}