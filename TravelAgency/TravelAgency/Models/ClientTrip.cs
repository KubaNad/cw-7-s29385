﻿namespace TravelAgency.Models;

public class ClientTrip
{
    public int IdClient { get; set; }
    public int Idtrip { get; set; }
    public int RegisteredAt { get; set; }
    public int PaymentDate { get; set; }
}