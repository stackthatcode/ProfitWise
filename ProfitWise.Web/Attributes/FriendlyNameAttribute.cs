using System;

namespace ProfitWise.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FriendlyNameAttribute : System.Attribute
    {
        public string FriendlyName { get; set; }
    }
}