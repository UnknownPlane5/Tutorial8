using Microsoft.AspNetCore.Mvc;
using Tutorial8.DTOs;
using Tutorial8.Exceptions;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IDbService _dbService;

    public PatientsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var patients = await _dbService.GetPatientsAsync(search);
        return Ok(patients);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AddBedAssignment(string pesel, [FromBody] BedAssignmentPostDto dto)
    {
        try
        {
            var id = await _dbService.AddBedAssignmentAsync(pesel, dto);
            return Created($"api/patients/{pesel}/bedassignments/{id}", new { id });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
