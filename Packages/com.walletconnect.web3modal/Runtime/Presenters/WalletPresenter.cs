using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnect.Web3Modal.Utils;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using Button = UnityEngine.UIElements.Button;

namespace WalletConnect.Web3Modal
{
    public class WalletPresenter : Presenter<WalletView>
    {
        private Wallet _wallet;
        
        private readonly Tabbed _tabbed;
        private readonly VisualElement _tabsContainer;

        private readonly QrCodePresenter _qrCodePresenter;
        private readonly DeepLinkPresenter _deepLinkPresenter;
        private readonly WebAppPresenter _webAppPresenter;

        private readonly Label _qrCodeTab;
        private readonly Label _deepLinkTab;
        private readonly Label _browserTab;
        private readonly VisualElement _qrCodeContent;
        private readonly VisualElement _deepLinkContent;
        private readonly VisualElement _webAppContent;
        private readonly QrCodeView _qrCodeView;
        private readonly DeepLinkView _deepLinkView;
        private readonly WebAppView _webAppView;

        private readonly Dictionary<VisualElement, PresenterBase> _tabContentToViewController = new();

        public WalletPresenter(RouterController router, VisualElement parent) : base(router)
        {
            // --- View
            View = new WalletView
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };;
            parent.Add(View);

            // --- Tabbed View References
            _tabbed = View.Q<Tabbed>();
            _tabsContainer = View.Q<VisualElement>(className: Tabbed.ClassNameTabsContainer);

            // --- Tabs References
            _qrCodeTab = View.Q<Label>("QrCodeTab");
            _deepLinkTab = View.Q<Label>("DeepLinkTab");
            _browserTab = View.Q<Label>("WebAppTab");

            // --- Tabs Content References and Controllers           
            _qrCodeContent = View.Q<VisualElement>("QrCodeContent");
            _deepLinkContent = View.Q<VisualElement>("DeepLinkContent");
            _webAppContent = View.Q<VisualElement>("WebAppContent");

            _qrCodeView = _qrCodeContent.Q<QrCodeView>();
            _deepLinkView = _deepLinkContent.Q<DeepLinkView>();
            _webAppView = _webAppContent.Q<WebAppView>();

            _qrCodePresenter = new QrCodePresenter(router, _qrCodeView);
            _deepLinkPresenter = new DeepLinkPresenter(router, _deepLinkView);
            _webAppPresenter = new WebAppPresenter(router, _webAppView);

            _tabContentToViewController.Add(_qrCodeContent, _qrCodePresenter);
            _tabContentToViewController.Add(_deepLinkContent, _deepLinkPresenter);
            _tabContentToViewController.Add(_webAppContent, _webAppPresenter);

            // --- Events
            _tabbed.ContentShown += element => _tabContentToViewController[element].OnVisible();
            _tabbed.ContentHidden += element => _tabContentToViewController[element].OnDisable();
            View.GetWalletClicked += OnGetWalletClicked;

            // --- Additional Setup
            HideAllTabs();
#if UNITY_ANDROID || UNITY_IOS
            _deepLinkTab.text = "Mobile";
#endif
        }

        private void HideAllTabs()
        {
            HideTab(_qrCodeTab);
            HideTab(_deepLinkTab);
            HideTab(_browserTab);
        }

        protected override void OnVisibleCore()
        {
            base.OnVisibleCore();

            if (WalletUtils.TryGetLastViewedWallet(out var wallet))
            {
                _wallet = wallet;
                Title = wallet.Name;

                ConfigureTabs(wallet);
                ConfigureGetWalletContainer(wallet);
            }
        }

        private void ConfigureGetWalletContainer(Wallet wallet)
        {
            var visible = !WalletUtils.IsWalletInstalled(wallet);
            View.GetWalletContainer.style.display = visible
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            if (!visible)
                return;

            View.GetWalletLabel.text = $"Don't have {wallet.Name}?".FontWeight500();
            View.LandscapeContinueInLabel.text = $"Continue in {wallet.Name}".FontWeight500();
        }

        protected void OnGetWalletClicked()
        {
#if UNITY_IOS
            Application.OpenURL(_wallet.AppStore);
#elif UNITY_ANDROID
            Application.OpenURL(_wallet.PlayStore);
#else
            // TODO: on desktop and webgl show the list of all available options
            Application.OpenURL(_wallet.Homepage);
#endif
        }

        protected override void OnHideCore()
        {
            base.OnHideCore();
            WalletUtils.RemoveLastViewedWallet();
        }

        protected override void OnDisableCore()
        {
            base.OnDisableCore();

            WalletUtils.RemoveLastViewedWallet();
            HideAllTabs();
        }

        private void ConfigureTabs(Wallet wallet)
        {
            var tabsCount = 0;
#if UNITY_IOS || UNITY_ANDROID
            // Mobile: Deep Link
            if (wallet is { MobileLink: not null })
            {
                ShowTab(_deepLinkTab);
                tabsCount++;
            }
#else
            // Desktop: QR Code
            ShowTab(_qrCodeTab);
            tabsCount++;

            // Desktop: Deep Link
            if (wallet.DesktopLink != null)
            {
                ShowTab(_deepLinkTab);
                tabsCount++;
            }
#endif
            // All: Browser
            if (wallet is { WebappLink: not null })
            {
                ShowTab(_browserTab);
                tabsCount++;
            }

            if (tabsCount == 1)
                HideTabsContainer();
            else
                ShowTabsContainer();

            UnityEventsDispatcher.InvokeNextFrame(_tabbed.ActivateFirstVisibleTab);
        }

        private void HideTabsContainer()
        {
            _tabsContainer.AddToClassList(Tabbed.ClassNameTabsContainerHidden);
        }

        private void ShowTabsContainer()
        {
            if (_tabsContainer.ClassListContains(Tabbed.ClassNameTabsContainerHidden))
                _tabsContainer.RemoveFromClassList(Tabbed.ClassNameTabsContainerHidden);
        }

        private static void HideTab(VisualElement tab)
        {
            tab.AddToClassList(Tabbed.ClassNameTabHidden);
        }

        private static void ShowTab(VisualElement tab)
        {
            if (tab.ClassListContains(Tabbed.ClassNameTabHidden))
                tab.RemoveFromClassList(Tabbed.ClassNameTabHidden);
        }
    }
}