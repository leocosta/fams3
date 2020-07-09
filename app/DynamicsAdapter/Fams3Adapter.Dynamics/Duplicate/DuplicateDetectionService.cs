﻿using Fams3Adapter.Dynamics.Address;
using Fams3Adapter.Dynamics.AssetOwner;
using Fams3Adapter.Dynamics.Identifier;
using Fams3Adapter.Dynamics.Name;
using Fams3Adapter.Dynamics.Person;
using Fams3Adapter.Dynamics.PhoneNumber;
using Fams3Adapter.Dynamics.RelatedPerson;
using Fams3Adapter.Dynamics.Vehicle;
using Newtonsoft.Json;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fams3Adapter.Dynamics.Duplicate
{
    public interface IDuplicateDetectionService
    {
        Task<string> GetDuplicateDetectHashData(object entity);
        Task<Guid> Exists(object fatherObj, object entity);
    }

    public class DuplicateDetectionService : IDuplicateDetectionService
    {
        private readonly IODataClient _oDataClient;
        public static IEnumerable<SSG_DuplicateDetectionConfig> _configs;
        public static Dictionary<string, string> EntityNameMap = new Dictionary<string, string>
        {
            {"PersonEntity", "ssg_person" },
            {"AddressEntity", "ssg_address" },
            {"IdentifierEntity", "ssg_identifier" },
            {"PhoneNumberEntity", "ssg_phonenumber" },
            {"AliasEntity", "ssg_alias"},
            {"VehicleEntity", "ssg_asset_vehicle"},
            {"AssetOwnerEntity", "ssg_assetowner"},
            {"RelatedPersonEntity", "SSG_identity" }
        };

        public DuplicateDetectionService(IODataClient oDataClient)
        {
            this._oDataClient = oDataClient;
        }

        /// <summary>
        /// make the entity hash data fields according to configuration.
        /// Example: config is : ssg_person, ssg_firstname|ssg_lastname
        ///     ssg_person person1: firstname="person1", lastname="lastname1"
        ///     it should return SHA512("person1lastname1")
        /// This is mainly used for Person
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>SHA512 </returns>
        public async Task<string> GetDuplicateDetectHashData(object entity)
        {
            if (_configs == null) await GetDuplicateDetectionConfig(CancellationToken.None);

            Type type = entity.GetType();
            string name;
            if (!EntityNameMap.TryGetValue(type.Name, out name))
            {
                return null;
            }

            SSG_DuplicateDetectionConfig config = _configs.FirstOrDefault(m => m.EntityName == name);
            if (config == null) return null;

            IList<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties());       

            return hashstring(GetConcateFieldsStr(config.DuplicateFieldList, props, entity));
        }

        /// <summary>
        /// check if existing fatherObj contains the same entity. The standard for "same" is the config from dynamics.
        /// if there is duplicated entity in this fatherObj, then return its guid.
        /// if no duplicate found, return guid.empty.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<Guid> Exists(object fatherObj, object entity)
        {
            if (_configs == null) await GetDuplicateDetectionConfig(CancellationToken.None);

            Type type = entity.GetType();
            string name;
            if (!EntityNameMap.TryGetValue(type.Name, out name))
            {
                return Guid.Empty;
            }

            SSG_DuplicateDetectionConfig config = _configs.FirstOrDefault(m => m.EntityName.ToLower() == name.ToLower());
            if (config == null) return Guid.Empty;

            IList<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties());
            string entityStr = GetConcateFieldsStr(config.DuplicateFieldList, props, entity);

            switch (type.Name)
            {
                case "IdentifierEntity":
                    foreach (SSG_Identifier identifier in ((SSG_Person)fatherObj).SSG_Identifiers)
                    {
                        if (GetConcateFieldsStr(config.DuplicateFieldList, props, identifier) == entityStr) return identifier.IdentifierId;
                    };
                    break;
                case "PhoneNumberEntity":
                    foreach (SSG_PhoneNumber phone in ((SSG_Person)fatherObj).SSG_PhoneNumbers)
                    {
                        if (GetConcateFieldsStr(config.DuplicateFieldList, props, phone) == entityStr) return phone.PhoneNumberId;
                    };
                    break;
                case "AddressEntity":
                    foreach (SSG_Address addr in ((SSG_Person)fatherObj).SSG_Addresses)
                    {
                        if (GetConcateFieldsStr(config.DuplicateFieldList, props, addr) == entityStr) return addr.AddressId;
                    };
                    break;
                case "AliasEntity":
                    foreach (SSG_Aliase alias in ((SSG_Person)fatherObj).SSG_Aliases)
                    {
                        if (GetConcateFieldsStr(config.DuplicateFieldList, props, alias) == entityStr) return alias.AliasId;
                    };
                    break;
                case "VehicleEntity":
                    foreach (SSG_Asset_Vehicle v in ((SSG_Person)fatherObj).SSG_Asset_Vehicles)
                    {
                        if (GetConcateFieldsStr(config.DuplicateFieldList, props, v) == entityStr) return v.VehicleId;
                    };
                    break;
                case "AssetOwnerEntity":
                    foreach (SSG_AssetOwner owner in ((SSG_Asset_Vehicle)fatherObj).SSG_AssetOwners)
                    {
                        if (GetConcateFieldsStr(config.DuplicateFieldList, props, owner) == entityStr) return owner.AssetOwnerId;
                    };
                    break;
                case "RelatedPersonEntity":
                    foreach (SSG_Identity relatedPerson in ((SSG_Person)fatherObj).SSG_Identities)
                    {
                        if (GetConcateFieldsStr(config.DuplicateFieldList, props, relatedPerson) == entityStr) return relatedPerson.RelatedPersonId;
                    };
                    break;
            }            

            return Guid.Empty;
        }

        private string GetConcateFieldsStr(string[] duplicateFieldList, IList<PropertyInfo> props, object entity)
        {
            string concatedString = string.Empty;
            foreach (string field in duplicateFieldList)
            {
                foreach (PropertyInfo p in props)
                {
                    JsonPropertyAttribute attr = p.GetCustomAttributes<JsonPropertyAttribute>().FirstOrDefault(m => m.PropertyName.ToLower() == field.ToLower());
                    if (attr != null)
                    {
                        object value = p.GetValue(entity, null);
                        if (value != null)
                            concatedString += value.ToString();
                        break;
                    }
                }
            }
            return concatedString.ToLower();
        }

        private async Task<bool> GetDuplicateDetectionConfig(CancellationToken cancellationToken)
        {
            if (_configs != null) return true;
            IEnumerable<SSG_DuplicateDetectionConfig> duplicateConfigs = await _oDataClient.For<SSG_DuplicateDetectionConfig>()
                .FindEntriesAsync(cancellationToken);

            SSG_DuplicateDetectionConfig[] array = duplicateConfigs.ToArray();
            for(int i=0; i<array.Count(); i++)
            {
                array[i].DuplicateFieldList=array[i].DuplicateFields.Split("|");
            }

            _configs = array.AsEnumerable<SSG_DuplicateDetectionConfig>();
            return true;
        }

        private static string hashstring(string input)
        {
            using (SHA512 sha512Hash = SHA512.Create())
            {
                //From String to byte array
                byte[] sourceBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                return hash;
            }
        }
    }
}
