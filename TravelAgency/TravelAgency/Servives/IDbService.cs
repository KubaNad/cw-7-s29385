using TravelAgency.Models;
using TravelAgency.Models.DTOs;

namespace TravelAgency.Servives;

public interface IDbService
{
    public Task<IEnumerable<TripGetDTO>> GetTripsDetailsAsync();
    public Task<IEnumerable<ClientWithTripsGetDTO>> GetClientTripsDetailsByIdAsync(int id);
    public Task<ClientGetDTO> GetClientDetailsByIdAsync(int id);

    public Task<Client> CreateClientAsync(ClientCreateDTO client);
}