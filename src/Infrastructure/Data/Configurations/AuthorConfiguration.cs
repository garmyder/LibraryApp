// LibraryApp.Infrastructure/Data/Configurations/AuthorConfiguration.cs

using LibraryApp.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.Infrastructure.Data.Configurations;

internal sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> b)
    {
        b.HasKey(x => x.AuthorId);
        b.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        b.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        b.Property(x => x.MiddleName).HasMaxLength(100);

        b.HasIndex(x => new { x.LastName, x.FirstName });
    }
}