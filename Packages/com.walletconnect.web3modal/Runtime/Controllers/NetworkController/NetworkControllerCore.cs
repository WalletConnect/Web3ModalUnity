using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal
{
    public class NetworkControllerCore : NetworkController
    {
        protected override Task InitializeAsyncCore(IEnumerable<Chain> supportedChains)
        {
            Chains = new ReadOnlyDictionary<string, Chain>(supportedChains.ToDictionary(c => c.ChainId, c => c));

            ActiveChain = null;

            return Task.CompletedTask;
        }

        protected override async Task ChangeActiveChainAsyncCore(Chain chain)
        {
            if (Web3Modal.ConnectorController.IsAccountConnected)
                // Request connector to change active chain.
                // If connector approves the change, it will trigger the ChainChanged event.
                await Web3Modal.ConnectorController.ChangeActiveChainAsync(chain);
            else
                ActiveChain = chain;

            Web3Modal.EventsController.SendEvent(new Event
            {
                name = "SWITCH_NETWORK",
                properties = new Dictionary<string, object>
                {
                    { "network", chain.ChainId }
                }
            });
        }

        protected override void ConnectorChainChangedHandlerCore(object sender, Connector.ChainChangedEventArgs e)
        {
            var chain = Chains.GetValueOrDefault(e.ChainId);

            ActiveChain = chain;
            OnChainChanged(new ChainChangedEventArgs(chain));
        }

        protected override async void ConnectorAccountConnectedHandlerCore(object sender, Connector.AccountConnectedEventArgs e)
        {
            var accounts = await e.GetAccounts();

            if (ActiveChain == null)
            {
                var defaultAccount = await e.GetAccount();

                if (Chains.TryGetValue(defaultAccount.ChainId, out var defaultAccountChain))
                {
                    ActiveChain = defaultAccountChain;
                    OnChainChanged(new ChainChangedEventArgs(defaultAccountChain));
                    return;
                }

                var account = Array.Find(accounts, a => Chains.ContainsKey(a.ChainId));
                if (account == default)
                {
                    ActiveChain = null;
                    OnChainChanged(new ChainChangedEventArgs(null));
                    return;
                }

                var chain = Chains[account.ChainId];

                ActiveChain = chain;
                OnChainChanged(new ChainChangedEventArgs(chain));
            }
            else
            {
                var defaultAccount = await e.GetAccount();
                if (defaultAccount.ChainId == ActiveChain.ChainId)
                    return;

                if (Array.Exists(accounts, a => a.ChainId == ActiveChain.ChainId))
                {
                    await ChangeActiveChainAsync(ActiveChain);
                    return;
                }

                var account = Array.Find(accounts, a => Chains.ContainsKey(a.ChainId));
                if (account == default)
                {
                    ActiveChain = null;
                    OnChainChanged(new ChainChangedEventArgs(null));
                }
                else
                {
                    var chain = Chains[account.ChainId];

                    ActiveChain = chain;
                    OnChainChanged(new ChainChangedEventArgs(chain));
                }
            }
        }
    }
}