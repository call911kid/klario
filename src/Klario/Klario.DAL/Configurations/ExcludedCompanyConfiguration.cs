using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Klario.DAL.Models;

namespace Klario.DAL.Configurations;

public class ExcludedCompanyConfiguration : IEntityTypeConfiguration<ExcludedCompany>
{
    public void Configure(EntityTypeBuilder<ExcludedCompany> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
