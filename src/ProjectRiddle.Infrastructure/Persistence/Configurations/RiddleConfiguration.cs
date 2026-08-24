using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for riddles and their structural ranges.
/// </summary>
public sealed class RiddleConfiguration : IEntityTypeConfiguration<Riddle>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Riddle> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Riddles");
        builder.HasKey(riddle => riddle.Id);
        builder.Property(riddle => riddle.Id).ValueGeneratedNever();
        builder.Property(riddle => riddle.Clue).IsRequired();
        builder.Property(riddle => riddle.Answer).IsRequired();
        builder.Property(riddle => riddle.AnswerPattern).IsRequired().HasMaxLength(128);
        builder.Property(riddle => riddle.Explanation).IsRequired();
        builder.Property(riddle => riddle.IsLesson).IsRequired();
        builder.HasIndex(riddle => riddle.IsLesson);
        builder.Property(riddle => riddle.PublicationState)
            .HasConversion<string>()
            .IsRequired();
        builder.Property(riddle => riddle.SofiaPublicationDate);
        builder.Property(riddle => riddle.CreatedAtUtc).IsRequired();
        builder.Property(riddle => riddle.UpdatedAtUtc).IsRequired();
        builder.HasIndex(riddle => riddle.SofiaPublicationDate)
            .IsUnique()
            .HasFilter("SofiaPublicationDate IS NOT NULL AND PublicationState IN ('Scheduled', 'Published')");

        builder.OwnsMany(
            riddle => riddle.Ranges,
            ranges =>
            {
                ranges.ToTable("RiddleRanges");
                ranges.WithOwner().HasForeignKey("RiddleId");
                ranges.HasKey(range => range.Id);
                ranges.Property(range => range.Id).ValueGeneratedNever();
                ranges.Property(range => range.Kind)
                    .HasConversion<string>()
                    .IsRequired();
                ranges.Property(range => range.Start).IsRequired();
                ranges.Property(range => range.End).IsRequired();
            });
        builder.Navigation(riddle => riddle.Ranges)
            .HasField("_ranges")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
