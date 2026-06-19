/* =====================================================
   NUVOX ADMIN — admin-dashboard.js (HTML5 DIALOG NATIVO)
   ===================================================== */

let allProducts = [];
let todasLasVentas = [];
let allUsuarios = [];
let allCupones = [];
let viewActual = 'resumen';

document.addEventListener('DOMContentLoaded', function () {
    const chartCanvas = document.getElementById('ventasChart');
    if (chartCanvas) {
        const ctx = chartCanvas.getContext('2d');
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'],
                datasets: [{
                    label: 'Ingresos ($)',
                    data: [120, 190, 150, 250, 220, 310, 240],
                    borderColor: '#ffc107',
                    backgroundColor: 'rgba(255, 193, 7, 0.15)',
                    borderWidth: 3, tension: 0.4, fill: true,
                    pointBackgroundColor: '#121212', pointBorderColor: '#ffc107', pointRadius: 4
                }]
            },
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } }
        });
    }

    const formPro = document.getElementById('formProducto');
    if (formPro) formPro.addEventListener('submit', guardarProducto);

    const formCup = document.getElementById('formCupon');
    if (formCup) formCup.addEventListener('submit', guardarCupon);

    cargarDatosGenerales();
    cargarArtistas();
});


// ─── LÓGICA DE PRODUCTOS Y ARTISTAS ───────────────────────

function cargarArtistas() {
    fetch('/Dashboard/ObtenerArtistas')
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                const select = document.getElementById('IdArtista');
                if (!select) return;

                select.innerHTML = '';
                res.data.forEach(a => {
                    select.innerHTML += `<option value="${a.idArtista}">${a.nombreArtista}</option>`;
                });
            }
        })
        .catch(e => console.log('Error cargando artistas:', e));
}

function abrirModalProducto() {
    document.getElementById('formProducto').reset();
    document.getElementById('IdProducto').value = '0';
    document.getElementById('modalTitleProducto').innerHTML = '<i class="fa-solid fa-cloud-arrow-up me-2"></i>Añadir Nuevo Producto';
    document.getElementById('modalProducto').showModal();
}

function editarProducto(id) {
    const p = allProducts.find(x => x.idPro == id || x.idProducto == id);
    if (!p) return;

    document.getElementById('IdProducto').value = p.idPro || p.idProducto;
    document.getElementById('NomPro').value = p.nomPro || p.nombreProducto;
    document.getElementById('IdCate').value = p.idCate || p.idCategoria;
    document.getElementById('Precio').value = p.precio;
    document.getElementById('RutImag').value = p.rutImag || p.rutaImagen || '';
    document.getElementById('Descripcion').value = p.descripcion || '';
    document.getElementById('Activo').checked = p.activo;

    if (p.idArtista) document.getElementById('IdArtista').value = p.idArtista;

    document.getElementById('modalTitleProducto').innerHTML = '<i class="fa-solid fa-pen-to-square me-2"></i>Editar Producto';
    document.getElementById('modalProducto').showModal();
}

function guardarProducto(e) {
    e.preventDefault();

    const btnSubmit = e.target.querySelector('button[type="submit"]');
    const textoOriginal = btnSubmit.innerHTML;
    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-2"></i> SUBIENDO ARCHIVOS...';

    const data = new FormData(document.getElementById('formProducto'));

    if (!document.getElementById('Activo').checked) {
        data.append('Activo', 'false');
    }

    fetch('/Dashboard/GuardarProducto', { method: 'POST', body: data })
        .then(r => {
            if (!r.ok) throw new Error("Código " + r.status + " en el servidor.");
            return r.json();
        })
        .then(res => {
            btnSubmit.disabled = false;
            btnSubmit.innerHTML = textoOriginal;

            // 🔥 CIERRE OBLIGATORIO: Se ejecuta siempre, haya éxito o error
            document.getElementById('modalProducto').close();

            if (res.success) {
                Swal.fire({ icon: 'success', title: '¡Pack Guardado!', background: '#121212', color: '#fff', timer: 1500, showConfirmButton: false });
                cargarDatosGenerales();
                setTimeout(() => mostrarView('viewProductos', 'linkProductos'), 500);
            } else {
                Swal.fire({ icon: 'error', title: 'Error en BD', text: res.message, background: '#121212', color: '#fff' });
            }
        })
        .catch(err => {
            btnSubmit.disabled = false;
            btnSubmit.innerHTML = textoOriginal;
            // 🔥 CIERRE OBLIGATORIO: Si hay error de red
            document.getElementById('modalProducto').close();
            Swal.fire({ icon: 'error', title: 'Fallo de Conexión', text: err.message, background: '#121212', color: '#fff' });
        });
}

