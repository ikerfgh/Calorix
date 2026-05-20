using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;
using Calorix.Models;
using System.Text.Json;

namespace Calorix.Controllers;

public class AlimentosController : Controller
{
    private readonly ICalorixFacade _facade;

    public AlimentosController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index(string q = "")
    {
        if (!CheckAuth(out int userId)) return RedirectToAction("Login", "Home");

        ViewBag.Alimentos = _facade.BuscarAlimentos(q);
        ViewBag.Recetas = _facade.ObtenerRecetasUsuario(userId);
        ViewBag.TodosAlimentos = _facade.ObtenerTodosAlimentos();
        ViewBag.Q = q;
        return View();
    }

    [HttpPost]
    public IActionResult CrearReceta(string nombre, string descripcion, string ingredientesJson)
    {
        if (!CheckAuth(out int userId)) return RedirectToAction("Login", "Home");

        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["Err"] = "El nombre de la receta es obligatorio.";
            return RedirectToAction("Index");
        }

        var ingredientes = new List<IngredienteReceta>();
        if (!string.IsNullOrEmpty(ingredientesJson))
        {
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var raw = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(ingredientesJson, opts);
                if (raw != null)
                    foreach (var item in raw)
                    {
                        int aId = 0;
                        float cant = 0;
                        foreach (var kv in item)
                        {
                            var key = kv.Key.ToLowerInvariant();
                            if (key == "alimentoid") aId = kv.Value.GetInt32();
                            else if (key == "cantidad") cant = kv.Value.GetSingle();
                        }
                        if (aId > 0 && cant > 0)
                            ingredientes.Add(new IngredienteReceta { AlimentoId = aId, Cantidad = cant });
                    }
            }
            catch { }
        }

        if (ingredientes.Count == 0)
        {
            TempData["Err"] = "Agrega al menos un ingrediente válido.";
            return RedirectToAction("Index");
        }

        var receta = _facade.CrearReceta(userId, nombre.Trim(), descripcion ?? "", ingredientes);
        TempData["Msg"] = $"✓ Receta \"{receta.Nombre}\" creada ({(int)receta.CaloriasTotal} kcal/porción).";
        return RedirectToAction("Index");
    }

    private bool CheckAuth(out int userId)
    {
        userId = 0;
        var id = HttpContext.Session.GetString("UserId");
        return id != null && HttpContext.Session.GetString("UserRole") != null && int.TryParse(id, out userId);
    }
}
