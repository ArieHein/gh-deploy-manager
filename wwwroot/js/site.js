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

            const row = form.closest(".modern-env-row") || form.closest(".kanban-env-cell") || form.closest("[data-current-version]");
            const progressBar = form.querySelector(".deploy-progress-bar") ?? row?.querySelector(".deploy-progress-bar");
            if (progressBar) {
                const isVerticalProgress =
                    form.classList.contains("kanban-deploy-form")
                    || form.classList.contains("modern-row-form")
                    || !!row?.classList.contains("modern-env-row");

                if (isVerticalProgress) {
                    progressBar.style.width = "100%";
                    progressBar.style.height = "15%";
                } else {
                    progressBar.style.width = "15%";
                }

                setTimeout(() => {
                    if (isVerticalProgress) {
                        progressBar.style.height = "55%";
                    } else {
                        progressBar.style.width = "55%";
                    }
                }, 160);
                setTimeout(() => {
                    if (isVerticalProgress) {
                        progressBar.style.height = "88%";
                    } else {
                        progressBar.style.width = "88%";
                    }
                }, 520);
                setTimeout(() => {
                    if (isVerticalProgress) {
                        progressBar.style.height = "100%";
                    } else {
                        progressBar.style.width = "100%";
                    }
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

        statusElement.textContent = "⏳";
        statusElement.classList.remove("status-chip-hidden", "status-chip-success", "status-chip-failed");
        statusElement.classList.add("status-chip-queued");

        // Add visual feedback to the current row/cell
        const kanbanCell = form.closest(".kanban-env-cell");
        if (kanbanCell) {
            kanbanCell.classList.add("is-deploying");
        }

        const modernRow = form.closest(".modern-env-row");
        if (modernRow) {
            modernRow.classList.add("is-deploying");
        }
    }
});
