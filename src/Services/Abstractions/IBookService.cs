using E_Library.Dtos.Books;
using ErrorOr;

namespace E_Library.Services.Abstractions;

public interface IBookService
{
    Task<ErrorOr<IEnumerable<BookResponseDto>>> GetAllAsync();
    Task<ErrorOr<BookResponseDto>> GetByIdAsync(EntityId id);
    Task<ErrorOr<BookResponseDto>> CreateAsync(CreateBookRequestDto dto);
    Task<ErrorOr<Success>> UpdateAsync(EntityId id, UpdateBookRequestDto dto);
    Task<ErrorOr<Deleted>> DeleteAsync(EntityId id);
    Task<ErrorOr<IEnumerable<BookResponseDto>>> GetFilteredAsync(BookFilterParameters filter);
}