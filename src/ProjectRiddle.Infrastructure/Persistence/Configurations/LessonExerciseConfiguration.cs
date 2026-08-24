using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectRiddle.Core.Models.Courses;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for lesson exercises.
/// </summary>
public sealed class LessonExerciseConfiguration : IEntityTypeConfiguration<LessonExercise>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LessonExercise> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("LessonExercises");
        builder.HasKey(exercise => exercise.Id);
        builder.Property(exercise => exercise.Id).ValueGeneratedNever();
        builder.Property(exercise => exercise.RiddleId).IsRequired();
        builder.Property(exercise => exercise.Ordinal).IsRequired();
        builder.Property(exercise => exercise.Setup);
        builder.Property(exercise => exercise.TeachingNote);
        builder.Property(exercise => exercise.IsActive).IsRequired();
        builder.HasIndex(exercise => exercise.RiddleId).IsUnique();

        // Restrict, not cascade: withdrawn content is deactivated rather than deleted, and a clue that an exercise
        // still points at must not disappear underneath it.
        builder.HasOne<Riddle>()
            .WithMany()
            .HasForeignKey(exercise => exercise.RiddleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
