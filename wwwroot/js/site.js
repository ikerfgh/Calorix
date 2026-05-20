/* ════════════════════════════════════════════════════════════
   CALORIX — site.js
   Helpers de UI compartidos por todas las vistas
   ════════════════════════════════════════════════════════════ */

// ── MODALES (overlay) ──
function openModal(id) {
  const el = document.getElementById(id);
  if (el) el.classList.add('open');
}
function closeModal(id) {
  const el = document.getElementById(id);
  if (el) el.classList.remove('open');
}
document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape') {
    document.querySelectorAll('.overlay.open').forEach(o => o.classList.remove('open'));
  }
});

// ── TOAST / NOTIF ──
function notify(msg, type = 'success') {
  let toast = document.getElementById('app-toast');
  if (!toast) {
    toast = document.createElement('div');
    toast.id = 'app-toast';
    toast.className = 'notif';
    toast.innerHTML = '<div class="notif-dot"></div><span id="app-toast-msg"></span>';
    document.body.appendChild(toast);
  }
  toast.classList.toggle('error', type === 'error');
  document.getElementById('app-toast-msg').textContent = msg;
  toast.classList.add('show');
  clearTimeout(toast._timer);
  toast._timer = setTimeout(() => toast.classList.remove('show'), 3000);
}
// alias para compatibilidad
function showToast(msg, type) { notify(msg, type); }

