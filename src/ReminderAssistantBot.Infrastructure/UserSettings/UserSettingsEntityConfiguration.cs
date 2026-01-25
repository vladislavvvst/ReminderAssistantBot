using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReminderAssistantBot.Infrastructure.UserSettings;

internal sealed class UserSettingsEntityConfiguration : IEntityTypeConfiguration<UserSettingsEntity>
{
    public void Configure(EntityTypeBuilder<UserSettingsEntity> builder)
    {
        builder.ToTable("UserSettings");
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.TimezoneKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
