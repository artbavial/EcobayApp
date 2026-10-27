(function () {
    "use strict";
    if (window.ecobayCallClicksBound) return;
    window.ecobayCallClicksBound = true;
    document.addEventListener("click", function (event) {
        if (!event.isTrusted || !(event.target instanceof Element)) return;
        if (/^\/(admin(?:panel)?|driver)(?:\/|$)/i.test(location.pathname)) return;
        var link = event.target.closest('a[href^="tel:"]');
        if (!link || event.defaultPrevented || event.button > 0) return;
        if (!window.crypto || !window.crypto.getRandomValues) return;
        var id = window.crypto.randomUUID ? window.crypto.randomUUID() :
            "10000000-1000-4000-8000-100000000000".replace(/[018]/g, function (c) {
                return (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16);
            });
        // Never intercept tel: navigation or wait for the response before dialing.
        fetch("/api/phone-click", {
            method: "POST", credentials: "same-origin", keepalive: true,
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ id: id, path: location.pathname })
        }).catch(function () { /* analytics must never break a phone call */ });
    }, true);
})();
