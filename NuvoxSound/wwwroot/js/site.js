/* =============================================
   1. LÓGICA DE LA MONEDA (USD / PEN)
   ============================================= */
const USD_PEN_RATE = 3.80; // Tipo de cambio

document.addEventListener("DOMContentLoaded", () => {
    const currencyOptions = document.querySelectorAll('.currency-option');

    currencyOptions.forEach(option => {
        option.addEventListener('click', function () {
            // Activar botón seleccionado
            currencyOptions.forEach(opt => opt.classList.remove('active'));
            this.classList.add('active');

            const selectedCurrency = this.getAttribute('data-curr');
            const priceElements = document.querySelectorAll('.product-price');

            // Actualizar todos los precios del catálogo
            priceElements.forEach(el => {
                let basePrice = parseFloat(el.getAttribute('data-usd-price'));

                if (!isNaN(basePrice)) {
                    if (selectedCurrency === 'PEN') {
                        let converted = (basePrice * USD_PEN_RATE).toFixed(2);
                        el.innerText = 'S/' + converted;
                    } else {
                        el.innerText = '$' + basePrice.toFixed(2);
                    }
                }
            });
        });
    });

    /* =============================================
       2. ANIMACIONES DE ONDAS (WAVEFORMS)
       ============================================= */
    document.querySelectorAll(".wave-bar").forEach(bar => {
        bar.style.animationDelay = `${(Math.random() * 1.2).toFixed(2)}s`;
        bar.style.animationDuration = `${(0.8 + Math.random() * 0.8).toFixed(2)}s`;
    });
});

/* =============================================
   3. QUICK VIEW MODAL
   ============================================= */
function openQuickView(id, name, cat, desc, priceUSD, coverClass) {
    const modal = document.getElementById("quickViewModal");
    const overlay = document.getElementById("modalOverlay");
    const content = document.getElementById("modalContent");

    const pricePEN = (priceUSD * USD_PEN_RATE).toFixed(2);
    const shortName = name.split(" ").slice(0, 2).join(" ").toUpperCase();

    // Inyectar HTML en el Modal (Incluyendo formulario real a C#)
    content.innerHTML = `
        <div class="modal-product">
            <div class="modal-cover ${coverClass}">
                <div class="cover-waveform mini">
                    ${Array.from({ length: 14 }, () => `<div class="wave-bar"></div>`).join("")}
                </div>
                <span class="cover-label-mini">${shortName}</span>
            </div>
            <div class="modal-info">
                <span class="modal-artist">${cat}</span>
                <h2 class="modal-name">${name}</h2>
                <p class="modal-desc">${desc}</p>

                <div class="modal-price-row">
                    <span class="modal-price">$${priceUSD.toFixed(2)} USD</span>
                    <span class="ms-3 text-muted" style="font-size: 0.9rem;">(aprox. S/${pricePEN} PEN)</span>
                </div>

                <form action="/Carrito/Agregar" method="post">
                    <input type="hidden" name="id" value="${id}" />
                    <button type="submit" class="btn-primary btn-full" onclick="closeModal()">
                        <i class="fa-solid fa-cart-plus"></i> Agregar al Carrito
                    </button>
                </form>
            </div>
        </div>
    `;

    // Asignar animaciones a las nuevas barras del modal
    content.querySelectorAll(".wave-bar").forEach(bar => {
        bar.style.animationDelay = `${(Math.random() * 1.2).toFixed(2)}s`;
        bar.style.animationDuration = `${(0.8 + Math.random() * 0.8).toFixed(2)}s`;
    });

    modal.classList.add("open");
    overlay.classList.add("open");
    document.body.style.overflow = "hidden"; // Bloquea scroll trasero
}

function closeModal() {
    document.getElementById("quickViewModal").classList.remove("open");
    document.getElementById("modalOverlay").classList.remove("open");
    document.body.style.overflow = ""; // Restaura scroll
}

// Cierre al dar clic fuera o en la X
const closeBtn = document.getElementById("modalClose");
const overlayE = document.getElementById("modalOverlay");
if (closeBtn) closeBtn.addEventListener("click", closeModal);
if (overlayE) overlayE.addEventListener("click", closeModal);

/* =====================================================
   NUVOX SOUND — site.js (Global Frontend)
   ===================================================== */
document.addEventListener('DOMContentLoaded', function () {

    // 1. Selector de Monedas (Visual Toggle)
    const currencySelector = document.getElementById('currencySelector');
    if (currencySelector) {
        const options = currencySelector.querySelectorAll('.currency-option');
        options.forEach(opt => {
            opt.addEventListener('click', function () {
                // Remove active de todas
                options.forEach(o => o.classList.remove('active'));
                // Agrega active a la seleccionada
                this.classList.add('active');

                const curr = this.getAttribute('data-curr');
                console.log("Moneda cambiada a:", curr);
                // Aquí podrías guardar la preferencia en LocalStorage o recargar precios
            });
        });
    }

    // 2. Dropdown de Idioma (Comportamiento Simple)
    const langLink = document.querySelector('.nav-item > .nav-link .fa-globe');
    if (langLink) {
        const parentItem = langLink.closest('.nav-item');
        const dropdown = parentItem.querySelector('.dropdown');

        if (dropdown) {
            parentItem.addEventListener('mouseenter', () => dropdown.style.display = 'block');
            parentItem.addEventListener('mouseleave', () => dropdown.style.display = 'none');
        }
    }
});

/* =========================================
   LÓGICA DEL CHATBOT NUVOX (CONEXIÓN C#)
   ========================================= */
function enviarMensajeChat() {
    const input = document.getElementById('chatInput');
    const mensaje = input.value.trim();
    if (!mensaje) return;

    const cajaMensajes = document.getElementById('chatMensajes');

    // 1. Dibuja el mensaje del usuario
    cajaMensajes.innerHTML += `
        <div class="chat-msg user-msg">
            <strong>Tú:</strong> ${mensaje}
        </div>`;

    input.value = '';
    cajaMensajes.scrollTop = cajaMensajes.scrollHeight;

    // 2. Dibuja el estado "Escribiendo..."
    const idEscribiendo = "escribiendo_" + Date.now();
    cajaMensajes.innerHTML += `<div id="${idEscribiendo}" style="color:#aaa; font-size:12px; align-self:flex-start;">Watson está pensando...</div>`;
    cajaMensajes.scrollTop = cajaMensajes.scrollHeight;

    // 3. Envía el texto a tu Backend C#
    fetch(`/Chat/EnviarMensaje?mensaje=${encodeURIComponent(mensaje)}`, { method: 'POST' })
        .then(res => res.json())
        .then(data => {
            // Elimina el "Escribiendo..."
            const elementoEscribiendo = document.getElementById(idEscribiendo);
            if (elementoEscribiendo) elementoEscribiendo.remove();

            // Dibuja la respuesta final que devuelve el C#
            cajaMensajes.innerHTML += `
                <div class="chat-msg watson-msg">
                    <strong>Watson:</strong> ${data.respuesta}
                </div>`;
            cajaMensajes.scrollTop = cajaMensajes.scrollHeight;
        })
        .catch(err => {
            const elementoEscribiendo = document.getElementById(idEscribiendo);
            if (elementoEscribiendo) elementoEscribiendo.remove();
            cajaMensajes.innerHTML += `<div style="color:red; font-size:12px; align-self:center;">Error al conectar con el servidor interno.</div>`;
        });
}