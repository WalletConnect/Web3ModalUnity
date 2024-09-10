using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WalletConnect.Web3Modal.Http
{
    public class PulseApiHeaderDecorator : Web3ModalApiHeaderDecorator
    {
        protected override Task<HttpResponseContext> SendAsyncCore(HttpRequestContext requestContext, CancellationToken cancellationToken, Func<HttpRequestContext, CancellationToken, Task<HttpResponseContext>> next)
        {
            requestContext.RequestHeaders["x-sdk-platform"] = Application.isMobilePlatform ? "mobile" : "desktop";
                
            return base.SendAsyncCore(requestContext, cancellationToken, next);
        }
    }
}