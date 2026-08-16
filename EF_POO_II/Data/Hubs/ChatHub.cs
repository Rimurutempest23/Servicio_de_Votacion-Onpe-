using EF_POO_II.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EF_POO_II.Data.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly SistemaVotacionContext _context;

    public ChatHub(SistemaVotacionContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var usuario = Context.User?.Identity?.Name ?? "Usuario";
        var historial = await _context.ChatMensajes
            .OrderByDescending(m => m.Fecha)
            .Take(50)
            .OrderBy(m => m.Fecha)
            .Select(m => new
            {
                m.Usuario,
                m.Mensaje,
                Hora = m.Fecha.ToString("HH:mm:ss")
            })
            .ToListAsync();

        await Clients.Caller.SendAsync("CargarHistorial", historial);

        await Clients.Caller.SendAsync(
            "RecibirMensaje",
            "Sistema",
            $"Hola {usuario}, estas conectado al chat electoral.",
            DateTime.Now.ToString("HH:mm:ss"));

        await base.OnConnectedAsync();
    }

    public async Task EnviarMensaje(string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return;
        }

        var usuario = Context.User?.Identity?.Name ?? "Usuario";
        var texto = mensaje.Trim();
        var chatMensaje = new ChatMensaje
        {
            Usuario = usuario,
            Mensaje = texto,
            Fecha = DateTime.Now
        };

        _context.ChatMensajes.Add(chatMensaje);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync(
            "RecibirMensaje",
            usuario,
            texto,
            chatMensaje.Fecha.ToString("HH:mm:ss"));
    }
}
