// LibraryApp.Infrastructure/Data/Configurations/SeriesConfiguration.cs

using LibraryApp.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.Infrastructure.Data.Configurations;

internal sealed class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
    public void Configure(EntityTypeBuilder<Series> b)
    {
        b.HasKey(x => x.SeriesId);
        b.Property(x => x.SeriesName).IsRequired().HasMaxLength(300);
        b.Ignore(x => x.BooksCount); // computed, not stored
    }
}