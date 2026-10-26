document.addEventListener("DOMContentLoaded", () => {
    const deployForms = document.querySelectorAll(".deploy-form");

    for (const form of deployForms) {
        form.addEventListener("submit", (event) => {
            if (form.dataset.submitting === "true") {
                return;
            }

            event.preventDefault();
            if (isDeployingSameVersion(form) && !window.confirm("This environment already has the selected version. Are you sure you want to deploy the same version again?")) {
                return;
            }

            form.dataset.submitting = "true";
            form.classList.add("is-deploying");
            setQueuedInlineStatus(form);

            const submitButton = form.querySelector("button[type='submit']");
            if (submitButton) {
                submitButton.disabled = true;
            }

            const progressBar = form.querySelector(".deploy-progress-bar");
            if (progressBar) {
                progressBar.style.width = "15%";
                setTimeout(() => {
                    progressBar.style.width = "55%";
                }, 160);
                setTimeout(() => {
                    progressBar.style.width = "88%";
                }, 520);
                setTimeout(() => {
                    progressBar.style.width = "100%";
                }, 900);
            }

            setTimeout(() => {
                form.submit();
            }, 1000);
        });
    }

    function isDeployingSameVersion(form) {
        const container = form.closest("[data-current-version]");
        const currentVersion = container?.dataset.currentVersion?.trim();
        const selectedVersion = form.querySelector("select[name='releaseVersion']")?.value?.trim();

        if (!currentVersion || !selectedVersion) {
            return false;
        }

        return currentVersion.toLowerCase() === selectedVersion.toLowerCase();
    }

    function setQueuedInlineStatus(form) {
        const statusElement = form
            .closest("[data-current-version]")
            ?.querySelector(".js-deploy-status");

        if (!statusElement) {
            return;
        }

        statusElement.textContent = "Queued";
        statusElement.classList.remove("status-chip-hidden", "status-chip-success");
        statusElement.classList.add("status-chip-queued");

        // For kanban UI, add visual feedback to the cell
        const cell = form.closest(".kanban-env-cell");
        if (cell) {
            cell.classList.add("is-deploying");
        }
    }
});
