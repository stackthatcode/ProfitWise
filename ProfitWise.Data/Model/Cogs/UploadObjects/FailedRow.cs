using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.General;

namespace ProfitWise.Data.Model.Cogs.UploadObjects
{
    public class FailedRow
    {
        public int RowNumber { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"Row {RowNumber}: {Message}";
        }

        public static FailedRow Make(int index, List<string> messages)
        {
            var output = new FailedRow();
            output.RowNumber = index + 1;
            output.Message = messages.ToDelimited("; ");
            return output;
        }
    }
}
