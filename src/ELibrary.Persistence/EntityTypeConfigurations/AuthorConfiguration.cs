using ELibrary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELibrary.Persistence.EntityTypeConfigurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors");

        // Первинний ключ (EF Core сам зрозуміє, що це Guid)
        builder.HasKey(a => a.Id);
        
        // кажемо EF що база не повинна сама генерувати ID
        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Biography)
            .HasMaxLength(2000)
            .HasDefaultValue(string.Empty);

        // Властивості аудиту з інтерфейсу
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);

        // Зв'язок: у одного автора багато книг
        builder.HasMany(a => a.Books)
            .WithOne(b => b.Author)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Дозволяємо EF працювати з колекцією Books, попри те, що у неї private setter
        builder.Navigation(a => a.Books)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}