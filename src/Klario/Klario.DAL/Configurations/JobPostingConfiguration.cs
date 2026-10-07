using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Klario.DAL.Models;

namespace Klario.DAL.Configurations;

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.HasKey(jp => jp.Id);

        builder.Property(jp => jp.ExternalJobId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(jp => jp.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(jp => jp.Company)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(jp => jp.Location)
            .HasMaxLength(250);

        builder.Property(jp => jp.Url)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(jp => jp.Source)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(jp => jp.DescriptionSnippet)
            .HasMaxLength(2000);

        builder.HasIndex(jp => new { jp.Source, jp.ExternalJobId })
            .IsUnique();

        builder.HasIndex(jp => jp.CreatedAt);

        builder.HasQueryFilter(jp => !jp.IsDeleted);
    }
}
