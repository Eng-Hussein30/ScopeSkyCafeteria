using Microsoft.AspNetCore.Http;

namespace ScopeSkyCafeteria.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file, string folder);
    Task DeleteAsync(string? filePath);
}