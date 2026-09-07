const API_BASE = '/api';

// Estado global de la app
let leadsData = [];
let currentFilter = { query: '', estado: '', producto: '', ciudad: '', pais: '' };

const PIPELINE_STAGES = [
  { key: 0, name: 'Nuevo', color: '#64748b' },
  { key: 1, name: 'Contactado', color: '#0ea5e9' },
  { key: 2, name: 'Calificado', color: '#6366f1' },
  { key: 3, name: 'Demo Agendada', color: '#a855f7' },
  { key: 4, name: 'Demo Realizada', color: '#ec4899' },
  { key: 5, name: 'Cotización', color: '#f59e0b' },
  { key: 6, name: 'Negociación', color: '#f97316' },
  { key: 7, name: 'Ganado', color: '#10b981' }
];

const OBJECTIONS = [
  {
    titulo: '1. "Está muy costoso"',
    pregunta: '¿Lo compara con alguna plataforma que utilizan actualmente o con el presupuesto que tenían pensado?',
    respuesta: 'Si es presupuesto, enfocar la configuración en lo necesario. Si compara con Excel, explicar que la propuesta conecta múltiples procesos y reduce horas de trabajo duplicado.',
    confirmar: 'Si encontráramos una configuración que encaje mejor con lo que realmente necesitan, ¿tendría sentido continuar evaluándolo?'
  },
  {
    titulo: '2. "Somos una iglesia muy pequeña"',
    pregunta: 'Más que el número de personas, ¿qué procesos manejan actualmente además del servicio principal?',
    respuesta: 'EkklesiApp es útil desde iglesias de 40 personas hasta megas. El valor real está en ordenar la membresía y el cuidado pastoral antes de crecer para no perder a la gente.',
    confirmar: '¿Qué dos procesos sienten que sería más importante ordenar desde ahora?'
  },
  {
    titulo: '3. "Usamos Excel y nos funciona"',
    pregunta: '¿Toda la información está en un solo archivo o diferentes ministerios manejan sus propios archivos? ¿Cómo saben cuándo alguien deja de congregarse?',
    respuesta: 'No se trata de reemplazar Excel por reemplazarlo, sino de conectar miembros, asistencia, células y seguimiento en tiempo real sin cruzar archivos a mano.',
    confirmar: '¿Hay algún proceso que actualmente dependa de cruzar varios archivos o chats de WhatsApp?'
  },
  {
    titulo: '4. "Ya tenemos otra plataforma (ej. Planning Center / Breeze)"',
    pregunta: '¿Qué es lo que más les gusta de ella? ¿Hay algún proceso que todavía tengan que manejar por fuera o pagar en dólares?',
    respuesta: 'Starnet ofrece soporte 100% en español en la misma zona horaria, precios en pesos colombianos y una suite completa (pastoral + alabanza + predicación) sin costos sorpresa.',
    confirmar: 'Si revisamos esas brechas específicas, ¿tendría sentido comparar cómo las trabaja EkklesiApp?'
  },
  {
    titulo: '5. "Nuestro pastor no es tecnológico"',
    pregunta: '¿Quién suele encargarse actualmente de la información y de los procesos administrativos?',
    respuesta: 'El pastor no tiene que operar técnicamente el sistema. Puede tener un acceso ejecutivo para ver resúmenes mientras secretarias y líderes de ministerio gestionan el día a día.',
    confirmar: '¿Cuentan con algún líder o administrador que apoye la operación?'
  },
  {
    titulo: '6. "Nos preocupa la seguridad de los datos"',
    pregunta: '¿Hay algún aspecto específico que quieran revisar: acceso, permisos o copias de seguridad?',
    respuesta: 'La información pertenece 100% a la iglesia. Manejamos aislamiento por organización, roles estrictos de permisos y respaldo continuo en la nube.',
    confirmar: '¿La inquietud queda en el control de acceso o hay algún requisito legal adicional?'
  },
  {
    titulo: '7. "¿ChordSync o SermonSync vienen incluidos?"',
    pregunta: '¿Tienen interés específico en el equipo de alabanza o en la preparación de sermones?',
    respuesta: 'Son productos especializados adicionales de STARNET. Pueden funcionar de forma independiente o integrarse al ecosistema con condiciones preferenciales en combo.',
    confirmar: '¿Les interesaría incluir una demostración para el ministerio musical o pastoral?'
  }
];

