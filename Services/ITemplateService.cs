using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface ITemplateService
    {
        Task<IEnumerable<TemplateItem>> GetAllAsync(string? search = null, string? deviceType = null);
        Task<TemplateItem?> GetByIdAsync(int id);
        Task<int> CreateAsync(TemplateItem template);
        Task UpdateAsync(TemplateItem template);
        Task DeleteAsync(int id);
    }
}
