using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Api.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Message).IsRequired().HasMaxLength(500);
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(n => n.UserId);

        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Ticket)
            .WithMany(t => t.Notifications)
            .HasForeignKey(n => n.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
