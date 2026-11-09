using System.ComponentModel.DataAnnotations;

namespace ChatbotTienda.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    public string Email { get; set; } = "";

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}