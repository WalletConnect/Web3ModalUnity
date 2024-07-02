using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnect.Web3Modal.Utils;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal
{
    public class ModalHeaderPresenter : Presenter<ModalHeader>
    {
        private readonly RouterController _routerController;
        private readonly ModalHeader _modalHeader;
        private readonly Label _title;
        private readonly Dictionary<ViewType, VisualElement> _leftSlotItems = new();

        private Coroutine _snackbarCoroutine;

        public ModalHeaderPresenter(RouterController routerController, ModalHeader modalHeader) : base(routerController)
        {
            View = modalHeader;

            _routerController = routerController;
            _modalHeader = modalHeader;

            _routerController.ViewChanged += ViewChangedHandler;
            Web3Modal.NotificationController.Notification += NotificationHandler;
            Web3Modal.ModalController.OpenStateChanged += ModalOpenStateChangedHandler;

            _title = new Label();
            _title.AddToClassList("text-paragraph");
            _modalHeader.body.Add(_title);

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
                _modalHeader.leftSlot.style.visibility = Visibility.Hidden;
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

            _modalHeader.ShowSnackbar(snackbarIconColor, icon, notification.message);

            yield return new WaitForSeconds(2);
            _modalHeader.HideSnackbar();

            _snackbarCoroutine = null;
        }

        private void ViewChangedHandler(object _, ViewChangedEventArgs args)
        {
            _title.text = args.newViewType == ViewType.None
                ? string.Empty
                : args.newPresenter.Title.FontWeight600();

            if (_leftSlotItems.TryGetValue(args.oldViewType, out var oldItem))
                oldItem.style.display = DisplayStyle.None;

            if (_leftSlotItems.TryGetValue(args.newViewType, out var newItem))
            {
                newItem.style.display = DisplayStyle.Flex;
                _modalHeader.leftSlot.style.visibility = Visibility.Visible;
            }
            else
            {
                _modalHeader.leftSlot.style.visibility = Visibility.Hidden;
            }

            if (args.newPresenter != null)
                _modalHeader.style.borderBottomWidth = args.newPresenter.HeaderBorder
                    ? 1
                    : 0;
        }
    }
}