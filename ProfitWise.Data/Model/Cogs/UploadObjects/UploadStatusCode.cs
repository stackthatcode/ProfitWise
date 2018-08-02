namespace ProfitWise.Data.Model.Cogs.UploadObjects
{
    public class UploadStatusCode
    {
        public const int Processing = 1;
        public const int Success = 2;
        public const int FailureSystemFault = 3;
        public const int FailureTooManyErrors = 4;
        public const int FailureZombied = 5;
    }
}
