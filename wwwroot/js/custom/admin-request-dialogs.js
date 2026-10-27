(function () {
    "use strict";
    const active = new Map();
    function syncLock() {
        document.documentElement.classList.toggle("admin-request-dialog-open",
            !!document.querySelector("dialog.admin-request-dialog[open]"));
    }
    window.ecobayRequestDialogs = {
        open(element, reference) {
            if (!element || active.has(element.id)) return;
            for (const [id, entry] of active) {
                if (!entry.element.isConnected) window.ecobayRequestDialogs.close(id);
            }
            const cancel = event => {
                event.preventDefault();
                reference.invokeMethodAsync("CancelRequestDialog").catch(() => {});
            };
            element.addEventListener("cancel", cancel);
            const resize = () => element.style.setProperty("--request-dialog-height",
                (window.visualViewport ? window.visualViewport.height : window.innerHeight) + "px");
            if (window.visualViewport) window.visualViewport.addEventListener("resize", resize);
            window.addEventListener("resize", resize);
            resize();
            active.set(element.id, { element, cancel, resize });
            element.showModal(); // Native top layer, inert background and focus containment.
            syncLock();
        },
        close(reference) {
            const id = typeof reference === "string" ? reference : reference?.id;
            const entry = active.get(id);
            const element = entry?.element || (typeof reference === "object" ? reference : null);
            if (element) {
                if (entry) {
                    element.removeEventListener("cancel", entry.cancel);
                    if (window.visualViewport) window.visualViewport.removeEventListener("resize", entry.resize);
                    window.removeEventListener("resize", entry.resize);
                }
                active.delete(id);
                if (element.open) element.close();
            }
            syncLock();
        }
    };
})();