// Inicialización
document.addEventListener('DOMContentLoaded', async () => {
  setupTabs();
  renderObjections();
  await loadMetrics();
  await loadLeads();

  // Búsqueda en tiempo real
  document.getElementById('searchInput')?.addEventListener('input', (e) => {
    currentFilter.query = e.target.value;
    filterAndRender();
  });
  document.getElementById('countryFilter')?.addEventListener('change', (e) => {
    currentFilter.pais = e.target.value;
    filterAndRender();
  });
  document.getElementById('productFilter')?.addEventListener('change', (e) => {
    currentFilter.producto = e.target.value;
    filterAndRender();
  });
});

function setupTabs() {
  const tabs = document.querySelectorAll('.tab-btn');
  tabs.forEach(tab => {
    tab.addEventListener('click', () => {
      tabs.forEach(t => t.classList.remove('active'));
      tab.classList.add('active');

      const target = tab.getAttribute('data-tab');
      document.querySelectorAll('.tab-view').forEach(view => {
        view.classList.toggle('hidden', view.id !== target);
      });
    });
  });
}

async function loadMetrics() {
  try {
    const res = await fetch(`${API_BASE}/metrics/summary`);
    if (res.ok) {
      const stats = await res.json();
      document.getElementById('mTotalLeads').textContent = stats.totalLeads;
      document.getElementById('mDemos').textContent = stats.totalDemosAgendadasORealizadas;
      document.getElementById('mGanados').textContent = stats.totalGanados;
      document.getElementById('mConversion').textContent = `${stats.tasaConversion}%`;
    }
  } catch (err) {
    console.error('Error cargando métricas:', err);
  }
}

async function loadLeads() {
  try {
    const res = await fetch(`${API_BASE}/leads`);
    if (res.ok) {
      leadsData = await res.json();
      renderKanban();
      renderDirectory();
    }
  } catch (err) {
    console.error('Error cargando leads:', err);
  }
}

function filterAndRender() {
  renderDirectory();
  renderKanban();
}

function getFilteredLeads() {
  return leadsData.filter(l => {
    const q = currentFilter.query.toLowerCase();
    const matchQ = !q ||
      l.nombreIglesia?.toLowerCase().includes(q) ||
      l.nombreContacto?.toLowerCase().includes(q) ||
      l.ciudad?.toLowerCase().includes(q) ||
      l.pais?.toLowerCase().includes(q) ||
      l.telefono?.includes(q);

    const matchProd = !currentFilter.producto || l.productoInteres.toString() === currentFilter.producto;
    const matchPais = !currentFilter.pais || (l.pais && l.pais.toLowerCase() === currentFilter.pais.toLowerCase());
    return matchQ && matchProd && matchPais;
  });
}

// Render Kanban
function renderKanban() {
  const board = document.getElementById('kanbanBoard');
  if (!board) return;
  board.innerHTML = '';

  const filtered = getFilteredLeads();

  PIPELINE_STAGES.forEach(stage => {
    const stageLeads = filtered.filter(l => l.estadoPipeline === stage.key);

    const col = document.createElement('div');
    col.className = 'kanban-col';
    col.innerHTML = `
      <div class="col-header">
        <div class="col-title" style="color: ${stage.color}">
          <span style="display:inline-block;width:8px;height:8px;border-radius:50%;background:${stage.color};"></span>
          ${stage.name}
        </div>
        <span class="col-count">${stageLeads.length}</span>
      </div>
      <div class="col-cards" id="stage-cards-${stage.key}"></div>
    `;

    const cardsContainer = col.querySelector(`#stage-cards-${stage.key}`);

    stageLeads.forEach(lead => {
      const card = document.createElement('div');
      card.className = 'lead-card';
      const prodBadgeClass = getProductBadgeClass(lead.productoInteres);
      const prodName = getProductName(lead.productoInteres);

      const isMobile = isMobilePhone(lead.telefono, lead.pais);
      const phoneBadge = lead.telefono ? (isMobile ? '<span class="badge-mobile">Móvil WA</span>' : '<span class="badge-landline">Fijo</span>') : '';
      const tondmBadge = lead.calificacionTondm?.calificacionCompletada ? '<span style="font-size:0.65rem;background:rgba(99,102,241,0.25);color:#818cf8;border:1px solid rgba(99,102,241,0.4);border-radius:4px;padding:0.1rem 0.35rem;font-weight:700;">TONDM ✓</span>' : '';

      card.innerHTML = `
        <div class="lead-card-header">
          <div class="lead-church-name">${escapeHtml(lead.nombreIglesia)}</div>
          <div style="display:flex;gap:0.3rem;align-items:center;">
            ${tondmBadge}
            <span class="badge-product ${prodBadgeClass}">${prodName}</span>
          </div>
        </div>
        <div class="lead-meta">
          <div>📍 ${escapeHtml(lead.ciudad || '')}, ${escapeHtml(lead.pais || 'Colombia')} ${lead.cantidadMiembros ? '· 👥 ' + lead.cantidadMiembros + ' m.' : ''}</div>
          <div>👤 ${escapeHtml(lead.nombreContacto || 'Pastor')} (${escapeHtml(lead.cargoContacto || 'Líder')})</div>
          ${lead.telefono ? `<div>📞 ${escapeHtml(lead.telefono)} ${phoneBadge}</div>` : ''}
          ${lead.email ? `<div>✉️ ${escapeHtml(lead.email)}</div>` : ''}
        </div>
        <div class="lead-footer-actions">
          <div style="display:flex;gap:0.25rem;flex-wrap:wrap;">
            ${lead.telefono ? `<button class="btn btn-success btn-sm" onclick="openWhatsAppModal(${lead.id})">💬 WA</button>` : ''}
            <button class="btn btn-tondm btn-sm" onclick="openTondmModal(${lead.id})" title="Calificar llamada TONDM">📞 TONDM</button>
            <button class="btn btn-secondary btn-sm" onclick="openEditLeadModal(${lead.id})" title="Editar iglesia">✏️</button>
            <button class="btn btn-secondary btn-sm" onclick="viewLeadDetail(${lead.id})" title="Ver ficha">📄</button>
            <button class="btn btn-danger btn-sm" onclick="deleteLead(${lead.id})" title="Eliminar iglesia">🗑️</button>
          </div>
          <div style="display:flex;gap:0.2rem;margin-top:0.3rem;">
            ${stage.key > 0 ? `<button class="btn btn-secondary btn-sm" title="Retroceder etapa" onclick="moveStage(${lead.id}, ${stage.key - 1})">◀</button>` : ''}
            ${stage.key < 7 ? `<button class="btn btn-primary btn-sm" title="Avanzar etapa" onclick="moveStage(${lead.id}, ${stage.key + 1})">▶</button>` : ''}
          </div>
        </div>
      `;
      cardsContainer.appendChild(card);
    });

    board.appendChild(col);
  });
}

