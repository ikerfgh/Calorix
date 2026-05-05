/* Calorix — site.js */

// ====== Modal helpers ======
function openModal(id) {
  const el = document.getElementById(id);
  if (el) { el.classList.add('open'); document.body.style.overflow = 'hidden'; }
}
function closeModal(id) {
  const el = document.getElementById(id);
  if (el) { el.classList.remove('open'); document.body.style.overflow = ''; }
}
document.addEventListener('click', e => {
  if (e.target.classList.contains('modal-overlay')) {
    e.target.classList.remove('open');
    document.body.style.overflow = '';
  }
});
document.addEventListener('keydown', e => {
  if (e.key === 'Escape') {
    document.querySelectorAll('.modal-overlay.open').forEach(m => {
      m.classList.remove('open');
      document.body.style.overflow = '';
    });
  }
});

// ====== Toast ======
function showToast(msg, type = 'success') {
  const t = document.createElement('div');
  t.className = `alert alert-${type}`;
  t.style.cssText = 'position:fixed;top:1rem;right:1rem;z-index:9999;min-width:280px;max-width:400px;animation:slideIn .2s ease;';
  t.innerHTML = `<span>${msg}</span>`;
  document.body.appendChild(t);
  setTimeout(() => t.remove(), 3500);
}

// ====== Charts — bar chart ======
function renderBarChart(containerId, data, meta = 0) {
  const container = document.getElementById(containerId);
  if (!container) return;
  const entries = Object.entries(data);
  const max = Math.max(...entries.map(([, v]) => v), 100);
  const today = new Date().toLocaleDateString('es-MX', { weekday: 'short' });

  container.innerHTML = entries.map(([label, value]) => {
    const pct = max > 0 ? (value / max) * 100 : 0;
    const isToday = label.toLowerCase() === today.toLowerCase();
    return `<div class="chart-bar-wrap" title="${label}: ${Math.round(value)} kcal">
      <div class="chart-bar-value">${value > 0 ? Math.round(value) : ''}</div>
      <div class="chart-bar ${isToday ? 'today' : ''}" style="height:${Math.max(pct, value > 0 ? 4 : 2)}%"></div>
      <div class="chart-bar-label">${label}</div>
    </div>`;
  }).join('');
}

// ====== Rings ======
function renderRing(svgId, value, max, colorClass) {
  const svg = document.getElementById(svgId);
  if (!svg) return;
  const r = 32;
  const circ = 2 * Math.PI * r;
  const pct = max > 0 ? Math.min(value / max, 1) : 0;
  svg.innerHTML = `
    <circle class="ring-bg" cx="40" cy="40" r="${r}" stroke-width="7"/>
    <circle class="ring-fill ${colorClass}" cx="40" cy="40" r="${r}" stroke-width="7"
      stroke-dasharray="${circ}" stroke-dashoffset="${circ * (1 - pct)}"/>`;
}

// ====== Nutrient preview calc ======
function calcNutrients(calorias, proteinas, carbos, grasas, cantidad, isReceta = false) {
  if (isReceta) {
    return {
      cal: Math.round(calorias * cantidad),
      prot: Math.round(proteinas * cantidad),
      carb: Math.round(carbos * cantidad),
      grasas: Math.round(grasas * cantidad)
    };
  }
  const f = cantidad / 100;
  return {
    cal: Math.round(calorias * f),
    prot: Math.round(proteinas * f),
    carb: Math.round(carbos * f),
    grasas: Math.round(grasas * f)
  };
}

function updatePreview(data) {
  const p = document.getElementById('food-preview');
  if (!p) return;
  p.classList.add('show');
  p.innerHTML = `
    <div class="flex justify-between items-center">
      <span class="font-semibold text-sm">${data.nombre}</span>
      <span class="font-bold text-primary">${data.cal} kcal</span>
    </div>
    <div class="food-macros">
      <div class="food-macro-item"><div class="food-macro-value" style="color:var(--color-blue-500)">${data.prot}g</div><div class="food-macro-label">Proteínas</div></div>
      <div class="food-macro-item"><div class="food-macro-value" style="color:var(--color-orange-500)">${data.carb}g</div><div class="food-macro-label">Carbos</div></div>
      <div class="food-macro-item"><div class="food-macro-value" style="color:var(--color-purple-500)">${data.grasas}g</div><div class="food-macro-label">Grasas</div></div>
    </div>`;
}

// ====== Unified Search (Registro) ======
let _searchAlimentos = [];
let _searchRecetas = [];
let _selectedItem = null;

function initUnifiedSearch(alimentos, recetas) {
  _searchAlimentos = alimentos;
  _searchRecetas = recetas;
}

