using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnectUnity.Core;
using WalletConnectUnity.UI;

namespace WalletConnect.Web3Modal
{
    public class NetworkSearchPresenter : Presenter<NetworkSearchView>
    {
        public override string Title
        {
            get => "Choose network";
        }

        private readonly List<VisualElement> _items = new();

        private readonly Dictionary<string, CardSelect> _netowrkItems = new();
        private string _highlightedChainId;

        public NetworkSearchPresenter(RouterController router, VisualElement parent) : base(router, parent)
        {
            View.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            Web3Modal.Initialized += (_, _) =>
            {
                foreach (var chain in Web3Modal.NetworkController.Chains.Values)
                {
                    var item = MakeNetworkItem(chain);

                    _netowrkItems[chain.ChainId] = item;
                    View.scrollView.Add(item);
                }

                Web3Modal.NetworkController.ChainChanged += ChainChangedHandler;

                var activeChain = Web3Modal.NetworkController.ActiveChain;
                if (activeChain != default)
                    HighlightActiveChain(activeChain.ChainId);
            };
        }

        private void ChainChangedHandler(object sender, NetworkController.ChainChangedEventArgs e)
        {
            if (e.Chain == null)
                return;

            HighlightActiveChain(e.Chain.ChainId);
        }

        private void HighlightActiveChain(string chainId)
        {
            if (_highlightedChainId != null && _netowrkItems.TryGetValue(_highlightedChainId, out var prevItem))
                prevItem.SetActivated(false);

            if (_netowrkItems.TryGetValue(chainId, out var item))
                item.SetActivated(true);

            _highlightedChainId = chainId;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ConfigureItemPaddings();
        }

        private CardSelect MakeNetworkItem(Chain chain)
        {
            var item = new CardSelect
            {
                LabelText = chain.Name
            };

            item.Clicked += async () =>
            {
                PlayerPrefs.SetString("WC_SELECTED_CHAIN_ID", chain.ChainId);
                if (!Web3Modal.IsAccountConnected)
                {
                    await Web3Modal.NetworkController.ChangeActiveChainAsync(chain);
                    Router.OpenView(ViewType.Connect);
                }
                else
                {
                    await ChangeChainWithTimeout(chain);
                }
            };

            var hexagon = Resources.Load<VectorImage>("WalletConnect/Web3Modal/Images/hexagon");
            var imageContainer = item.Q<VisualElement>(CardSelect.NameImageContainer);
            imageContainer.style.backgroundImage = new StyleBackground(hexagon);
            imageContainer.style.width = 52;
            item.Q<VisualElement>(CardSelect.NameIconImageBorder).style.display = DisplayStyle.None;

            var remoteSprite = RemoteSpriteFactory.GetRemoteSprite<Image>(chain.ImageUrl);
            item.Icon = remoteSprite;

            _items.Add(item);
            return item;
        }

        private async Task ChangeChainWithTimeout(Chain chain)
        {
            try
            {
                var changeChainTask = Web3Modal.NetworkController.ChangeActiveChainAsync(chain);

                await Task.Delay(TimeSpan.FromMilliseconds(70));

                if (changeChainTask.IsCompleted)
                    Router.GoBack();
                else
                    Router.OpenView(ViewType.NetworkLoading);

                await changeChainTask;
            }
            catch (Exception e)
            {
                Web3Modal.NotificationController.Notify(NotificationType.Error, e.Message);
                Router.GoBack();
                throw;
            }
        }

        private void ConfigureItemPaddings(IList<VisualElement> items = null)
        {
            var scrollViewWidth = View.scrollView.resolvedStyle.width;
            const float itemWidth = 79;
            var itemsCanFit = Mathf.FloorToInt(scrollViewWidth / itemWidth);

            var padding = (scrollViewWidth - itemsCanFit * itemWidth) / itemsCanFit / 2;
            items ??= _items;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.style.paddingLeft = padding;
                item.style.paddingRight = padding;
            }
        }
    }
}