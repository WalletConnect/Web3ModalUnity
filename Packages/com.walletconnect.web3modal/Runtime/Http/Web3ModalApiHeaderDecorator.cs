using System;
using System.Threading;
using System.Threading.Tasks;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal.Http
{
    public class Web3ModalApiHeaderDecorator : HttpClientDecorator
    {
        protected override Task<HttpResponseContext> SendAsyncCore(HttpRequestContext requestContext, CancellationToken cancellationToken, Func<HttpRequestContext, CancellationToken, Task<HttpResponseContext>> next)
        {
            requestContext.RequestHeaders["x-project-id"] = ProjectConfiguration.Load().Id;
            requestContext.RequestHeaders["x-sdk-type"] = SdkMetadata.Type;
            requestContext.RequestHeaders["x-sdk-version"] = SdkMetadata.Version;

            return next(requestContext, cancellationToken);
        }
    }
}