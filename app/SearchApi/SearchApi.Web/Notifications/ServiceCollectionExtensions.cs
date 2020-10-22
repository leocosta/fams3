using BcGov.Fams3.SearchApi.Contracts.PersonSearch;
using BcGov.Fams3.SearchApi.Contracts.Rfi;
using Microsoft.Extensions.DependencyInjection;

namespace SearchApi.Web.Notifications
{
    public static class ServiceCollectionExtensions
    {
        public static void AddWebHooks(this IServiceCollection services)
        {
            services.AddHttpClient<ISearchApiNotifier<PersonSearchAdapterEvent>, WebHookNotifierSearchEventStatus>();
            services.AddHttpClient<IRfiApiNotifier<RequestForInformationEvent>, WebHookNotifierRfiEventStatus>();
        }
    }
}