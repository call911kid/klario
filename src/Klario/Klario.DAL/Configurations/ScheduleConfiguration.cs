using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Klario.DAL.Models;

namespace Klario.DAL.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("Schedules");

        builder.HasKey(s => new { s.SearchProfileId, s.Source });

        builder.Property(s => s.Source)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.IntervalMinutes)
            .IsRequired()
            .HasDefaultValue(15);

        builder.Property(s => s.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(s => s.SearchProfile)
            .WithMany(sp => sp.Schedules)
            .HasForeignKey(s => s.SearchProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(s => !s.SearchProfile.IsDeleted);
    }
}
