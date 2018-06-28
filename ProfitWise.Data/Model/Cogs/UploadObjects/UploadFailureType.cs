namespace ProfitWise.Data.Model.Cogs.UploadObjects
{
    public class UploadFailureType
    {
        public const int TooManyInvalidRows = 1;
        public const int NoRowsWereSuccessfullyProcceed = 2;
        public const int SystemFaultPreventedProcessing = 3;
    }
}
