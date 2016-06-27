using System;
using System.Collections.Specialized;

namespace Push.Utilities.Helpers
{
    public static class NameValueCollectionExtensions
    {
        public static int GetAndTryParseAsInt(this NameValueCollection collection, string name, int defaultValue = 0)
        {
            return collection[name] == null ? defaultValue : Int32.Parse(collection[name]);
        }

        public static string GetAndTryParseAsString(this NameValueCollection collection, string name, string defaultValue = null)
        {
            return collection[name] ?? defaultValue;
        }

        public static bool GetAndTryParseAsBool(this NameValueCollection collection, string name, bool defaultValue = false)
        {
            return collection[name] == null ? defaultValue : Boolean.Parse(collection[name]);
        }
    }
}

