using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectRiddle.Core.Models.Courses;

namespace ProjectRiddle.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for lessons, their owned prerequisites, and their exercises.
/// </summary>
public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Lessons");
        builder.HasKey(lesson => lesson.Id);
        builder.Property(lesson => lesson.Id).ValueGeneratedNever();
        builder.Property(lesson => lesson.Key).IsRequired().HasMaxLength(64);
        builder.Property(lesson => lesson.Ordinal).IsRequired();
        builder.Property(lesson => lesson.Title).IsRequired().HasMaxLength(200);
        builder.Property(lesson => lesson.Intro);
        builder.Property(lesson => lesson.Kind).HasConversion<string>().IsRequired();
        builder.Property(lesson => lesson.IsActive).IsRequired();

        // Prerequisites are addressed by lesson key across course boundaries, so the key is unique curriculum-wide
        // rather than only within its course.
        builder.HasIndex(lesson => lesson.Key).IsUnique();

        builder.OwnsMany(
            lesson => lesson.Prerequisites,
            prerequisites =>
            {
                prerequisites.ToTable("LessonPrerequisites");
                prerequisites.WithOwner().HasForeignKey("LessonId");
                prerequisites.Property(prerequisite => prerequisite.LessonKey).IsRequired().HasMaxLength(64);
                prerequisites.HasKey("LessonId", nameof(LessonPrerequisite.LessonKey));
            });
        builder.Navigation(lesson => lesson.Prerequisites)
            .HasField("_prerequisites")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(lesson => lesson.Exercises)
            .WithOne()
            .HasForeignKey("LessonId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(lesson => lesson.Exercises)
            .HasField("_exercises")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
