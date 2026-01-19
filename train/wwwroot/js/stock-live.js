(function () {
    if (!window.signalR) return;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/stock")
        .withAutomaticReconnect()
        .build();

    // Update any stock cell that declares data-product-id
    function updateStockCell(productId, newStock) {
        const rows = document.querySelectorAll(`[data-product-id='${productId}']`);
        rows.forEach(row => {
            const cell = row.querySelector(".stock-cell");
            if (cell) cell.textContent = newStock;
            // Optional: badge color if low stock
            if (row.classList) {
                row.classList.toggle("table-warning", Number(newStock) > 0 && Number(newStock) <= 5);
                row.classList.toggle("table-danger", Number(newStock) === 0);
            }
        });
    }

    connection.on("StockChanged", (productId, newStock) => {
        updateStockCell(productId, newStock);
        // Optional toast
        const toast = document.createElement("div");
        toast.className = "alert alert-info position-fixed top-0 end-0 m-3";
        toast.style.zIndex = 1080;
        toast.textContent = `Stock updated: #${productId} = ${newStock}`;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 2500);
    });

    connection.start().catch(console.error);
})();
