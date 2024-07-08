using UnityEngine;
using UnityEngine.UIElements;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnect.UI
{
    public class QrCode : VisualElement
    {
        public const string Name = "qrcode";

        private static readonly CustomStyleProperty<Color> BgColorProperty = new("--wui-qrcode-bg-color");
        private static readonly CustomStyleProperty<Color> FgColorProperty = new("--wui-qrcode-fg-color");

        private readonly Image _qrcodeImage;

        private Color _bgColor = Color.white;
        private Color _fgColor = Color.black;
        private string _data;

        public QrCode() : this(string.Empty)
        {
        }

        public QrCode(string data)
        {
            var asset = Resources.Load<VisualTreeAsset>("WalletConnect/Web3Modal/Components/QrCode/QrCode");
            asset.CloneTree(this);

            AddToClassList(Name);

            _qrcodeImage = this.Q<Image>("qrcode-image");

            Data = data;

            RegisterCallback(new EventCallback<CustomStyleResolvedEvent>(CustomStyleResolvedHandler));
        }

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
                    _qrcodeImage.image = QRCode.EncodeTexture(value, _fgColor, _bgColor);
                    _data = value;
                }
            }
        }

        private void CustomStyleResolvedHandler(CustomStyleResolvedEvent evt)
        {
            _ = evt.customStyle.TryGetValue(BgColorProperty, out _bgColor);
            _ = evt.customStyle.TryGetValue(FgColorProperty, out _fgColor);
        }

        public new class UxmlFactory : UxmlFactory<QrCode>
        {
        }
    }
}