function eliminarProducto(id) {
    Swal.fire({ title: '¿Eliminar Producto?', icon: 'warning', showCancelButton: true, confirmButtonColor: '#dc3545', background: '#121212', color: '#fff' })
        .then((result) => {
            if (result.isConfirmed) {
                fetch(`/Dashboard/EliminarProducto?id=${id}`, { method: 'POST' })
                    .then(r => r.json())
                    .then(res => { if (res.success) { cargarDatosGenerales(); setTimeout(() => mostrarView('viewProductos', 'linkProductos'), 500); } });
            }
        });
}


// ─── LÓGICA DE CUPONES ───────────────────────────────────────
function abrirModalCupon() {
    document.getElementById('formCupon').reset();
    document.getElementById('IdCupon').value = '0';
    document.getElementById('modalCupon').showModal();
}

function guardarCupon(e) {
    e.preventDefault();
    const data = new FormData(document.getElementById('formCupon'));

    fetch('/Dashboard/GuardarCupon', { method: 'POST', body: data })
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                document.getElementById('modalCupon').close();
                Swal.fire({ icon: 'success', title: 'Guardado', background: '#121212', color: '#fff', timer: 1500, showConfirmButton: false });
                cargarCupones();
            }
        });
}

function cargarCupones() {
    fetch('/Dashboard/ObtenerCupones')
        .then(r => r.json())
        .then(res => {
            allCupones = res.data || [];
            let tbody = document.getElementById('tbCuponesBody');
            if (allCupones.length === 0) {
                if (tbody) tbody.innerHTML = '';
                document.getElementById('emptyCuponesMsg').style.display = 'block';
                return;
            }
            document.getElementById('emptyCuponesMsg').style.display = 'none';
            if (tbody) tbody.innerHTML = allCupones.map(c => `
                <tr style="border-bottom: 1px solid rgba(255,255,255,0.05);">
                    <td class="fw-bold fs-5 px-4 text-warning">${c.codigo}</td>
                    <td class="text-white fw-bold">-${c.porcentaje}% OFF</td>
                    <td class="text-muted">${c.fechaExpiracion}</td>
                    <td><span class="badge bg-success">Activo</span></td>
                    <td class="text-end px-4">
                        <button class="btn btn-sm btn-outline-danger border-0 fs-5 p-2 rounded-circle" onclick="eliminarCupon(${c.idCupon})"><i class="fa-solid fa-trash-can"></i></button>
                    </td>
                </tr>`).join('');
        });
}

function eliminarCupon(id) {
    Swal.fire({ title: '¿Eliminar Cupón?', icon: 'warning', showCancelButton: true, confirmButtonColor: '#dc3545', background: '#121212', color: '#fff' })
        .then((result) => {
            if (result.isConfirmed) {
                fetch(`/Dashboard/EliminarCupon?id=${id}`, { method: 'POST' })
                    .then(r => r.json())
                    .then(res => { if (res.success) cargarCupones(); });
            }
        });
}


// ─── LÓGICA DE USUARIOS ──────────────────────────────────────
function cargarUsuarios() {
    fetch('/Dashboard/ObtenerUsuarios')
        .then(r => r.json())
        .then(res => {
            allUsuarios = res.data || [];
            let tbody = document.getElementById('tbUsuariosBody');
            if (allUsuarios.length === 0) {
                if (tbody) tbody.innerHTML = '';
                document.getElementById('emptyUsuariosMsg').style.display = 'block';
                return;
            }
            document.getElementById('emptyUsuariosMsg').style.display = 'none';
            if (tbody) tbody.innerHTML = allUsuarios.map(u => `
                <tr style="border-bottom: 1px solid rgba(255,255,255,0.05);">
                    <td class="px-4 fw-bold text-white"><i class="fa-solid fa-user-circle me-2 text-muted"></i>${u.nombres} ${u.apellidos}</td>
                    <td class="text-muted">${u.correo}</td>
                    <td><span class="badge ${u.activo ? 'bg-success' : 'bg-danger'}">${u.activo ? 'Activo' : 'Bloqueado'}</span></td>
                    <td class="text-end px-4">
                        <button class="btn btn-sm ${u.activo ? 'btn-outline-danger' : 'btn-outline-success'} fw-bold" onclick="cambiarEstadoUsuario(${u.idUsuario}, ${!u.activo})">
                            <i class="fa-solid ${u.activo ? 'fa-ban' : 'fa-check'} me-1"></i> ${u.activo ? 'Bloquear' : 'Activar'}
                        </button>
                    </td>
                </tr>`).join('');
        });
}

