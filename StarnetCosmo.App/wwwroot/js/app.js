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

      card.innerHTML = `
        <div class="lead-card-header">
          <div class="lead-church-name">${escapeHtml(lead.nombreIglesia)}</div>
          <span class="badge-product ${prodBadgeClass}">${prodName}</span>
        </div>
        <div class="lead-meta">
          <div>📍 ${escapeHtml(lead.ciudad || '')}, ${escapeHtml(lead.pais || 'Colombia')} ${lead.cantidadMiembros ? '· 👥 ' + lead.cantidadMiembros + ' m.' : ''}</div>
          <div>👤 ${escapeHtml(lead.nombreContacto || 'Pastor')} (${escapeHtml(lead.cargoContacto || 'Líder')})</div>
          ${lead.telefono ? '<div>📞 ' + escapeHtml(lead.telefono) + '</div>' : ''}
        </div>
        <div class="lead-footer-actions">
          <div style="display:flex;gap:0.3rem;">
            ${lead.telefono ? `<button class="btn btn-success btn-sm" onclick="openWhatsAppModal(${lead.id})">💬 WhatsApp</button>` : ''}
            <button class="btn btn-secondary btn-sm" onclick="viewLeadDetail(${lead.id})">📄 Ficha</button>
          </div>
          <div style="display:flex;gap:0.2rem;">
            ${stage.key > 0 ? `<button class="btn btn-secondary btn-sm" title="Retroceder" onclick="moveStage(${lead.id}, ${stage.key - 1})">◀</button>` : ''}
            ${stage.key < 7 ? `<button class="btn btn-primary btn-sm" title="Avanzar" onclick="moveStage(${lead.id}, ${stage.key + 1})">▶</button>` : ''}
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

    tr.innerHTML = `
      <td><strong>${escapeHtml(lead.nombreIglesia)}</strong></td>
      <td><strong>${escapeHtml(lead.pais || 'Colombia')}</strong> · <span style="color:var(--text-secondary);">${escapeHtml(lead.ciudad || '')}</span></td>
      <td>${escapeHtml(lead.nombreContacto || 'Pastor')}</td>
      <td>${escapeHtml(lead.telefono || 'Sin teléfono')}</td>
      <td><span class="badge-product ${getProductBadgeClass(lead.productoInteres)}">${prodName}</span></td>
      <td><span style="font-weight:600;">${stageName}</span></td>
      <td>
        <div style="display:flex;gap:0.3rem;">
          ${lead.telefono ? `<button class="btn btn-success btn-sm" onclick="openWhatsAppModal(${lead.id})">WhatsApp</button>` : ''}
          <button class="btn btn-secondary btn-sm" onclick="viewLeadDetail(${lead.id})">Ver</button>
          <button class="btn btn-secondary btn-sm" onclick="exportHandoff(${lead.id})">HUB</button>
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
