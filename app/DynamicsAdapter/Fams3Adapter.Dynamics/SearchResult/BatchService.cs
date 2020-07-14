using Fams3Adapter.Dynamics.Address;
using Fams3Adapter.Dynamics.Identifier;
using Fams3Adapter.Dynamics.Person;
using Fams3Adapter.Dynamics.ResultTransaction;
using Simple.OData.Client;
using System.Threading;
using System.Threading.Tasks;

namespace Fams3Adapter.Dynamics.SearchResult
{
    public interface IBatchService
    {
        void SetPerson(PersonEntity person, SSG_SearchRequestResultTransaction transaction);
        void SetAddress(AddressEntity[] addresses, SSG_SearchRequestResultTransaction transaction);
        void SetIdentifier(IdentifierEntity[] addresses, SSG_SearchRequestResultTransaction transaction);
        Task ExecuteAsync( CancellationToken cancellationToken);
    }
    public class BatchService : IBatchService
    {
        private  ODataBatch _oDataBatch;
        private PersonEntity _personToSave;
        public BatchService(ODataBatch oDataBatch)
        {
            _oDataBatch = oDataBatch;
        }
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _oDataBatch.ExecuteAsync(cancellationToken);
        }

        public void SetAddress(AddressEntity[] addresses, SSG_SearchRequestResultTransaction transaction)
        {
            foreach (var address in addresses)
            {
                address.PersonEntity = _personToSave;
                _oDataBatch += c => c
                .For<SSG_Address>()
                .Set(address)
                .InsertEntryAsync(false);

                if (transaction != null) { transaction.NewAddress = address; AddTransaction(transaction); }
            }
        }

        public void SetIdentifier(IdentifierEntity[] identifiers, SSG_SearchRequestResultTransaction transaction)
        {
            foreach (var identifier in identifiers)
            {
                identifier.PersonEntity = _personToSave;
                _oDataBatch += c => c
                .For<SSG_Identifier>()
                .Set(identifier)
                .InsertEntryAsync(false);

                if (transaction != null) { transaction.NewResultIdentifier = identifier; AddTransaction(transaction); }
            }
        }

        public void SetPerson(PersonEntity person, SSG_SearchRequestResultTransaction transaction)
        {
            _oDataBatch += c => c
                 .For<SSG_Person>()
                 .Set(person)
                 .InsertEntryAsync(false);

            if (transaction != null ) { transaction.NewPerson = person; AddTransaction(transaction); }
            _personToSave = person;
        }

        private void AddTransaction( SSG_SearchRequestResultTransaction transaction)
        {
                _oDataBatch += c => c
               .For<SSG_SearchRequestResultTransaction>()
               .Set(transaction).InsertEntryAsync(false);
            
        }
    }
}
