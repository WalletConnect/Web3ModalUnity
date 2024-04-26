using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnectUnity.UI;

namespace WalletConnect.Web3Modal
{
    public class ModalController : MonoBehaviour
    {
        [field: SerializeField] public UIDocument UIDocument { get; private set; }

        public Modal Modal { get; private set; }

        public VisualElement Web3Modal { get; private set; }

        public VisualElement ModalBody
        {
            get => Modal.body;
        }

        public RouterController RouterController { get; private set; }

        public ModalHeaderPresenter ModalHeaderPresenter { get; private set; }

        public bool IsOpen
        {
            get => Web3Modal.visible;
        }

        public event EventHandler Closed;

        public Task InitializeAsync()
        {
            Web3Modal = UIDocument.rootVisualElement.Children().First();

            Modal = Web3Modal.Q<Modal>();

            RouterController = new RouterController(Modal.body);
            RouterController.ViewChanged += ViewChangedHandler;

            ModalHeaderPresenter = new ModalHeaderPresenter(RouterController, Modal.header);

            WCLoadingAnimator.Instance.PauseAnimation();

            return Task.CompletedTask;
        }

        private void ViewChangedHandler(object _, ViewChangedEventArgs args)
        {
            if (args.newViewType == ViewType.None)
                Close();
        }

        public void Open(ViewType view)
        {
            Web3Modal.visible = true;
            RouterController.OpenView(view);
            WCLoadingAnimator.Instance.ResumeAnimation();
        }

        public void Close()
        {
            Web3Modal.visible = false;
            WCLoadingAnimator.Instance.PauseAnimation();
            RouterController.CloseAllViews();
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                RouterController.GoBack();
        }
    }
}