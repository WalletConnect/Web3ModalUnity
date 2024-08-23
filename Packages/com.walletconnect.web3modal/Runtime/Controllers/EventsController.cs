using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using WalletConnect.Web3Modal.Http;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal
{
    public class EventsController
    {
        private const string BasePath = "https://pulse.walletconnect.org/";
        private const int TimoutSeconds = 5;

        private readonly UnityHttpClient _httpClient = new(new Uri(BasePath), TimeSpan.FromSeconds(TimoutSeconds), new Web3ModalApiHeaderDecorator());

        // private Queue<Event> _eventsQueue = new();
        private AnalyticsState _state = AnalyticsState.Loading;

        public async Task InitializeAsync(Web3ModalConfig config, ApiController apiController)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _state = AnalyticsState.Disabled;
            return;
#endif

            if (!Web3Modal.Config.enableAnalytics)
            {
                _state = AnalyticsState.Disabled;
                return;
            }

            await LoadRemoteAnalyticsConfig(apiController);
        }


        private async Task LoadRemoteAnalyticsConfig(ApiController apiController)
        {
            try
            {
                var response = await apiController.GetAnalyticsConfigAsync();

                _state = response.isAnalyticsEnabled
                    ? AnalyticsState.Enabled
                    : AnalyticsState.Disabled;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _state = AnalyticsState.Disabled;
            }
        }

        public async void SendEvent(Event @event)
        {
            try
            {
                if (_state == AnalyticsState.Disabled)
                    return;

                var request = new EventRequest
                {
                    eventId = Guid.NewGuid().ToString(),
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    bundleId = Application.identifier,
                    props = @event
                };

                var requestJson = JsonConvert.SerializeObject(request);

                Debug.Log($"[EventsController] Sending event: {@event.name}.\n\nRequest payload:\n {requestJson}");

                await _httpClient.PostAsync("e", requestJson);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private enum AnalyticsState
        {
            Loading,
            Enabled,
            Disabled
        }
    }

    [Serializable]
    internal struct EventRequest
    {
        public string eventId;
        public long timestamp;
        public string bundleId;
        public Event props;
    }

    [Serializable]
    public struct Event
    {
        [JsonProperty("type")]
        public const string type = "track";

        [JsonProperty("event")]
        public string name;

        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> properties;
    }
}