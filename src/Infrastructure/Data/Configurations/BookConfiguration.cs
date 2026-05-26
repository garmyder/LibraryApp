// Configurations/BookConfiguration.cs
using LibraryApp.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.Infrastructure.Data.Configurations;

internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> b)
    {
        b.HasKey(x => x.BookId);

        b.Property(x => x.Title).IsRequired().HasMaxLength(500);
        b.Property(x => x.FilePath).IsRequired().HasMaxLength(1000);
        b.Property(x => x.Format).HasConversion<string>().HasMaxLength(10);
        b.Property(x => x.SeriesNumber).HasMaxLength(20);
        b.Property(x => x.Published).HasMaxLength(50);
        b.Property(x => x.CoverPath).HasMaxLength(1000);

        b.HasOne(x => x.Series)
            .WithMany(x => x.Books)
            .HasForeignKey(x => x.SeriesId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Genre)
            .WithMany(x => x.Books)
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Language)
            .WithMany(x => x.Books)
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasMany(x => x.Authors)
            .WithMany(x => x.Books)
            .UsingEntity("BookAuthors");

        b.HasMany(x => x.Tags)
            .WithMany(x => x.Books)
            .UsingEntity("BookTags");
    }
}