﻿using System;
using BcGov.Fams3.SearchApi.Contracts.PersonSearch;

namespace SearchApi.Core.Test.Fake
{
    public class FakePersonSearchAccepted : PersonSearchAccepted
    {
      
            public Guid SearchRequestId { get; set; }

            public string SearchRequestKey { get; set; }

            public DateTime TimeStamp { get; set; }

            public ProviderProfile ProviderProfile { get; set; }

        public string Message { get; set; }
    }
}
