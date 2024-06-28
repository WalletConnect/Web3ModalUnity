using UnityEngine;
using UnityEngine.UI;

namespace WalletConnect.Web3Modal.CustomizationSample
{
    public class Dapp : MonoBehaviour
    {
        [SerializeField] private Button _connectButton;

        private void Awake()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            _connectButton.interactable = false;
        }

        private async void Start()
        {
            Debug.Log("Init Web3Modal...");
            try
            {
                await Web3Modal.InitializeAsync();
                
                Web3Modal.AccountConnected += async (_, e) =>
                {
                    _connectButton.interactable = false;
                };

                Web3Modal.AccountDisconnected += (_, _) =>
                {
                    _connectButton.interactable = true;
                };
                
                var resumed = await Web3Modal.ConnectorController.TryResumeSessionAsync();
                
                _connectButton.interactable = !resumed;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Web3Modal] Initialization failed: {e.Message}");
                return;
            }
        }

        public void OnConnectButton()
        {
            Web3Modal.OpenModal();
        }
    }
}