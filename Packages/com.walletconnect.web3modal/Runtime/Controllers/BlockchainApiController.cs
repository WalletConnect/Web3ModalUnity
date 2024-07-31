using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WalletConnect.Web3Modal.Http;
using WalletConnect.Web3Modal.Model.BlockchainApi;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal
{
    public class BlockchainApiController
    {
        private const string BasePath = "https://rpc.walletconnect.org/v1/";
        private const int TimoutSeconds = 5;

        private string _clientIdQueryParam;
        
        private readonly UnityHttpClient _httpClient = new(new Uri(BasePath), TimeSpan.FromSeconds(TimoutSeconds));
        
        private readonly IDictionary<string, string> _getBalanceHeaders = new Dictionary<string, string>
        {
            {"x-sdk-version", SdkMetadata.Version}
        };

        public async Task<GetIdentityResponse> GetIdentityAsync(string address)
        {
            var projectId = ProjectConfiguration.Load().Id;
            var path = $"identity/{address}?projectId={projectId}";

            if (string.IsNullOrWhiteSpace(_clientIdQueryParam))
            {
                var wc = WalletConnectConnector.WalletConnectInstance;
                if (wc is { IsInitialized: true })
                {
                    var rawClientId = await wc.SignClient.Core.Crypto.GetClientId();
                    _clientIdQueryParam = $"&clientId={Uri.EscapeDataString(rawClientId)}";
                }
            }

            if (!string.IsNullOrWhiteSpace(_clientIdQueryParam))
                path += _clientIdQueryParam;

            return await _httpClient.GetAsync<GetIdentityResponse>(path);
        }

        public async Task<GetBalanceResponse> GetBalanceAsync(string address)
        {
            var projectId = ProjectConfiguration.Load().Id;
            return await _httpClient.GetAsync<GetBalanceResponse>($"account/{address}/balance?projectId={projectId}&currency=usd", headers: _getBalanceHeaders);
        }
    }
}