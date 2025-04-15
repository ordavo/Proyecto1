function InicializarFormularioTransacciones(urlObtenerCategorias) {
    if ($("#TipoOperacionId").length) {
        $("#TipoOperacionId").change(async function () {
            const valorSeleccionado = $(this).val();

            const respuesta = await fetch(urlObtenerCategorias, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ tipoOperacionId: valorSeleccionado }) // ✅ Mejor forma de enviar el dato
            });

            if (!respuesta.ok) {
                console.error("Error al obtener categorías:", await respuesta.text());
                return;
            }

            const json = await respuesta.json();
            console.log("Categorías recibidas:", json);

            if (!json || json.length === 0) {
                console.warn("No se recibieron categorías.");
                $("#CategoriaId").html('<option value="">No hay categorías disponibles</option>'); // ✅ Mensaje claro
                return;
            }

            const opciones = json.map(categoria => `<option value="${categoria.value}">${categoria.text}</option>`).join('');
            $("#CategoriaId").html(opciones);
        });
    }
}
