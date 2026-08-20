namespace DomainTracker.Business.Validation
{
    public static class DomainNameRules
    {
        public const int MaxLength = 255;

        public const string Pattern = @"^([a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,63}$";
    }
}
