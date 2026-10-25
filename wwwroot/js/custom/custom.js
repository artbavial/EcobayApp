/* =====================================================================
   Ecobay.by — анимации и интерактив страницы.

   Принципы:
   • Один вход: ecobay.initPage(). Вызывается из Blazor (Home.OnAfterRenderAsync)
     после того, как интерактивный компонент отрисовал DOM. На статических страницах
     (контакты, соглашение) — автоматически по DOMContentLoaded.
   • initPage идемпотентна: повторный вызов убирает старые ScrollTrigger/Swiper и создаёт заново.
   • Все клики/hover — через делегирование на document (переживают перерисовку DOM Blazor'ом).
   • Нативная прокрутка вместо smooth-scrollbar: smooth-scrollbar перестраивал DOM внутри
     Blazor-компонента и ломал position: fixed (промо-окно, меню).
   • prefers-reduced-motion — анимации отключаются.
   ===================================================================== */

(function () {
    "use strict";

    // -----------------------------------------------------------------
    // Выбор города (localStorage / sessionStorage)
    // -----------------------------------------------------------------
    var CITY_KEY = "ecobay.city";
    var REMEMBER_KEY = "ecobay.city.remember";
    var DISMISS_KEY = "ecobay.city.dismissed";

    function safe(fn, fallback) {
        try { return fn(); } catch (e) { return fallback; }
    }

    function isRemember() {
        return safe(function () { return localStorage.getItem(REMEMBER_KEY) !== "0"; }, true);
    }

    window.ecobayCity = {
        /** Сохранённый выбор: slug города, "all" (вся Беларусь) или null. */
        get: function () {
            var value = safe(function () {
                return isRemember() ? localStorage.getItem(CITY_KEY) : sessionStorage.getItem(CITY_KEY);
            }, null);
            if (value) return value;
            var dismissed = safe(function () { return sessionStorage.getItem(DISMISS_KEY) === "1"; }, false);
            return dismissed ? "all" : null;
        },
        set: function (slug) {
            if (!slug || !/^(all|[a-z0-9-]{1,50})$/.test(slug)) return;
            safe(function () {
                if (isRemember()) localStorage.setItem(CITY_KEY, slug);
                else sessionStorage.setItem(CITY_KEY, slug);
            });
        },
        clear: function () {
            safe(function () {
                localStorage.removeItem(CITY_KEY);
                sessionStorage.removeItem(CITY_KEY);
                sessionStorage.removeItem(DISMISS_KEY);
            });
        },
        /** Окно закрыли крестиком — не показываем до конца сеанса. */
        dismiss: function () {
            safe(function () { sessionStorage.setItem(DISMISS_KEY, "1"); });
        },
        setRemember: function (enabled) {
            safe(function () {
                localStorage.setItem(REMEMBER_KEY, enabled ? "1" : "0");
                if (!enabled) localStorage.removeItem(CITY_KEY);
            });
        }
    };

    // -----------------------------------------------------------------
    // Утилиты
    // -----------------------------------------------------------------
    var reduceMotion = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    var hasGsap = function () { return typeof window.gsap !== "undefined"; };
    var hasScrollTrigger = function () { return hasGsap() && typeof window.ScrollTrigger !== "undefined"; };

    function $all(selector, root) {
        return Array.prototype.slice.call((root || document).querySelectorAll(selector));
    }

    function navHeight() {
        var nav = document.querySelector(".starta-nav");
        return nav ? nav.getBoundingClientRect().height : 0;
    }

    function closeMobileMenu() {
        $all(".menuIcon").forEach(function (i) { i.classList.remove("close"); });
        $all(".starta-mobile-nav").forEach(function (n) { n.classList.remove("menu-active"); });
        document.documentElement.classList.remove("menu-open");
    }

    function scrollToTarget(target) {
        var y;
        if (target === 0 || target === "top") {
            y = 0;
        } else {
            var el = typeof target === "string" ? document.querySelector(target) : target;
            if (!el) return false;
            // Позиция считается в момент клика (раньше — один раз при загрузке, до загрузки картинок).
            y = el.getBoundingClientRect().top + window.pageYOffset - (navHeight() + 20);
            if (el.id === "top") y = 0;
        }

        if (hasGsap() && window.ScrollToPlugin && !reduceMotion) {
            gsap.to(window, { scrollTo: { y: Math.max(0, y), autoKill: false }, duration: 1.1, ease: "power3.inOut" });
        } else {
            window.scrollTo({ top: Math.max(0, y), behavior: reduceMotion ? "auto" : "smooth" });
        }
        return true;
    }

    function setActiveMenu(hash) {
        $all(".starta-menu li a").forEach(function (a) {
            var h = safe(function () { return new URL(a.href, location.href).hash; }, "");
            a.classList.toggle("active", h === hash);
        });
    }

    // -----------------------------------------------------------------
    // Глобальные обработчики (навешиваются один раз)
    // -----------------------------------------------------------------
    var globalsBound = false;

    function bindGlobalHandlers() {
        if (globalsBound) return;
        globalsBound = true;

        // Кнопки: «взрыв» круга от точки входа курсора.
        // mouseover/mouseout с проверкой relatedTarget = mouseenter/mouseleave, но работает через делегирование
        // (раньше mouseover срабатывал на каждом дочернем элементе и анимация «дёргалась»).
        document.addEventListener("mouseover", function (e) {
            var btn = e.target.closest && e.target.closest(".starta-button");
            if (!btn || btn.contains(e.relatedTarget)) return;
            var hover = btn.querySelector(".starta-button-hover");
            if (!hover) return;
            var r = btn.getBoundingClientRect();
            hover.style.left = (e.clientX - r.left) + "px";
            hover.style.top = (e.clientY - r.top) + "px";
            hover.classList.remove("desplode-circle");
            hover.classList.add("explode-circle");
        });
        document.addEventListener("mouseout", function (e) {
            var btn = e.target.closest && e.target.closest(".starta-button");
            if (!btn || btn.contains(e.relatedTarget)) return;
            var hover = btn.querySelector(".starta-button-hover");
            if (!hover) return;
            var r = btn.getBoundingClientRect();
            hover.style.left = (e.clientX - r.left) + "px";
            hover.style.top = (e.clientY - r.top) + "px";
            hover.classList.remove("explode-circle");
            hover.classList.add("desplode-circle");
        });

        // Подчёркивание пунктов меню
        document.addEventListener("mouseover", function (e) {
            var a = e.target.closest && e.target.closest(".menu-animation");
            if (a && !a.contains(e.relatedTarget)) a.classList.add("hover");
        });
        document.addEventListener("mouseout", function (e) {
            var a = e.target.closest && e.target.closest(".menu-animation");
            if (a && !a.contains(e.relatedTarget)) a.classList.remove("hover");
        });

        document.addEventListener("click", function (e) {
            if (!e.target.closest) return;

            // Мобильное меню (раньше три разных обработчика переключали его по очереди и меню «не открывалось»)
            var icon = e.target.closest(".menuIcon");
            if (icon) {
                e.preventDefault();
                var nav = document.querySelector(".starta-mobile-nav");
                var open = !icon.classList.contains("close");
                icon.classList.toggle("close", open);
                if (nav) nav.classList.toggle("menu-active", open);
                document.documentElement.classList.toggle("menu-open", open);
                return;
            }

            // Видео: грузим iframe только по клику (раньше два YouTube-плеера грузились сразу и тормозили страницу)
            var play = e.target.closest(".video-block .startplay");
            if (play) {
                var block = play.closest(".video-block");
                var frame = block && block.querySelector("iframe");
                if (frame) {
                    var src = frame.getAttribute("data-src") || frame.getAttribute("src") || "";
                    if (src && src !== "about:blank") {
                        frame.src = src + (src.indexOf("?") > -1 ? "&" : "?") + "autoplay=1";
                    }
                    frame.style.display = "block";
                    var closeBtn = block.querySelector(".close");
                    if (closeBtn) closeBtn.style.display = "block";
                }
                return;
            }

            var close = e.target.closest(".video-block .close");
            if (close) {
                var vb = close.closest(".video-block");
                var f = vb && vb.querySelector("iframe");
                if (f) {
                    f.src = "about:blank"; // останавливаем воспроизведение
                    f.style.display = "none";
                }
                close.style.display = "none";
                return;
            }

            if (e.target.closest(".backToTop")) {
                e.preventDefault();
                scrollToTarget(0);
                setActiveMenu("#top");
                return;
            }

            // Якорные ссылки вида "/gomel#about" и "/#plan".
            // Раньше $("/gomel#about") бросал ошибку jQuery, и весь остальной код инициализации не выполнялся.
            var link = e.target.closest("a[href*='#']");
            if (link && !link.target) {
                var url = safe(function () { return new URL(link.href, location.href); }, null);
                if (url && url.hash && url.pathname === location.pathname && url.origin === location.origin) {
                    var target = safe(function () { return document.querySelector(decodeURIComponent(url.hash)); }, null);
                    if (target) {
                        e.preventDefault();
                        closeMobileMenu();
                        scrollToTarget(target);
                        setActiveMenu(url.hash);
                        if (history.replaceState) history.replaceState(null, "", url.pathname + url.search + url.hash);
                    }
                }
            }

            // Ссылка внутри мобильного меню (например, «Сменить город») — закрываем меню.
            if (e.target.closest(".starta-mobile-nav a")) closeMobileMenu();
        });

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") closeMobileMenu();
        });

        // Липкое меню + кнопка «наверх». requestAnimationFrame — не чаще одного раза за кадр.
        var ticking = false;
        window.addEventListener("scroll", function () {
            if (ticking) return;
            ticking = true;
            requestAnimationFrame(function () {
                ticking = false;
                updateStickyNav();
            });
        }, { passive: true });

        var resizeTimer;
        window.addEventListener("resize", function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                sizeTestimonialCircles();
                if (window.innerWidth > 992) closeMobileMenu();
                if (hasScrollTrigger()) ScrollTrigger.refresh();
            }, 150);
        });

        // После загрузки картинок высоты секций меняются — пересчитываем точки срабатывания.
        window.addEventListener("load", function () {
            if (hasScrollTrigger()) ScrollTrigger.refresh();
            sizeTestimonialCircles();
        });
    }

    // -----------------------------------------------------------------
    // Липкое меню
    // -----------------------------------------------------------------
    var STICKY_AT = 300;

    function ensureBackToTop() {
        if (!document.querySelector(".backToTop")) {
            var b = document.createElement("div");
            b.className = "backToTop";
            b.setAttribute("role", "button");
            b.setAttribute("aria-label", "Наверх");
            b.innerHTML = '<i class="fa-solid fa-arrow-up"></i>';
            document.body.appendChild(b);
        }
    }

    function updateStickyNav() {
        var nav = document.querySelector(".starta-nav");
        var header = nav && nav.closest("header");
        var y = window.pageYOffset || document.documentElement.scrollTop;
        var sticky = y >= STICKY_AT;

        if (nav && header) {
            if (sticky && !nav.classList.contains("sticky")) {
                // Резервируем место под меню, чтобы контент не «прыгал» при переходе в position: fixed.
                header.style.paddingTop = nav.offsetHeight + "px";
                nav.classList.add("sticky");
            } else if (!sticky && nav.classList.contains("sticky")) {
                nav.classList.remove("sticky");
                header.style.paddingTop = "";
            }
        }

        var toTop = document.querySelector(".backToTop");
        if (toTop) toTop.classList.toggle("is-visible", y >= 500);
    }

    // -----------------------------------------------------------------
    // Виджеты
    // -----------------------------------------------------------------
    var swiperInstance = null;

    function initSwiper() {
        if (typeof window.Swiper === "undefined") return;
        var el = document.querySelector(".testimonialsSlides");
        if (!el) return;

        if (swiperInstance) {
            safe(function () { swiperInstance.destroy(true, true); });
            swiperInstance = null;
        }

        swiperInstance = new Swiper(el, {
            slidesPerView: 1,
            loop: true,
            speed: 700,
            autoHeight: true, // раньше высота фиксировалась по первому слайду и длинные отзывы обрезались
            grabCursor: true,
            autoplay: reduceMotion ? false : { delay: 6000, disableOnInteraction: false, pauseOnMouseEnter: true }
        });
    }

    function sizeTestimonialCircles() {
        $all(".testimonialCircle").forEach(function (c) {
            var h = c.parentElement ? c.parentElement.offsetHeight : 0;
            if (h) {
                c.style.width = h + "px";
                c.style.height = h + "px";
            }
        });
    }

    function initCircleText() {
        var el = document.getElementById("circle-text");
        // SVG textPath is already centered and must not be rebuilt by CircleType.
        if (el && el.namespaceURI === "http://www.w3.org/2000/svg") return;
        if (!el || typeof window.CircleType === "undefined" || el.dataset.ctInit === "1") return;
        new CircleType(el);
        el.dataset.ctInit = "1";
    }

    function initIconColors() {
        // Было "rgb(--primary-color)" — некорректный CSS, у первой иконки цвет не применялся.
        var colors = ["var(--primary-color)", "rgb(255, 202, 96)", "rgb(63, 223, 254)"];
        $all(".starta-icon").forEach(function (el, i) {
            el.style.backgroundColor = colors[i % colors.length];
        });
    }

    function initButtons() {
        $all(".starta-button-2 > .starta-button-hover").forEach(function (h) { h.remove(); });
    }

    // -----------------------------------------------------------------
    // GSAP-анимации
    // -----------------------------------------------------------------
    var gsapContext = null;

    function isBelowFold(el) {
        // Анимируем появление только того, что ещё не видно. То, что уже на экране в момент
        // инициализации, не прячем — иначе контент «мигает» (сначала виден, потом исчезает и выезжает).
        return el.getBoundingClientRect().top > window.innerHeight * 0.92;
    }

    var revealed = [];

    function insideRevealed(el) {
        // Не анимируем элемент, если уже анимируется его контейнер (иначе двойное появление).
        for (var i = 0; i < revealed.length; i++) {
            if (revealed[i] !== el && revealed[i].contains(el)) return true;
        }
        return false;
    }

    function reveal(selector, vars, stagger) {
        var items = $all(selector).filter(function (el) { return isBelowFold(el) && !insideRevealed(el); });
        if (!items.length) return;
        revealed = revealed.concat(items);

        ScrollTrigger.batch(items, {
            start: "top 88%",
            once: true,
            onEnter: function (batch) {
                gsap.to(batch, {
                    opacity: 1, x: 0, y: 0, scale: 1,
                    duration: vars.duration || 0.8,
                    ease: vars.ease || "power3.out",
                    stagger: stagger || 0.12,
                    overwrite: true,
                    clearProps: "transform,opacity"
                });
            }
        });

        gsap.set(items, {
            opacity: 0,
            x: vars.x || 0,
            y: vars.y === undefined ? 40 : vars.y,
            scale: vars.scale || 1
        });
    }

    function countUp(el) {
        // Первый текстовый узел (рядом стоит иконка-стрелка)
        var node = Array.prototype.find.call(el.childNodes, function (n) {
            return n.nodeType === 3 && n.textContent.trim().length;
        });
        if (!node) return;
        var target = parseInt(node.textContent.replace(/\D/g, ""), 10);
        if (!target) return;

        var obj = { v: 0 };
        gsap.to(obj, {
            v: target,
            duration: 1.6,
            ease: "power2.out",
            scrollTrigger: { trigger: el, start: "top 90%", once: true },
            onUpdate: function () { node.textContent = Math.round(obj.v).toLocaleString("ru-RU"); },
            onComplete: function () { node.textContent = target.toLocaleString("ru-RU"); }
        });
        node.textContent = "0";
    }

    function initGsap() {
        if (!hasScrollTrigger()) return;

        gsap.registerPlugin(ScrollTrigger);
        if (window.ScrollToPlugin) gsap.registerPlugin(ScrollToPlugin);

        // Убираем всё, что было создано прошлым вызовом
        if (gsapContext) {
            gsapContext.revert();
            gsapContext = null;
        }

        revealed = [];
        if (reduceMotion) return;

        gsapContext = gsap.context(function () {
            // Параллакс фона и фигур первого экрана (scrub — привязка к прокрутке).
            // Раньше: pin: true без trigger «прибивал» страницу, а смещение фигур не работало.
            var hero = document.querySelector("header#top");
            if (hero) {
                gsap.to(".heroBG img", {
                    yPercent: 12, scale: 1.08, ease: "none",
                    scrollTrigger: { trigger: hero, start: "top top", end: "bottom top", scrub: true }
                });
                $all(".shapes img").forEach(function (img, i) {
                    gsap.to(img, {
                        y: 60 + i * 35, rotation: i % 2 ? 12 : -12, ease: "none",
                        scrollTrigger: { trigger: hero, start: "top top", end: "bottom top", scrub: true }
                    });
                });
            }

            // Блок «Наши цели»: картинка выезжает слева, карточка — снизу, полоса прогресса
            // заполняется до реального процента из БД (раньше всегда до 75%).
            $all(".bar .fill").forEach(function (fill) {
                var p = parseFloat(fill.getAttribute("data-progress"));
                if (isNaN(p)) return;
                gsap.fromTo(fill, { width: "0%" }, {
                    width: p + "%", duration: 1.6, ease: "power2.out",
                    scrollTrigger: { trigger: fill, start: "top 92%", once: true }
                });
            });
            $all(".starta-performance h4.starta-h4.d-flex").forEach(countUp);

            // Сначала — контейнеры (карточки, блоки), затем — отдельные тексты вне этих контейнеров.
            reveal(".animate-img", { x: -120, y: 0, duration: 1 });
            reveal(".starta-performance", { y: 50 });
            reveal(".featureSingle", { y: 40 }, 0.15);
            reveal(".starta-plans .plan", { y: 60 }, 0.18);
            reveal(".details_img", { y: 60, scale: 0.94 }, 0.18);
            reveal(".starta-about .video-block", { y: 60, scale: 0.97, duration: 1 });
            reveal(".accordion-item", { y: 30 }, 0.1);
            reveal(".testimonialsSlides", { y: 40 });
            reveal(".starta-contacts .col-md-6", { y: 40 }, 0.15);
            reveal(".starta-footer .row > div", { y: 30 }, 0.12);
            reveal(".contact-box", { y: 30 });

            reveal("section .starta-heading", { y: 20, duration: 0.6 });
            reveal("section .starta-h1", { y: 30 });
            reveal("section .starta-desc", { y: 24, duration: 0.7 });
            reveal(".starta-list li", { x: -30, y: 0, duration: 0.6 }, 0.08);
            reveal(".details-list li", { x: -30, y: 0 }, 0.12);
            reveal(".starta-block-img", { y: 50, scale: 0.97 });

            // Подсветка пункта меню текущей секции
            ["top", "about", "services", "plan", "team", "faqs"].forEach(function (id) {
                var section = document.getElementById(id);
                if (!section) return;
                ScrollTrigger.create({
                    trigger: section,
                    start: "top 40%",
                    end: "bottom 40%",
                    onToggle: function (self) { if (self.isActive) setActiveMenu("#" + id); }
                });
            });
        });

        ScrollTrigger.refresh();
    }

    // -----------------------------------------------------------------
    // Публичное API
    // -----------------------------------------------------------------
    window.ecobay = {
        initPage: function () {
            bindGlobalHandlers();
            ensureBackToTop();
            closeMobileMenu();
            initButtons();
            initIconColors();
            initCircleText();
            initSwiper();
            sizeTestimonialCircles();
            initGsap();
            updateStickyNav();
            document.documentElement.classList.add("ecobay-ready");
        },

        /** Блокировка прокрутки страницы под модальным окном. */
        lockScroll: function (lock) {
            document.documentElement.classList.toggle("modal-open", !!lock);
        }
    };

    // Совместимость со старыми вызовами из Razor-кода
    window.reinitAllAnimations = window.ecobay.initPage;

    // Статические страницы (без интерактивного Blazor) инициализируем сами.
    // Главная страница помечена data-interactive-page — её инициализирует Blazor после отрисовки,
    // иначе анимации запускались дважды (на пререндере и после подключения SignalR).
    function autoInit() {
        if (document.querySelector("[data-interactive-page]")) {
            bindGlobalHandlers(); // меню и якоря работают ещё до подключения Blazor
            ensureBackToTop();
            updateStickyNav();
            return;
        }
        window.ecobay.initPage();
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", autoInit);
    else autoInit();
})();
