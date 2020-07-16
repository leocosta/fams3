using Fams3Adapter.Dynamics.Address;
using Fams3Adapter.Dynamics.Identifier;
using Fams3Adapter.Dynamics.Person;
using Fams3Adapter.Dynamics.ResultTransaction;
using Simple.OData.Client;
using System.Threading;
using System.Threading.Tasks;

namespace Fams3Adapter.Dynamics.SearchResult
{
    public interface IBatchEngine
    {
        Task ExecuteAsync(ODataBatch oDataBatch, CancellationToken cancellationToken);

    }
    
    public  class BatchEngine : IBatchEngine
    {

        public  async Task ExecuteAsync( ODataBatch oDataBatch, CancellationToken cancellationToken)
        {
            await oDataBatch.ExecuteAsync(cancellationToken);
        }

   

    }
}