function cambiarEstadoUsuario(id, estadoNuevo) {
    Swal.fire({ title: '¿Cambiar acceso del usuario?', icon: 'warning', showCancelButton: true, confirmButtonColor: '#ffc107', background: '#121212', color: '#fff' })
        .then((result) => {
            if (result.isConfirmed) {
                fetch(`/Dashboard/CambiarEstadoUsuario?id=${id}&estado=${estadoNuevo}`, { method: 'POST' })
                    .then(r => r.json())
                    .then(res => { if (res.success) cargarUsuarios(); });
            }
        });
}


// ─── NAVEGACIÓN Y CARGAS GENERALES (SPA) ─────────────────────
function mostrarView(viewId, linkId) {
    viewActual = viewId.replace('view', '').toLowerCase();

    ['viewResumen', 'viewVentas', 'viewCategorias', 'viewProductos', 'viewCupones', 'viewUsuarios'].forEach(id => {
        let el = document.getElementById(id);
        if (el) el.style.display = 'none';
    });

    document.querySelectorAll('.sidebar-nav .nav-item').forEach(l => l.classList.remove('active'));
    document.getElementById(viewId).style.display = 'block';
    document.getElementById(linkId).classList.add('active');

    if (viewId === 'viewVentas') cargarReporte();
    if (viewId === 'viewCategorias') renderizarCajasCategorias(allProducts);
    if (viewId === 'viewProductos') {
        const titleTable = document.getElementById('txtTituloTabla');
        if (titleTable) titleTable.innerText = "Todos los Productos";
        renderizarTablaProductos(allProducts);
    }
    if (viewId === 'viewUsuarios') cargarUsuarios();
    if (viewId === 'viewCupones') cargarCupones();
}

function confirmarLogoutAdmin(e) {
    e.preventDefault();
    Swal.fire({
        title: '¿Salir del Panel?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#ffc107', cancelButtonColor: '#343a40',
        confirmButtonText: 'Sí, cerrar sesión', background: '#121212', color: '#fff'
    }).then((res) => { if (res.isConfirmed) window.location.href = '/Auth/Logout'; });
}

function cargarDatosGenerales() {
    fetch('/Dashboard/ObtenerProductos')
        .then(r => r.json())
        .then(json => { allProducts = json.data || []; })
        .catch(e => console.log('Esperando conexión...'));
}

function cargarReporte() {
    const i = document.getElementById('fechaInicio')?.value || '';
    const f = document.getElementById('fechaFin')?.value || '';

    fetch(`/Dashboard/ObtenerReporteVentas?inicio=${i}&fin=${f}`)
        .then(r => r.json())
        .then(res => {
            todasLasVentas = res.data || [];
            let tbody = document.getElementById('tbVentasBody');
            if (todasLasVentas.length === 0) {
                if (tbody) tbody.innerHTML = '';
                document.getElementById('emptyVentasMsg').style.display = 'block';
                return;
            }
            document.getElementById('emptyVentasMsg').style.display = 'none';
            if (tbody) tbody.innerHTML = todasLasVentas.map(v => `
                <tr style="border-bottom: 1px solid rgba(255,255,255,0.05);">
                    <td class="px-4 fw-bold" style="color: var(--amber);">#NUV-${(v.idVenta || 0).toString().padStart(5, '0')}</td>
                    <td class="text-muted small">${v.fecha}</td>
                    <td class="text-white fw-bold">${v.cliente}</td>
                    <td class="text-end px-4 fw-bold text-white">$${parseFloat(v.total).toFixed(2)}</td>
                </tr>`).join('');
        });
}

function buscarGlobal() {
    const q = document.getElementById('txtBuscarCat').value.trim().toLowerCase();
    const filtrados = allProducts.filter(p => {
        const nombrePro = (p.nombreProducto || p.nomPro || '').toLowerCase();
        const nombreCat = (p.nombreCategoria || p.nomCat || '').toLowerCase();
        return nombrePro.includes(q) || nombreCat.includes(q);
    });

    if (viewActual === 'categorias') renderizarCajasCategorias(filtrados);
    else if (viewActual === 'productos') renderizarTablaProductos(filtrados);
}