// Render Directorio
function renderDirectory() {
  const tbody = document.getElementById('directoryTbody');
  if (!tbody) return;
  tbody.innerHTML = '';

  const filtered = getFilteredLeads();

  if (filtered.length === 0) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--text-muted);padding:2rem;">No se encontraron prospectos con los filtros actuales.</td></tr>`;
    return;
  }

  filtered.forEach(lead => {
    const tr = document.createElement('tr');
    const stageName = PIPELINE_STAGES.find(s => s.key === lead.estadoPipeline)?.name || 'Nuevo';
    const prodName = getProductName(lead.productoInteres);

    const isMobile = isMobilePhone(lead.telefono, lead.pais);
    const phoneBadge = lead.telefono ? (isMobile ? '<span class="badge-mobile">Móvil WA</span>' : '<span class="badge-landline">Fijo</span>') : '';
    const tondmStatus = lead.calificacionTondm?.calificacionCompletada ? '<span style="color:#818cf8;font-weight:700;">TONDM ✓</span>' : '<span style="color:var(--text-muted);font-size:0.75rem;">Pendiente</span>';

    tr.innerHTML = `
      <td>
        <strong>${escapeHtml(lead.nombreIglesia)}</strong>
        ${lead.direccion ? `<div style="font-size:0.75rem;color:var(--text-secondary);">${escapeHtml(lead.direccion)}</div>` : ''}
      </td>
      <td><strong>${escapeHtml(lead.pais || 'Colombia')}</strong> · <span style="color:var(--text-secondary);">${escapeHtml(lead.ciudad || '')}</span></td>
      <td>${escapeHtml(lead.nombreContacto || 'Pastor')}</td>
      <td>${escapeHtml(lead.telefono || 'Sin teléfono')} ${phoneBadge}</td>
      <td><span class="badge-product ${getProductBadgeClass(lead.productoInteres)}">${prodName}</span></td>
      <td>
        <span style="font-weight:600;">${stageName}</span>
        <div style="font-size:0.75rem;">${tondmStatus}</div>
      </td>
      <td>
        <div style="display:flex;gap:0.25rem;flex-wrap:wrap;">
          ${lead.telefono ? `<button class="btn btn-success btn-sm" onclick="openWhatsAppModal(${lead.id})" title="Enviar WhatsApp">WA</button>` : ''}
          <button class="btn btn-tondm btn-sm" onclick="openTondmModal(${lead.id})" title="Calificar TONDM">TONDM</button>
          <button class="btn btn-secondary btn-sm" onclick="openEditLeadModal(${lead.id})" title="Editar iglesia">Editar</button>
          <button class="btn btn-secondary btn-sm" onclick="viewLeadDetail(${lead.id})" title="Ver ficha">Ficha</button>
          <button class="btn btn-secondary btn-sm" onclick="exportHandoff(${lead.id})" title="Copiar Handoff">HUB</button>
          <button class="btn btn-danger btn-sm" onclick="deleteLead(${lead.id})" title="Eliminar iglesia">✕</button>
        </div>
      </td>
    `;
    tbody.appendChild(tr);
  });
}

