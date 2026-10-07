using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Klario.DAL.Models;

namespace Klario.DAL.Configurations;

public class SearchProfileConfiguration : IEntityTypeConfiguration<SearchProfile>
{
    public void Configure(EntityTypeBuilder<SearchProfile> builder)
    {
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(sp => sp.Description)
            .HasMaxLength(500);

        builder.Property(sp => sp.TelegramChatId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(sp => sp.BotToken)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(sp => sp.Workplace)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sp => sp.Experience)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sp => sp.JobType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasQueryFilter(sp => !sp.IsDeleted);
    }
}
