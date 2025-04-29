using Microsoft.Data.SqlClient;
using TravelAgency.Exceptions;
using TravelAgency.Models.DTOs;

namespace TravelAgency.Servives;

public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Default");
    
    public async Task<IEnumerable<TripGetDTO>> GetTripsDetailsAsync()
    {
        var result = new List<TripGetDTO>();

        await using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM Trip";
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TripGetDTO
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                Countries   = new List<string>()
            });
        }

        await reader.DisposeAsync();
        
        foreach (var tripGetDto in result)
        {
            int id = tripGetDto.IdTrip;
            string sql2 = "SELECT C.Name FROM Trip " +
                            "LEFT JOIN Country_Trip CT on Trip.IdTrip = CT.IdTrip " +
                            "INNER JOIN Country C on CT.IdCountry = C.IdCountry where Trip.IdTrip = @id";
            await using var command2 = new SqlCommand(sql2, connection);
            command2.Parameters.AddWithValue("@id", id);
            await using var reader2 = await command2.ExecuteReaderAsync();
            while (await reader2.ReadAsync())
            {
                tripGetDto.Countries.Add(reader2.GetString(0));
            }
        }

        return result;
    }

    public async Task<IEnumerable<ClientWithTripsGetDTO>> GetClientTripsDetailsByIdAsync(int id)
    {
        var result = new List<ClientWithTripsGetDTO>();;

        await using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT T.IdTrip, Name, Description, DateFrom, DateTo, MaxPeople, RegisteredAt, PaymentDate " +
                           "FROM Client " +
                           "LEFT JOIN Client_Trip CT on Client.IdClient = CT.IdClient " +
                           "LEFT JOIN Trip T on CT.IdTrip = T.IdTrip WHERE Client.IdClient = @id";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        
        if (!reader.HasRows)
        {
            throw new NotFoundException($"Client with id: {id} does not exist");
        }
        
        while (await reader.ReadAsync())
        {
            result.Add(new ClientWithTripsGetDTO
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                RegisteredAt = await reader.IsDBNullAsync(6)
                    ? (int?)null 
                    : reader.GetInt32(6),
                PaymentDate  = await reader.IsDBNullAsync(7)
                    ? (int?)null 
                    : reader.GetInt32(7)
            });
        }

        // await reader.DisposeAsync();
        
        return result;
    }
    
}