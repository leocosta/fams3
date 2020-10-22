using System.Threading;
using System.Threading.Tasks;
using BcGov.Fams3.SearchApi.Contracts.PersonSearch;
using BcGov.Fams3.SearchApi.Contracts.Rfi;
using MassTransit;
using Microsoft.Extensions.Logging;
using SearchApi.Web.Notifications;
using Serilog.Context;

namespace SearchApi.Web.Search
{
    public abstract class RfiEventConsumer :IConsumer<RequestForInformationEvent>
    {

        private readonly ILogger<RfiEventConsumer> _logger;


        private readonly IRfiApiNotifier<RequestForInformationEvent> _rfiNotifier;

        public RfiEventConsumer(IRfiApiNotifier<RequestForInformationEvent> rfiNotifier, ILogger<RfiEventConsumer> logger)
        {
            _rfiNotifier = rfiNotifier;
            _logger = logger;

        }

		public async Task Consume(ConsumeContext<RequestForInformationEvent> context)
		{
			var cts = new CancellationTokenSource();
			_logger.LogInformation($"received new {nameof(RequestForInformationEvent)} event");
			await _rfiNotifier.NotifyEventAsync(context.Message.Id.ToString(), context.Message, "test", cts.Token);
		}
	}
}