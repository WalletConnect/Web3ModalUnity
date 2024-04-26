using UnityEngine;
using UnityEngine.UIElements;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnect.UI
{
    public class QrCode : VisualElement
    {
        public static readonly string ussClassName = "qrcode";

        public string Data
        {
            get => _data;
            set
            {
#if UNITY_EDITOR
                // To bypass UI Builder errors
                if (WCLoadingAnimator.Instance == null)
                    return;
#endif

                if (string.IsNullOrWhiteSpace(value))
                {
                    WCLoadingAnimator.Instance.Subscribe(_qrcodeImage);
                    _data = value;
                }
                else
                {
                    WCLoadingAnimator.Instance.Unsubscribe(_qrcodeImage);
                    _qrcodeImage.image = QRCode.EncodeTexture(value);
                    _data = value;
                }
            }
        }

        private readonly Image _qrcodeImage;
        private string _data;

        public new class UxmlFactory : UxmlFactory<QrCode>
        {
        }

        public QrCode() : this(string.Empty)
        {
        }

        public QrCode(string data)
        {
            var asset = Resources.Load<VisualTreeAsset>("WalletConnect/Web3Modal/Components/QrCode/QrCode");
            asset.CloneTree(this);

            AddToClassList(ussClassName);

            _qrcodeImage = this.Q<Image>("qrcode-image");

            Data = data;
        }
    }
}