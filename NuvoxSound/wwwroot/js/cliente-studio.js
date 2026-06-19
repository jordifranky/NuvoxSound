/* ========================================================
   NUVOX STUDIO - LÓGICA DEL CLIENTE
   ======================================================== */

// 1. FUNCIÓN PARA CAMBIAR ENTRE PESTAÑAS (SPA)
function mostrarVistaCliente(viewId, linkId) {
    // Ocultar todas las vistas del cliente
    document.getElementById('viewLibreria').style.display = 'none';
    document.getElementById('viewCompras').style.display = 'none';
    document.getElementById('viewPerfil').style.display = 'none';

    // Quitar clase 'active' de todos los botones del menú
    document.querySelectorAll('.sidebar-nav .nav-item').forEach(link => {
        link.classList.remove('active');
    });

    // Mostrar la vista seleccionada y marcar el botón como activo
    document.getElementById(viewId).style.display = 'block';
    document.getElementById(linkId).classList.add('active');
}

/* ========================================================
   LÓGICA DEL PANEL DE PERFIL Y LOCALIZACIÓN
   ======================================================== */
document.addEventListener('DOMContentLoaded', function () {

    // 2. CARGAMOS LA API DE PAÍSES (CDN Seguro)
    fetch('https://cdn.jsdelivr.net/gh/umpirsky/country-list@master/data/es/country.json')
        .then(response => {
            if (!response.ok) throw new Error("Error en la red");
            return response.json();
        })
        .then(data => {
            const selectPais = document.getElementById('selectPais');
            if (selectPais) {
                selectPais.innerHTML = '<option value="">Selecciona tu país...</option>';

                // Convertimos el JSON en un array y lo ordenamos alfabéticamente
                const paises = Object.entries(data).map(([code, name]) => ({ code, name }));
                paises.sort((a, b) => a.name.localeCompare(b.name));

                paises.forEach(country => {
                    const option = document.createElement('option');
                    option.value = country.code;
                    option.textContent = country.name;
                    if (country.code === 'PE') option.selected = true; // Perú por defecto
                    selectPais.appendChild(option);
                });
            }
        })
        .catch(error => {
            console.error('Error al cargar la API de países:', error);
            const selectPais = document.getElementById('selectPais');
            if (selectPais) {
                selectPais.innerHTML = '<option value="">Error al cargar países. Intenta recargar.</option>';
            }
        });

    // 3. FUNCIÓN PARA EL BOTÓN "GUARDAR CAMBIOS" (SweetAlert2)
    const formPerfil = document.getElementById('formPerfil');
    if (formPerfil) {
        formPerfil.addEventListener('submit', function (e) {
            e.preventDefault(); // Evitamos que la página se recargue

            // Mostramos animación de carga
            Swal.fire({
                title: 'Actualizando Datos...',
                text: 'Guardando tus preferencias de productor',
                allowOutsideClick: false,
                background: '#121212',
                color: '#fff',
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Simulamos el tiempo de respuesta del servidor (1 segundo)
            setTimeout(() => {
                Swal.fire({
                    icon: 'success',
                    title: '¡Perfil Actualizado!',
                    text: 'Tus datos se guardaron correctamente en la base de datos.',
                    background: '#121212',
                    color: '#fff',
                    confirmButtonColor: '#ffc107' // Color ámbar Nuvox
                });
                // Aquí en el futuro conectarás con tu Backend C#
            }, 1000);
        });
    }
});

/* ========================================================
   4. FUNCIÓN PARA EL BOTÓN "CERRAR SESIÓN"
   ======================================================== */
function confirmarLogoutCliente(e) {
    e.preventDefault(); // Detenemos la redirección automática

    Swal.fire({
        title: '¿Deseas cerrar sesión?',
        text: "Tendrás que volver a ingresar tus credenciales para descargar tus packs.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#ffc107',
        cancelButtonColor: '#343a40',
        confirmButtonText: 'Sí, salir de mi cuenta',
        cancelButtonText: 'Cancelar',
        background: '#121212',
        color: '#fff'
    }).then((result) => {
        if (result.isConfirmed) {
            // Si el usuario confirma, lo redirigimos a tu ruta real de C# para destruir la cookie
            window.location.href = '/Auth/Logout';
        }
    });
}