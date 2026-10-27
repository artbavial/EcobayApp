export function open(dialog) {
    if (!dialog || !dialog.isConnected) return;
    if (!dialog.dataset.usersBound) {
        dialog.dataset.usersBound = "1";
        const requestClose = () => {
            const button = dialog.querySelector("[data-users-close]");
            if (button && !button.disabled) button.click();
        };
        dialog.addEventListener("cancel", event => {
            event.preventDefault();
            requestClose();
        });
        let outsidePointer = false;
        const outside = event => {
            const r = dialog.getBoundingClientRect();
            return event.clientX < r.left || event.clientX > r.right || event.clientY < r.top || event.clientY > r.bottom;
        };
        dialog.addEventListener("pointerdown", event => { outsidePointer = event.target === dialog && outside(event); });
        dialog.addEventListener("click", event => {
            if (outsidePointer && event.target === dialog && outside(event)) requestClose();
            outsidePointer = false;
        });
    }
    if (!dialog.open) dialog.showModal();
}
export function close(dialog) {
    if (dialog && dialog.open) dialog.close();
}
