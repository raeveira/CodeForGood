(function () {
    const sortKey = 'dealSort';
    const hiddenKey = 'dealHiddenStatuses';
    const script = document.currentScript;
    const activeSort = script.dataset.activeSort;
    const options = JSON.parse(script.dataset.sortOptions);
    const url = new URL(window.location.href);
    let hidden = [];
    let storageError = null;

    try {
        const savedSort = localStorage.getItem(sortKey);
        if (!url.searchParams.has('sort') && options.includes(savedSort) && savedSort !== activeSort) {
            document.documentElement.style.visibility = 'hidden';
            url.searchParams.set('sort', savedSort);
            window.location.replace(url.href);
            return;
        }

        localStorage.setItem(sortKey, activeSort);
        const storedHidden = JSON.parse(localStorage.getItem(hiddenKey) || '[]');
        if (!Array.isArray(storedHidden) || !storedHidden.every(status => typeof status === 'string')) {
            throw new Error('Invalid saved status filters.');
        }
        hidden = storedHidden;
    } catch (exception) {
        storageError = exception;
    }

    const secure = window.location.protocol === 'https:' ? '; Secure' : '';
    document.cookie = `dealSort=${encodeURIComponent(activeSort)}; Path=/; Max-Age=31536000; SameSite=Lax${secure}`;

    const initialFilter = document.createElement('style');
    initialFilter.textContent = hidden.map(status =>
        `.DealCard[data-status="${CSS.escape(status)}"] { display: none; }`).join('\n');
    document.head.appendChild(initialFilter);

    window.addEventListener('DOMContentLoaded', () => {
        const modal = document.getElementById('sortModal');
        const form = modal.querySelector('form');
        const filters = [...modal.querySelectorAll('.StatusFilter')];
        const selectAll = document.getElementById('statusSelectAll');
        const cards = [...document.querySelectorAll('.DealCard')];
        const emptyMessage = document.getElementById('noDealsMessage');
        const error = document.getElementById('sortStorageError');

        function reportStorageError(exception) {
            console.error('Unable to save or restore deal sorting and filters.', exception);
            error.textContent = 'Deal sorting and filters could not be saved or restored in this browser.';
            error.hidden = false;
        }

        function syncSelectAll() {
            const checked = filters.filter(filter => filter.checked).length;
            selectAll.checked = checked === filters.length;
            selectAll.indeterminate = checked > 0 && checked < filters.length;
        }

        function applyFilter() {
            let visible = 0;
            cards.forEach(card => {
                card.hidden = hidden.includes(card.dataset.status);
                if (!card.hidden) visible++;
            });
            emptyMessage.hidden = visible > 0 || cards.length === 0;
            filters.forEach(filter => filter.checked = !hidden.includes(filter.value));
            syncSelectAll();
        }

        if (storageError) reportStorageError(storageError);
        applyFilter();
        initialFilter.remove();

        selectAll.addEventListener('change', () => {
            filters.forEach(filter => filter.checked = selectAll.checked);
            syncSelectAll();
        });
        filters.forEach(filter => filter.addEventListener('change', syncSelectAll));

        form.addEventListener('submit', event => {
            const selectedSort = form.querySelector('input[name="sort"]:checked');
            try {
                localStorage.setItem(hiddenKey, JSON.stringify(filters.filter(filter => !filter.checked).map(filter => filter.value)));
                if (selectedSort) localStorage.setItem(sortKey, selectedSort.value);
            } catch (exception) {
                event.preventDefault();
                reportStorageError(exception);
            }
        });

        document.getElementById('openSortModal').addEventListener('click', () => modal.showModal());
        document.getElementById('closeSortModal').addEventListener('click', () => { applyFilter(); modal.close(); });
        document.getElementById('cancelSortModal').addEventListener('click', () => { applyFilter(); modal.close(); });
    });
})();
