
using System;
using BcGov.Fams3.SearchApi.Contracts.Rfi;

namespace SearchApi.Web.Notifications
{
	public class RfiFinalizedEvent : RequestForInformationFailedEvent
	{
		public string Message {get;set;}

		public Guid Id {get;set;}

		public DateTime TimeStamp {get;set;}
	}
}