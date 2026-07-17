namespace POS.Application.Modules.Catalog.DTOs;

public record CategoryDto(Guid Id, string Name, int ProductCount);

public record UpsertCategoryRequest(string Name);
