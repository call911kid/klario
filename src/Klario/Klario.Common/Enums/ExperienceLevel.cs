namespace Klario.Common.Enums
{
    [Flags]
    public enum ExperienceLevel
    {
        None = 0,
        Internship = 1 << 0,
        Junior = 1 << 1,
        MidLevel = 1 << 2,
    }
}
