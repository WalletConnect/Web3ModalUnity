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
        protected UIDocument UIDocument { get; private set; }

        protected Modal Modal { get; private set; }

        protected VisualElement Web3Modal { get; private set; }

        protected RouterController RouterController { get; private set; }

        protected ModalHeaderPresenter ModalHeaderPresenter { get; private set; }
        
        private readonly ModalOpenStateChangedEventArgs _openStateChangedEventArgsTrueOnOpen = new(true);
        private readonly ModalOpenStateChangedEventArgs _openStateChangedEventArgsTrueOnClose = new(false);
        
        public override Task InitializeAsyncCore()
        {
            Debug.Log("Initializing ModalControllerUtk...");
            var web3Modal = WalletConnect.Web3Modal.Web3Modal.Instance;
            UIDocument = web3Modal.GetComponentInChildren<UIDocument>(true);
            
            Web3Modal = UIDocument.rootVisualElement.Children().First();

            Modal = Web3Modal.Q<Modal>();

            RouterController = new RouterController(Modal.body);
            RouterController.ViewChanged += ViewChangedHandler;

            ModalHeaderPresenter = new ModalHeaderPresenter(RouterController, Modal.header);

            WCLoadingAnimator.Instance.PauseAnimation();
            
            UnityEventsDispatcher.Instance.Tick += TickHandler;

            Debug.Log("ModalControllerUtk initialized.");

            return Task.CompletedTask;
        }

        private void ViewChangedHandler(object _, ViewChangedEventArgs args)
        {
            if (args.newViewType == ViewType.None)
                CloseCore();
        }

        public override void OpenCore(ViewType view)
        {
            Web3Modal.visible = true;
            RouterController.OpenView(view);
            WCLoadingAnimator.Instance.ResumeAnimation();
            OnOpenStateChanged(_openStateChangedEventArgsTrueOnOpen);
        }

        public override void CloseCore()
        {
            Web3Modal.visible = false;
            WCLoadingAnimator.Instance.PauseAnimation();
            RouterController.CloseAllViews();
            OnOpenStateChanged(_openStateChangedEventArgsTrueOnClose);
        }

        private void TickHandler()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                RouterController.GoBack();
        }
    }
}