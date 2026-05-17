using E_Library.Dtos.Authors;
using ErrorOr;

namespace E_Library.Services.Abstractions;

public interface IAuthorService
{
    Task<ErrorOr<IEnumerable<AuthorShortResponseDto>>> GetAllAsync();
    Task<ErrorOr<AuthorResponseDto?>> GetByIdAsync(EntityId id);
    Task<ErrorOr<AuthorResponseDto>> CreateAsync(CreateAuthorRequestDto dto);
    Task<ErrorOr<Success>> UpdateAsync(EntityId id, UpdateAuthorRequestDto dto);
    Task<ErrorOr<Deleted>> DeleteAsync(EntityId id);
}