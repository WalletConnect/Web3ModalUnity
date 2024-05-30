using System.Threading.Tasks;
using UnityEngine;
using WalletConnect.Web3Modal.WebGl.Modal;

using NativeViewType = WalletConnect.Web3Modal.ViewType;
using WebGlViewType = WalletConnect.Web3Modal.WebGl.Modal.ViewType;

namespace WalletConnect.Web3Modal.WebGl
{
    /// <summary>
    /// Modal Controller for the web implementation of the Web3Modal that uses Wagmi.
    /// </summary>
    public class ModalControllerWebGl : ModalController
    {
        public override Task InitializeAsyncCore()
        {
            ModalInterop.StateChanged += StateChangedHandler;
            return Task.CompletedTask;
        }

        private void StateChangedHandler(ModalState modalState)
        {
            WebGLInput.captureAllKeyboardInput = !modalState.open;
            OnOpenStateChanged(new ModalOpenStateChangedEventArgs(modalState.open));
        }

        public override void OpenCore(NativeViewType view)
        {
            var viewType = ConvertViewType(view);
            ModalInterop.Open(new OpenModalParameters(viewType));
        }

        public override void CloseCore()
        {
            ModalInterop.Close();
        }
        
        private static WebGlViewType ConvertViewType(NativeViewType viewType)
        {
            return viewType switch
            {
                NativeViewType.Connect => WebGlViewType.Connect,
                NativeViewType.None => WebGlViewType.Connect,
                NativeViewType.Account => WebGlViewType.Account,
                NativeViewType.WalletSearch => WebGlViewType.AllWallets,
                NativeViewType.NetworkSearch => WebGlViewType.Networks,
                NativeViewType.QrCode => WebGlViewType.ConnectingWalletConnect,
                NativeViewType.Wallet => WebGlViewType.ConnectWallets,
                NativeViewType.NetworkLoading => WebGlViewType.Networks,
                _ => throw new System.NotImplementedException()
            };
        }
    }
}