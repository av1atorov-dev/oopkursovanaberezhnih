using PrisonApp.Models;

namespace PrisonApp.Services;

public interface IDataService
{
    Task<List<Prisoner>> LoadAsync();

    Task SaveAsync(IEnumerable<Prisoner> prisoners);
}
