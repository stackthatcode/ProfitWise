using System.Collections.Generic;

namespace ProfitWise.Data.Model.Cogs.UploadObjects
{
    public class ValidationSequence<T>
    {
        public List<Rule<T>> Rules { get; set; }

        public ValidationSequence()
        {
            Rules = new List<Rule<T>>();
        }

        public ValidationSequence<T> Add(Rule<T> rule)
        {
            Rules.Add(rule);
            return this;
        }


        public ValidationResult Run(T input)
        {
            var output = new ValidationResult();

            foreach (var rule in Rules)
            {
                if (rule.Test(input))
                {
                    continue;
                }
                output.FailureMessages.Add(rule.ValidationMessage);

                if (rule.InstantFailure)
                {
                    return output;
                }
            }
            return output;
        }
    }
}

