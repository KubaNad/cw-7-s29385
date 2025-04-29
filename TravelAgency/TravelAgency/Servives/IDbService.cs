using TravelAgency.Models.DTOs;

namespace TravelAgency.Servives;

public interface IDbService
{
    public Task<IEnumerable<TripGetDTO>> GetTripsDetailsAsync();
}