function getProductName(val) {
  switch (val) {
    case 0: return 'EkklesiApp';
    case 1: return 'ChordSync';
    case 2: return 'SermonSync';
    case 3: return 'Suite Completa';
    default: return 'EkklesiApp';
  }
}

function getProductBadgeClass(val) {
  switch (val) {
    case 0: return 'badge-ekklesiapp';
    case 1: return 'badge-chordsync';
    case 2: return 'badge-sermonsync';
    case 3: return 'badge-suitecompleta';
    default: return 'badge-ekklesiapp';
  }
}

// Mover etapa
async function moveStage(id, newStage) {
  try {
    const res = await fetch(`${API_BASE}/leads/${id}/stage`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ nuevoEstado: newStage })
    });
    if (res.ok) {
      await loadLeads();
      await loadMetrics();
    }
  } catch (err) {
    console.error('Error moviendo etapa:', err);
  }
}

// Modal WhatsApp con Scripts del Manual
let activeWhatsAppLead = null;

async function openWhatsAppModal(leadId) {
  const lead = leadsData.find(l => l.id === leadId);
  if (!lead) return;
  activeWhatsAppLead = lead;

  document.getElementById('waChurchTitle').textContent = lead.nombreIglesia;
  document.getElementById('waPhoneText').textContent = lead.telefono;

  updateWhatsAppPreview();
  document.getElementById('whatsappModal').classList.remove('hidden');
}

function updateWhatsAppPreview() {
  if (!activeWhatsAppLead) return;
  const scriptType = document.getElementById('waScriptSelect').value;

  fetch(`${API_BASE}/leads/${activeWhatsAppLead.id}/whatsapp-link?script=${scriptType}`)
    .then(r => r.json())
    .then(data => {
      const url = new URL(data.url);
      const text = url.searchParams.get('text') || '';
      document.getElementById('waMessagePreview').value = text;
      document.getElementById('waSendBtn').onclick = () => window.open(data.url, '_blank');
    });
}

function closeWhatsAppModal() {
  document.getElementById('whatsappModal').classList.add('hidden');
}

// Render Objeciones (Modelo de Aprendizaje)
function renderObjections() {
  const container = document.getElementById('objectionsList');
  if (!container) return;
  container.innerHTML = '';

  OBJECTIONS.forEach((obj, idx) => {
    const item = document.createElement('div');
    item.className = 'accordion-item';
    item.innerHTML = `
      <div class="accordion-header" onclick="toggleAccordion(${idx})">
        <span>${obj.titulo}</span>
        <span>▼</span>
      </div>
      <div class="accordion-body hidden" id="accordion-body-${idx}">
        <div class="evprc-step"><strong>1. Preguntar / Aclarar:</strong> "${obj.pregunta}"</div>
        <div class="evprc-step"><strong>2. Responder:</strong> ${obj.respuesta}</div>
        <div class="evprc-step"><strong>3. Confirmar:</strong> "${obj.confirmar}"</div>
      </div>
    `;
    container.appendChild(item);
  });
}

function toggleAccordion(idx) {
  const body = document.getElementById(`accordion-body-${idx}`);
  if (body) {
    body.classList.toggle('hidden');
  }
}

