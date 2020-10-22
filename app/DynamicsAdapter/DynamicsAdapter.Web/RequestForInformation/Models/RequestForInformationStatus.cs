using System;

namespace DynamicsAdapter.Web.Rfi.Models
{
    public abstract class RequestForInformationStatus
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
