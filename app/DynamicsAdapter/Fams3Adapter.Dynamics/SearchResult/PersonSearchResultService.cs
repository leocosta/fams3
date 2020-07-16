using Fams3Adapter.Dynamics.Address;
using Fams3Adapter.Dynamics.AssetOwner;
using Fams3Adapter.Dynamics.BankInfo;
using Fams3Adapter.Dynamics.Duplicate;
using Fams3Adapter.Dynamics.Employment;
using Fams3Adapter.Dynamics.Identifier;
using Fams3Adapter.Dynamics.InsuranceClaim;
using Fams3Adapter.Dynamics.Name;
using Fams3Adapter.Dynamics.OtherAsset;
using Fams3Adapter.Dynamics.Person;
using Fams3Adapter.Dynamics.PhoneNumber;
using Fams3Adapter.Dynamics.RelatedPerson;
using Fams3Adapter.Dynamics.ResultTransaction;
using Fams3Adapter.Dynamics.SearchApiRequest;
using Fams3Adapter.Dynamics.Vehicle;
using Microsoft.Extensions.Logging;
using Simple.OData.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fams3Adapter.Dynamics.SearchResult
{
    public interface IPersonSearchResultService
    {
        void SaveIdentifier(IdentifierEntity[] identifiers, CancellationToken cancellationToken);
        void SaveAddress(AddressEntity[] address, CancellationToken cancellationToken);
        Task<SSG_PhoneNumber> CreatePhoneNumber(PhoneNumberEntity phoneNumber, CancellationToken cancellationToken);
        Task<SSG_Aliase> CreateName(AliasEntity name, CancellationToken cancellationToken);
        Task<SSG_Identity> CreateRelatedPerson(RelatedPersonEntity name, CancellationToken cancellationToken);
        Task SavePerson(PersonEntity person, SSG_SearchApiRequest searchApiRequest, int? dataProviderId, SSG_Identifier sourceIdentifer, CancellationToken cancellationToken);
        Task<SSG_Employment> CreateEmployment(EmploymentEntity employment, CancellationToken cancellationToken);
        Task<SSG_EmploymentContact> CreateEmploymentContact(SSG_EmploymentContact employmentContact, CancellationToken cancellationToken);
        Task<SSG_Asset_BankingInformation> CreateBankInfo(BankingInformationEntity bankInfo, CancellationToken cancellationToken);
        Task<SSG_Asset_Vehicle> CreateVehicle(VehicleEntity vehicle, CancellationToken cancellationToken);
        Task<SSG_AssetOwner> CreateAssetOwner(AssetOwnerEntity owner, CancellationToken cancellationToken);
        Task<SSG_Asset_Other> CreateOtherAsset(AssetOtherEntity asset, CancellationToken cancellationToken);
        Task<SSG_Asset_WorkSafeBcClaim> CreateCompensationClaim(CompensationClaimEntity claim, CancellationToken cancellationToken);
        Task<SSG_Asset_ICBCClaim> CreateInsuranceClaim(ICBCClaimEntity claim, CancellationToken cancellationToken);
        Task<SSG_SimplePhoneNumber> CreateSimplePhoneNumber(SSG_SimplePhoneNumber phone, CancellationToken cancellationToken);
        Task<SSG_InvolvedParty> CreateInvolvedParty(SSG_InvolvedParty involvedParty, CancellationToken cancellationToken);
        Task<SSG_SearchRequestResultTransaction> CreateTransaction(SSG_SearchRequestResultTransaction transaction, CancellationToken cancellationToken);

    }
    public    class PersonSearchResultService : IPersonSearchResultService
    {

        
        private readonly IDuplicateDetectionService _duplicateDetectService;
        private readonly ILogger<PersonSearchResultService> _logger;
        private  ODataBatch _oDataBatch;
        private readonly IODataClient _oDataClient;
        private readonly IBatchEngine _batchEngine;
        private PersonEntity _personToSave;
        private SSG_Person _savedPerson;
        SSG_Identifier _sourceIdentifer;
        SSG_SearchApiRequest _searchApiRequest;
        int? _dataProvider;

        public PersonSearchResultService(IODataClient oDataClient,ODataBatch oDataBatch, IBatchEngine batch, IDuplicateDetectionService duplicateDetectService, ILogger<PersonSearchResultService> logger)
        {
          
            _duplicateDetectService = duplicateDetectService;
            _batchEngine = batch;
            _oDataBatch = oDataBatch;
            _oDataClient = oDataClient;
            _logger = logger;
        }

        public void SaveAddress(AddressEntity[] addresses, CancellationToken cancellationToken)
        {
            foreach (var address in addresses)
            {
                address.PersonEntity = _personToSave;
                _oDataBatch += c => c
                .For<SSG_Address>()
                .Set(address)
                .InsertEntryAsync(false);

                if (_sourceIdentifer != null)
                {
                    AddTransaction(new SSG_SearchRequestResultTransaction()
                    {
                        NewAddress = address
                    }, cancellationToken);
                }
            }
        }   

        public Task<SSG_AssetOwner> CreateAssetOwner(AssetOwnerEntity owner, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_Asset_BankingInformation> CreateBankInfo(BankingInformationEntity bankInfo, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_Asset_WorkSafeBcClaim> CreateCompensationClaim(CompensationClaimEntity claim, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_Employment> CreateEmployment(EmploymentEntity employment, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_EmploymentContact> CreateEmploymentContact(SSG_EmploymentContact employmentContact, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public void SaveIdentifier(IdentifierEntity[] identifiers, CancellationToken cancellationToken)
        {
            foreach (var identifier in identifiers)
            {
                identifier.PersonEntity = _personToSave;
                _oDataBatch += c => c
                .For<SSG_Identifier>()
                .Set(identifier)
                .InsertEntryAsync(false);

                if (_sourceIdentifer != null)
                {
                   
                    AddTransaction(new SSG_SearchRequestResultTransaction()
                    {
                        NewResultIdentifier = identifier
                    }, cancellationToken);
                }
            }
        }

        private void AddTransaction( SSG_SearchRequestResultTransaction transaction, CancellationToken cancellationToken)
        {

            transaction.SourceIdentifier = _sourceIdentifer;
            transaction.SearchApiRequest = _searchApiRequest;
            transaction.InformationSource = _dataProvider;

            _oDataBatch += c => c
              .For<SSG_SearchRequestResultTransaction>()
              .Set(transaction).InsertEntryAsync(false, cancellationToken);
        }

        public Task<SSG_Asset_ICBCClaim> CreateInsuranceClaim(ICBCClaimEntity claim, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_InvolvedParty> CreateInvolvedParty(SSG_InvolvedParty involvedParty, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_Aliase> CreateName(AliasEntity name, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_Asset_Other> CreateOtherAsset(AssetOtherEntity asset, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_PhoneNumber> CreatePhoneNumber(PhoneNumberEntity phoneNumber, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_Identity> CreateRelatedPerson(RelatedPersonEntity name, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_SimplePhoneNumber> CreateSimplePhoneNumber(SSG_SimplePhoneNumber phone, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_SearchRequestResultTransaction> CreateTransaction(SSG_SearchRequestResultTransaction transaction, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SSG_Asset_Vehicle> CreateVehicle(VehicleEntity vehicle, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public async Task SavePerson(PersonEntity person, SSG_SearchApiRequest searchApiRequest,  int? dataProviderId, SSG_Identifier sourceIdentifer, CancellationToken cancellationToken)
        {
            
            _searchApiRequest = searchApiRequest;
            _dataProvider = dataProviderId;
            _sourceIdentifer = sourceIdentifer;
            _logger.LogDebug($"Attempting to add found person to batch  records for SearchRequest[{_searchApiRequest.SearchRequestId}]");

            _logger.LogDebug($"Checking if found person exists - Person -  {person.FirstName}");

            person.DuplicateDetectHash = await _duplicateDetectService.GetDuplicateDetectHashData(person);
            string hashData = person.DuplicateDetectHash;
            var p = await this._oDataClient.For<SSG_Person>()
                    .Filter(x => x.DuplicateDetectHash == hashData)
                    .FindEntryAsync(cancellationToken);

            if (p == null)
            {
                _oDataBatch += c => c
                .For<SSG_Person>()
                .Set(person)
                .InsertEntryAsync(false);


                if (_sourceIdentifer != null)
                {
                    AddTransaction(new SSG_SearchRequestResultTransaction()
                    {
                        NewPerson = person
                    }, cancellationToken);
                }
                _personToSave = person;
            }
            else
            {
                var duplicatedPerson = await _oDataClient.For<SSG_Person>()
                        .Key(p.PersonId)
                        .Expand(x => x.SSG_Addresses)
                        .Expand(x => x.SSG_Identifiers)
                        .Expand(x => x.SSG_Aliases)
                        .Expand(x => x.SSG_Asset_BankingInformations)
                        .Expand(x => x.SSG_Asset_ICBCClaims)
                        .Expand(x => x.SSG_Asset_Others)
                        .Expand(x => x.SSG_Asset_Vehicles)
                        .Expand(x => x.SSG_Asset_WorkSafeBcClaims)
                        .Expand(x => x.SSG_Employments)
                        .Expand(x => x.SSG_Identities)
                        .Expand(x => x.SSG_PhoneNumbers)
                        .Expand(x => x.SearchRequest)
                        .FindEntryAsync(cancellationToken);
                duplicatedPerson.IsDuplicated = true;
                _savedPerson = duplicatedPerson;
            }

        }
    }
}
