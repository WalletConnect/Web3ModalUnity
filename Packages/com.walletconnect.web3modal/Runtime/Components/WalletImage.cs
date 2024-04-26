using UnityEngine.UIElements;
using WalletConnect.Web3Modal.Utils;

namespace WalletConnect.UI
{
    public class WalletImage : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<WalletImage, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlEnumAttributeDescription<VisualElementSize> tSize = new()
            {
                name = "size"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var image = ve as WalletImage;
                image.Size = tSize.GetValueFromBag(bag, cc);

                image.AddToClassList(ussClassName);
            }
        }

        public VisualElementSize Size
        {
            get => _size;
            set
            {
                _size = value;
                switch (value)
                {
                    case VisualElementSize.Small:
                        AddToClassList(imageSmallUssClassName);
                        break;
                    case VisualElementSize.Medium:
                        AddToClassList(imageMediumUssClassName);
                        break;
                    case VisualElementSize.Large:
                        AddToClassList(imageLargeUssClassName);
                        break;
                }
            }
        }

        public static readonly string ussClassName = "wallet-image";
        public static readonly string imageSmallUssClassName = "wallet-image--size-small";
        public static readonly string imageMediumUssClassName = "wallet-image--size-medium";
        public static readonly string imageLargeUssClassName = "wallet-image--size-large";

        private VisualElementSize _size;

        public WalletImage()
        {
            AddToClassList(ussClassName);
        }
    }
}