document.addEventListener("DOMContentLoaded", function () {
    console.log("Booking page loaded.");

    // Quantity validation (example)
    const qtyInput = document.getElementById("qtyInput");

    if (qtyInput) {
        qtyInput.addEventListener("change", function () {
            if (qtyInput.value < 1) {
                qtyInput.value = 1;
            }
        });
    }

    // Promotion change event (example)
    const promoSelect = document.getElementById("promoSelect");

    if (promoSelect) {
        promoSelect.addEventListener("change", function () {
            console.log("Selected Promotion ID:", promoSelect.value);
        });
    }
});