// Ficha de detalle y Handoff STARNET HUB
async function viewLeadDetail(id) {
  try {
    const res = await fetch(`${API_BASE}/leads/${id}`);
    if (!res.ok) return;
    const lead = await res.json();

    document.getElementById('detailChurchName').textContent = lead.nombreIglesia;
    document.getElementById('detailContent').innerHTML = `
      <div class="form-grid">
        <div><strong>Ciudad / País:</strong> ${escapeHtml(lead.ciudad)}, ${escapeHtml(lead.pais)}</div>
        <div><strong>Contacto:</strong> ${escapeHtml(lead.nombreContacto || 'N/D')} (${escapeHtml(lead.cargoContacto || 'Pastor')})</div>
        <div><strong>Teléfono:</strong> ${escapeHtml(lead.telefono || 'N/D')}</div>
        <div><strong>Correo:</strong> ${escapeHtml(lead.email || 'N/D')}</div>
        <div><strong>Miembros:</strong> ${lead.cantidadMiembros || 'N/D'} | <strong>Sedes:</strong> ${lead.numeroSedes}</div>
        <div><strong>Herramienta Actual:</strong> ${escapeHtml(lead.herramientaActual || 'Excel/Papel')}</div>
        <div class="form-group full-width"><strong>Dolor Principal:</strong> ${escapeHtml(lead.dolorPrincipal || 'No especificado')}</div>
        <div class="form-group full-width"><strong>Notas:</strong> ${escapeHtml(lead.notas || 'Sin notas')}</div>
      </div>
      <h4 style="margin-top:1.25rem;margin-bottom:0.5rem;font-size:0.9rem;color:var(--cyan);">Historial de Interacciones</h4>
      <div style="max-height:180px;overflow-y:auto;font-size:0.8rem;background:var(--bg-main);padding:0.75rem;border-radius:6px;">
        ${lead.interacciones?.map(i => `
          <div style="border-bottom:1px solid rgba(255,255,255,0.05);padding:0.3rem 0;">
            <span style="color:var(--text-muted);">${new Date(i.fecha).toLocaleDateString()}</span> - 
            <strong>${escapeHtml(i.titulo)}:</strong> ${escapeHtml(i.detalle)}
          </div>
        `).join('') || '<p style="color:var(--text-muted);">Sin interacciones registradas.</p>'}
      </div>
    `;

    document.getElementById('leadDetailModal').classList.remove('hidden');
  } catch (err) {
    console.error('Error obteniendo detalle:', err);
  }
}

function closeDetailModal() {
  document.getElementById('leadDetailModal').classList.add('hidden');
}

// Exportar Handoff STARNET HUB
async function exportHandoff(id) {
  try {
    const res = await fetch(`${API_BASE}/leads/${id}/starnet-hub-export`);
    if (res.ok) {
      const data = await res.json();
      document.getElementById('handoffContent').value = data.handoff;
      document.getElementById('handoffModal').classList.remove('hidden');
    }
  } catch (err) {
    console.error('Error generando Handoff:', err);
  }
}

function copyHandoffToClipboard() {
  const text = document.getElementById('handoffContent');
  text.select();
  navigator.clipboard.writeText(text.value);
  alert('¡Expediente de STARNET HUB copiado al portapapeles!');
}

function closeHandoffModal() {
  document.getElementById('handoffModal').classList.add('hidden');
}

// Modal Nuevo Lead
function openNewLeadModal() {
  document.getElementById('newLeadForm').reset();
  document.getElementById('newLeadModal').classList.remove('hidden');
}

function closeNewLeadModal() {
  document.getElementById('newLeadModal').classList.add('hidden');
}

async function saveNewLead(e) {
  e.preventDefault();
  const form = document.getElementById('newLeadForm');
  const payload = {
    nombreIglesia: form.nombreIglesia.value,
    pais: form.pais.value || 'Colombia',
    ciudad: form.ciudad.value || 'Colombia',
    nombreContacto: form.nombreContacto.value,
    cargoContacto: form.cargoContacto.value || 'Pastor',
    telefono: form.telefono.value,
    email: form.email.value,
    cantidadMiembros: form.cantidadMiembros.value ? parseInt(form.cantidadMiembros.value) : null,
    numeroSedes: form.numeroSedes.value ? parseInt(form.numeroSedes.value) : 1,
    herramientaActual: form.herramientaActual.value,
    dolorPrincipal: form.dolorPrincipal.value,
    productoInteres: parseInt(form.productoInteres.value),
    estadoPipeline: 0,
    origen: 6, // Manual
    notas: form.notas.value
  };

  try {
    const res = await fetch(`${API_BASE}/leads`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      closeNewLeadModal();
      await loadLeads();
      await loadMetrics();
      alert('Lead creado exitosamente.');
    } else {
      const err = await res.json();
      alert(`Error: ${err.message}`);
    }
  } catch (err) {
    console.error('Error guardando lead:', err);
  }
}

// Modal Importar Masivo (Scraper)
function openBulkModal() {
  document.getElementById('bulkJsonInput').value = JSON.stringify([
    {
      nombreIglesia: "Iglesia Misionera La Esperanza",
      ciudad: "Bucaramanga",
      telefono: "3189991122",
      nombreContacto: "Pastor Marcos Benítez",
      cantidadMiembros: 140,
      productoInteres: 0,
      origen: 5
    },
    {
      nombreIglesia: "Comunidad de Fe y Vida",
      ciudad: "Pereira",
      telefono: "3015557788",
      nombreContacto: "Pastor Samuel Torres",
      cantidadMiembros: 210,
      productoInteres: 1,
      origen: 0
    }
  ], null, 2);
  document.getElementById('bulkModal').classList.remove('hidden');
}

