document.addEventListener("DOMContentLoaded", function () {
    const ticketRadios = Array.from(document.querySelectorAll('input[name="TicketTypeId"]'));
    const qtyInput = document.getElementById("qtyInput");
    const promoRadios = Array.from(document.querySelectorAll('input[name="PromotionId"]'));
    const promoItems = Array.from(document.querySelectorAll('.promotion-item'));
    const elPrice = document.getElementById("summaryPrice");
    const elQty = document.getElementById("summaryQty");
    const elSubtotal = document.getElementById("summarySubtotal");
    const elDiscount = document.getElementById("summaryDiscount");
    const elTotal = document.getElementById("summaryTotal");

    function parseNumber(v) {
        return isNaN(parseFloat(v)) ? 0 : parseFloat(v);
    }

    function formatCurrency(amount) {
        return new Intl.NumberFormat(undefined, { style: "currency", currency: "USD" }).format(amount);
    }

    function getSelectedTicketData() {
        const selected = ticketRadios.find(r => r.checked);
        if (!selected) return { price: 0, seats: 0 };
        return {
            price: parseNumber(selected.dataset.price),
            seats: parseInt(selected.dataset.seats || "0", 10),
            id: selected.value
        };
    }

    function getSelectedPromoDiscount() {
        const selectedPromo = promoRadios.find(r => r.checked && r.offsetParent !== null); // only visible
        if (!selectedPromo) return 0;
        return parseNumber(selectedPromo.dataset.discount || "0");
    }

    function filterPromotions() {
        const selectedTicket = getSelectedTicketData().id;
        promoItems.forEach(item => {
            if (item.dataset.ticketType === selectedTicket) {
                item.style.display = "flex";
            } else {
                item.style.display = "none";
                // uncheck hidden promotions
                const input = item.querySelector("input");
                if (input) input.checked = false;
            }
        });
    }

    function updateSummary() {
        const { price, seats } = getSelectedTicketData();
        const qty = Math.max(1, parseInt(qtyInput.value || "1", 10));
        const discountPct = getSelectedPromoDiscount();
        const subtotal = price * qty;
        const discountAmount = Math.round((subtotal * (discountPct / 100)) * 100) / 100;
        const total = Math.round((subtotal - discountAmount) * 100) / 100;

        elPrice.textContent = price ? formatCurrency(price) : "—";
        elQty.textContent = qty;
        elSubtotal.textContent = subtotal ? formatCurrency(subtotal) : "—";
        elDiscount.textContent = discountPct ? `${discountPct}% (-${formatCurrency(discountAmount)})` : "—";
        elTotal.textContent = total ? formatCurrency(total) : "—";
    }

    // Event listeners
    ticketRadios.forEach(r => r.addEventListener("change", () => {
        filterPromotions();
        updateSummary();
    }));
    promoRadios.forEach(r => r.addEventListener("change", updateSummary));
    if (qtyInput) qtyInput.addEventListener("input", updateSummary);

    // Initial selection: first ticket + "no promotion"
    if (ticketRadios.length && !ticketRadios.some(r => r.checked)) {
        ticketRadios[0].checked = true;
    }
    filterPromotions();
    if (promoRadios.length && !promoRadios.some(r => r.checked && r.offsetParent !== null)) {
        promoRadios[promoRadios.length - 1].checked = true; // default: "No promotion"
    }

    updateSummary();
});
