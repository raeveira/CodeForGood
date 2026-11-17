(function () {
    const storageKey = 'dealsSidebarWidth';
    const minimumWidth = 240;
    const maximumWidth = 600;
    const minimumContentWidth = 450;
    let preferredWidth = 300;
    let storageError = null;

    try {
        const saved = localStorage.getItem(storageKey);
        if (saved !== null && Number.isFinite(Number(saved)) && Number(saved) > 0) {
            preferredWidth = Number(saved);
        }
    } catch (exception) {
        storageError = exception;
    }

    const initialWidth = Math.round(Math.max(minimumWidth, Math.min(maximumWidth, preferredWidth)));
    document.documentElement.style.setProperty('--sidebar-width', `${initialWidth}px`);

    window.addEventListener('DOMContentLoaded', () => {
        const main = document.querySelector('.HomeMain');
        const handle = document.getElementById('sidebarResizeHandle');
        const error = document.getElementById('sidebarResizeError');
        const mobile = window.matchMedia('(max-width: 768px)');
        let drag = null;

        function reportStorageError(exception) {
            console.error('Unable to save or restore the deals list width.', exception);
            error.textContent = 'The deals list width could not be saved in this browser.';
            error.hidden = false;
        }

        if (storageError) reportStorageError(storageError);

        function maxWidth() {
            return Math.max(minimumWidth, Math.min(maximumWidth,
                main.clientWidth - handle.offsetWidth - minimumContentWidth));
        }

        function renderWidth() {
            const max = maxWidth();
            const width = Math.round(Math.max(minimumWidth, Math.min(max, preferredWidth)));
            main.style.setProperty('--sidebar-width', `${width}px`);
            handle.setAttribute('aria-valuemax', max);
            handle.setAttribute('aria-valuenow', width);
            handle.setAttribute('aria-valuetext', `${width} pixels`);
            return width;
        }

        function saveWidth() {
            try {
                localStorage.setItem(storageKey, String(preferredWidth));
                error.hidden = true;
            } catch (exception) {
                reportStorageError(exception);
            }
        }

        handle.addEventListener('pointerdown', event => {
            if (event.button !== 0 || mobile.matches) return;
            event.preventDefault();
            drag = { id: event.pointerId, startX: event.clientX, startWidth: renderWidth(), originalWidth: preferredWidth };
            handle.setPointerCapture(event.pointerId);
        });

        handle.addEventListener('pointermove', event => {
            if (!drag || event.pointerId !== drag.id) return;
            preferredWidth = drag.startWidth + event.clientX - drag.startX;
            preferredWidth = renderWidth();
        });

        handle.addEventListener('pointerup', event => {
            if (!drag || event.pointerId !== drag.id) return;
            drag = null;
            saveWidth();
        });

        handle.addEventListener('pointercancel', event => {
            if (!drag || event.pointerId !== drag.id) return;
            preferredWidth = drag.originalWidth;
            drag = null;
            renderWidth();
        });

        handle.addEventListener('keydown', event => {
            if (mobile.matches) return;
            const width = renderWidth();
            switch (event.key) {
                case 'ArrowLeft': preferredWidth = width - 10; break;
                case 'ArrowRight': preferredWidth = width + 10; break;
                case 'Home': preferredWidth = minimumWidth; break;
                case 'End': preferredWidth = maxWidth(); break;
                default: return;
            }
            event.preventDefault();
            preferredWidth = renderWidth();
            saveWidth();
        });

        window.addEventListener('resize', renderWidth);
        renderWidth();
    });
})();