function closeBulkModal() {
  document.getElementById('bulkModal').classList.add('hidden');
}

async function executeBulkImport() {
  const jsonText = document.getElementById('bulkJsonInput').value;
  try {
    const parsed = JSON.parse(jsonText);
    const res = await fetch(`${API_BASE}/leads/bulk`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(parsed)
    });

    if (res.ok) {
      const result = await res.json();
      alert(`Importación completada:\n• Insertados: ${result.totalInsertados}\n• Omitidos (duplicados/inválidos): ${result.totalDuplicadosOmitidos}`);
      closeBulkModal();
      await loadLeads();
      await loadMetrics();
    } else {
      alert('Error en la importación de datos.');
    }
  } catch (e) {
    alert('JSON inválido: Por favor revise la sintaxis.');
  }
}

// Helper para detectar teléfono móvil
function isMobilePhone(phone, pais) {
  if (!phone) return false;
  const digits = phone.replace(/[^\d]/g, '');
  if (!pais || pais.toLowerCase().includes('colombia')) {
    if (digits.startsWith('573') && digits.length === 12) return true;
    if (digits.startsWith('3') && digits.length === 10) return true;
  }
  return digits.length >= 10;
}

// ==========================================
// CRUD: EDITAR IGLESIA
// ==========================================
async function openEditLeadModal(id) {
  try {
    const res = await fetch(`${API_BASE}/leads/${id}`);
    if (!res.ok) {
      alert('No se pudo cargar la información de la iglesia.');
      return;
    }
    const lead = await res.json();

    document.getElementById('editLeadId').value = lead.id;
    document.getElementById('editNombreIglesia').value = lead.nombreIglesia || '';
    document.getElementById('editPais').value = lead.pais || 'Colombia';
    document.getElementById('editCiudad').value = lead.ciudad || '';
    document.getElementById('editDireccion').value = lead.direccion || '';
    document.getElementById('editTelefono').value = lead.telefono || '';
    document.getElementById('editEmail').value = lead.email || '';
    document.getElementById('editSitioWeb').value = lead.sitioWeb || '';
    document.getElementById('editRedesSociales').value = lead.redesSociales || '';
    document.getElementById('editNombreContacto').value = lead.nombreContacto || '';
    document.getElementById('editCargoContacto').value = lead.cargoContacto || '';
    document.getElementById('editProductoInteres').value = lead.productoInteres ?? 0;
    document.getElementById('editEstadoPipeline').value = lead.estadoPipeline ?? 0;
    document.getElementById('editPrioridad').value = lead.prioridad || 'Media';
    document.getElementById('editEsDecisor').value = lead.esDecisor ? 'true' : 'false';
    document.getElementById('editNotas').value = lead.notas || '';

    document.getElementById('editLeadModal').classList.remove('hidden');
  } catch (err) {
    console.error('Error al abrir modal de edición:', err);
  }
}

function closeEditLeadModal() {
  document.getElementById('editLeadModal').classList.add('hidden');
}

