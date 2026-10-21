using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio
{
    private readonly HotelContext _context;

    public ReservaRepositorio(HotelContext context)
    {
        _context = context;
    }

    public async Task<List<Habitacion>> ObtenerHabitacionesDisponiblesAsync(DateOnly ingreso, DateOnly salida)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo && !h.Reservas.Any(r =>
                r.Estado != "Cancelada" &&
                r.FechaIngreso < salida &&
                r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public async Task<List<Reserva>> ObtenerTodasAsync()
    {
        return await _context.Reservas
            .AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoHabitacionNavigation)
            .OrderByDescending(r => r.FechaIngreso)
            .ThenBy(r => r.IdHabitacionNavigation.Numero)
            .ToListAsync();
    }

    public async Task<List<Huesped>> ObtenerHuespedesActivosAsync()
    {
        return await _context.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Apellidos)
            .ThenBy(h => h.Nombres)
            .ToListAsync();
    }

    public async Task<List<Habitacion>> ObtenerHabitacionesActivasAsync()
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo)
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public async Task<bool> EstaReservadaAsync(int idHabitacion, DateOnly ingreso, DateOnly salida)
    {
        return await _context.Reservas
            .AsNoTracking()
            .AnyAsync(r => r.IdHabitacion == idHabitacion &&
                r.Estado != "Cancelada" &&
                r.FechaIngreso < salida &&
                r.FechaSalida > ingreso);
    }

    public async Task CrearAsync(Reserva reserva)
    {
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CancelarAsync(int id)
    {
        var reserva = await _context.Reservas.FirstOrDefaultAsync(r => r.IdReserva == id);
        if (reserva is null || (reserva.Estado != "Pendiente" && reserva.Estado != "Confirmada"))
        {
            return false;
        }

        reserva.Estado = "Cancelada";
        await _context.SaveChangesAsync();
        return true;
    }
}
