(() => {
    const modal = document.getElementById("moduleModal");
    const form = modal.querySelector("form");
    const title = document.getElementById("moduleModalTitle");
    const submit = document.getElementById("moduleSubmit");
    const name = document.getElementById("Form_Name");
    const complexity = document.getElementById("Form_Complexity");

    modal.addEventListener("show.bs.modal", event => {
        const trigger = event.relatedTarget;
        if (!trigger) return;

        const editing = trigger.hasAttribute("data-module-edit");
        form.action = editing ? trigger.dataset.action : modal.dataset.createAction;
        title.textContent = editing ? "Edit module" : "New module";
        submit.textContent = editing ? "Save changes" : "Create module";
        name.value = editing ? trigger.dataset.name : "";
        complexity.value = editing ? trigger.dataset.complexity : "";
        const validation = form.querySelector(".ModuleValidation");
        validation.replaceChildren();
    });

    modal.addEventListener("shown.bs.modal", () => name.focus());

    if (modal.dataset.showModal === "true") {
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }

    document.querySelectorAll(".ModuleDeleteForm").forEach(form => {
        form.addEventListener("submit", event => {
            if (!window.confirm(`Delete "${form.dataset.name}"?`)) event.preventDefault();
        });
    });
})();
