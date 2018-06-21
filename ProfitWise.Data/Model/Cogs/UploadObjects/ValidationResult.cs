using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Cogs.UploadObjects
{

    public class ValidationResult
    {
        public bool Success => FailureMessages.Any();
        public List<string> FailureMessages { get; set; }

        public ValidationResult()
        {
            FailureMessages = new List<string>();
        }

        public static ValidationResult InstantFailure(string reason)
        {
            return new ValidationResult()
            {
                FailureMessages = new List<string> { reason },
            };
        }
    }
}
