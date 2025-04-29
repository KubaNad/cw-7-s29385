using System.ComponentModel.DataAnnotations;

namespace TravelAgency.Models.DTOs;

public class ClientCreateDTO
{
    [Required(ErrorMessage = "Imię jest wymagane.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "Imię musi mieć od 1 do 30 znaków.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Nazwisko jest wymagane.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "Nazwisko musi mieć od 1 do 30 znaków.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email jest wymagany.")]
    [StringLength(30, ErrorMessage = "Email może mieć maksymalnie 30 znaków.")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu e-mail.")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Nieprawidłowy numer telefonu.")]
    [StringLength(15, ErrorMessage = "Numer telefonu może mieć maksymalnie 15 znaków.")]
    public string Telephone { get; set; }

    [Required(ErrorMessage = "PESEL jest wymagany.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL musi składać się z 11 cyfr.")]
    public string Pesel { get; set; }
}