using System;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnectUnity.Core.Utils;
using DeviceType = WalletConnectUnity.Core.Utils.DeviceType;

namespace WalletConnect.UI
{
    public class WalletSearchView : VisualElement
    {
        public const string Name = "wallet-search-view";
        public static readonly string ClassNameList = $"{Name}__list";
        public static readonly string ClassNameInput = $"{Name}__input";
        public static readonly string ClassNameQrCodeLink = $"{Name}__qr-code-link";

        public event Action<float> ScrollValueChanged
        {
            add => scroller.valueChanged += value;
            remove => scroller.valueChanged -= value;
        }

        public event Action QrCodeLinkClicked
        {
            add => qrCodeLink.Clicked += value;
            remove => qrCodeLink.Clicked -= value;
        }

        public event Action<string> SearchInputValueChanged;

        public readonly ScrollView scrollView;
        public readonly Scroller scroller;
        public readonly TextInput searchInput;
        public readonly IconLink qrCodeLink;
        public readonly VisualElement leftSlot;
        public readonly VisualElement rightSlot;

        public new class UxmlFactory : UxmlFactory<WalletSearchView>
        {
        }

        public WalletSearchView()
        {
            var asset = Resources.Load<VisualTreeAsset>("WalletConnect/Web3Modal/Views/WalletSearchView/WalletSearchView");
            asset.CloneTree(this);

            AddToClassList(Name);

            var deviceType = DeviceUtils.GetDeviceType();

            // --- Search Input
            searchInput = this.Q<TextInput>();
            searchInput.leftSlot.Add(new Image
            {
                vectorImage = Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_medium_magnifier")
            });
            searchInput.RegisterCallback<ChangeEvent<string>>(evt => SearchInputValueChanged?.Invoke(evt.newValue));

            // --- ScrollView
            scrollView = this.Q<ScrollView>();
            scrollView.mode = ScrollViewMode.Vertical;
            scrollView.mouseWheelScrollSize = 50;
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;

            scroller = scrollView.Q<Scroller>();

            // --- Slots
            leftSlot = this.Q<VisualElement>("left-slot");
            rightSlot = this.Q<VisualElement>("right-slot");

            // --- QR Code Link
            qrCodeLink = this.Q<IconLink>(ClassNameQrCodeLink);
            if (deviceType is DeviceType.Phone)
                qrCodeLink.image.vectorImage = Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_regular_qrcode");
            else
                qrCodeLink.style.display = DisplayStyle.None;
        }
    }
}