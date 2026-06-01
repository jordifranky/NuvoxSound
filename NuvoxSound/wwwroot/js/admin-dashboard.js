/* =====================================================
   NUVOX ADMIN — admin-dashboard.js
   ===================================================== */

let todasLasVentas = [];
let paginaActualVentas = 1;
const ventasPorPagina = 8;

document.addEventListener('DOMContentLoaded', function () {
    // Si la vista de ventas está visible al cargar, ejecutamos el reporte
    if (document.getElementById('viewVentas') && document.getElementById('viewVentas').style.display !== 'none') {
        cargarReporte();
    }
});

// ─── NAVEGACIÓN DE VISTAS (SPA) ──────────────────────────

function mostrarView(viewId, linkId) {
    // 1. Ocultar todas las vistas
    document.getElementById('viewResumen').style.display = 'none';
    document.getElementById('viewVentas').style.display = 'none';
    document.getElementById('viewCategorias').style.display = 'none';
    document.getElementById('viewProductos').style.display = 'none';

    // 2. Quitar clase 'active' del sidebar
    document.querySelectorAll('.sidebar-nav .nav-item').forEach(link => {
        link.classList.remove('active');
    });

    // 3. Mostrar la vista seleccionada y activar el link
    document.getElementById(viewId).style.display = 'block';
    document.getElementById(linkId).classList.add('active');

    // 4. Si entramos a Ventas, cargamos los datos automáticamente
    if (viewId === 'viewVentas') {
        cargarReporte();
    }
}

// ─── LÓGICA DE REPORTE DE VENTAS ─────────────────────────

function cargarReporte() {
    const inicio = document.getElementById('fechaInicio')?.value || '';
    const fin = document.getElementById('fechaFin')?.value || '';

    const tbody = document.getElementById('tbVentasBody');
    const emptyMsg = document.getElementById('emptyVentasMsg');

    // Estado de carga
    if (tbody) tbody.innerHTML = '<tr><td colspan="4" class="text-center py-4 text-muted"><i class="fa-solid fa-spinner fa-spin me-2"></i> Buscando transacciones...</td></tr>';
    if (emptyMsg) emptyMsg.style.display = 'none';

    // Llamada AJAX al controlador (Asegúrate de tener un endpoint que devuelva JSON)
    // Cambia la URL según el controlador que uses (ej. /Dashboard/ObtenerReporteVentas)
    fetch(`/Dashboard/ObtenerReporteVentas?inicio=${inicio}&fin=${fin}`)
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                todasLasVentas = res.data || [];
                paginaActualVentas = 1;
                renderizarTablaVentas();
            } else {
                if (tbody) tbody.innerHTML = '';
                Swal.fire('Atención', 'No se pudieron cargar las ventas o no hay datos.', 'info');
            }
        })
        .catch(err => {
            console.error("Error al cargar ventas:", err);
            if (tbody) tbody.innerHTML = '';
            Swal.fire('Error', 'Problema de conexión con el servidor.', 'error');
        });
}

function renderizarTablaVentas() {
    const tbody = document.getElementById('tbVentasBody');
    const emptyMsg = document.getElementById('emptyVentasMsg');

    if (!tbody) return;

    // Si NO hay ventas
    if (todasLasVentas.length === 0) {
        tbody.innerHTML = '';
        if (emptyMsg) emptyMsg.style.display = 'block';
        return;
    }

    // Si SÍ hay ventas
    if (emptyMsg) emptyMsg.style.display = 'none';

    const inicio = (paginaActualVentas - 1) * ventasPorPagina;
    const fin = inicio + ventasPorPagina;
    const items = todasLasVentas.slice(inicio, fin);

    tbody.innerHTML = items.map(v => `
        <tr style="border-bottom: 1px solid rgba(255,255,255,0.05); transition: 0.2s;" onmouseover="this.style.backgroundColor='rgba(255,255,255,0.02)'" onmouseout="this.style.backgroundColor='transparent'">
            <td class="px-4 fw-bold" style="color: var(--amber);">#NUV-${(v.idVenta || 0).toString().padStart(5, '0')}</td>
            <td class="text-muted small">${v.fecha}</td>
            <td>
                <div class="text-white fw-bold">${v.cliente}</div>
                <div class="text-muted small">${v.email}</div>
            </td>
            <td class="text-end px-4 fw-bold text-white">$${v.total.toFixed(2)}</td>
        </tr>
    `).join('');
}

// ─── CERRAR SESIÓN CON SWEETALERT ────────────────────────

function confirmarLogout(e) {
    if (e) e.preventDefault();

    Swal.fire({
        title: '¿Cerrar Sesión?',
        text: "Saldrás del panel de administración.",
        icon: 'warning',
        showCancelButton: true,
        background: '#0e1535',
        color: '#f5f0e8',
        confirmButtonColor: '#f0a500',
        cancelButtonColor: '#303030',
        confirmButtonText: '<span style="color:#000; font-weight:bold;">SÍ, SALIR</span>',
        cancelButtonText: 'CANCELAR',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            // Te redirige a la acción de C# que destruye la cookie
            window.location.href = '/Auth/Logout';
        }
    });
}