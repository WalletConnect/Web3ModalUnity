using System;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

namespace WalletConnect.Web3Modal.CustomizationSample
{
    public class Dapp : MonoBehaviour
    {
        [SerializeField] private Button _connectButton;
        [SerializeField] private Button _accountButton;

        private void Awake()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            _connectButton.interactable = false;
            _accountButton.interactable = false;
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
                    _accountButton.interactable = true;
                };

                Web3Modal.AccountDisconnected += (_, _) =>
                {
                    _connectButton.interactable = true;
                    _accountButton.interactable = false;
                };

                var resumed = await Web3Modal.ConnectorController.TryResumeSessionAsync();

                _connectButton.interactable = !resumed;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Web3Modal] Initialization failed: {e.Message}");
            }
        }

        public static Color32[] EncodePixels(string textForEncoding, int width = 512, int height = 512)
        {
            var qrCodeEncodingOptions = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width,
                Margin = 4,
                QrVersion = 11
            };

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = qrCodeEncodingOptions
            };

            return writer.Write(textForEncoding);
        }

        public void OnConnectButton()
        {
            Web3Modal.OpenModal();
        }

        public void OnAccountButton()
        {
            Web3Modal.OpenModal(ViewType.Account);
        }
    }
}