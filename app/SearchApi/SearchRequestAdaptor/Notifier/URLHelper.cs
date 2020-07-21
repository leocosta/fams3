﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SearchRequestAdaptor.Notifier
{
    public static class URLHelper
    {
        public static bool TryCreateUri(string baseUrl, string path, string searchRequestKey, out Uri uri)
        {
            uri = null;
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                return false;
            }

            return Uri.TryCreate(baseUri, Path.Combine(baseUri.LocalPath, path, searchRequestKey), out uri);
        }
    }
}
