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

        var ingredientes = new List<IngredienteReceta>();
        if (!string.IsNullOrEmpty(ingredientesJson))
        {
            try
            {
                var raw = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(ingredientesJson);
                if (raw != null)
                    foreach (var item in raw)
                        ingredientes.Add(new IngredienteReceta
                        {
                            AlimentoId = item["alimentoId"].GetInt32(),
                            Cantidad = item["cantidad"].GetSingle()
                        });
            }
            catch { }
        }

        if (ingredientes.Count > 0)
            _facade.CrearReceta(userId, nombre, descripcion, ingredientes);

        TempData["Msg"] = "Receta creada exitosamente.";
        return RedirectToAction("Index");
    }

    private bool CheckAuth(out int userId)
    {
        userId = 0;
        var id = HttpContext.Session.GetString("UserId");
        return id != null && HttpContext.Session.GetString("UserRole") != null && int.TryParse(id, out userId);
    }
}
