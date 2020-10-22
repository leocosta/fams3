using System;

namespace BcGov.Fams3.SearchApi.Contracts.Rfi
{
    public interface RequestForInformationEvent
    {
        /// <summary>
        /// This is GUID of SearchApiRequestId
        /// </summary>
        Guid Id { get; }
        DateTime TimeStamp { get; }
    }

	public interface RequestForInformationSucceededEvent : RequestForInformationEvent{

	}
	
	public interface RequestForInformationFailedEvent : RequestForInformationEvent{
		string Message {get;}
	}
}