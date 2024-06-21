using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.Web3Modal.Utils;

namespace WalletConnect.UI
{
    public class AccountView : VisualElement
    {
        public const string Name = "account-view";
        public static readonly string NameProfile = $"{Name}__profile";
        public static readonly string NameProfileAddress = $"{Name}__profile-address";
        public static readonly string NameProfileAvatarImage = $"{Name}__profile-avatar-image";
        public static readonly string NameProfileBalanceValue = $"{Name}__profile-balance-value";
        public static readonly string NameProfileBalanceSymbol = $"{Name}__profile-balance-symbol";
        public static readonly string NameButtons = $"{Name}__buttons";
        
        public VisualElement Profile { get; }
        public Label ProfileAddress { get; }
        public Image ProfileAvatarImage { get; }
        public Label ProfileBalanceValue { get; }
        public Label ProfileBalanceSymbol { get; }
        public VisualElement Buttons { get; }
        
        
        public new class UxmlFactory : UxmlFactory<AccountView>
        {
        }

        public AccountView()
        {
            var asset = Resources.Load<VisualTreeAsset>("WalletConnect/Web3Modal/Views/AccountView/AccountView");
            asset.CloneTree(this);

            name = Name;
            
            Profile = this.Q<VisualElement>(NameProfile);
            ProfileAddress = Profile.Q<Label>(NameProfileAddress);
            ProfileAvatarImage = Profile.Q<Image>(NameProfileAvatarImage);
            ProfileBalanceValue = Profile.Q<Label>(NameProfileBalanceValue);
            ProfileBalanceSymbol = Profile.Q<Label>(NameProfileBalanceSymbol);
            Buttons = this.Q<VisualElement>(NameButtons);
        }

        public void SetProfileName(string value)
        {
            ProfileAddress.text = value.FontWeight600();
        }

        public void SetBalance(string value)
        {
            ProfileBalanceValue.text = value.FontWeight500();
        }
        
        public void SetBalanceSymbol(string value)
        {
            ProfileBalanceSymbol.text = value.FontWeight500();
        }
    }
}