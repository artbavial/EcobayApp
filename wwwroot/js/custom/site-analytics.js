(function () {
    "use strict";
    if (window.ecobayAnalytics) return;
    const excluded = () => /^\/(admin(?:panel)?|driver)(\/|$)/i.test(location.pathname);
    const uuid = () => crypto.randomUUID();
    const read = key => { try { return JSON.parse(localStorage.getItem(key)); } catch (_) { return null; } };
    const save = (key, value) => { try { localStorage.setItem(key, JSON.stringify(value)); } catch (_) { /* in-memory fallback */ } };
    const params = new URLSearchParams(location.search);
    const campaign = {
        utmSource: (params.get("utm_source") || "").slice(0, 200),
        utmMedium: (params.get("utm_medium") || "").slice(0, 100),
        campaign: (params.get("utm_campaign") || "").slice(0, 200),
        content: (params.get("utm_content") || "").slice(0, 200),
        term: (params.get("utm_term") || "").slice(0, 200),
        yclid: params.has("yclid"), gclid: params.has("gclid") || params.has("gbraid") || params.has("wbraid")
    };
    const fingerprint = JSON.stringify(campaign);
    const incoming = !!(campaign.utmSource || campaign.yclid || campaign.gclid);
    let landingReferrer = document.referrer;
    try {
        const redirected = JSON.parse(sessionStorage.getItem("ecobay.analytics.redirect"));
        sessionStorage.removeItem("ecobay.analytics.redirect");
        if (redirected && redirected.path === location.pathname && Date.now() - redirected.at < 30000)
            landingReferrer = redirected.referrer || "";
    } catch (_) { /* unavailable storage must not affect page actions */ }
    let visitor, visit, queue = [], sending = false, calcTimer, started = new Set(), seenBlocks = new Set();
    try {
        visitor = read("ecobay.analytics.visitor");
        if (!visitor || !visitor.id || visitor.expires < Date.now())
            visitor = { id: uuid(), expires: Date.now() + 365 * 86400000 };
        save("ecobay.analytics.visitor", visitor);
    } catch (_) { return; }

    function context() {
        const saved = read("ecobay.analytics.visit");
        const now = Date.now();
        if (saved && saved.last > (visit?.last || 0)) visit = saved;
        if (!visit || now - visit.last >= 30 * 60000 ||
            incoming && !visit.incomingChecked && visit.fingerprint !== fingerprint) {
            visit = { id: uuid(), last: now, fingerprint: incoming ? fingerprint : "", attribution: campaign,
                referrer: landingReferrer, incomingChecked: true, lastCalc: "" };
            started.clear(); seenBlocks.clear();
        }
        visit.last = now;
        // Check an external campaign once per page, not on every internal action.
        visit.incomingChecked = true;
        save("ecobay.analytics.visit", visit);
        return visit;
    }
    // A different tagged arrival creates a fresh visit, internal navigation keeps attribution.
    const previous = read("ecobay.analytics.visit");
    if (previous) { previous.incomingChecked = false; visit = previous; }

    async function flush() {
        if (sending || !queue.length) return;
        sending = true;
        while (queue.length) {
            const item = queue[0];
            try {
                const response = await fetch("/api/site-event", { method: "POST", credentials: "same-origin",
                    keepalive: true, headers: { "Content-Type": "application/json" }, body: JSON.stringify(item) });
                if (!response.ok) {
                    if (response.status >= 500 || response.status === 429) break;
                }
                queue.shift();
            } catch (_) { break; }
        }
        sending = false;
    }
    function track(kind, data) {
        if (excluded() || navigator.webdriver) return;
        const v = context();
        if (queue.length >= 60) return;
        queue.push(Object.assign({ id: uuid(), visitId: v.id, visitorId: visitor.id, kind,
            path: location.pathname, referrer: v.referrer }, v.attribution, data || {}));
        void flush();
    }
    window.ecobayAnalytics = {
        track,
        visitId: () => { if (excluded()) return null; const v = context(); void flush(); return v.id; }
    };
    if (excluded()) return;
    context();
    track("page_view");
    setInterval(() => { if (queue.length) void flush(); }, 10000);
    window.addEventListener("online", () => void flush());
    document.addEventListener("visibilitychange", () => {
        if (document.visibilityState === "hidden") {
            // Same event IDs: server-side idempotency handles overlap with in-flight fetch.
            queue.slice(0, 8).forEach(item => navigator.sendBeacon("/api/site-event",
                new Blob([JSON.stringify(item)], { type: "application/json" })));
        }
    });
    function calc() {
        const input = document.querySelector(".calculator-weight-input");
        const root = input?.closest("[data-analytics-calculator]");
        if (!root) return;
        const weight = Number(input.value), rate = Number(root.dataset.rate), to = Number(root.dataset.rateTo);
        if (!Number.isFinite(weight) || weight <= 0 || weight > 1000000) return;
        const v = context();
        const signature = [location.pathname, weight, rate, to, root.dataset.mode].join("|");
        if (v.lastCalc === signature) return;
        v.lastCalc = signature; save("ecobay.analytics.visit", v);
        track("calculator", { weight, amountFrom: Math.round(weight * rate) / 100,
            amountTo: to > 0 ? Math.round(weight * to) / 100 : null, location: root.dataset.mode || "range" });
    }
    document.addEventListener("input", event => {
        if (!event.isTrusted || !(event.target instanceof Element)) return;
        const form = event.target.closest("[data-analytics-form]");
        if (form && !started.has(context().id)) { started.add(visit.id); track("form_start"); }
        if (event.target.matches(".calculator-weight-input")) {
            clearTimeout(calcTimer); calcTimer = setTimeout(calc, 700);
        }
    }, true);
    document.addEventListener("submit", event => {
        if (event.isTrusted && event.target.matches("[data-analytics-form]")) track("form_attempt");
    }, true);
    document.addEventListener("click", event => {
        if (!event.isTrusted || !(event.target instanceof Element) || event.button > 0 || event.defaultPrevented) return;
        const link = event.target.closest('a[href^="tel:"]');
        if (link) {
            const place = link.dataset.analyticsLocation || (link.closest("footer") ? "footer" :
                link.closest("nav") ? "navigation" : link.closest("#top") ? "hero" : "contacts");
            track("phone_click", { location: place });
        }
        const button = event.target.closest("[data-analytics-calculator] button");
        if (button) { clearTimeout(calcTimer); calcTimer = setTimeout(calc, 50); }
    }, true);
    function observe() {
        if (!window.IntersectionObserver) return;
        const observer = new IntersectionObserver(entries => entries.forEach(entry => {
            if (!entry.isIntersecting) return;
            const name = entry.target.dataset.analyticsBlock || entry.target.id;
            const key = context().id + ":" + location.pathname + ":" + name;
            if (!seenBlocks.has(key)) { seenBlocks.add(key); track("block_view", { location: name }); }
            observer.unobserve(entry.target);
        }), { threshold: 0.15 });
        document.querySelectorAll("[data-analytics-block],section[id]").forEach(el => observer.observe(el));
    }
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", observe, { once: true });
    else observe();
})();
