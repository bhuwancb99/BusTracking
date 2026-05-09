/* ================================================================
   BUS TRACKING SYSTEM – site.js
   ================================================================ */

(function () {
    'use strict';

    // ── Sidebar toggle ───────────────────────────────────────────
    const sidebar = document.getElementById('sidebar');
    const toggleBtn = document.getElementById('sidebarToggle');
    const COLLAPSED_KEY = 'sidebar_collapsed';

    function applySidebarState() {
        const collapsed = localStorage.getItem(COLLAPSED_KEY) === '1';
        if (sidebar) sidebar.classList.toggle('collapsed', collapsed);
    }

    if (toggleBtn && sidebar) {
        applySidebarState();
        toggleBtn.addEventListener('click', () => {
            sidebar.classList.toggle('collapsed');
            sidebar.classList.toggle('open');          // mobile
            localStorage.setItem(COLLAPSED_KEY,
                sidebar.classList.contains('collapsed') ? '1' : '0');
        });
    }

    // ── Mobile overlay close ─────────────────────────────────────
    document.addEventListener('click', (e) => {
        if (!sidebar) return;
        if (!sidebar.contains(e.target) && !toggleBtn?.contains(e.target)) {
            sidebar.classList.remove('open');
        }
    });

    // ── Auto-dismiss alerts ──────────────────────────────────────
    document.querySelectorAll('.alert[role="alert"]').forEach(alert => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 5000);
    });

    // ── Confirm delete (data-confirm attribute) ──────────────────
    document.querySelectorAll('form[data-confirm]').forEach(form => {
        form.addEventListener('submit', (e) => {
            if (!confirm(form.dataset.confirm || 'Are you sure?')) {
                e.preventDefault();
            }
        });
    });

    // ── Password show/hide toggle ────────────────────────────────
    document.querySelectorAll('[data-toggle-password]').forEach(btn => {
        btn.addEventListener('click', () => {
            const targetId = btn.dataset.togglePassword;
            const input = document.getElementById(targetId);
            const icon = btn.querySelector('i');
            if (!input) return;
            input.type = input.type === 'password' ? 'text' : 'password';
            if (icon) {
                icon.className = input.type === 'password'
                    ? 'bi bi-eye' : 'bi bi-eye-slash';
            }
        });
    });

    // ── AJAX JSON helper ─────────────────────────────────────────
    window.busTrack = {
        post: async function (url, data) {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const res = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token || ''
                },
                body: JSON.stringify(data)
            });
            return res.json();
        },

        showToast: function (message, type = 'success') {
            const container = document.getElementById('toastContainer')
                || (() => {
                    const d = document.createElement('div');
                    d.id = 'toastContainer';
                    d.className = 'toast-container position-fixed bottom-0 end-0 p-3';
                    d.style.zIndex = 9999;
                    document.body.appendChild(d);
                    return d;
                })();

            const colors = { success: 'bg-success', danger: 'bg-danger', warning: 'bg-warning', info: 'bg-info' };
            const toast = document.createElement('div');
            toast.className = `toast align-items-center text-white ${colors[type] || 'bg-secondary'} border-0`;
            toast.setAttribute('role', 'alert');
            toast.innerHTML = `
                <div class="d-flex">
                    <div class="toast-body">${message}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>`;
            container.appendChild(toast);
            const bsToast = new bootstrap.Toast(toast, { delay: 4000 });
            bsToast.show();
            toast.addEventListener('hidden.bs.toast', () => toast.remove());
        },

        // ── Confirm delete via AJAX ──────────────────────────────
        confirmDelete: async function (url, message, onSuccess) {
            if (!confirm(message || 'Are you sure you want to delete this?')) return;
            const result = await busTrack.post(url, {});
            if (result.success) {
                busTrack.showToast(result.message || 'Deleted.', 'success');
                if (typeof onSuccess === 'function') onSuccess();
            } else {
                busTrack.showToast(result.message || 'Error.', 'danger');
            }
        }
    };

    // ── Live bus tracking map (Leaflet) ──────────────────────────
    const mapEl = document.getElementById('busMap');
    if (mapEl && typeof L !== 'undefined') {
        const tripId = mapEl.dataset.tripId;
        const map = L.map('busMap').setView([20.5937, 78.9629], 13);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors'
        }).addTo(map);

        const busIcon = L.divIcon({
            className: '',
            html: '<i class="bi bi-bus-front-fill text-primary" style="font-size:28px"></i>',
            iconSize: [32, 32],
            iconAnchor: [16, 16]
        });

        let busMarker = null;

        async function refreshLocation() {
            if (!tripId) return;
            try {
                const res = await fetch(`/api/location/${tripId}/latest`);
                const data = await res.json();
                if (data.success && data.data) {
                    const { latitude, longitude } = data.data;
                    if (!busMarker) {
                        busMarker = L.marker([latitude, longitude], { icon: busIcon }).addTo(map);
                        map.setView([latitude, longitude], 15);
                    } else {
                        busMarker.setLatLng([latitude, longitude]);
                    }
                    document.getElementById('lastUpdated')?.textContent
                        && (document.getElementById('lastUpdated').textContent
                            = 'Updated: ' + new Date().toLocaleTimeString());
                }
            } catch (e) {
                console.warn('Location fetch failed', e);
            }
        }

        refreshLocation();
        setInterval(refreshLocation, 10000);   // poll every 10s
    }

    // ── Boarding status update (driver trip page) ────────────────
    document.querySelectorAll('.btn-boarding').forEach(btn => {
        btn.addEventListener('click', async function () {
            const tripId = this.dataset.tripId;
            const studentId = this.dataset.studentId;
            const stopId = this.dataset.stopId;
            const status = this.dataset.status;

            const result = await busTrack.post(
                `/api/trips/${tripId}/boarding`,
                { StudentId: studentId, StopId: stopId, BoardingStatus: status }
            );
            if (result.success) {
                busTrack.showToast(result.message, 'success');
                const row = document.getElementById(`student-row-${studentId}`);
                if (row) row.querySelector('.boarding-status').textContent = status;
            } else {
                busTrack.showToast(result.message, 'danger');
            }
        });
    });

})();
