using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;

namespace WalletConnect.Web3Modal.CustomizationSample
{
    public class CustomAccountPresenter : AccountPresenter
    {
        public CustomAccountPresenter(RouterController router, VisualElement parent) : base(router, parent)
        {
        }

        protected override void BuildButtons(VisualElement buttonsListView)
        {
            CreateOpenSeaButton(buttonsListView);

            base.BuildButtons(buttonsListView);
        }

        private void CreateOpenSeaButton(VisualElement buttonsListView)
        {
            var openSeaIcon = Resources.Load<Sprite>("OpenSea Logo");
            var openSeaButton = new ListItem(
                "OpenSea",
                openSeaIcon,
                OnOpenSeaButtonClick,
                iconType: ListItem.IconType.Circle,
                iconStyle: ListItem.IconStyle.Accent);

            Buttons.Add(openSeaButton);
            buttonsListView.Add(openSeaButton);
        }

        private void OnOpenSeaButtonClick()
        {
            var address = Web3Modal.AccountController.Address;
            var url = $"https://opensea.io/{address}";
            Application.OpenURL(url);
        }
    }
}