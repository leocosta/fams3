﻿using Newtonsoft.Json;

namespace BcGov.Fams3.Utils.Object
{
    public static class ObjExtensions
    {
        public static T Clone<T>(this T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
