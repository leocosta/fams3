
using System;
using BcGov.Fams3.SearchApi.Contracts.Rfi;

namespace SearchApi.Web.Notifications
{
	public class RfiFinalizedEvent : RequestForInformationFailedEvent
	{
		public string Message => throw new NotImplementedException();

		public Guid Id => throw new NotImplementedException();

		public DateTime TimeStamp => throw new NotImplementedException();
	}
}