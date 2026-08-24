using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectRiddle.Core.Models.Courses;

namespace ProjectRiddle.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for primer pages.
/// </summary>
public sealed class PrimerPageConfiguration : IEntityTypeConfiguration<PrimerPage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PrimerPage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PrimerPages");

        // The ordinal is the identity of a page. Nothing records progress against one, so no surrogate is needed.
        builder.HasKey(page => page.Ordinal);
        builder.Property(page => page.Ordinal).ValueGeneratedNever();
        builder.Property(page => page.Title).IsRequired().HasMaxLength(200);
        builder.Property(page => page.Body).IsRequired();
        builder.Property(page => page.Figure).HasMaxLength(64);
        builder.Property(page => page.IsActive).IsRequired();
    }
}
