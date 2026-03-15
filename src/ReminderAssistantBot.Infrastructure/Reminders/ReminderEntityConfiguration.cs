using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReminderAssistantBot.Infrastructure.Reminders;

internal sealed class ReminderEntityConfiguration : IEntityTypeConfiguration<ReminderEntity>
{
    public void Configure(EntityTypeBuilder<ReminderEntity> builder)
    {
        builder.ToTable("Reminders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.DueAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.SentAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.LeaseUntilUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.Status).IsRequired();

        builder.Property(x => x.AttemptCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.Status, x.DueAtUtc, x.LeaseUntilUtc });
    }
}
