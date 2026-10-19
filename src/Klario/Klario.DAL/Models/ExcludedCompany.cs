using System;

namespace Klario.DAL.Models;

public class ExcludedCompany : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
}
