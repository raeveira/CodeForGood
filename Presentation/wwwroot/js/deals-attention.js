(function () {
    const main = document.querySelector('.HomeMain');
    const error = document.getElementById('attentionStorageError');
    const key = `dealAttention:${main.dataset.attentionUser.toLowerCase()}`;
    let seen = new Set();
    try {
        const stored = JSON.parse(localStorage.getItem(key) || '[]');
        if (!Array.isArray(stored) || !stored.every(value => typeof value === 'string')) {
            throw new Error('Invalid attention data in local storage.');
        }
        seen = new Set(stored);
    } catch (exception) {
        console.error('Unable to restore attention state.', exception);
        error.textContent = 'Attention read state could not be restored in this browser.';
        error.hidden = false;
    }

    if (main.dataset.selectedDeal) seen.add(`deal:${main.dataset.selectedDeal}`);
    const current = document.querySelector('[data-current-requirement-events]');
    if (current) {
        JSON.parse(current.dataset.currentRequirementEvents).forEach(id => seen.add(id));
    }

    try {
        localStorage.setItem(key, JSON.stringify([...seen]));
    } catch (exception) {
        console.error('Unable to save attention state.', exception);
        error.textContent = 'Attention read state could not be saved in this browser.';
        error.hidden = false;
    }

    document.querySelectorAll('[data-attention-events]').forEach(item => {
        const events = JSON.parse(item.dataset.attentionEvents);
        item.querySelector('.AttentionBadge').hidden =
            item.dataset.attentionPending !== 'true' && events.every(id => seen.has(id));
    });
})();
