using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnectUnity.Core;
using WalletConnectUnity.UI;

namespace WalletConnect.Web3Modal
{
    /// <summary>
    /// ModalController for Unity UI Toolkit
    /// </summary>
    public class ModalControllerUtk : ModalController
    {
        public UIDocument UIDocument { get; private set; }

        public Modal Modal { get; private set; }

        public VisualElement Web3ModalElement { get; private set; }

        public RouterController RouterController { get; private set; }

        protected ModalHeaderPresenter ModalHeaderPresenter { get; private set; }

        private readonly ModalOpenStateChangedEventArgs _openStateChangedEventArgsTrueOnOpen = new(true);
        private readonly ModalOpenStateChangedEventArgs _openStateChangedEventArgsTrueOnClose = new(false);

        protected override Task InitializeAsyncCore()
        {
            var web3Modal = Web3Modal.Instance;
            UIDocument = web3Modal.GetComponentInChildren<UIDocument>(true);

            Web3ModalElement = UIDocument.rootVisualElement.Children().First();

            Modal = Web3ModalElement.Q<Modal>();

            RouterController = new RouterController(Modal.body);
            RouterController.ViewChanged += ViewChangedHandler;

            ModalHeaderPresenter = new ModalHeaderPresenter(RouterController, Modal);

            WCLoadingAnimator.Instance.PauseAnimation();

            UnityEventsDispatcher.Instance.Tick += TickHandler;

            return Task.CompletedTask;
        }

        private void ViewChangedHandler(object _, ViewChangedEventArgs args)
        {
            if (args.newViewType == ViewType.None)
                CloseCore();
        }

        protected override void OpenCore(ViewType view)
        {
            Web3ModalElement.visible = true;
            RouterController.OpenView(view);
            WCLoadingAnimator.Instance.ResumeAnimation();
            OnOpenStateChanged(_openStateChangedEventArgsTrueOnOpen);

            Web3Modal.EventsController.SendEvent(new Event
            {
                name = "MODAL_OPEN",
                properties = new Dictionary<string, object>
                {
                    { "connected", Web3Modal.IsAccountConnected }
                }
            });
        }

        protected override void CloseCore()
        {
            Web3ModalElement.visible = false;
            WCLoadingAnimator.Instance.PauseAnimation();
            RouterController.CloseAllViews();
            OnOpenStateChanged(_openStateChangedEventArgsTrueOnClose);

            Web3Modal.EventsController.SendEvent(new Event
            {
                name = "MODAL_CLOSE",
                properties = new Dictionary<string, object>
                {
                    { "connected", Web3Modal.IsAccountConnected }
                }
            });
        }

        private void TickHandler()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                RouterController.GoBack();
        }
    }
}