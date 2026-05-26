using Tutorial8.DTOs;

namespace Tutorial8.Services;

public interface IDbService
{
    Task<IEnumerable<PatientGetDto>> GetPatientsAsync(string? search);
    Task<int> AddBedAssignmentAsync(string pesel, BedAssignmentPostDto dto);
}