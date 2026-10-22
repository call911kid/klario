using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Klario.DAL.Models;

namespace Klario.DAL.Configurations;

public class LogConfiguration : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.ToTable("Logs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Level)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(l => l.TimeStamp)
            .IsRequired();

        builder.Property(l => l.MachineName)
            .HasMaxLength(128);

        builder.HasIndex(l => l.TimeStamp);
    }
}