async function saveEditLead(e) {
  e.preventDefault();
  const id = document.getElementById('editLeadId').value;
  const payload = {
    nombreIglesia: document.getElementById('editNombreIglesia').value,
    pais: document.getElementById('editPais').value,
    ciudad: document.getElementById('editCiudad').value,
    direccion: document.getElementById('editDireccion').value,
    telefono: document.getElementById('editTelefono').value,
    email: document.getElementById('editEmail').value,
    sitioWeb: document.getElementById('editSitioWeb').value,
    redesSociales: document.getElementById('editRedesSociales').value,
    nombreContacto: document.getElementById('editNombreContacto').value,
    cargoContacto: document.getElementById('editCargoContacto').value,
    productoInteres: parseInt(document.getElementById('editProductoInteres').value),
    estadoPipeline: parseInt(document.getElementById('editEstadoPipeline').value),
    prioridad: document.getElementById('editPrioridad').value,
    esDecisor: document.getElementById('editEsDecisor').value === 'true',
    notas: document.getElementById('editNotas').value
  };

  try {
    const res = await fetch(`${API_BASE}/leads/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      closeEditLeadModal();
      await loadLeads();
      await loadMetrics();
      alert('Iglesia actualizada correctamente.');
    } else {
      const err = await res.json();
      alert(`Error al actualizar: ${err.message || 'Verifique los datos.'}`);
    }
  } catch (err) {
    console.error('Error actualizando iglesia:', err);
  }
}

function confirmDeleteFromEdit() {
  const id = document.getElementById('editLeadId').value;
  closeEditLeadModal();
  deleteLead(id);
}

// ==========================================
// CRUD: ELIMINAR IGLESIA
// ==========================================
async function deleteLead(id) {
  const lead = leadsData.find(l => l.id == id);
  const nombre = lead ? lead.nombreIglesia : `con ID ${id}`;
  if (!confirm(`¿Estás seguro de que deseas eliminar permanentemente la iglesia "${nombre}"?\nEsta acción no se puede deshacer.`)) {
    return;
  }

  try {
    const res = await fetch(`${API_BASE}/leads/${id}`, {
      method: 'DELETE'
    });

    if (res.ok) {
      await loadLeads();
      await loadMetrics();
      alert(`La iglesia "${nombre}" ha sido eliminada exitosamente.`);
    } else {
      alert('No se pudo eliminar el registro.');
    }
  } catch (err) {
    console.error('Error eliminando lead:', err);
  }
}

// ==========================================
// CALIFICACIÓN T-O-N-D-M (LLAMADA COMERCIAL)
// ==========================================
async function openTondmModal(leadId) {
  const lead = leadsData.find(l => l.id == leadId);
  if (!lead) return;

  document.getElementById('tondmLeadId').value = lead.id;
  document.getElementById('tondmChurchSubtitle').textContent = `${lead.nombreIglesia} · ${lead.ciudad || ''}, ${lead.pais || ''} (Tel: ${lead.telefono || 'Sin teléfono'})`;

  // Limpiar campos por defecto o precargar datos del lead
  document.getElementById('tondmUrgencia').value = 'Corto plazo (1 mes)';
  document.getElementById('tondmFechaTentativa').value = lead.fechaTentativaImplementacion || '';
  document.getElementById('tondmEventoProximo').checked = false;

  document.getElementById('tondmHerramienta').value = lead.herramientaActual || '';
  document.getElementById('tondmMiembros').value = lead.cantidadMiembros || '';
  document.getElementById('tondmSedes').value = lead.numeroSedes || 1;
  document.getElementById('tondmCelulas').checked = lead.tieneCelulas || false;
  document.getElementById('tondmCantidadCelulas').value = '';
  document.getElementById('tondmEscuela').checked = lead.tieneEscuelaFormacion || false;
  document.getElementById('tondmProcesosFuera').value = lead.procesosPorFuera || '';

  document.getElementById('tondmDolor').value = lead.dolorPrincipal || '';
  document.getElementById('tondmNivelDolor').value = 'Alto';
  document.getElementById('tondmDetalleNecesidad').value = '';

  document.getElementById('tondmNombreDecisor').value = lead.nombreContacto || '';
  document.getElementById('tondmCargoDecisor').value = lead.cargoContacto || '';
  document.getElementById('tondmDecisorPresente').checked = lead.esDecisor || false;
  document.getElementById('tondmProcesoDecision').value = lead.quienDecide || '';

  document.getElementById('tondmPresupuesto').value = lead.presupuestoEstimado || '';
  document.getElementById('tondmMoneda').value = 'COP';
  document.getElementById('tondmDisposicion').value = 'Media (Depende de cómo vean la Demo consultiva)';

  document.getElementById('tondmResultado').value = 'Demo Agendada';
  document.getElementById('tondmCalificadoPor').value = 'Comercial STARNET';
  document.getElementById('tondmNotas').value = '';

  // Consultar si ya tiene calificación TONDM previa guardada
  try {
    const res = await fetch(`${API_BASE}/leads/${leadId}/tondm`);
    if (res.ok) {
      const t = await res.json();
      if (t) {
        if (t.urgenciaImplementacion) document.getElementById('tondmUrgencia').value = t.urgenciaImplementacion;
        if (t.fechaTentativaImplementacion) document.getElementById('tondmFechaTentativa').value = t.fechaTentativaImplementacion;
        document.getElementById('tondmEventoProximo').checked = t.tieneEventoProximo || false;

        if (t.herramientaActual) document.getElementById('tondmHerramienta').value = t.herramientaActual;
        if (t.cantidadMiembros) document.getElementById('tondmMiembros').value = t.cantidadMiembros;
        if (t.numeroSedes) document.getElementById('tondmSedes').value = t.numeroSedes;
        document.getElementById('tondmCelulas').checked = t.tieneCelulas || false;
        if (t.cantidadCelulas) document.getElementById('tondmCantidadCelulas').value = t.cantidadCelulas;
        document.getElementById('tondmEscuela').checked = t.tieneEscuelaFormacion || false;
        if (t.procesosPorFuera) document.getElementById('tondmProcesosFuera').value = t.procesosPorFuera;

        if (t.dolorPrincipal) document.getElementById('tondmDolor').value = t.dolorPrincipal;
        if (t.nivelDolor) document.getElementById('tondmNivelDolor').value = t.nivelDolor;
        if (t.detalleNecesidad) document.getElementById('tondmDetalleNecesidad').value = t.detalleNecesidad;

        if (t.nombreDecisor) document.getElementById('tondmNombreDecisor').value = t.nombreDecisor;
        if (t.cargoDecisor) document.getElementById('tondmCargoDecisor').value = t.cargoDecisor;
        document.getElementById('tondmDecisorPresente').checked = t.decisorPresenteEnLlamada || false;
        if (t.procesoDecision) document.getElementById('tondmProcesoDecision').value = t.procesoDecision;

        if (t.presupuestoEstimado) document.getElementById('tondmPresupuesto').value = t.presupuestoEstimado;
        if (t.moneda) document.getElementById('tondmMoneda').value = t.moneda;
        if (t.disposicionInversion) document.getElementById('tondmDisposicion').value = t.disposicionInversion;

        if (t.resultadoLlamada) document.getElementById('tondmResultado').value = t.resultadoLlamada;
        if (t.calificadoPor) document.getElementById('tondmCalificadoPor').value = t.calificadoPor;
        if (t.notasLlamada) document.getElementById('tondmNotas').value = t.notasLlamada;
      }
    }
  } catch (ex) {
    // Si no hay previa, se mantiene con la precarga
  }

  document.getElementById('tondmModal').classList.remove('hidden');
}

function closeTondmModal() {
  document.getElementById('tondmModal').classList.add('hidden');
}

async function saveTondm(e) {
  e.preventDefault();
  const leadId = document.getElementById('tondmLeadId').value;
  const payload = {
    urgenciaImplementacion: document.getElementById('tondmUrgencia').value,
    fechaTentativaImplementacion: document.getElementById('tondmFechaTentativa').value,
    tieneEventoProximo: document.getElementById('tondmEventoProximo').checked,

    cantidadMiembros: document.getElementById('tondmMiembros').value ? parseInt(document.getElementById('tondmMiembros').value) : null,
    numeroSedes: document.getElementById('tondmSedes').value ? parseInt(document.getElementById('tondmSedes').value) : 1,
    tieneCelulas: document.getElementById('tondmCelulas').checked,
    cantidadCelulas: document.getElementById('tondmCantidadCelulas').value ? parseInt(document.getElementById('tondmCantidadCelulas').value) : null,
    tieneEscuelaFormacion: document.getElementById('tondmEscuela').checked,
    herramientaActual: document.getElementById('tondmHerramienta').value,
    procesosPorFuera: document.getElementById('tondmProcesosFuera').value,

    dolorPrincipal: document.getElementById('tondmDolor').value,
    nivelDolor: document.getElementById('tondmNivelDolor').value,
    detalleNecesidad: document.getElementById('tondmDetalleNecesidad').value,

    nombreDecisor: document.getElementById('tondmNombreDecisor').value,
    cargoDecisor: document.getElementById('tondmCargoDecisor').value,
    decisorPresenteEnLlamada: document.getElementById('tondmDecisorPresente').checked,
    procesoDecision: document.getElementById('tondmProcesoDecision').value,

    presupuestoEstimado: document.getElementById('tondmPresupuesto').value ? parseFloat(document.getElementById('tondmPresupuesto').value) : null,
    moneda: document.getElementById('tondmMoneda').value,
    disposicionInversion: document.getElementById('tondmDisposicion').value,

    resultadoLlamada: document.getElementById('tondmResultado').value,
    calificadoPor: document.getElementById('tondmCalificadoPor').value,
    notasLlamada: document.getElementById('tondmNotas').value,
    calificacionCompletada: true
  };

  try {
    const res = await fetch(`${API_BASE}/leads/${leadId}/tondm`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      closeTondmModal();
      await loadLeads();
      await loadMetrics();
      alert('¡Calificación T-O-N-D-M guardada con éxito!\nLos datos se sincronizaron con el expediente comercial.');
    } else {
      const err = await res.json();
      alert(`Error al guardar calificación: ${err.message || 'Verifique los campos.'}`);
    }
  } catch (err) {
    console.error('Error guardando TONDM:', err);
  }
}

// Helper escape HTML
function escapeHtml(text) {
  if (!text) return '';
  return text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");
}
