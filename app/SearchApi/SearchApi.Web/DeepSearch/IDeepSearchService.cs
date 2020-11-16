using BcGov.Fams3.Redis;
using BcGov.Fams3.SearchApi.Contracts.Person;
using BcGov.Fams3.SearchApi.Contracts.PersonSearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SearchApi.Web.Configuration;
using SearchApi.Web.Controllers;
using SearchApi.Web.DeepSearch.Schema;
using SearchApi.Web.Messaging;
using SearchApi.Web.Notifications;
using SearchApi.Web.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SearchApi.Web.DeepSearch
{
    public interface IDeepSearchService
    {
   

        Task UpdateDataPartner(string searchRequestKey, string dataPartner, string eventName);
        
        Task UpdateParameters(string eventName, PersonSearchCompleted eventStatus, string searchRequestKey);
        Task  DeleteFromCache(string searchRequestKey);

        Task<bool> IsWaveSearchReadyToFinalize(string searchRequestKey);

    }

    public class DeepSearchService : IDeepSearchService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<DeepSearchService> _logger;
        private readonly IDeepSearchDispatcher _deepSearchDispatcher;
        private readonly DeepSearchOptions _deepSearchOptions;
      
        public DeepSearchService(ICacheService cacheService, ILogger<DeepSearchService> logger, IOptions<DeepSearchOptions> deepSearchOptions, IDeepSearchDispatcher deepSearchDispatcher)
        {
            _cacheService = cacheService;
            _logger = logger;
            _deepSearchOptions = deepSearchOptions.Value;
     
             _deepSearchDispatcher = deepSearchDispatcher;
        }






        private async Task<bool> CurrentWaveIsCompleted(string searchRequestKey)
        {
            try
            {
                return JsonConvert.SerializeObject(await _cacheService.GetRequest(searchRequestKey)).AllFastSearchPartnerCompleted();

            }
            catch (Exception exception)
            {
                _logger.LogError($"Check Data Partner Status Failed. [] for {searchRequestKey}. [{exception.Message}]");
                return false;
            }
        }
        private async Task<bool> AllSearchDataPartnerIsCompleted(string searchRequestKey)
        {
            try
            {
                return JsonConvert.SerializeObject(await _cacheService.GetRequest(searchRequestKey)).AllPartnerCompleted();

            }
            catch (Exception exception)
            {
                _logger.LogError($"Check Data Partner Status Failed. [] for {searchRequestKey}. [{exception.Message}]");
                return false;
            }
        }
        public async Task UpdateDataPartner(string searchRequestKey, string dataPartner, string eventName)
        {
            try
            {
                
                if (eventName.Equals(EventName.Completed) || eventName.Equals(EventName.Rejected))
                {
                    _logger.LogInformation($"Updating data partner as completed for {dataPartner} for {eventName} event.");
                    var searchRequest = JsonConvert.SerializeObject(await _cacheService.GetRequest(searchRequestKey)).UpdateDataPartner(dataPartner);
                    await _cacheService.SaveRequest(searchRequest);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Update Data Partner as completed Failed. [{eventName}] for {searchRequestKey}. [{exception.Message}]");

            }
        }

       

        public  async Task DeleteFromCache(string searchRequestKey)
        {
            try
            {
                if (JsonConvert.SerializeObject(await _cacheService.GetRequest(searchRequestKey)).AllPartnerCompleted())
                await _cacheService.DeleteRequest(searchRequestKey);
                IEnumerable<string> keys = await SearchDeepSearchKeys(searchRequestKey);
                foreach (var key in keys)
                    await _cacheService.Delete(key);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Delete search request failed. for {searchRequestKey}. [{exception.Message}]");
            }
        }

        public async Task UpdateParameters(string eventName, PersonSearchCompleted eventStatus, string searchRequestKey)
        {
            IEnumerable<WaveSearchData> waveSearches = await GetWaveDataForSearch(searchRequestKey);
            if (eventName.Equals(EventName.Completed))
            {
                List<PersonalIdentifier> existingIds = new List<PersonalIdentifier>();

                foreach (var waveitem in waveSearches)
                {
                    _logger.LogInformation($"Store all existing params {waveitem.DataPartner}");
                   

                    foreach (var person in waveitem.AllParameter)
                    {
                        foreach (var identifier in person.Identifiers)
                        {
                            if (!existingIds.Any(i => i.Type == identifier.Type && i.Value == identifier.Value))
                                existingIds.Add(identifier);
                        }
                    }


                }

                var matchedPersons = eventStatus.MatchedPersons;
                List<PersonalIdentifier> foundId = new List<PersonalIdentifier>();
                _logger.LogInformation($"Store all newly found  params from {eventStatus.ProviderProfile.Name}");

                foreach (var person in matchedPersons)
                {
                    foreach (var identifier in person.Identifiers)
                    {
                        if (!foundId.Any(i => i.Type == identifier.Type && i.Value == identifier.Value))
                            foundId.Add(identifier);
                    }
                }

                foreach (var waveitem in waveSearches)
                {
                    PersonalIdentifierType[] paramsRegistry = ListDataPartnerRegistry(waveitem.DataPartner);

                    ExtractIds(eventStatus, existingIds, waveitem.DataPartner, foundId, paramsRegistry, waveSearches, out IEnumerable<PersonalIdentifier> filteredExistingIdentifierForDataPartner, out IEnumerable<PersonalIdentifier> filteredNewFoundIdentifierForDataPartner);

                    var newToBeUsedId = filteredExistingIdentifierForDataPartner.DetailedCompare(filteredNewFoundIdentifierForDataPartner);

                    _logger.LogInformation($"{newToBeUsedId.Count()} ids to be stored as new parameter for {waveitem.DataPartner}");

                    if (newToBeUsedId.Count() != 0)
                    {
                        waveitem.AllParameter.Add(new Person { Identifiers = newToBeUsedId });
                        if (waveitem.NewParameter == null || waveitem.NewParameter?.Count == 0)
                            waveitem.NewParameter = new List<Person> { new Person { Identifiers = newToBeUsedId } };

                        else
                            waveitem.NewParameter[0] = new Person { Identifiers = newToBeUsedId };
                    }
                    
                    await _cacheService.Save(searchRequestKey.DeepSearchKey(waveitem.DataPartner), waveitem);

                }

            }

        }

        private PersonalIdentifierType[] ListDataPartnerRegistry(string  dataPartner)
        {
            PersonalIdentifierType[] paramsRegistry = new PersonalIdentifierType[] { };

            try
            {
                paramsRegistry = Registry.DataPartnerParameters[dataPartner];

            }
            catch (Exception ex)
            {
                _logger.LogError($"{dataPartner} registry not found. Error: {ex.Message}");
            }

            return paramsRegistry;
        }

        private void ExtractIds(PersonSearchCompleted eventStatus, List<PersonalIdentifier> existingIds,string DataPartner, List<PersonalIdentifier>  foundId, PersonalIdentifierType[] paramsRegistry, IEnumerable<WaveSearchData> waveSearches, out IEnumerable<PersonalIdentifier> filteredExistingIdentifierForDataPartner, out IEnumerable<PersonalIdentifier> filteredNewFoundIdentifierForDataPartner)
        {
            List<PersonalIdentifier> existingIds = new List<PersonalIdentifier>();

            foreach (var waveitem in waveSearches)
            {
                _logger.LogInformation($"Store all parameters {waveitem.DataPartner}");
                waveitem.AllParameter.ForEach(p => existingIds.AddRange(p.Identifiers));
            }

            _logger.LogInformation($"Existing {existingIds.Count()} Identifier exists existingIds={JsonConvert.SerializeObject(existingIds)}");
            filteredExistingIdentifierForDataPartner = existingIds.Where(identifer => paramsRegistry.Contains(identifer.Type));
            _logger.LogInformation($"{existingIds.Count()} Identifier matched the require types for {eventStatus.ProviderProfile.Name}");


            _logger.LogInformation($"New found {foundId.Count()} Identifiers was returned by {eventStatus.ProviderProfile.Name}");
            filteredNewFoundIdentifierForDataPartner = foundId.Where(identifer => paramsRegistry.Contains(identifer.Type));
            _logger.LogInformation($"{filteredNewFoundIdentifierForDataPartner.Count()} returned Identifier matched the required types for {DataPartner}");
        }




        private async Task<IEnumerable<WaveSearchData>> GetWaveDataForSearch(string searchRequestKey)
        {
            List<WaveSearchData> waveMetaDatas = new List<WaveSearchData>();
            IEnumerable<string> keys = await SearchDeepSearchKeys(searchRequestKey);

            foreach (var key in keys)
            {
                waveMetaDatas.Add(JsonConvert.DeserializeObject<WaveSearchData>(await _cacheService.Get(key)));
            }

            return waveMetaDatas.AsEnumerable();
        }

        private async Task<IEnumerable<string>> SearchDeepSearchKeys(string searchRequestKey)
        {
            return await _cacheService.SearchKeys($"deepsearch-{searchRequestKey}*");
        }

        public async Task<bool> IsWaveSearchReadyToFinalize(string searchRequestKey)
        {
           
            var waveData = await GetWaveDataForSearch(searchRequestKey);
            if (waveData.Any())
            {
                if (!await CurrentWaveIsCompleted(searchRequestKey))
                    return false;

                if (waveData.All(x => x.CurrentWave == _deepSearchOptions.MaxWaveCount) || NoNewParameter(waveData))
                {
                    _logger.Log(LogLevel.Information, $"all reach Max wave or no new parameters.");
                    return true;
                }
                else
                {
                    foreach (var wave in waveData)
                    {
                        if (wave.NewParameter != null)
                        {
                            foreach (var person in wave.NewParameter)
                            {
                                await _deepSearchDispatcher.StartAnotherWave(searchRequestKey, wave, person, wave.NumberOfRetries, wave.TimeBetweenRetries);
                            }
                        }
                        else
                        {
                            string cacheKey = searchRequestKey.DeepSearchKey(wave.DataPartner);
                            var waveMetaData = await _cacheService.Get(cacheKey);
                            if (waveMetaData != null)
                            {
                                _logger.Log(LogLevel.Information, $"{cacheKey} has an active wave but no new parameter");
                                WaveSearchData metaData = JsonConvert.DeserializeObject<WaveSearchData>(waveMetaData);
                                _logger.Log(LogLevel.Information, $"{cacheKey} Current Metadata Wave : {metaData.CurrentWave}");
                                metaData.CurrentWave++;
                                metaData.NewParameter = null;
                                await _cacheService.Save(cacheKey, metaData);
                                _logger.Log(LogLevel.Information, $"{cacheKey} New wave {metaData.CurrentWave} saved");
                            }
                        }
                    }
                    return false;

                }
            }
            else
            {
                return await AllSearchDataPartnerIsCompleted(searchRequestKey);
            }
        }

        private static bool NoNewParameter(IEnumerable<WaveSearchData> waveData)
        {
            return  waveData.Count(w => w.NewParameter == null) == waveData.Count();
        }

    }
}