function renderizarCajasCategorias(productos) {
    const grid = document.getElementById('contenedorCategorias');
    if (!grid) return;
    grid.innerHTML = '';

    if (!productos || productos.length === 0) {
        grid.innerHTML = '<div class="col-12 text-center py-5"><p class="text-muted">No se encontraron productos.</p></div>';
        return;
    }

    const grupos = productos.reduce((acc, p) => {
        const cat = p.nombreCategoria || p.nomCat || 'General';
        if (!acc[cat]) acc[cat] = { count: 0 };
        acc[cat].count++;
        return acc;
    }, {});

    Object.keys(grupos).forEach(cat => {
        grid.innerHTML += `
            <div class="col-12 col-sm-6 col-md-4 col-xl-3">
                <div class="card h-100 border-0 shadow-sm" 
                     style="background-color: #151515; border-radius: 14px; cursor: pointer; transition: all 0.2s ease;" 
                     onclick="abrirCategoria('${cat}')"
                     onmouseover="this.style.backgroundColor='#1c1c1c'; this.style.transform='translateY(-4px)'"
                     onmouseout="this.style.backgroundColor='#151515'; this.style.transform='translateY(0)'">
                    <div class="card-body p-4 d-flex flex-column">
                        <div class="mb-3">
                            <i class="fa-solid fa-layer-group mb-2" style="color: var(--amber); font-size: 1.5rem;"></i>
                            <div class="text-white-50 small fw-bold">${grupos[cat].count} items</div>
                        </div>
                        <h5 class="text-white fw-bold mb-4" style="font-size: 1.15rem;">${cat}</h5>
                        <div class="mt-auto">
                            <span class="text-uppercase fw-bold" style="color: var(--amber); font-size: 0.75rem;">
                                VER PRODUCTOS <i class="fa-solid fa-arrow-right ms-1"></i>
                            </span>
                        </div>
                    </div>
                </div>
            </div>`;
    });
}

function abrirCategoria(nombreCat) {
    const filtrados = allProducts.filter(p => p.nombreCategoria === nombreCat || p.nomCat === nombreCat);
    const titleTable = document.getElementById('txtTituloTabla');
    if (titleTable) titleTable.innerText = `Categoría: ${nombreCat}`;
    mostrarView('viewProductos', 'linkCategorias');
    renderizarTablaProductos(filtrados);
}

function renderizarTablaProductos(items) {
    const container = document.getElementById('contenedorProductos');
    if (!container) return;
    if (items.length === 0) {
        container.innerHTML = '<div class="text-center py-5"><p class="text-muted">No hay productos en esta categoría.</p></div>';
        return;
    }
    const rows = items.map(p => `
        <tr style="border-bottom: 1px solid rgba(255,255,255,0.05);">
            <td class="px-4"><i class="${p.rutImag || p.rutaImagen || 'fa-solid fa-compact-disc'} fs-4" style="color: var(--amber);"></i></td>
            <td><div class="fw-bold text-white">${p.nomPro || p.nombreProducto}</div><div class="text-muted small">${p.nomCat || p.nombreCategoria || ''}</div></td>
            <td class="fw-bold" style="color: var(--amber);">$${p.precio.toFixed(2)}</td>
            <td><span class="badge ${p.activo ? 'bg-success' : 'bg-danger'}">${p.activo ? 'Activo' : 'Inactivo'}</span></td>
            <td class="text-end px-4">
                <button class="btn btn-sm btn-outline-info me-2" onclick="editarProducto(${p.idPro || p.idProducto})"><i class="fa-solid fa-pen-to-square"></i></button>
                <button class="btn btn-sm btn-outline-danger" onclick="eliminarProducto(${p.idPro || p.idProducto})"><i class="fa-solid fa-trash-can"></i></button>
            </td>
        </tr>`).join('');
    container.innerHTML = `<table class="table table-dark table-hover mb-0 align-middle table-transparent"><thead style="border-bottom: 2px solid var(--amber);"><tr><th class="py-3 px-4">Tipo</th><th>Nombre del Pack</th><th>Precio</th><th>Estado</th><th class="text-end px-4">Acciones</th></tr></thead><tbody>${rows}</tbody></table>`;
}