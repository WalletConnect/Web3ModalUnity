using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Networking;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Networking;

namespace WalletConnect.Web3Modal
{
    public class ApiController
    {
        public readonly UnityWebRequestWalletsFactory walletsRequestsFactory;

        public ApiController()
        {
            walletsRequestsFactory = new UnityWebRequestWalletsFactory(
                includedWalletIds: Web3Modal.Config.includedWalletIds,
                excludedWalletIds: Web3Modal.Config.excludedWalletIds
            );
        }

        public async ValueTask<GetWalletsResponse> GetWallets(int page, int count, string search = null)
        {
            if (page < 1)
                throw new System.ArgumentOutOfRangeException(nameof(page), "Page must be greater than 0");

            if (count < 1)
                throw new System.ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");

            return await GetWalletsCore(page, count);
        }

        protected virtual async Task<GetWalletsResponse> GetWalletsCore(int page, int count, string search = null)
        {
            using var uwr = walletsRequestsFactory.GetWallets(page, count, search);

            // TODO: use Awaitable in Unity 2023.1+
            var tcs = new TaskCompletionSource<GetWalletsResponse>();
            UnityEventsDispatcher.Instance.StartCoroutine(SendWebRequest(uwr, tcs));
            return await tcs.Task;
        }

        protected static IEnumerator SendWebRequest<T>(
            UnityWebRequest uwr,
            TaskCompletionSource<T> tcs)
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                // TODO: use custom ex type
                tcs.SetException(new System.Exception($"Failed to send web request: {uwr.error}"));
                yield break;
            }

            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(uwr.downloadHandler.text);
            tcs.SetResult(response);
        }
    }
}