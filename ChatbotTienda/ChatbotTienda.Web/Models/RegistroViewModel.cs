using System.ComponentModel.DataAnnotations;

namespace ChatbotTienda.Web.Models;

public class RegistroViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [StringLength(200)]
    public string Email { get; set; } = "";

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
    
    [Display(Name = "Confirmar contraseña")]
    [Required(ErrorMessage = "Confirmá la contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarPassword { get; set; } = "";
}