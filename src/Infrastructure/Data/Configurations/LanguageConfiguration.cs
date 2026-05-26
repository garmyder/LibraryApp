// LibraryApp.Infrastructure/Data/Configurations/LanguageConfiguration.cs

using LibraryApp.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.Infrastructure.Data.Configurations;

internal sealed class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> b)
    {
        b.HasKey(x => x.LanguageId);
        b.Property(x => x.Code).IsRequired().HasMaxLength(10);
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.Code).IsUnique();
    }
}