using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace WalletConnect.UI
{
    public class NetworkSearchView : VisualElement
    {
        public const string Name = "network-search-view";
        public static readonly string NameList = $"{Name}__list";
        public static readonly string NameInput = $"{Name}__input";

        public readonly ScrollView scrollView;
        public readonly TextInput searchInput;

        public new class UxmlFactory : UxmlFactory<NetworkSearchView>
        {
        }

        public NetworkSearchView()
        {
            var asset = Resources.Load<VisualTreeAsset>("WalletConnect/Web3Modal/Views/NetworkSearchView/NetworkSearchView");
            asset.CloneTree(this);

            name = Name;

            scrollView = this.Q<ScrollView>(NameList);
        }
    }
}