function doSearch(query) {
  const q = query.toLowerCase().trim();
  const rAlimentos = q ? _searchAlimentos.filter(a => a.nombre.toLowerCase().includes(q) || a.categoria.toLowerCase().includes(q)) : _searchAlimentos;
  const rRecetas = q ? _searchRecetas.filter(r => r.nombre.toLowerCase().includes(q)) : _searchRecetas;
  renderSearchResults(rAlimentos, rRecetas);
}

function renderSearchResults(alimentos, recetas) {
  const box = document.getElementById('search-results-box');
  if (!box) return;
  if (!alimentos.length && !recetas.length) {
    box.innerHTML = '<div style="padding:1rem;text-align:center;color:var(--color-muted-foreground);font-size:.875rem">Sin resultados</div>';
    box.classList.add('open');
    return;
  }
  let html = '';
  if (alimentos.length) {
    html += `<div class="search-section-header">🥗 Alimentos</div>`;
    html += alimentos.slice(0, 8).map(a =>
      `<div class="search-item" onclick="selectSearchItem(${a.id}, false, '${esc(a.nombre)}', ${a.calorias}, ${a.proteinas}, ${a.carbohidratos}, ${a.grasas})">
        <div><div class="search-item-name">${a.nombre}</div><div class="search-item-meta">${a.categoria} · por 100g</div></div>
        <div class="search-item-cal">${a.calorias} kcal</div>
      </div>`
    ).join('');
  }
  if (recetas.length) {
    html += `<div class="search-section-header">📋 Mis Recetas</div>`;
    html += recetas.slice(0, 5).map(r =>
      `<div class="search-item" onclick="selectSearchItem(${r.id}, true, '${esc(r.nombre)}', ${r.caloriasTotal}, ${r.proteinasTotal}, ${r.carbosTotal}, ${r.grasasTotal})">
        <div><div class="search-item-name">${r.nombre}</div><div class="search-item-meta">Receta · por porción</div></div>
        <div class="search-item-cal">${r.caloriasTotal} kcal</div>
      </div>`
    ).join('');
  }
  box.innerHTML = html;
  box.classList.add('open');
}

function esc(s) { return String(s).replace(/'/g, "\\'"); }

function selectSearchItem(id, esReceta, nombre, calorias, proteinas, carbos, grasas) {
  _selectedItem = { id, esReceta, nombre, calorias, proteinas, carbos, grasas };
  document.getElementById('search-input').value = nombre;
  document.getElementById('search-results-box').classList.remove('open');
  document.getElementById('hidden-alimento-id').value = id;
  document.getElementById('hidden-es-receta').value = esReceta ? 'true' : 'false';

  const cantInput = document.getElementById('cantidad-input');
  const cantLabel = document.getElementById('cantidad-label');
  if (cantLabel) cantLabel.textContent = esReceta ? 'Porciones' : 'Cantidad (gramos)';
  if (cantInput) {
    cantInput.value = esReceta ? '1' : '100';
    cantInput.dispatchEvent(new Event('input'));
  }
}

document.addEventListener('DOMContentLoaded', () => {
  // Close search on outside click
  document.addEventListener('click', e => {
    const box = document.getElementById('search-results-box');
    const inp = document.getElementById('search-input');
    if (box && inp && !box.contains(e.target) && e.target !== inp)
      box.classList.remove('open');
  });

  // Cantidad → update preview
  const cantInput = document.getElementById('cantidad-input');
  if (cantInput) {
    cantInput.addEventListener('input', () => {
      if (!_selectedItem) return;
      const qty = parseFloat(cantInput.value) || 0;
      const n = calcNutrients(_selectedItem.calorias, _selectedItem.proteinas, _selectedItem.carbos, _selectedItem.grasas, qty, _selectedItem.esReceta);
      updatePreview({ nombre: _selectedItem.nombre, ...n });
    });
  }
});

// ====== AJAX register food ======
async function registrarComida(form) {
  const alimentoId = document.getElementById('hidden-alimento-id').value;
  const esReceta = document.getElementById('hidden-es-receta').value === 'true';
  const cantidad = document.getElementById('cantidad-input').value;
  const momento = document.getElementById('momento-select').value;

  if (!alimentoId || !cantidad || !momento) {
    showToast('Completa todos los campos.', 'error');
    return;
  }

  const res = await fetch('/Registro/Registrar', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: `alimentoId=${alimentoId}&esReceta=${esReceta}&cantidad=${cantidad}&momento=${momento}`
  });
  const data = await res.json();
  if (data.ok) {
    showToast('Registro guardado ✓');
    setTimeout(() => location.reload(), 800);
  } else {
    showToast('Error al registrar.', 'error');
  }
}

async function eliminarRegistro(id) {
  if (!confirm('¿Eliminar este registro?')) return;
  const res = await fetch('/Registro/Eliminar', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: `registroId=${id}`
  });
  const data = await res.json();
  if (data.ok) {
    const row = document.getElementById(`reg-${id}`);
    if (row) row.remove();
    showToast('Registro eliminado.');
    setTimeout(() => location.reload(), 1000);
  }
}

