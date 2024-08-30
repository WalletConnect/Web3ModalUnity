using mixpanel;
using Skibitsky.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WalletConnect.Web3Modal.Sample
{
    public class AppKitInit : MonoBehaviour
    {
        [SerializeField] private SceneReference _menuScene;
        
        private WalletConnectUnity.Core.Chain currentChain = new WalletConnectUnity.Core.Chain(
            chainNamespace: "eip155", 
            chainReference: "97",
            name: "Binance Smart Chain Testnet",
            viemName: "bscTestnet",
            nativeCurrency: new WalletConnectUnity.Core.Currency
            (
                "Binance Coin",
                "tBNB",
                18
            ),
            blockExplorer: new WalletConnectUnity.Core.BlockExplorer
                ("BscScan", "https://testnet.bscscan.com"),
            rpcUrl: "https://data-seed-prebsc-1-s1.bnbchain.org:8545/",
            isTestnet: true,
            imageUrl: "https://cryptologos.cc/logos/binance-coin-bnb-logo.png" // Example image URL for Binance Coin
        );

        private async void Start()
        {
            Debug.Log($"[AppKit Init] Initializing AppKit...");
            await Web3Modal.InitializeAsync(new Web3ModalConfig
            {
                supportedChains = new[]
                {
                    currentChain
                }
            });
            
            // await Web3Modal.InitializeAsync();

            var wc = WalletConnectConnector.WalletConnectInstance;
            if (wc is { IsInitialized: true })
            {
                var clientId = await wc.SignClient.Core.Crypto.GetClientId();
                Mixpanel.Identify(clientId);
            }

            Debug.Log($"[AppKit Init] AppKit initialized. Loading menu scene...");
            SceneManager.LoadScene(_menuScene);
        }
    }
}