using System;
using System.Collections.Generic;

namespace ProfitWise.Data.Model.Cogs.UploadObjects
{
    public class Rule<T>
    {
        public Func<T, bool> Test { get; set; }
        public bool InstantFailure { get; set; }
        public string ValidationMessage { get; set; }

        public Rule(Func<T, bool> test, string validationMessage, bool instantFailure = false)
        {
            Test = test;
            ValidationMessage = validationMessage;
            InstantFailure = instantFailure;
        }
    }
}
