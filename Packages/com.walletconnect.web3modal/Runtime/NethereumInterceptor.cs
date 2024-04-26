// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Nethereum.JsonRpc.Client;
// using Nethereum.WalletConnect;
// using WalletConnectSharp.Sign;
// using WalletConnectUnity.Core;
//
// namespace WalletConnect.Web3Modal
// {
//     public class NethereumInterceptor : RequestInterceptor
//     {
//         private readonly IWalletConnect _walletConnectInstance;
//         private readonly NethereumWalletConnectInterceptor _nethereumWalletConnectInterceptor;
//
//         public NethereumInterceptor(IWalletConnect walletConnectInstance) : this(
//             new NethereumWalletConnectService(walletConnectInstance.SignClient))
//         {
//             _walletConnectInstance = WalletConnectUnity.Core.WalletConnect.Instance;
//         }
//
//         public NethereumInterceptor(INethereumWalletConnectService walletConnectService)
//         {
//             _walletConnectInstance = WalletConnectUnity.Core.WalletConnect.Instance;
//             _nethereumWalletConnectInterceptor = new NethereumWalletConnectInterceptor(walletConnectService);
//         }
//
//         public NethereumInterceptor(WalletConnectSignClient walletConnectSignClient)
//         {
//             _walletConnectInstance = WalletConnectUnity.Core.WalletConnect.Instance;
//             _nethereumWalletConnectInterceptor = new NethereumWalletConnectInterceptor(walletConnectSignClient);
//         }
//
//         public override Task<object> InterceptSendRequestAsync<T>(
//             Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync,
//             RpcRequest request,
//             string route = null)
//         {
//             if (!ActiveSessionIncludesMethod(request.Method))
//                 return base.InterceptSendRequestAsync(interceptedSendRequestAsync, request, route);
//
//             var result = _nethereumWalletConnectInterceptor.InterceptSendRequestAsync(interceptedSendRequestAsync, request, route);
//
// #if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
//             var activeSessionTopic = _walletConnectInstance.ActiveSession.Topic;
//             _walletConnectInstance.Linker.OpenSessionRequestDeepLinkAfterMessageFromSession(activeSessionTopic);
// #endif
//
//             return result;
//         }
//
//         public override Task<object> InterceptSendRequestAsync<T>(
//             Func<string, string, object[], Task<T>> interceptedSendRequestAsync,
//             string method,
//             string route = null, params object[] paramList)
//         {
//             if (!ActiveSessionIncludesMethod(method))
//                 return base.InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList);
//
//             var result = _nethereumWalletConnectInterceptor.InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList);
//
// #if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
//             var activeSessionTopic = _walletConnectInstance.ActiveSession.Topic;
//             _walletConnectInstance.Linker.OpenSessionRequestDeepLinkAfterMessageFromSession(activeSessionTopic);
// #endif
//
//             return result;
//         }
//
//         private bool ActiveSessionIncludesMethod(string method)
//         {
//             var @namespace = _walletConnectInstance.SignClient.AddressProvider.DefaultNamespace;
//             var activeSession = _walletConnectInstance.ActiveSession;
//             return activeSession.Namespaces[@namespace].Methods.Contains(method);
//         }
//     }
// }

