using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio _repositorio;

    public HabitacionesController(HabitacionRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index(int? tipo)
    {
        ViewBag.TipoSeleccionado = tipo;
        ViewBag.Tipos = new SelectList(await _repositorio.ObtenerTiposActivosAsync(), "IdTipoHabitacion", "Nombre", tipo);
        return View(await _repositorio.ObtenerActivasAsync(tipo));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var habitacion = await _repositorio.ObtenerPorIdAsync(id.Value);
        return habitacion is null ? NotFound() : View(habitacion);
    }

    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync();
        return View(new Habitacion { Activo = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Habitacion habitacion)
    {
        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(Habitacion.Reservas));

        if (await _repositorio.ExisteNumeroAsync(habitacion.Numero))
        {
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número de habitación ya existe.");
        }

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        await _repositorio.CrearAsync(habitacion);
        TempData["Mensaje"] = "La habitación se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var habitacion = await _repositorio.ObtenerPorIdAsync(id.Value);
        if (habitacion is null)
        {
            return NotFound();
        }

        await CargarTiposAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(Habitacion.Reservas));

        if (await _repositorio.ExisteNumeroAsync(habitacion.Numero, id))
        {
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número de habitación ya existe.");
        }

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        await _repositorio.ActualizarAsync(habitacion);
        TempData["Mensaje"] = "La habitación se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var habitacion = await _repositorio.ObtenerPorIdAsync(id.Value);
        return habitacion is null ? NotFound() : View(habitacion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repositorio.DesactivarAsync(id);
        TempData["Mensaje"] = "La habitación se desactivó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarTiposAsync(int? seleccionado = null)
    {
        ViewBag.Tipos = new SelectList(
            await _repositorio.ObtenerTiposActivosAsync(),
            "IdTipoHabitacion",
            "Nombre",
            seleccionado);
    }
}
