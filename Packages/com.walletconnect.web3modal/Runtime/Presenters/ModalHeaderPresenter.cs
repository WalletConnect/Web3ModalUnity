using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnect.Web3Modal.Utils;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal
{
    public class ModalHeaderPresenter
    {
        public readonly RouterController routerController;
        public readonly ModalHeader modalHeader;

        public readonly Label title;

        private readonly Dictionary<ViewType, VisualElement> _leftSlotItems = new();

        private Coroutine _snackbarCoroutine;

        public ModalHeaderPresenter(RouterController routerController, ModalHeader modalHeader)
        {
            this.routerController = routerController;
            this.modalHeader = modalHeader;

            this.routerController.ViewChanged += ViewChangedHandler;
            Web3Modal.NotificationController.Notification += NotificationHandler;
            Web3Modal.ModalController.OpenStateChanged += ModalOpenStateChangedHandler;

            title = new Label();
            title.AddToClassList("text-paragraph");
            this.modalHeader.body.Add(title);

            // Create Back button and add it to the left slot
            var goBackIconLink = new IconLink(
                Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_medium_chevronleft"),
                routerController.GoBack)
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };
            modalHeader.leftSlot.Add(goBackIconLink);

            // Assign buttons to the corresponding view types
            _leftSlotItems.Add(ViewType.QrCode, goBackIconLink);
            _leftSlotItems.Add(ViewType.Wallet, goBackIconLink);
            _leftSlotItems.Add(ViewType.WalletSearch, goBackIconLink);
            _leftSlotItems.Add(ViewType.NetworkSearch, goBackIconLink);
            _leftSlotItems.Add(ViewType.NetworkLoading, goBackIconLink);

            // Close button
            modalHeader.rightSlot.Add(new IconLink(
                Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_bold_xmark"),
                Web3Modal.CloseModal
            ));
        }

        private void ModalOpenStateChangedHandler(object _, ModalOpenStateChangedEventArgs e)
        {
            if (!e.IsOpen)
            {
                modalHeader.leftSlot.style.visibility = Visibility.Hidden;
            }
        }

        private void NotificationHandler(object sender, NotificationEventArgs notification)
        {
            if (_snackbarCoroutine != null)
                UnityEventsDispatcher.Instance.StopCoroutine(_snackbarCoroutine);

            _snackbarCoroutine = UnityEventsDispatcher.Instance.StartCoroutine(ShowSnackbarCoroutine(notification));
        }

        private IEnumerator ShowSnackbarCoroutine(NotificationEventArgs notification)
        {
            var snackbarIconColor = notification.type switch
            {
                NotificationType.Error => Snackbar.IconColor.Error,
                NotificationType.Success => Snackbar.IconColor.Success,
                _ => Snackbar.IconColor.Success // TODO: change to info
            };

            var icon = notification.type switch
            {
                NotificationType.Error => Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_bold_warningcircle"),
                NotificationType.Success => Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_bold_checkmark"),
                _ => Resources.Load<VectorImage>("WalletConnect/Web3Modal/Icons/icon_bold_warningcircle")
            };

            modalHeader.ShowSnackbar(snackbarIconColor, icon, notification.message);

            yield return new WaitForSeconds(2);
            modalHeader.HideSnackbar();

            _snackbarCoroutine = null;
        }

        private void ViewChangedHandler(object _, ViewChangedEventArgs args)
        {
            title.text = args.newViewType == ViewType.None
                ? string.Empty
                : args.newPresenter.Title.FontWeight600();

            if (_leftSlotItems.TryGetValue(args.oldViewType, out var oldItem))
                oldItem.style.display = DisplayStyle.None;

            if (_leftSlotItems.TryGetValue(args.newViewType, out var newItem))
            {
                newItem.style.display = DisplayStyle.Flex;
                modalHeader.leftSlot.style.visibility = Visibility.Visible;
            }
            else
            {
                modalHeader.leftSlot.style.visibility = Visibility.Hidden;
            }

            if (args.newPresenter != null)
                modalHeader.style.borderBottomWidth = args.newPresenter.HeaderBorder
                    ? 1
                    : 0;
        }
    }
}