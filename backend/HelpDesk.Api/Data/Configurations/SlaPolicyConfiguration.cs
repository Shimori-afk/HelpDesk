using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Api.Data.Configurations;

public class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.Property(s => s.Priority).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(s => s.Priority).IsUnique();
    }
}
