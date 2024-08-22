using mixpanel;
using Skibitsky.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WalletConnect.Web3Modal.Sample
{
    public class AppKitInit : MonoBehaviour
    {
        [SerializeField] private SceneReference _menuScene;

        private async void Start()
        {
            Debug.Log($"[AppKit Init] Initializing AppKit...");
            await Web3Modal.InitializeAsync();

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