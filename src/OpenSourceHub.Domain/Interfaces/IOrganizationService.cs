using OpenSourceHub.Domain.Entities;

namespace OpenSourceHub.Domain.Interfaces;

public interface IOrganizationService
{
    Task<OrganizationInfo?> GetOrganizationAsync(string orgName, CancellationToken ct = default);
    Task<List<ContributorInfo>> GetOrganizationMembersAsync(string orgName, int count = 30, CancellationToken ct = default);
    Task<List<RepositoryInfo>> GetOrganizationRepositoriesAsync(string orgName, int count = 30, CancellationToken ct = default);
    Task<List<OrganizationInfo>> GetUserOrganizationsAsync(string username, CancellationToken ct = default);
}
