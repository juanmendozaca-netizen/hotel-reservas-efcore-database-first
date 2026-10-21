using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio
{
    private readonly HotelContext _context;

    public HabitacionRepositorio(HotelContext context)
    {
        _context = context;
    }

    public async Task<List<Habitacion>> ObtenerActivasAsync(int? idTipoHabitacion = null)
    {
        var consulta = _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo);

        if (idTipoHabitacion.HasValue)
        {
            consulta = consulta.Where(h => h.IdTipoHabitacion == idTipoHabitacion.Value);
        }

        return await consulta.OrderBy(h => h.Numero).ToListAsync();
    }

    public async Task<Habitacion?> ObtenerPorIdAsync(int id)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id);
    }

    public async Task<List<TipoHabitacion>> ObtenerTiposActivosAsync()
    {
        return await _context.TipoHabitacions
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task<bool> ExisteNumeroAsync(string numero, int? idExcluir = null)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .AnyAsync(h => h.Numero == numero && (!idExcluir.HasValue || h.IdHabitacion != idExcluir.Value));
    }

    public async Task CrearAsync(Habitacion habitacion)
    {
        _context.Habitacions.Add(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Habitacion habitacion)
    {
        _context.Habitacions.Update(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task DesactivarAsync(int id)
    {
        var habitacion = await _context.Habitacions.FirstOrDefaultAsync(h => h.IdHabitacion == id);
        if (habitacion is null)
        {
            return;
        }

        habitacion.Activo = false;
        await _context.SaveChangesAsync();
    }
}
