using Microsoft.Data.SqlClient;
using TravelAgency.Exceptions;
using TravelAgency.Models;
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
                // O to zapytać na zajęciach !!!!!!!!!!!
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

    public async Task<ClientGetDTO> GetClientDetailsByIdAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT IdClient, FirstName, LastName, Email, Telephone, Pesel FROM Client where IdClient = @id";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        
        if (!await reader.ReadAsync())
        {
            throw new NotFoundException($"Client with id: {id} does not exist");
        }

        return new ClientGetDTO
        {
            IdClient = reader.GetInt32(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2),
            Email = reader.GetString(3),
            Telephone = reader.GetString(4),
            Pesel = reader.GetString(5),
        };
    }

    

    public async Task<Client> CreateClientAsync(ClientCreateDTO client)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql = "INSERT INTO Client(FirstName, LastName, Email, Telephone, Pesel) " +
                           "Values  (@FirstName, @LastName, @Email, @Telephone, @Pesel); Select scope_identity()";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);
        await connection.OpenAsync();
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return new Client()
        {
            IdClient = id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel
        };
    }

    public async Task RegisterClientForTrip(int id, int idTrip)
    {
        await using var connection = new SqlConnection(_connectionString);
        
        //czy klient istnieje 
        const string sql = "SELECT 1 FROM Client where IdClient = @id";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        
        if (!await reader.ReadAsync())
        {
            throw new NotFoundException($"Client with id: {id} does not exist");
        }
        
        await reader.DisposeAsync();
        
        //czy Wycieczka istnieje 
        const string sql2 = "SELECT 1 FROM Trip where IdTrip = @idTrip";
        await using var command2 = new SqlCommand(sql2, connection);
        command2.Parameters.AddWithValue("@idTrip", idTrip);
        // await connection.OpenAsync();
        await using var reader2 = await command2.ExecuteReaderAsync();
        
        if (!await reader2.ReadAsync())
        {
            throw new NotFoundException($"Trip with id: {idTrip} does not exist");
        }
        
        await reader2.DisposeAsync();
        
        //czy są wolne miejsca
        const string sql3 = "SELECT Count(1) FROM Client_Trip where IdTrip = @idTrip";
        await using var command3 = new SqlCommand(sql3, connection);
        command3.Parameters.AddWithValue("@idTrip", idTrip);
        // await connection.OpenAsync();
        
        var participants = Convert.ToInt32(await command3.ExecuteScalarAsync());
        
        const string sql4 = "SELECT MaxPeople FROM Trip where IdTrip = @idTrip";
        await using var command4 = new SqlCommand(sql4, connection);
        command4.Parameters.AddWithValue("@idTrip", idTrip);
        // await connection.OpenAsync();
        
        var maxPeople = Convert.ToInt32(await command4.ExecuteScalarAsync());

        if (participants >= maxPeople)
        {
            throw new CustomExeption("Trip has reached participant limit");
        }
        
        //Przypisanie 
        
        const string sql5 = "INSERT INTO Client_Trip(IdClient, IdTrip, RegisteredAt) values (@IdClient, @IdTrip, @RegisteredAt)";
        await using var command5 = new SqlCommand(sql5, connection);
        command5.Parameters.AddWithValue("@IdClient", id);
        command5.Parameters.AddWithValue("@IdTrip", idTrip);
        command5.Parameters.AddWithValue("@RegisteredAt", DateTime.Now.ToString("yyyyMMdd"));

        var numOfRows = await command5.ExecuteNonQueryAsync();
        
        if (numOfRows == 0)
        {
            throw new NotFoundException($"Sth went wrong");
        }
        
        
    }
    
    
}