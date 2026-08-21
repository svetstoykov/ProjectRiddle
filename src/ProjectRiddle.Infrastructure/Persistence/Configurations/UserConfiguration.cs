using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectRiddle.Core.Enums.Users;
using ProjectRiddle.Core.Models.Users;

namespace ProjectRiddle.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for local accounts.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.Email).IsRequired().HasMaxLength(256);
        builder.Property(user => user.NormalizedEmail).IsRequired().HasMaxLength(256);
        builder.HasIndex(user => user.NormalizedEmail).IsUnique();
        builder.Property(user => user.PasswordHash).IsRequired();
        builder.Property(user => user.Role)
            .HasConversion(
                role => role == UserRole.Admin ? "admin" : "user",
                value => value == "admin" ? UserRole.Admin : UserRole.User)
            .IsRequired();
        builder.Property(user => user.CreatedAtUtc).IsRequired();
    }
}
