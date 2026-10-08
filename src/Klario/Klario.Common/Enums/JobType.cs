namespace Klario.Common.Enums
{
    [Flags]
    public enum JobType
    {
        None = 0,
        FullTime = 1 << 0,
        PartTime = 1 << 1,
        Contract = 1 << 2,
        Internship = 1 << 3,
    }
}
