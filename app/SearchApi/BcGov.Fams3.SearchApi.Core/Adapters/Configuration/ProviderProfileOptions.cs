﻿using BcGov.Fams3.SearchApi.Contracts.Person;
using BcGov.Fams3.SearchApi.Contracts.PersonSearch;
using System.ComponentModel.DataAnnotations;

namespace BcGov.Fams3.SearchApi.Core.Adapters.Configuration
{
    /// <summary>
    /// Represents a configurable ProviderProfile
    /// </summary>
    public class ProviderProfileOptions : ProviderProfile
    {
        [Required]
        public string Name { get; set; }

        public SearchSpeedType SearchSpeedType { get; set; }

        public TransferMode Mode { get; set; }
    }
}