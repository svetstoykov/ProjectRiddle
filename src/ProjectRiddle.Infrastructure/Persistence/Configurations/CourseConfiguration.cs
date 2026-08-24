using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectRiddle.Core.Models.Courses;

namespace ProjectRiddle.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for courses and their lessons.
/// </summary>
public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Courses");
        builder.HasKey(course => course.Id);
        builder.Property(course => course.Id).ValueGeneratedNever();
        builder.Property(course => course.Key).IsRequired().HasMaxLength(64);
        builder.Property(course => course.Ordinal).IsRequired();
        builder.Property(course => course.Title).IsRequired().HasMaxLength(200);
        builder.Property(course => course.Intro).IsRequired();
        builder.Property(course => course.IsActive).IsRequired();
        builder.HasIndex(course => course.Key).IsUnique();

        builder.HasMany(course => course.Lessons)
            .WithOne()
            .HasForeignKey("CourseId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(course => course.Lessons)
            .HasField("_lessons")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
