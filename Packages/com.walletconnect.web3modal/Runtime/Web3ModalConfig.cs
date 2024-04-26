using System;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal
{
    [Serializable]
    public class Web3ModalConfig
    {
        public string[] includedWalletIds;
        public string[] excludedWalletIds;

        public ushort connectViewWalletsCountMobile = 3;
        public ushort connectViewWalletsCountDesktop = 2;

        public Chain[] supportedChains =
        {
            ChainConstants.Chains.Ethereum,
            ChainConstants.Chains.Arbitrum,
            ChainConstants.Chains.Polygon,
            ChainConstants.Chains.Avalanche,
            ChainConstants.Chains.Optimism,
            ChainConstants.Chains.Base,
            ChainConstants.Chains.Celo,
            ChainConstants.Chains.Ronin
        };
    }
}