using System.Collections.Generic;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Model.Cogs.UploadObjects
{
    public class ImportContext
    {
        public const int FailureLimit = 1000;

        public PwShop PwShop { get; set; }
        public long FileUploadId { get; set; }

        public int SuccessfulRowCount { get; private set; }
        public List<FailedRow> FailedRows { get; private set; }
        public bool ReachedFailureLimit => FailedRows.Count >= FailureLimit;
        public int TotalRows => SuccessfulRowCount + FailedRows.Count;


        public void ReportSuccess()
        {
            SuccessfulRowCount++;
        }

        public void ReportFailure(int index, ValidationResult validationResult)
        {
            FailedRows.Add(FailedRow.Make(index, validationResult.FailureMessages));
        }

        public ImportContext(PwShop shop, long fileUploadId)
        {
            PwShop = shop;
            FileUploadId = fileUploadId;
            FailedRows = new List<FailedRow>();
        }
    }
}
