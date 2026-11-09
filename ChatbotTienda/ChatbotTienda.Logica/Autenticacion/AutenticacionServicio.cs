using ChatbotTienda.Entidades;
using ChatbotTienda.Entidades.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatbotTienda.Logica.Autenticacion;

public class AutenticacionServicio : IAutenticacionServicio
{
    private readonly ChatbotDbContext _context;
    private readonly PasswordHasher<Usuario> _hasher = new();

    public AutenticacionServicio(ChatbotDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> RegistrarAsync(string nombre, string email, string password)
    {
        email = email.Trim().ToLowerInvariant();

        if (await _context.Usuarios.AnyAsync(u => u.Email == email))
            return null;

        var usuario = new Usuario
        {
            Nombre = nombre.Trim(),
            Email = email,
            Rol = RolUsuario.Cliente
        };
        usuario.PasswordHash = _hasher.HashPassword(usuario, password);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string email, string password)
    {
        email = email.Trim().ToLowerInvariant();

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        if (usuario == null)
            return null;

        var resultado = _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
        return resultado == PasswordVerificationResult.Failed ? null : usuario;
    }
}