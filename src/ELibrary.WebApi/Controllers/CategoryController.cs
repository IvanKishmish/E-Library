using ELibrary.Application.Features.Categories.Commands.CreateCategory;
using ELibrary.Application.Features.Categories.Commands.DeleteCategory;
using ELibrary.Application.Features.Categories.Commands.UpdateCategory;
using ELibrary.Application.Features.Categories.Queries.GetAllCategories;
using ELibrary.Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApi.Controllers;

[Route("api/Categories")]
public sealed class CategoryController(ISender mediator) : ApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllCategoriesQuery();
        var result = await Mediator.Send(query, cancellationToken);

        return result.Match(
            categories => Ok(categories),
            errors => Problem(errors)
            );
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(EntityId id, CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return result.Match(
            category => Ok(category),
            errors => Problem(errors)
            );
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        return result.Match(categoryId => CreatedAtAction(
            nameof(GetById),
            new { id = categoryId },
            new { id = categoryId }),
            errors => Problem(errors));
    }
    
    /// Фронтенд надсилає JSON БЕЗ поля 'id' (передається лише те, що змінюється).
    /// ASP.NET створює об'єкт команди з дефолтним Guid, а синтаксис 'with { Id = id }'
    /// безпечно підставляє правильний ідентифікатор безпосередньо з URL-маршруту.
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(EntityId id, UpdateCategoryCommand requestCommand, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(requestCommand with { Id = id }, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
            );
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(EntityId id, CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
        );
    }
}