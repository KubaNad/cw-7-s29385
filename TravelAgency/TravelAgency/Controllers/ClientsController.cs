using Microsoft.AspNetCore.Mvc;
using TravelAgency.Exceptions;
using TravelAgency.Models.DTOs;
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
    
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetClientById([FromRoute] int id)
    {
        try
        {
            return Ok(await dbService.GetClientDetailsByIdAsync(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    

    [HttpPost]
    public async Task<IActionResult> CreateClient(
        [FromBody] ClientCreateDTO body
    )
    {
        var client = await dbService.CreateClientAsync(body);
        return Created($"clients/{client.IdClient}", client);
    }
}