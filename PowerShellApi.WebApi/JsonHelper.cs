using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerShellRestApi.WebApi
{
    static class JsonHelper
    {
        public static object Convert(JToken token)
        {
            switch (token)
            {
                case JObject obj:
                    return new Dictionary<string, object>(obj.Properties().ToDictionary(pair => pair.Name, pair => Convert(pair.Value)));
                case JArray array:
                    return new List<object>(array.Select(item => Convert(item)));
                case JValue value:
                    return value.Value;
            }
            throw new NotSupportedException();
        }
    }
}
