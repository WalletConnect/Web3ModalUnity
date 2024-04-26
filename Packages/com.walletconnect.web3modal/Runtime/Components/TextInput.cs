using UnityEngine;
using UnityEngine.UIElements;

namespace WalletConnect.UI
{
    public class TextInput : VisualElement
    {
        public static string ClassName = "text-input";
        public readonly string ClassNameContainer = $"{ClassName}__container";
        public readonly string ClassNameLeftSlot = $"{ClassName}__left-slot";
        public readonly string ClassNameRightSlot = $"{ClassName}__right-slot";

        public readonly VisualElement container;
        public readonly VisualElement leftSlot;
        public readonly VisualElement rightSlot;

        private readonly TextField _textField;

        public new class UxmlFactory : UxmlFactory<TextInput>
        {
        }

        public TextInput()
        {
            var asset = Resources.Load<VisualTreeAsset>("WalletConnect/Web3Modal/Components/TextInput/TextInput");
            asset.CloneTree(this);

            AddToClassList(ClassName);

            container = this.Q<VisualElement>(ClassNameContainer);
            leftSlot = this.Q<VisualElement>(ClassNameLeftSlot);
            rightSlot = this.Q<VisualElement>(ClassNameRightSlot);
            _textField = this.Q<TextField>();

            _textField.RegisterCallback<FocusInEvent, VisualElement>(
                (evt, c) => c.AddToClassList("text-input__container--focused"),
                container
            );

            _textField.RegisterCallback<FocusOutEvent, VisualElement>(
                (evt, c) => c.RemoveFromClassList("text-input__container--focused"),
                container
            );
        }
    }
}