// ── CHART de barras (números siempre visibles + densidad adaptativa) ──
// El 4to argumento puede ser un string ('kg' / 'kcal') por compatibilidad,
// o un objeto opts: { unit, clickable, onClick(label) }.
function renderBarChart(containerId, data, todayKey, unit, opts) {
  const el = document.getElementById(containerId);
  if (!el || !data) return;
  // Compatibilidad: si el 4to arg es objeto, lo usamos como opts.
  if (typeof unit === 'object' && unit !== null) { opts = unit; unit = opts.unit; }
  opts = opts || {};
  const u = unit || 'kcal';
  const clickable = !!opts.clickable;
  const entries = Object.entries(data);
  const count = entries.length;

  // Densidad adaptativa: ajusta padding, ancho de barra y rotación de etiquetas
  el.classList.remove('bar-chart-compact', 'bar-chart-dense', 'bar-chart-ultra');
  if (count > 60) el.classList.add('bar-chart-ultra');
  else if (count > 20) el.classList.add('bar-chart-dense');
  else if (count > 10) el.classList.add('bar-chart-compact');

  const vals = entries.map(([, v]) => +v || 0);
  const nonZero = vals.filter(v => v > 0);
  const minVal = nonZero.length ? Math.min(...nonZero) : 0;
  const max = Math.max(...vals, minVal + 1);
  // para peso (rango angosto) baseline = min - 5%, sino 0
  const baseline = u === 'kg' && minVal > 0 ? minVal * 0.95 : 0;
  const range = Math.max(max - baseline, 0.1);

  // Salta etiquetas/valores si hay muchas barras (mantiene legibilidad)
  const labelStep = count > 60 ? 14 : count > 24 ? 5 : count > 12 ? 2 : 1;
  const valueStep = count > 60 ? 14 : count > 40 ? 4 : count > 24 ? 3 : 1;

  el.innerHTML = entries.map(([label, val], i) => {
    const isToday = todayKey ? label === todayKey : (i === entries.length - 1);
    const h = val > 0 ? Math.max(((val - baseline) / range) * 110, 4) : 0;
    const display = u === 'kg' ? (+val).toFixed(1) : Math.round(val);
    const showLabel = (i % labelStep === 0) || i === count - 1;
    const showValue = val > 0 && ((i % valueStep === 0) || i === count - 1 || isToday);
    const safeLabel = String(label).replace(/"/g, '&quot;');
    return `
      <div class="bar-col${clickable ? ' bar-col-click' : ''}" title="${label}: ${display} ${u}${clickable ? ' · Click para detalle' : ''}"
           ${clickable ? `data-label="${safeLabel}" onclick="onBarClick(this)"` : ''}>
        <div class="bar-value">${showValue ? display : ''}</div>
        <div class="bar${isToday ? ' today' : ''}" style="height:${h}px"></div>
        <div class="bar-label">${showLabel ? label : ''}</div>
      </div>`;
  }).join('');

  if (clickable && opts.onClick) {
    el._barOnClick = opts.onClick;
  }
}

// Click delegate: usa el handler asociado al contenedor del bar
function onBarClick(barCol) {
  const label = barCol.getAttribute('data-label');
  const chart = barCol.closest('.bar-chart');
  if (chart && typeof chart._barOnClick === 'function') {
    chart._barOnClick(label);
  }
}

// ── MODAL DETALLE DE DÍA / SEMANA (calorías + macros + comidas + navegación) ──
// Estado del modal de semana
let _modalWeek = null;       // { weekOffset, weekLabel, dias: [...], isCurrentWeek }
let _modalDayIndex = 0;      // 0..6
let _modalAjaxBase = null;   // ej. '/Seguimiento/SemanaDetalle' (null = sin navegación)

// API simple para abrir un solo día (modo legacy: sin navegación de semanas)
function openDayDetail(detail) {
  if (!detail) return;
  const overlay = document.getElementById('modal-day-detail');
  if (!overlay) return;
  _modalAjaxBase = null;
  _modalWeek = null;
  renderDayDetail(detail, /*showWeekNav*/ false);
}

// API para abrir el modal con navegación de semanas
// week: { weekOffset, weekLabel, isCurrentWeek, dias: [...] }
// dayIndex: índice 0..6 dentro de la semana
// ajaxBase: endpoint para fetch al cambiar de semana, ej. '/Seguimiento/SemanaDetalle'
function openWeekDayDetail(week, dayIndex, ajaxBase) {
  if (!week || !week.dias || !week.dias.length) return;
  _modalWeek = week;
  _modalDayIndex = Math.max(0, Math.min(6, dayIndex || 0));
  _modalAjaxBase = ajaxBase || null;
  renderModalWeek();
}

// Click handlers de navegación
async function modalWeekPrev() {
  if (!_modalWeek || !_modalAjaxBase) return;
  await fetchWeekAndRender(_modalWeek.weekOffset - 1);
}
async function modalWeekNext() {
  if (!_modalWeek || !_modalAjaxBase) return;
  // No permitimos avanzar al futuro más allá de la semana actual
  if (_modalWeek.weekOffset >= 0) return;
  await fetchWeekAndRender(_modalWeek.weekOffset + 1);
}
function modalSelectDay(idx) {
  _modalDayIndex = Math.max(0, Math.min(6, idx));
  renderModalWeek();
}

async function fetchWeekAndRender(newOffset) {
  if (!_modalAjaxBase) return;
  try {
    const url = _modalAjaxBase + (_modalAjaxBase.includes('?') ? '&' : '?') + 'weekOffset=' + newOffset;
    const r = await fetch(url);
    const data = await r.json();
    if (!data.ok) { notify('No se pudo cargar la semana', 'error'); return; }
    _modalWeek = data;
    renderModalWeek();
  } catch (e) {
    notify('Error de conexión', 'error');
  }
}

function renderModalWeek() {
  const w = _modalWeek;
  if (!w || !w.dias) return;
  const detail = w.dias[_modalDayIndex] || w.dias[0];
  renderDayDetail(detail, /*showWeekNav*/ true);
}

function renderDayDetail(detail, showWeekNav) {
  if (!detail) return;
  const overlay = document.getElementById('modal-day-detail');
  if (!overlay) return;

  const pct = (v, m) => m > 0 ? Math.round((v / m) * 100) : 0;
  const klass = (p) => p < 70 ? 'macro-bad' : p > 110 ? 'macro-warn' : 'macro-ok';

  const pCal = pct(detail.cal, detail.metaCal);
  const pProt = pct(detail.prot, detail.metaProt);
  const pCarb = pct(detail.carb, detail.metaCarb);
  const pGras = pct(detail.gras, detail.metaGras);

  const momentos = ['Desayuno', 'Comida', 'Cena', 'Snack'];
  const emojis = { 'Desayuno': '☀️', 'Comida': '🌤️', 'Cena': '🌙', 'Snack': '🍎' };
  const regs = detail.registros || [];

  let comidasHtml = '';
  if (!regs.length) {
    comidasHtml = '<div class="empty" style="padding:18px"><div class="empty-icon">🍽️</div><div class="text-xs text-muted">Sin comidas registradas este día</div></div>';
  } else {
    momentos.forEach(m => {
      const items = regs.filter(r => r.momento === m);
      if (!items.length) return;
      const totalM = items.reduce((s, r) => s + (r.cal || 0), 0);
      comidasHtml += `<div class="dd-mom">
        <div class="dd-mom-head">
          <span class="dd-mom-title">${emojis[m] || ''} ${m}</span>
          <span class="dd-mom-total">${Math.round(totalM)} kcal</span>
        </div>
        ${items.map(r => `
          <div class="dd-meal">
            <div class="dd-meal-name">${esc(r.nombre)}</div>
            <div class="dd-meal-sub">${r.cantidad} ${esc(r.unidad || '')} · P:${r.prot}g · C:${r.carb}g · G:${r.gras}g</div>
            <div class="dd-meal-cal">${r.cal} kcal</div>
          </div>`).join('')}
      </div>`;
    });
  }

  // Navegación de semana (opcional)
  let weekNavHtml = '';
  if (showWeekNav && _modalWeek) {
    const canNext = _modalWeek.weekOffset < 0;
    const dayTabs = _modalWeek.dias.map((d, i) => {
      const isActive = i === _modalDayIndex;
      const isFuture = d.esFuturo;
      return `<button type="button" class="dd-day-tab${isActive ? ' active' : ''}${d.esHoy ? ' today' : ''}${isFuture ? ' future' : ''}"
              ${isFuture ? 'disabled title="Día futuro"' : `onclick="modalSelectDay(${i})"`}>
              <span class="dd-day-tab-name">${esc(d.label || '')}</span>
              <span class="dd-day-tab-cal">${d.cal > 0 ? d.cal : '·'}</span>
            </button>`;
    }).join('');
    weekNavHtml = `
      <div class="dd-week-nav">
        <button type="button" class="dd-nav-btn" onclick="modalWeekPrev()" title="Semana anterior">◀</button>
        <div class="dd-week-label">${esc(_modalWeek.weekLabel || '')}</div>
        <button type="button" class="dd-nav-btn${canNext ? '' : ' disabled'}"
                ${canNext ? 'onclick="modalWeekNext()"' : 'disabled'} title="Semana siguiente">▶</button>
      </div>
      <div class="dd-day-tabs">${dayTabs}</div>
    `;
  }

  const html = `
    ${weekNavHtml}
    <div class="dd-head">
      <div class="dd-day-label">${esc(detail.label || '')}</div>
      <div class="dd-day-date">${esc(detail.fechaTexto || detail.fecha || '')}</div>
      ${detail.esHoy ? '<span class="badge badge-green" style="margin-top:6px">Hoy</span>' : ''}
    </div>

    <div class="dd-macro-grid">
      <div class="dd-macro ${klass(pCal)}">
        <div class="dd-macro-label">Calorías</div>
        <div class="dd-macro-val">${detail.cal}<span>/${detail.metaCal} kcal</span></div>
        <div class="dd-macro-bar"><div class="dd-macro-fill" style="width:${Math.min(pCal,100)}%"></div></div>
        <div class="dd-macro-pct">${pCal}%</div>
      </div>
      <div class="dd-macro ${klass(pProt)}">
        <div class="dd-macro-label" style="color:#5bb8f5">Proteínas</div>
        <div class="dd-macro-val">${detail.prot}<span>/${detail.metaProt} g</span></div>
        <div class="dd-macro-bar"><div class="dd-macro-fill" style="width:${Math.min(pProt,100)}%;background:#5bb8f5"></div></div>
        <div class="dd-macro-pct">${pProt}%</div>
      </div>
      <div class="dd-macro ${klass(pCarb)}">
        <div class="dd-macro-label" style="color:#f5a623">Carbos</div>
        <div class="dd-macro-val">${detail.carb}<span>/${detail.metaCarb} g</span></div>
        <div class="dd-macro-bar"><div class="dd-macro-fill" style="width:${Math.min(pCarb,100)}%;background:#f5a623"></div></div>
        <div class="dd-macro-pct">${pCarb}%</div>
      </div>
      <div class="dd-macro ${klass(pGras)}">
        <div class="dd-macro-label" style="color:#3ddc84">Grasas</div>
        <div class="dd-macro-val">${detail.gras}<span>/${detail.metaGras} g</span></div>
        <div class="dd-macro-bar"><div class="dd-macro-fill" style="width:${Math.min(pGras,100)}%;background:#3ddc84"></div></div>
        <div class="dd-macro-pct">${pGras}%</div>
      </div>
    </div>

    <div class="dd-comidas-title">Comidas registradas</div>
    <div class="dd-comidas">${comidasHtml}</div>
  `;
  document.getElementById('day-detail-body').innerHTML = html;
  openModal('modal-day-detail');
}

// ── CHART de líneas (SVG con puntos visibles, valores en hover y al inicio/fin) ──
function renderLineChart(containerId, data, opts) {
  const el = document.getElementById(containerId);
  if (!el || !data) return;
  const o = opts || {};
  const unit = o.unit || 'kg';
  const colorMain = o.color || '#3ddc84';
  const colorArea = o.colorArea || 'rgba(61,220,132,0.18)';
  const showAllValues = o.showAllValues !== false; // default: mostrar todos los valores

  const entries = Object.entries(data);
  const count = entries.length;
  if (count === 0) { el.innerHTML = '<div class="line-empty">Sin datos</div>'; return; }

  const vals = entries.map(([, v]) => +v || 0);
  const nonZero = vals.filter(v => v > 0);
  const minVal = nonZero.length ? Math.min(...nonZero) : 0;
  const maxVal = Math.max(...vals, minVal + 0.5);
  // Padding del rango para que la línea no toque los bordes (más headroom arriba para etiquetas)
  const rangeRaw = Math.max(maxVal - minVal, 0.5);
  const baseline = unit === 'kg' && minVal > 0 ? minVal - rangeRaw * 0.15 : 0;
  const ceiling = maxVal + rangeRaw * 0.30;
  const range = Math.max(ceiling - baseline, 0.5);

  // Geometría del SVG: usar el ancho real del contenedor para que el texto no se estire
  // (con preserveAspectRatio="none" el texto se distorsiona si el viewBox no coincide).
  const measuredW = el.clientWidth || el.parentElement?.clientWidth || 600;
  const W = Math.max(Math.round(measuredW), 240);
  const H = 220;
  // padL más amplio para dejar espacio al eje Y. padR mínimo para que la línea use casi todo el ancho.
  const padL = 44, padR = 18, padT = 18, padB = 38;
  const innerW = W - padL - padR;
  const innerH = H - padT - padB;

  // Etiquetas eje X: muestra cada N
  const labelStep = count > 60 ? 14 : count > 24 ? 5 : count > 12 ? 2 : 1;

  // Eje Y: calcular "nice step" y valores de referencia
  const niceStep = (rng) => {
    if (rng <= 0) return 1;
    const targetSteps = 4;
    const raw = rng / targetSteps;
    const mag = Math.pow(10, Math.floor(Math.log10(raw)));
    const norm = raw / mag;
    const nice = norm < 1.5 ? 1 : norm < 3 ? 2 : norm < 7 ? 5 : 10;
    return nice * mag;
  };
  const yStep = niceStep(ceiling - baseline);
  const yStart = Math.ceil(baseline / yStep) * yStep;
  const yAxisVals = [];
  for (let v = yStart; v <= ceiling + 1e-9 && yAxisVals.length < 8; v = +(v + yStep).toFixed(6)) {
    yAxisVals.push(+v.toFixed(6));
  }
  const yDecimals = yStep >= 1 ? 0 : yStep >= 0.1 ? 1 : 2;

  const points = entries.map(([label, val], i) => {
    const v = +val || 0;
    const x = padL + (count === 1 ? innerW / 2 : (i / (count - 1)) * innerW);
    const yNorm = v > 0 ? (v - baseline) / range : 0;
    const y = padT + innerH - yNorm * innerH;
    return { x, y, v, label, hasData: v > 0, i };
  });

  // Localizar índices del máximo y mínimo global (sólo entre puntos con datos)
  let maxIdx = -1, minIdx = -1;
  points.forEach(p => {
    if (!p.hasData) return;
    if (maxIdx === -1 || p.v > points[maxIdx].v) maxIdx = p.i;
    if (minIdx === -1 || p.v < points[minIdx].v) minIdx = p.i;
  });
  if (maxIdx === minIdx) minIdx = -1; // si todos iguales, no marca dos veces

  // Path de la línea (solo conectar puntos con datos)
  const dataPoints = points.filter(p => p.hasData);
  let pathD = '';
  if (dataPoints.length >= 1) {
    pathD = dataPoints.map((p, idx) => `${idx === 0 ? 'M' : 'L'}${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(' ');
  }
  // Path del área (debajo de la línea hasta baseline)
  let areaD = '';
  if (dataPoints.length >= 2) {
    const last = dataPoints[dataPoints.length - 1];
    const first = dataPoints[0];
    areaD = pathD + ` L${last.x.toFixed(1)},${(padT + innerH).toFixed(1)} L${first.x.toFixed(1)},${(padT + innerH).toFixed(1)} Z`;
  }

  // Cuadrícula horizontal alineada a los valores del eje Y + etiquetas del eje Y
  const yToScreen = (v) => padT + innerH - ((v - baseline) / range) * innerH;
  const gridLines = yAxisVals.map(v => {
    const y = yToScreen(v);
    return `<line x1="${padL}" y1="${y.toFixed(1)}" x2="${(W - padR).toFixed(1)}" y2="${y.toFixed(1)}" stroke="rgba(255,255,255,0.06)" stroke-width="1"/>`;
  }).join('');
  const yAxisLabels = yAxisVals.map(v => {
    const y = yToScreen(v);
    return `<text x="${(padL - 8).toFixed(1)}" y="${(y + 3.5).toFixed(1)}" text-anchor="end"
            font-size="10" fill="var(--text3)" font-family="Inter,system-ui,sans-serif"
            font-variant-numeric="tabular-nums">${v.toFixed(yDecimals)}</text>`;
  }).join('');

  // Etiquetas del eje X
  const xLabels = points.map((p, i) => {
    const show = (i % labelStep === 0) || i === count - 1;
    if (!show) return '';
    return `<text x="${p.x.toFixed(1)}" y="${(H - 14).toFixed(1)}" text-anchor="middle"
            font-size="10" fill="var(--text3)" font-family="Inter,sans-serif">${esc(p.label)}</text>`;
  }).join('');

  // Valores numéricos en "pill" sobre cada punto. Features (primero/último/min/max) en verde,
  // resto en estilo neutro. Si hay >50 puntos sólo muestra features para evitar saturación.
  const valLabels = points.map((p, i) => {
    if (!p.hasData) return '';
    const isEdge = i === 0 || i === count - 1;
    const isMax = i === maxIdx;
    const isMin = i === minIdx;
    const isFeature = isEdge || isMax || isMin;
    if (count > 50 && !isFeature) return '';
    const display = unit === 'kg' ? p.v.toFixed(1) : Math.round(p.v).toString();

    const fontSize = 11;
    const charW = 6.2;
    const pillW = display.length * charW + 10;
    const pillH = 17;
    const offset = 16;
    // Min siempre debajo, Max siempre arriba; edges arriba; resto alterna
    const above = isMax ? true : isMin ? false : (isEdge || i % 2 === 0);
    const yCenter = above
      ? Math.max(p.y - offset, padT + pillH / 2 + 2)
      : Math.min(p.y + offset, padT + innerH - pillH / 2 - 2);
    // Clamp horizontal para que el pill no se salga por izquierda/derecha
    const xCenter = Math.min(Math.max(p.x, pillW / 2 + 2), W - pillW / 2 - 2);
    const pillX = xCenter - pillW / 2;
    const pillY = yCenter - pillH / 2;
    const textY = yCenter + fontSize * 0.36;

    // Todos los pills idénticos: misma paleta verde, mismo borde, mismo glow.
    const textFill = colorMain;
    const bgFill = 'rgba(61,220,132,0.18)';
    const bgStroke = colorMain;
    const pillClass = ' line-val-edge';

    return `<g class="line-val-pill${pillClass}" data-i="${i}">
      <rect x="${pillX.toFixed(1)}" y="${pillY.toFixed(1)}" width="${pillW.toFixed(1)}" height="${pillH}" rx="5" ry="5"
            fill="${bgFill}" stroke="${bgStroke}" stroke-width="0.8"/>
      <text x="${xCenter.toFixed(1)}" y="${textY.toFixed(1)}" text-anchor="middle"
            font-size="${fontSize}" font-weight="600" fill="${textFill}"
            font-family="Inter,system-ui,sans-serif" class="line-val-label">${display}</text>
    </g>`;
  }).join('');

  // Puntos (círculos) con tooltip nativo. Reduce tamaño cuando hay muchos para evitar fusión visual.
  const baseR = count > 24 ? 3 : count > 14 ? 3.5 : 4;
  const dots = points.map((p, i) => {
    if (!p.hasData) return '';
    const isLast = i === count - 1;
    const isFirst = i === 0;
    const isMax = i === maxIdx;
    const isMin = i === minIdx;
    const isFeature = isLast || isFirst || isMax || isMin;
    const display = unit === 'kg' ? p.v.toFixed(1) : Math.round(p.v).toString();
    const r = isLast ? baseR + 1.5 : isFeature ? baseR + 0.5 : baseR;
    return `<g class="line-dot-g" data-i="${i}">
      <circle cx="${p.x.toFixed(1)}" cy="${p.y.toFixed(1)}" r="9" fill="transparent" class="line-dot-hit"></circle>
      <circle cx="${p.x.toFixed(1)}" cy="${p.y.toFixed(1)}" r="${r}" fill="${colorMain}" stroke="var(--bg2)" stroke-width="${count > 24 ? 1.5 : 2}" class="line-dot${isLast ? ' last' : ''}">
        <title>${esc(p.label)}: ${display} ${unit}</title>
      </circle>
    </g>`;
  }).join('');

  el.innerHTML = `
    <svg class="line-chart-svg" viewBox="0 0 ${W} ${H}" preserveAspectRatio="xMidYMid meet"
         xmlns="http://www.w3.org/2000/svg" role="img" aria-label="Gráfica de línea">
      <defs>
        <linearGradient id="lc-grad-${containerId}" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stop-color="${colorMain}" stop-opacity="0.35"/>
          <stop offset="100%" stop-color="${colorMain}" stop-opacity="0"/>
        </linearGradient>
      </defs>
      ${gridLines}
      ${yAxisLabels}
      ${areaD ? `<path d="${areaD}" fill="url(#lc-grad-${containerId})" stroke="none"/>` : ''}
      ${pathD ? `<path d="${pathD}" fill="none" stroke="${colorMain}" stroke-width="2.2"
                       stroke-linecap="round" stroke-linejoin="round"
                       pathLength="1" class="line-path"/>` : ''}
      ${dots}
      ${valLabels}
      ${xLabels}
    </svg>`;

  // Re-render on container resize para que el viewBox siga al ancho real
  if (!el._lcResizeBound) {
    el._lcResizeBound = true;
    let _t = null;
    const ro = new ResizeObserver(() => {
      clearTimeout(_t);
      _t = setTimeout(() => {
        const w = el.clientWidth || 0;
        if (Math.abs(w - (el._lcLastW || 0)) > 4) {
          el._lcLastW = w;
          renderLineChart(containerId, data, opts);
        }
      }, 120);
    });
    ro.observe(el);
    el._lcLastW = el.clientWidth || 0;
  }
}

// ── ANILLO SVG ──
function renderRing(svgId, value, max, colorVar) {
  const svg = document.getElementById(svgId);
  if (!svg) return;
  const radius = 30;
  const circ = 2 * Math.PI * radius;
  const pct = max > 0 ? Math.min(value / max, 1) : 0;
  const offset = circ * (1 - pct);
  svg.innerHTML = `
    <circle cx="36" cy="36" r="${radius}" stroke="var(--bg3)" stroke-width="6" fill="none"/>
    <circle cx="36" cy="36" r="${radius}" stroke="${colorVar}" stroke-width="6" fill="none"
            stroke-dasharray="${circ.toFixed(2)}" stroke-dashoffset="${offset.toFixed(2)}"
            stroke-linecap="round" transform="rotate(-90 36 36)"/>`;
}

// ── BUSCADOR UNIFICADO PARA REGISTRO ──
let _searchAlimentos = [], _searchRecetas = [];
let _selectedItem = null; // {id, esReceta, nombre, cal, prot, carb, gras, emoji}

function initUnifiedSearch(alimentos, recetas) {
  _searchAlimentos = alimentos || [];
  _searchRecetas = recetas || [];
}

function buscarParaRegistro(val) {
  const dd = document.getElementById('fsr-dropdown');
  if (!dd) return;
  const q = (val || '').toLowerCase().trim();
  const ma = q
    ? _searchAlimentos.filter(a => (a.nombre || '').toLowerCase().includes(q) || (a.categoria || '').toLowerCase().includes(q))
    : _searchAlimentos;
  const mr = q
    ? _searchRecetas.filter(r => (r.nombre || '').toLowerCase().includes(q))
    : _searchRecetas;
  let html = '';
  if (ma.length) {
    html += `<div class="fsr-section-lbl">Alimentos</div>`;
    html += ma.map(a => `
      <div class="fsr-item" onclick="selItem(${a.id}, false)">
        <span class="fsr-emoji">🥗</span>
        <div style="flex:1">
          <div class="fsr-name">${esc(a.nombre)}</div>
          <div class="fsr-sub">${esc(a.categoria || '')} · por 100g</div>
        </div>
        <div class="fsr-cal">${Math.round(a.calorias)} kcal</div>
      </div>`).join('');
  }
  if (mr.length) {
    html += `<div class="fsr-section-lbl">Mis Recetas</div>`;
    html += mr.map(r => `
      <div class="fsr-item" onclick="selItem(${r.id}, true)">
        <span class="fsr-emoji">🍽️</span>
        <div style="flex:1">
          <div class="fsr-name">${esc(r.nombre)}</div>
          <div class="fsr-sub">Receta personalizada</div>
        </div>
        <div class="fsr-cal">${Math.round(r.caloriasTotal)} kcal/porción</div>
      </div>`).join('');
  }
  if (!html) html = `<div class="fsr-empty">Sin resultados${q ? ' para "' + esc(val) + '"' : ''}</div>`;
  dd.innerHTML = html;
  dd.classList.add('open');
}

function esc(s) {
  return String(s == null ? '' : s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

function selItem(id, esReceta) {
  const item = esReceta
    ? _searchRecetas.find(r => r.id === id)
    : _searchAlimentos.find(a => a.id === id);
  if (!item) return;
  _selectedItem = {
    id, esReceta,
    nombre: item.nombre,
    cal: esReceta ? item.caloriasTotal : item.calorias,
    prot: esReceta ? item.proteinasTotal : item.proteinas,
    carb: esReceta ? item.carbosTotal : item.carbohidratos,
    gras: esReceta ? item.grasasTotal : item.grasas,
    cat: item.categoria
  };
  const chip = document.getElementById('sel-chip');
  if (chip) {
    document.getElementById('chip-emoji').textContent = esReceta ? '🍽️' : '🥗';
    document.getElementById('chip-name').textContent = item.nombre;
    document.getElementById('chip-sub').textContent = esReceta
      ? `${Math.round(_selectedItem.cal)} kcal/porción · Receta personalizada`
      : `${Math.round(_selectedItem.cal)} kcal/100g · ${item.categoria || ''}`;
    chip.style.display = 'flex';
  }
  const qtyUnit = document.getElementById('qty-unit');
  if (qtyUnit) qtyUnit.textContent = esReceta ? 'porciones' : 'gramos';
  const qtyInput = document.getElementById('inp-cantidad');
  if (qtyInput) qtyInput.value = esReceta ? '1' : '100';
  const dd = document.getElementById('fsr-dropdown');
  if (dd) dd.classList.remove('open');
  const search = document.getElementById('reg-search');
  if (search) search.value = '';
  previewMacros();
}

function clearSel() {
  _selectedItem = null;
  const chip = document.getElementById('sel-chip');
  if (chip) chip.style.display = 'none';
  const prev = document.getElementById('prev-box');
  if (prev) prev.style.display = 'none';
  const qty = document.getElementById('inp-cantidad');
  if (qty) qty.value = '100';
  const qtyUnit = document.getElementById('qty-unit');
  if (qtyUnit) qtyUnit.textContent = 'gramos';
}

function previewMacros() {
  const prev = document.getElementById('prev-box');
  if (!prev) return;
  if (!_selectedItem) { prev.style.display = 'none'; return; }
  const qty = parseFloat(document.getElementById('inp-cantidad').value) || 0;
  const f = _selectedItem.esReceta ? qty : qty / 100;
  prev.style.display = 'block';
  document.getElementById('pv-cal').textContent = (_selectedItem.cal * f).toFixed(0);
  document.getElementById('pv-prot').textContent = (_selectedItem.prot * f).toFixed(1);
  document.getElementById('pv-carb').textContent = (_selectedItem.carb * f).toFixed(1);
  document.getElementById('pv-gras').textContent = (_selectedItem.gras * f).toFixed(1);
}

// ── REGISTRO COMIDA (POST AJAX) ──
async function registrarComida() {
  if (!_selectedItem) { notify('Selecciona un alimento o receta', 'error'); return; }
  const qty = parseFloat(document.getElementById('inp-cantidad').value) || 1;
  const momento = (document.querySelector('#momento-tabs .tab.active')?.dataset.momento) || 'Desayuno';
  const formData = new FormData();
  formData.append('alimentoId', _selectedItem.id);
  formData.append('esReceta', _selectedItem.esReceta);
  formData.append('cantidad', qty);
  formData.append('momento', momento);
  try {
    const r = await fetch('/Registro/Registrar', { method: 'POST', body: formData });
    const data = await r.json();
    if (data.ok) {
      notify(`✓ ${_selectedItem.nombre} registrado en ${momento}`);
      setTimeout(() => location.reload(), 350);
    } else {
      notify('Error al registrar', 'error');
    }
  } catch (err) {
    notify('Error de conexión', 'error');
  }
}

async function eliminarRegistro(id) {
  if (!confirm('¿Eliminar este registro?')) return;
  const formData = new FormData();
  formData.append('registroId', id);
  try {
    const r = await fetch('/Registro/Eliminar', { method: 'POST', body: formData });
    const data = await r.json();
    if (data.ok) {
      notify('✓ Registro eliminado');
      setTimeout(() => location.reload(), 300);
    } else {
      notify('Error al eliminar', 'error');
    }
  } catch (err) {
    notify('Error de conexión', 'error');
  }
}

// ── MOMENTO TABS ──
function setMomento(m, btn) {
  document.querySelectorAll('#momento-tabs .tab').forEach(t => t.classList.remove('active'));
  if (btn) btn.classList.add('active');
}

// ── CERRAR DROPDOWN AL CLICK FUERA ──
document.addEventListener('click', e => {
  if (!e.target.closest('#reg-search') && !e.target.closest('#fsr-dropdown')) {
    document.getElementById('fsr-dropdown')?.classList.remove('open');
  }
});

// ── RECETAS (modal Alimentos) ──
let _recetaIngredientes = [];
let _recetaAlimentosCatalogo = [];

function initRecetaCatalogo(alimentos) {
  _recetaAlimentosCatalogo = alimentos || [];
}

function addIngrediente(alimentoId, nombre, cantidad) {
  _recetaIngredientes.push({
    alimentoId: parseInt(alimentoId),
    nombre, cantidad: parseFloat(cantidad)
  });
  renderIngredientes();
}

function removeIngrediente(idx) {
  _recetaIngredientes.splice(idx, 1);
  renderIngredientes();
}

function renderIngredientes() {
  const el = document.getElementById('ingredientes-list');
  if (!el) return;
  if (!_recetaIngredientes.length) {
    el.innerHTML = '<div class="text-xs text-muted">Sin ingredientes aún.</div>';
    return;
  }
  el.innerHTML = _recetaIngredientes.map((ing, i) => `
    <div class="flex items-center justify-between" style="background:var(--bg3);padding:8px 12px;border-radius:8px;margin-bottom:6px">
      <span class="text-sm">${esc(ing.nombre)} · ${ing.cantidad}g</span>
      <button class="chip-clear" onclick="removeIngrediente(${i})">✕</button>
    </div>`).join('');
}

function submitReceta() {
  const nombre = document.getElementById('receta-nombre').value.trim();
  if (!nombre) { notify('Escribe el nombre de la receta', 'error'); return; }
  if (!_recetaIngredientes.length) { notify('Agrega al menos un ingrediente', 'error'); return; }
  document.getElementById('receta-ingredientes-json').value = JSON.stringify(
    _recetaIngredientes.map(i => ({ alimentoId: i.alimentoId, cantidad: i.cantidad }))
  );
  document.getElementById('receta-form').submit();
}

// ── PACIENTES (vincular AJAX) ──
async function vincularPaciente(usuarioId) {
  const formData = new FormData();
  formData.append('usuarioId', usuarioId);
  try {
    const r = await fetch('/Pacientes/Vincular', { method: 'POST', body: formData });
    const data = await r.json();
    if (data.ok) {
      notify('✓ Paciente vinculado');
      setTimeout(() => location.reload(), 400);
    } else {
      notify('Error al vincular', 'error');
    }
  } catch (err) {
    notify('Error de conexión', 'error');
  }
}

// ── PLANES ──
let _planComidas = [];
function addComidaPlan() {
  const momento = document.getElementById('plan-momento').value;
  const desc = document.getElementById('plan-desc').value.trim();
  const cal = parseFloat(document.getElementById('plan-cal').value) || 0;
  if (!desc) { notify('Escribe la descripción de la comida', 'error'); return; }
  _planComidas.push({ momento, descripcion: desc, calorias: cal });
  document.getElementById('plan-desc').value = '';
  document.getElementById('plan-cal').value = '';
  renderPlanComidas();
}
function removePlanComida(i) { _planComidas.splice(i, 1); renderPlanComidas(); }
function renderPlanComidas() {
  const el = document.getElementById('plan-comidas-list');
  if (!el) return;
  if (!_planComidas.length) {
    el.innerHTML = '<div class="text-xs text-muted">Sin comidas aún.</div>';
    return;
  }
  el.innerHTML = _planComidas.map((c, i) => `
    <div class="plan-meal-row">
      <span class="plan-meal-label">${esc(c.momento)}</span>
      <span class="plan-meal-desc">${esc(c.descripcion)}</span>
      ${c.calorias ? `<span class="plan-meal-cal">${c.calorias} kcal</span>` : ''}
      <button class="chip-clear" onclick="removePlanComida(${i})">✕</button>
    </div>`).join('');
}
async function submitPlan() {
  const nombre = document.getElementById('plan-nombre').value.trim();
  const checks = document.querySelectorAll('input[name="plan-paciente-chk"]:checked');
  const pacienteIds = Array.from(checks).map(c => parseInt(c.value)).filter(Boolean);
  if (!nombre) { notify('Escribe el nombre del plan', 'error'); return; }
  if (pacienteIds.length === 0) { notify('Selecciona al menos un paciente', 'error'); return; }
  if (!_planComidas.length) { notify('Agrega al menos una comida', 'error'); return; }
  const formData = new FormData();
  formData.append('nombre', nombre);
  formData.append('pacienteIdsJson', JSON.stringify(pacienteIds));
  formData.append('comidasJson', JSON.stringify(_planComidas.map(c => ({
    Momento: c.momento, Descripcion: c.descripcion, Calorias: c.calorias, RecetaId: c.recetaId ?? null, AlimentoIds: []
  }))));
  try {
    const r = await fetch('/Planes/Crear', { method: 'POST', body: formData });
    const data = await r.json();
    if (data.ok) {
      notify(`✓ Plan creado para ${data.creados} paciente(s)`);
      setTimeout(() => location.reload(), 500);
    } else {
      notify(data.mensaje || 'Error al crear plan', 'error');
    }
  } catch (err) {
    notify('Error de conexión', 'error');
  }
}
