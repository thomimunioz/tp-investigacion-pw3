document.addEventListener("DOMContentLoaded", () => {
    const chatContainer = document.getElementById("chatContainer");
    const inputMensaje = document.getElementById("inputMensaje");
    const btnEnviar = document.getElementById("btnEnviar");
    const formChat = document.getElementById("formChat");

    function obtenerConversacionId() {
        const dataId = chatContainer?.dataset?.conversacionId;
        if (dataId && parseInt(dataId) > 0) return parseInt(dataId);

        const partes = window.location.pathname.split('/').filter(p => p.length > 0);
        const ultimo = partes[partes.length - 1];
        if (ultimo && !isNaN(parseInt(ultimo)) && parseInt(ultimo) > 0) return parseInt(ultimo);

        const params = new URLSearchParams(window.location.search);
        if (params.get("id")) return parseInt(params.get("id"));

        return 0;
    }

    function scrollAlFondo() {
        if (chatContainer) chatContainer.scrollTop = chatContainer.scrollHeight;
    }
    scrollAlFondo();

    if (formChat) {
        formChat.addEventListener("submit", async (e) => {
            e.preventDefault();
            const texto = inputMensaje.value.trim();
            if (!texto) return;

            const convId = obtenerConversacionId();
            if (convId <= 0) {
                alert("No se pudo detectar el número de conversación actual.");
                return;
            }

            // 1. Burbuja inmediata del usuario
            const hora = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            agregarBurbujaUsuario(texto, hora);
            inputMensaje.value = "";
            inputMensaje.disabled = true;
            btnEnviar.disabled = true;
            scrollAlFondo();

            const spinnerId = agregarIndicadorEscribiendo();

            try {
                const response = await fetch('/Chat/Enviar', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        conversacionId: convId,
                        mensaje: texto
                    })
                });

                const data = await response.json();
                removerIndicador(spinnerId);

                if (response.ok && data.ok) {
                    agregarBurbujaBot(data.respuesta, data.fecha);

                    if (data.estado === "Derivada" || data.estado === "Resuelta") {
                        bloquearChat(data.estado);
                    } else {
                        inputMensaje.disabled = false;
                        btnEnviar.disabled = false;
                        inputMensaje.focus();
                    }
                } else {
                    console.error("Respuesta no exitosa:", data);
                    alert(data.error || "Ocurrió un error en el servidor.");
                    inputMensaje.disabled = false;
                    btnEnviar.disabled = false;
                }
            } catch (err) {
                console.error("Error al conectar con /Chat/Enviar:", err);
                removerIndicador(spinnerId);
                alert("Error de conexión con el servidor.");
                inputMensaje.disabled = false;
                btnEnviar.disabled = false;
            }

            scrollAlFondo();
        });
    }

    function agregarBurbujaUsuario(texto, hora) {
        const div = document.createElement("div");
        div.className = "d-flex justify-content-end mb-3";
        div.innerHTML = `
            <div class="bg-primary text-white p-3 rounded-4 shadow-sm" style="max-width: 75%;">
                <p class="mb-1">${escapeHtml(texto)}</p>
                <small class="d-block text-end opacity-75" style="font-size: 0.75rem;">${hora}</small>
            </div>`;
        chatContainer.appendChild(div);
    }

    function agregarBurbujaBot(texto, hora) {
        const div = document.createElement("div");
        div.className = "d-flex justify-content-start mb-3";
        div.innerHTML = `
            <div class="bg-white border p-3 rounded-4 shadow-sm" style="max-width: 75%;">
                <small class="text-primary fw-bold d-block mb-1">🤖 Asistente</small>
                <p class="mb-1 text-dark">${escapeHtml(texto)}</p>
                <small class="d-block text-end text-muted" style="font-size: 0.75rem;">${hora}</small>
            </div>`;
        chatContainer.appendChild(div);
    }

    function agregarIndicadorEscribiendo() {
        const id = "spinner-" + Date.now();
        const div = document.createElement("div");
        div.id = id;
        div.className = "d-flex justify-content-start mb-3";
        div.innerHTML = `
            <div class="bg-white border p-3 rounded-4 shadow-sm text-muted">
                <small>🤖 El asistente está escribiendo...</small>
            </div>`;
        chatContainer.appendChild(div);
        scrollAlFondo();
        return id;
    }

    function removerIndicador(id) {
        const el = document.getElementById(id);
        if (el) el.remove();
    }

    function bloquearChat(estado) {
        inputMensaje.disabled = true;
        btnEnviar.disabled = true;
        inputMensaje.placeholder = "Conversación " + estado.toLowerCase();

        const badge = document.getElementById("badgeEstado");
        if (badge) {
            badge.textContent = estado;
            badge.className = "badge " + (estado === "Derivada" ? "bg-warning text-dark" : "bg-secondary");
        }

        const alerta = document.getElementById("alertaEstado");
        if (alerta) {
            alerta.className = "alert m-3 mb-0 " + (estado === "Derivada" ? "alert-warning" : "alert-secondary");
            alerta.innerHTML = estado === "Derivada"
                ? "<span>⚠️ Esta conversación fue derivada a un agente humano. El envío de mensajes quedó deshabilitado.</span>"
                : "<span>✓ Esta conversación fue finalizada.</span>";
        }
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
});
