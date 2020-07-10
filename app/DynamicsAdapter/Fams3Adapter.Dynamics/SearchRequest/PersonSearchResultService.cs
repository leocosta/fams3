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
using Fams3Adapter.Dynamics.Vehicle;
using Simple.OData.Client;
using System.Threading;
using System.Threading.Tasks;

namespace Fams3Adapter.Dynamics.SearchRequest
{
    public interface IPersonSearchResultService
    {
        void CreateIdentifier(IdentifierEntity[] identifiers, CancellationToken cancellationToken);
        Task<SSG_Address> CreateAddress(AddressEntity address, CancellationToken cancellationToken);
        Task<SSG_PhoneNumber> CreatePhoneNumber(PhoneNumberEntity phoneNumber, CancellationToken cancellationToken);
        Task<SSG_Aliase> CreateName(AliasEntity name, CancellationToken cancellationToken);
        Task<SSG_Identity> CreateRelatedPerson(RelatedPersonEntity name, CancellationToken cancellationToken);
        Task SavePerson(PersonEntity person, CancellationToken cancellationToken);
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
        private readonly IODataClient _oDataClient;

        private readonly IDuplicateDetectionService _duplicateDetectService;
        private ODataBatch oDataBatch;
        private PersonEntity _personToSave;
        private SSG_Person _savedPerson;

        public PersonSearchResultService(IODataClient oDataClient, IDuplicateDetectionService duplicateDetectService)
        {
            this._oDataClient = oDataClient;
            this._duplicateDetectService = duplicateDetectService;
            oDataBatch = new ODataBatch(_oDataClient);
        }

        public Task<SSG_Address> CreateAddress(AddressEntity address, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
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

        public void CreateIdentifier(IdentifierEntity[] identifiers, CancellationToken cancellationToken)
        {
            foreach (var identifier in identifiers)
            {
                identifier.PersonEntity = _personToSave;
                oDataBatch += c => c
                .For<SSG_Identifier>()
                .Set(identifier)
                .InsertEntryAsync(false);
            }

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

        public async Task SavePerson(PersonEntity person, CancellationToken cancellationToken)
        {
            person.DuplicateDetectHash = await _duplicateDetectService.GetDuplicateDetectHashData(person);
            string hashData = person.DuplicateDetectHash;
            var p = await this._oDataClient.For<SSG_Person>()
                    .Filter(x => x.DuplicateDetectHash == hashData)
                    .FindEntryAsync(cancellationToken);

            if (p == null)
            {
                oDataBatch += c => c
                .For<SSG_Person>()
                .Set(person)
                .InsertEntryAsync(cancellationToken);

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
