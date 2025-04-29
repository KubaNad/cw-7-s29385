using Microsoft.AspNetCore.Mvc;
using TravelAgency.Exceptions;
using TravelAgency.Servives;

namespace TravelAgency.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IDbService dbService) : ControllerBase
{
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTripsById([FromRoute] int id)
    {
        try
        {
            return Ok(await dbService.GetClientTripsDetailsByIdAsync(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}