// ====== Receta builder ======
let _recetaIngredientes = [];

function addIngrediente(alimentoId, nombre, cantidad) {
  _recetaIngredientes.push({ alimentoId: parseInt(alimentoId), nombre, cantidad: parseFloat(cantidad) });
  renderIngredientes();
}

function removeIngrediente(idx) {
  _recetaIngredientes.splice(idx, 1);
  renderIngredientes();
}

function renderIngredientes() {
  const box = document.getElementById('ingredientes-list');
  if (!box) return;
  if (!_recetaIngredientes.length) {
    box.innerHTML = '<p class="text-xs text-muted">Sin ingredientes aún.</p>';
    return;
  }
  box.innerHTML = _recetaIngredientes.map((i, idx) =>
    `<div class="ingredient-item">
      <span class="ingredient-item-name">${i.nombre}</span>
      <span class="ingredient-item-qty">${i.cantidad}g</span>
      <button type="button" class="btn btn-ghost btn-sm" onclick="removeIngrediente(${idx})">✕</button>
    </div>`
  ).join('');
}

function submitReceta() {
  const nombre = document.getElementById('receta-nombre').value.trim();
  const desc = document.getElementById('receta-desc').value.trim();
  if (!nombre || !_recetaIngredientes.length) {
    showToast('Nombre e ingredientes son requeridos.', 'error');
    return;
  }
  document.getElementById('receta-ingredientes-json').value = JSON.stringify(
    _recetaIngredientes.map(i => ({ alimentoId: i.alimentoId, cantidad: i.cantidad }))
  );
  document.getElementById('receta-form').submit();
}

// ====== Vincular paciente ======
async function vincularPaciente(usuarioId) {
  const res = await fetch('/Pacientes/Vincular', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: `usuarioId=${usuarioId}`
  });
  const data = await res.json();
  if (data.ok) { showToast('Paciente vinculado ✓'); setTimeout(() => location.reload(), 800); }
  else showToast('Error al vincular.', 'error');
}

// ====== Plan comidas builder ======
let _planComidas = [];

function addComidaPlan() {
  const momento = document.getElementById('plan-momento').value;
  const desc = document.getElementById('plan-desc').value.trim();
  const calorias = parseFloat(document.getElementById('plan-cal').value) || 0;
  if (!momento || !desc) { showToast('Momento y descripción son requeridos.', 'error'); return; }
  _planComidas.push({ momento, descripcion: desc, calorias, alimentoIds: [] });
  renderPlanComidas();
  document.getElementById('plan-desc').value = '';
  document.getElementById('plan-cal').value = '';
}

function renderPlanComidas() {
  const box = document.getElementById('plan-comidas-list');
  if (!box) return;
  box.innerHTML = _planComidas.map((c, i) =>
    `<div class="ingredient-item">
      <span class="momento-badge momento-${c.momento}">${c.momento}</span>
      <span class="ingredient-item-name">${c.descripcion}</span>
      <span class="ingredient-item-qty">${c.calorias} kcal</span>
      <button type="button" class="btn btn-ghost btn-sm" onclick="removePlanComida(${i})">✕</button>
    </div>`
  ).join('') || '<p class="text-xs text-muted">Sin comidas aún.</p>';
}

function removePlanComida(i) { _planComidas.splice(i, 1); renderPlanComidas(); }

async function submitPlan() {
  const nombre = document.getElementById('plan-nombre').value.trim();
  const pacienteId = document.getElementById('plan-paciente').value;
  if (!nombre || !pacienteId || !_planComidas.length) {
    showToast('Completa nombre, paciente y al menos una comida.', 'error');
    return;
  }
  const res = await fetch('/Planes/Crear', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: `pacienteId=${pacienteId}&nombre=${encodeURIComponent(nombre)}&comidasJson=${encodeURIComponent(JSON.stringify(_planComidas))}`
  });
  const data = await res.json();
  if (data.ok) { showToast('Plan creado ✓'); closeModal('modal-plan'); setTimeout(() => location.reload(), 1000); }
  else showToast('Error al crear el plan.', 'error');
}

// CSS animation
const style = document.createElement('style');
style.textContent = '@keyframes slideIn{from{transform:translateX(100%);opacity:0}to{transform:translateX(0);opacity:1}}';
document.head.appendChild(style);
