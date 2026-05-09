using E_Library.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Library.Database.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(a => a.LastName)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(a => a.Biography)
            .IsRequired(false) // Біографія може бути необов'язковою
            .HasMaxLength(300);

        // Налаштування доступу до колекції через backing field
        builder.Metadata
            .FindNavigation(nameof(Author.Books))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}