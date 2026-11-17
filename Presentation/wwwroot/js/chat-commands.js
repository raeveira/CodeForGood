(function () {
    const popup = document.getElementById('slashCommandPopup');
    if (!popup) return;

    const input = popup.parentElement.querySelector('textarea');
    const option = document.getElementById('deadlineCommand');

    function updatePopup() {
        const text = input.value.trimStart().toLowerCase();
        const isCommandPrefix = text.startsWith('/') && '/deadline'.startsWith(text);
        const isDeadlineArgument = /^\/deadline\s+[\d-]*$/.test(text);
        popup.hidden = !(isCommandPrefix || isDeadlineArgument);
    }

    function selectCommand() {
        if (!/^\/deadline\s+/i.test(input.value.trimStart())) {
            input.value = '/deadline ';
        }
        input.focus();
        input.setSelectionRange(input.value.length, input.value.length);
        popup.hidden = true;
    }

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
