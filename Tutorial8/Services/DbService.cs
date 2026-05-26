using Microsoft.EntityFrameworkCore;
using Tutorial8.Data;
using Tutorial8.DTOs;
using Tutorial8.Exceptions;
using Tutorial8.Models;

namespace Tutorial8.Services;

public class DbService : IDbService
{
    private readonly HospitalContext _dbContext;

    public DbService(HospitalContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<PatientGetDto>> GetPatientsAsync(string? search)
    {
        var query = _dbContext.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, $"%{search}%") ||
                EF.Functions.Like(p.LastName, $"%{search}%"));
        }

        return await query
            .Select(p => new PatientGetDto
            {
                Pesel = p.Pesel,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Age = p.Age,
                Sex = p.Sex ? "Male" : "Female",
                Admissions = p.Admissions.Select(a => new AdmissionDto
                {
                    Id = a.Id,
                    AdmissionDate = a.AdmissionDate,
                    DischargeDate = a.DischargeDate,
                    Ward = new WardDto
                    {
                        Id = a.Ward.Id,
                        Name = a.Ward.Name,
                        Description = a.Ward.Description
                    }
                }).ToList(),
                BedAssignments = p.BedAssignments.Select(ba => new BedAssignmentDto
                {
                    Id = ba.Id,
                    From = ba.From,
                    To = ba.To,
                    Bed = new BedDto
                    {
                        Id = ba.BedNavigation.Id,
                        BedType = new BedTypeDto
                        {
                            Id = ba.BedNavigation.BedType.Id,
                            Name = ba.BedNavigation.BedType.Name,
                            Description = ba.BedNavigation.BedType.Description
                        },
                        Room = new RoomDto
                        {
                            Id = ba.BedNavigation.Room.Id,
                            HasTv = ba.BedNavigation.Room.HasTv,
                            Ward = new WardDto
                            {
                                Id = ba.BedNavigation.Room.Ward.Id,
                                Name = ba.BedNavigation.Room.Ward.Name,
                                Description = ba.BedNavigation.Room.Ward.Description
                            }
                        }
                    }
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<int> AddBedAssignmentAsync(string pesel, BedAssignmentPostDto dto)
    {
        var patient = await _dbContext.Patients.FindAsync(pesel)
            ?? throw new NotFoundException($"Patient with PESEL '{pesel}' not found.");

        var to = dto.To ?? DateTime.MaxValue;

        var bed = await _dbContext.Beds
            .Where(b =>
                b.BedType.Name == dto.BedType &&
                b.Room.Ward.Name == dto.Ward &&
                !b.BedAssignments.Any(ba =>
                    ba.From < to &&
                    (ba.To == null || ba.To > dto.From)))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"No available bed of type '{dto.BedType}' in ward '{dto.Ward}' for the requested period.");

        var assignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = bed.Id,
            From = dto.From,
            To = dto.To
        };

        await _dbContext.BedAssignments.AddAsync(assignment);
        await _dbContext.SaveChangesAsync();
        return assignment.Id;
    }
}