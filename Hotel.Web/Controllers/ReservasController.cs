using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio _repositorio;

    public ReservasController(ReservaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _repositorio.ObtenerTodasAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        var fechaIngreso = ingreso ?? DateOnly.FromDateTime(DateTime.Today);
        var fechaSalida = salida ?? fechaIngreso.AddDays(1);
        ViewBag.Ingreso = fechaIngreso;
        ViewBag.Salida = fechaSalida;

        if (fechaSalida <= fechaIngreso)
        {
            ModelState.AddModelError("", "La fecha de salida debe ser posterior a la fecha de ingreso.");
            return View(new List<Habitacion>());
        }

        return View(await _repositorio.ObtenerHabitacionesDisponiblesAsync(fechaIngreso, fechaSalida));
    }

    public async Task<IActionResult> Create(int? habitacion)
    {
        await CargarListasAsync(null, habitacion);
        return View(new Reserva
        {
            IdHabitacion = habitacion ?? 0,
            FechaIngreso = DateOnly.FromDateTime(DateTime.Today),
            FechaSalida = DateOnly.FromDateTime(DateTime.Today).AddDays(1),
            Estado = "Pendiente"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reserva reserva)
    {
        ModelState.Remove(nameof(Reserva.Estado));
        ModelState.Remove(nameof(Reserva.FechaRegistro));
        ModelState.Remove(nameof(Reserva.IdHabitacionNavigation));
        ModelState.Remove(nameof(Reserva.IdHuespedNavigation));

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        if (reserva.FechaSalida <= reserva.FechaIngreso)
        {
            ModelState.AddModelError(nameof(Reserva.FechaSalida), "La fecha de salida debe ser posterior al ingreso.");
        }

        if (reserva.FechaIngreso < hoy)
        {
            ModelState.AddModelError(nameof(Reserva.FechaIngreso), "La fecha de ingreso no puede ser anterior a hoy.");
        }

        if (reserva.FechaSalida > reserva.FechaIngreso &&
            await _repositorio.EstaReservadaAsync(reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida))
        {
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación ya está reservada en el rango indicado.");
        }

        if (!ModelState.IsValid)
        {
            await CargarListasAsync(reserva.IdHuesped, reserva.IdHabitacion);
            return View(reserva);
        }

        reserva.Estado = "Pendiente";
        await _repositorio.CrearAsync(reserva);
        TempData["Mensaje"] = "La reserva se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var cancelada = await _repositorio.CancelarAsync(id);
        TempData["Mensaje"] = cancelada
            ? "La reserva se canceló correctamente."
            : "La reserva no puede cancelarse en su estado actual.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync(int? huespedSeleccionado = null, int? habitacionSeleccionada = null)
    {
        var huespedes = await _repositorio.ObtenerHuespedesActivosAsync();
        var habitaciones = await _repositorio.ObtenerHabitacionesActivasAsync();

        ViewBag.Huespedes = new SelectList(
            huespedes.Select(h => new { h.IdHuesped, Nombre = $"{h.Apellidos}, {h.Nombres} - {h.Dni}" }),
            "IdHuesped",
            "Nombre",
            huespedSeleccionado);
        ViewBag.Habitaciones = new SelectList(
            habitaciones.Select(h => new { h.IdHabitacion, Nombre = $"{h.Numero} - {h.IdTipoHabitacionNavigation.Nombre}" }),
            "IdHabitacion",
            "Nombre",
            habitacionSeleccionada);
    }
}
