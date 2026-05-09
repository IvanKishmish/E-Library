using E_Library.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Library.Database.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(b => b.Description)
            .HasMaxLength(300);

        builder.Property(b => b.Price)
            .IsRequired()
            .HasPrecision(18, 2); // 18 цифр всього, 2 після коми - стандарт для грошей

        // Зв'язок: Книга -> Автор
        builder.HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .IsRequired() // Робить зв'язок обов'язковим (замінює атрибут [Required])
            .OnDelete(DeleteBehavior.Cascade); // Видалення автора видалить його книги

        // Зв'язок: Книга -> Категорія
        builder.HasOne<Category>() // Використовуємо дженерик, бо у Category немає List<Book>
            .WithMany()
            .HasForeignKey(b => b.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); // Забороняє видаляти категорію, якщо є прив'язані книги
    }
}