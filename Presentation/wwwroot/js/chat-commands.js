(function () {
    const popup = document.getElementById('slashCommandPopup');
    if (!popup) return;

    const input = popup.parentElement.querySelector('textarea');
    const option = document.getElementById('deadlineCommand');
    const composer = document.getElementById('deadlineComposer');
    const date = document.getElementById('requestedDeadline');
    const cancel = document.getElementById('cancelDeadline');
    const form = input.closest('form');
    let previousText = '';

    function updatePopup() {
        const text = input.value.trimStart().toLowerCase();
        const isCommandPrefix = text.startsWith('/') && '/deadline'.startsWith(text);
        const isDeadlineArgument = /^\/deadline\s+[\d-]*$/.test(text);
        popup.hidden = !(isCommandPrefix || isDeadlineArgument);
    }

    function selectCommand() {
        previousText = input.value;
        const existingDate = input.value.trim().match(/^\/deadline\s+(\d{4}-\d{2}-\d{2})$/i);
        date.value = existingDate ? existingDate[1] : '';
        input.hidden = true;
        composer.hidden = false;
        date.disabled = false;
        popup.hidden = true;
        date.focus();
    }

    cancel.addEventListener('click', () => {
        date.disabled = true;
        composer.hidden = true;
        input.hidden = false;
        input.value = previousText;
        input.focus();
        popup.hidden = true;
    });

    form.addEventListener('submit', event => {
        if (date.disabled) return;
        if (!date.checkValidity()) {
            event.preventDefault();
            date.reportValidity();
            return;
        }
        input.value = `/deadline ${date.value}`;
    });

    input.addEventListener('input', updatePopup);
    input.addEventListener('focus', updatePopup);
    input.addEventListener('keydown', event => {
        if (popup.hidden || event.isComposing) return;
        if (event.key === 'Escape') {
            popup.hidden = true;
            event.preventDefault();
        } else if (event.key === 'Enter' && !event.shiftKey &&
            /^\/[a-z]*$/i.test(input.value.trim())) {
            event.preventDefault();
            selectCommand();
        }
    });
    option.addEventListener('click', selectCommand);
    popup.parentElement.addEventListener('focusout', event => {
        if (!popup.parentElement.contains(event.relatedTarget)) popup.hidden = true;
    });
})();
