/* ============================================================
   FIDS Backoffice - Main JavaScript
   ============================================================ */

/* --- Char counter for Zone Name input --- */
function initCharCounter(inputSelector, counterSelector) {
    const input = document.querySelector(inputSelector);
    const counter = document.querySelector(counterSelector);
    if (!input || !counter) return;

    const max = input.getAttribute('maxlength') || 50;
    counter.textContent = input.value.length + ' / ' + max;
    input.addEventListener('input', function () {
        counter.textContent = this.value.length + ' / ' + max;
    });
}

/* --- SweetAlert2 confirm delete --- */
function confirmDelete(formId, message) {
    message = message || 'Are you sure you want to delete this item?';
    Swal.fire({
        title: 'Confirm Delete',
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#f4516c',
        cancelButtonColor: '#e4e6ef',
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        reverseButtons: true,
        customClass: {
            cancelButton: 'text-dark'
        }
    }).then(function (result) {
        if (result.isConfirmed) {
            document.getElementById(formId).submit();
        }
    });
    return false;
}

/* --- SweetAlert2 Toast notification --- */
function showToast(icon, title) {
    Swal.fire({
        toast: true,
        position: 'top-end',
        icon: icon,
        title: title,
        showConfirmButton: false,
        timer: 2500,
        timerProgressBar: true
    });
}

document.addEventListener('DOMContentLoaded', function () {
    initCharCounter('input[name="FlightNumber"]', '.char-count');
});
