using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Models.Riddles.Progress;
using ProjectRiddle.Infrastructure.Identity;

namespace ProjectRiddle.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for account-owned riddle progress.
/// </summary>
public sealed class RiddleProgressConfiguration : IEntityTypeConfiguration<RiddleProgress>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RiddleProgress> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("RiddleProgress");
        builder.HasKey(progress => progress.Id);
        builder.Property(progress => progress.Id).ValueGeneratedNever();
        builder.Property(progress => progress.AccountId).IsRequired();
        builder.Property(progress => progress.RiddleId).IsRequired();
        builder.Property(progress => progress.Status)
            .HasConversion<string>()
            .IsRequired();
        builder.Property(progress => progress.AnswerAttemptCount).IsRequired();
        builder.Property(progress => progress.UpdatedAtUtc).IsRequired();
        builder.Ignore(progress => progress.UsedHints);
        builder.Ignore(progress => progress.RevealedPositions);
        builder.Ignore(progress => progress.LetterRevealCount);
        builder.HasIndex(progress => new { progress.AccountId, progress.RiddleId }).IsUnique();
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(progress => progress.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Riddle>()
            .WithMany()
            .HasForeignKey(progress => progress.RiddleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(
            progress => progress.Hints,
            hints =>
            {
                hints.ToTable("RiddleProgressHints");
                hints.WithOwner().HasForeignKey("RiddleProgressId");
                hints.Property(hint => hint.Kind)
                    .HasConversion<string>()
                    .IsRequired();
                hints.HasKey("RiddleProgressId", nameof(RiddleProgressHint.Kind));
            });
        builder.Navigation(progress => progress.Hints)
            .HasField("_hints")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(
            progress => progress.Positions,
            positions =>
            {
                positions.ToTable("RiddleProgressPositions");
                positions.WithOwner().HasForeignKey("RiddleProgressId");
                positions.Property(position => position.LetterPosition).IsRequired();
                positions.HasKey("RiddleProgressId", nameof(RiddleProgressPosition.LetterPosition));
            });
        builder.Navigation(progress => progress.Positions)
            .HasField("_positions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
