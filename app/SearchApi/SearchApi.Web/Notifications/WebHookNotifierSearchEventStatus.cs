using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SearchApi.Web.Configuration;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Fams3.SearchApi.Contracts.PersonSearch;
using SearchApi.Web.DeepSearch;
using BcGov.Fams3.SearchApi.Contracts.Person;

namespace SearchApi.Web.Notifications
{

    public class WebHookNotifierSearchEventStatus :  ISearchApiNotifier<PersonSearchAdapterEvent>
    {

        private readonly HttpClient _httpClient;
        private readonly SearchApiOptions _searchApiOptions;
        private readonly IDeepSearchService _deepSearchService;
        private readonly ILogger<WebHookNotifierSearchEventStatus> _logger;
     

        public WebHookNotifierSearchEventStatus(HttpClient httpClient, IOptions<SearchApiOptions> searchApiOptions,
            ILogger<WebHookNotifierSearchEventStatus> logger, IDeepSearchService deepSearchService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _searchApiOptions = searchApiOptions.Value;
     
            _deepSearchService = deepSearchService;
        }

        public async Task NotifyEventAsync(string searchRequestKey, PersonSearchAdapterEvent eventStatus, string eventName,
           CancellationToken cancellationToken)
        {
            var webHookName = "PersonSearch";
            foreach (var webHook in _searchApiOptions.WebHooks)
            {

                if (!URLHelper.TryCreateUri(webHook.Uri, eventName, $"{searchRequestKey}", out var endpoint))
                {
                    _logger.LogWarning(
                        $"The webHook {webHookName} notification uri is not established or is not an absolute Uri for {webHook.Name}. Set the WebHook.Uri value on SearchApi.WebHooks settings.");
                    return;
                }

                using var request = new HttpRequestMessage();

                try
                {
                    StringContent content;
                    if (eventName == EventName.Finalized)
                    {
                        _logger.LogInformation($"Initiating {eventName} event for {eventStatus.SearchRequestKey} for {eventStatus.ProviderProfile.Name}");
                        PersonSearchEvent finalizedSearch = new PersonSearchFinalizedEvent()
                        {
                            SearchRequestKey = eventStatus.SearchRequestKey,
                            Message = "Search Request Finalized",
                            SearchRequestId = eventStatus.SearchRequestId,
                            TimeStamp = DateTime.Now
                        };
                        _logger.LogInformation(JsonConvert.SerializeObject(finalizedSearch));
                        content = new StringContent(JsonConvert.SerializeObject(finalizedSearch));
                    }
                    else
                    {
                        _logger.LogInformation($"Send event to Dynadaptor, event = {JsonConvert.SerializeObject(eventStatus)}.");
                        content = new StringContent(JsonConvert.SerializeObject(eventStatus));
                    }

                    content.Headers.ContentType =
                        System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                    request.Content = content;
                    request.Method = HttpMethod.Post;
                    request.Headers.Accept.Add(
                        System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
                    request.RequestUri = endpoint;
                    var response = await _httpClient.SendAsync(request, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError(
                            $"The webHook {webHookName} notification has not executed status {eventName} successfully for {webHook.Name} webHook. The error code is {response.StatusCode.GetHashCode()}.");
                        return;
                    }
                    _logger.LogInformation(
                        $"The webHook {webHookName} notification has notified {eventName} successfully.");
                }

                catch (Exception exception)
                {
                    _logger.LogError($"The webHook {webHookName} notification failed for {eventName}. [{exception.Message}]");
                }
            }
            if (EventName.Finalized.Equals(eventName))
                await _deepSearchService.DeleteFromCache(searchRequestKey);
            else
                await ProcessEvents(searchRequestKey, eventStatus, eventName);

        }

        private async Task ProcessEvents(string searchRequestKey, PersonSearchAdapterEvent eventStatus, string eventName)
        {
            await _deepSearchService.UpdateDataPartner(searchRequestKey, eventStatus.ProviderProfile.Name, eventName);

            if (EventName.Completed.Equals(eventName) && eventStatus.ProviderProfile.SearchSpeedType == SearchSpeedType.Fast)
                await _deepSearchService.UpdateParameters(eventName, (PersonSearchCompleted)eventStatus, searchRequestKey);

            await ProcessWaveSearch(searchRequestKey, eventName, eventStatus.ProviderProfile.Name);

           
               
        }

        private async Task ProcessWaveSearch(string searchRequestKey, string eventName,string dataPartner )
        {
            if (!EventName.Finalized.Equals(eventName))
            {



                if (EventName.Completed.Equals(eventName))
                {
                    if (await _deepSearchService.IsWaveSearchReadyToFinalize(searchRequestKey))
                    {
                        PersonSearchAdapterEvent finalizedSearch = new PersonSearchFinalizedEvent()
                        {
                            SearchRequestKey = searchRequestKey,
                            Message = "Search Request Finalized",
                            SearchRequestId = Guid.NewGuid(),
                            TimeStamp = DateTime.Now,
                            ProviderProfile = new ProviderProfileDetails { Name = dataPartner }
                        };
                        await NotifyEventAsync(searchRequestKey, (PersonSearchFinalized)finalizedSearch, EventName.Finalized, new CancellationToken());



                    }
                }
            }
        }

    }
}
