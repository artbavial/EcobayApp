// ========================
// REINIT ANIMATIONS (главная функция)
// ========================
window.reinitAllAnimations = () => {
    console.log("Reinitializing animations...");
    setTimeout(() => {
        window.initHoverButtons();
        window.initVideoBlock();
        window.initCircleText();
        window.initGsapAnimations();
        window.initSwiper();
        window.initMobileMenu();
    }, 50);
};

// ========================
// SWIPER INITIALIZATION
// ========================
window._swiperInstances = window._swiperInstances || {};

window.initSwiper = () => {
    if (typeof Swiper === "undefined") return;

    const selector = ".testimonialsSlides";
    const el = document.querySelector(selector);
    if (!el) return;

    if (window._swiperInstances[selector]) {
        try {
            window._swiperInstances[selector].destroy(true, true);
        } catch (e) { }
        delete window._swiperInstances[selector];
    }

    window._swiperInstances[selector] = new Swiper(selector, {
        slidesPerView: 1,
        loop: true,
        autoplay: { delay: 3000 },
        speed: 400
    });
};

// ========================
// GSAP ANIMATIONS
// ========================
window.initGsapAnimations = () => {
    if (typeof gsap === "undefined") return;

    if (gsap.killTweensOf) {
        gsap.killTweensOf(".animate-img, .animate-img2, .animate-img3");
    }
    if (window.ScrollTrigger && window.ScrollTrigger.getAll) {
        window.ScrollTrigger.getAll().forEach(t => t.kill());
    }

    gsap.from(".animate-img", { opacity: 0, y: 50, duration: 1 });
    gsap.from(".animate-img2", { opacity: 0, x: -50, duration: 1 });
    gsap.from(".animate-img3", { opacity: 0, scale: 0.8, duration: 1 });
};

// ========================
// CIRCLE TEXT
// ========================
window.initCircleText = () => {
    const el = document.getElementById("circle-text");
    if (!el || typeof CircleType === "undefined") return;

    if (el.dataset.ctInit === "1") {
        el.dataset.ctInit = "0";
    }

    new CircleType(el);
    el.dataset.ctInit = "1";
};

// ========================
// VIDEO BLOCK
// ========================
window.initVideoBlock = () => {
    const $frame = $(".video-block iframe");
    const $play = $(".video-block .startplay");
    const $close = $(".video-block .close");

    if ($frame.length === 0) return;

    $play.off("click").on("click", () => {
        $frame.show();
    });

    $close.off("click").on("click", () => {
        $frame.hide();
    });

    const el = document.getElementById("circle-text");
    if (el && !el.dataset.ctInit && typeof CircleType !== "undefined") {
        new CircleType(el);
        el.dataset.ctInit = "1";
    }
};

// ========================
// MOBILE MENU (Blazor-совместимо)
// ========================
window.initMobileMenu = () => {
    const menuIcon = document.querySelector(".menuIcon");
    const mobileNav = document.querySelector(".starta-mobile-nav");

    if (!menuIcon || !mobileNav) {
        console.log("Menu elements not found");
        return;
    }

    console.log("Initializing mobile menu...");

    // Снимаем все старые обработчики
    $(menuIcon).off("click");

    // Навешиваем новые
    $(menuIcon).on("click", function (e) {
        e.stopPropagation();
        console.log("Menu toggled");
        $(this).toggleClass("close");
        $(mobileNav).toggleClass("menu-active");
    });

    // Закрытие меню при клике на ссылку
    $(mobileNav).find("a").off("click").on("click", function () {
        $(menuIcon).removeClass("close");
        $(mobileNav).removeClass("menu-active");
    });
};

// ========================
// HOVER BUTTONS
// ========================
window.initHoverButtons = () => {
    const $buttons = $(".starta-button");
    if ($buttons.length === 0) return;

    $buttons.off("mouseover mouseleave");

    $buttons.on("mouseover", function (e) {
        const relX = e.pageX - $(this).offset().left;
        const relY = e.pageY - $(this).offset().top;
        $(this).find(".starta-button-hover")
            .css({ left: relX, top: relY })
            .removeClass("desplode-circle")
            .addClass("explode-circle");
    });

    $buttons.on("mouseleave", function (e) {
        const relX = e.pageX - $(this).offset().left;
        const relY = e.pageY - $(this).offset().top;
        $(this).find(".starta-button-hover")
            .css({ left: relX, top: relY })
            .removeClass("explode-circle")
            .addClass("desplode-circle");
    });
};

// ========================
// DOCUMENT READY
// ========================
$(document).ready(function () {
    console.log("Document ready");

    // Loader
    gsap.to(".loader", 0.5, {
        delay: 3,
        opacity: 0,
    });
    $(".loader").css("pointer-events", "none");

    // Menu toggle function
    function menu(menuIcon) {
        menuIcon.toggleClass("close");
        $(".starta-mobile-nav").toggleClass("menu-active");
    }

    // Обработчик для .menuIcon (если используется через класс)
    $(document).off("click", ".menuIcon").on("click", ".menuIcon", function () {
        menu($(this));
    });

    // Smooth scroll
    const ScrollArea = document.getElementById("scroll-content");
    if (ScrollArea) {
        const options = {
            damping: 0.1,
            speed: 1,
            renderByPixel: true,
            continuousScrolling: true,
            syncCallbacks: true,
            alwaysShowTracks: true,
        };
        var scrollbar = Scrollbar.init(ScrollArea, options);

        $(".starta-nav").addClass("transitionNav");
        $(".menuIcon").addClass("transitionNav");

        if ($(".backToTop").length === 0) {
            $("body").append('<div class="backToTop"><i class="fa-solid fa-arrow-up"></i></div>');
        }

        scrollbar.addListener((status) => {
            const offset = status.offset;

            if (offset.y >= 500) {
                $(".starta-nav").addClass("sticky");
                $(".menuIcon").css("top", offset.y + 38 + "px");
                $(".sticky").css("top", offset.y + "px");
                $(".backToTop").css({ opacity: "1", transform: "translateY(0px)" });
                setTimeout(() => {
                    $(".starta-nav").removeClass("transitionNav");
                    $(".menuIcon").removeClass("transitionNav");
                }, 1000);
            } else {
                $(".starta-nav").css("top", 0 + "px");
                $(".starta-nav").removeClass("sticky");
                $(".menuIcon").css("top", 0 + 38 + "px");
                $(".backToTop").css({ opacity: "0", transform: "translateY(100%)" });
                $(".starta-nav").addClass("transitionNav");
                $(".menuIcon").addClass("transitionNav");
            }

            $(".starta-mobile-nav").css("top", offset.y + "px");
        });

        $(".backToTop").off("click").on("click", function (e) {
            const target = $("#top");
            const targetEl = $(target);
            if (targetEl.length === 0) return;
            const targetRect = targetEl.offset();
            e.preventDefault();
            gsap.to(scrollbar, {
                scrollTo: targetRect.top,
                duration: 2.5,
                ease: "power4.inOut",
            });

            $(".starta-menu li a").removeClass("active");
        });
    }

    // Menu Hover
    $(document).off("mouseover mouseleave", ".menu-animation");
    $(document).on("mouseover", ".menu-animation", function () {
        $(this).addClass("hover");
    });
    $(document).on("mouseleave", ".menu-animation", function () {
        $(this).removeClass("hover");
    });

    // Button animations hover
    $(document).off("mouseover mouseleave", ".starta-button");
    $(document).on("mouseover", ".starta-button", function (e) {
        var relX = e.pageX - $(this).offset().left;
        var relY = e.pageY - $(this).offset().top;
        $(this).find(".starta-button-hover").css({ left: relX, top: relY });
        $(this).find(".starta-button-hover").removeClass("desplode-circle");
        $(this).find(".starta-button-hover").addClass("explode-circle");
    });

    $(document).on("mouseleave", ".starta-button", function (e) {
        var relX = e.pageX - $(this).offset().left;
        var relY = e.pageY - $(this).offset().top;
        $(this).find(".starta-button-hover").css({ left: relX, top: relY });
        $(this).find(".starta-button-hover").removeClass("explode-circle");
        $(this).find(".starta-button-hover").addClass("desplode-circle");
    });

    $(".starta-button-2").each(function () {
        $(this).children(".starta-button-hover").remove();
    });

    // GSAP plugins
    if (typeof gsap !== "undefined" && typeof ScrollTrigger !== "undefined" && typeof ScrollToPlugin !== "undefined") {
        gsap.registerPlugin(ScrollTrigger, ScrollToPlugin);

        if (ScrollArea) {
            scrollbar.addListener(ScrollTrigger.update);
            ScrollTrigger.defaults({ scroller: ScrollArea });

            ScrollTrigger.scrollerProxy("#scroll-content", {
                scrollTop(value) {
                    if (arguments.length) {
                        scrollbar.scrollTop = value;
                    }
                    return scrollbar.scrollTop;
                },
            });
        }

        // GSAP Timelines
        let shapes = gsap.timeline({
            scrollTrigger: {
                scrub: true,
                pin: true,
                start: "top top",
                end: "+=100%",
            },
        });

        shapes.to(".shapes img", {
            y: 80,
            duration: 1,
        });

        let imgBLock = gsap.timeline({
            scrollTrigger: {
                trigger: ".animate-img",
                start: "center 80%",
                end: "bottom 10%",
            },
        });
        imgBLock.from(".animate-img", {
            x: -500,
            duration: 0.7,
            opacity: 0,
        });
        imgBLock.to(".animate-img", {
            x: 0,
            duration: 0.7,
            opacity: 1,
        });

        imgBLock.from(".fill", {
            width: 0,
        });
        imgBLock.to(".fill", {
            width: "75%",
        });

        let imgBLock2 = gsap.timeline({
            scrollTrigger: {
                trigger: ".animate-img2",
                start: "center 50%",
                end: "bottom 10%",
            },
        });

        imgBLock2.from(".animate-img2", {
            x: -500,
            duration: 0.7,
            opacity: 0,
        });
        imgBLock2.to(".animate-img2", {
            x: 0,
            duration: 0.7,
            opacity: 1,
        });

        let imgBLock3 = gsap.timeline({
            scrollTrigger: {
                trigger: ".animate-img3",
                start: "top center",
                end: "bottom 10%",
            },
        });

        imgBLock3.from(".animate-img3", {
            x: -500,
            duration: 0.7,
            opacity: 0,
        });
        imgBLock3.to(".animate-img3", {
            x: 0,
            duration: 0.7,
            opacity: 1,
        });
    }

    // Icon colors
    let iconbg = ["rgb(--primary-color)", "rgb(255,202,96)", "rgb(63,223,254)"];

    $(".starta-icon").each(function (i) {
        let colorIndex = i % iconbg.length;
        $(this).css("background-color", iconbg[colorIndex]);
    });

    // YT Link copy
    $(".ytLink").each(function () {
        $(this)
            .closest(".imgOverlay")
            .find(".servicePlay")
            .attr("data-copy", $(this).parent("a").attr("href"));
    });

    $(document).off("click", ".servicePlay").on("click", ".servicePlay", function () {
        const videoLink = $(this).attr("data-copy");
        const tempInput = $("<input>");
        $("body").append(tempInput);
        tempInput.val(videoLink).select();
        document.execCommand("copy");
        tempInput.remove();
        $(this).children("span").text("Copied!");
    });

    // Circle Text
    const circleTextEl = document.getElementById("circle-text");
    if (circleTextEl && typeof CircleType !== "undefined" && !circleTextEl.dataset.ctInit) {
        new CircleType(circleTextEl);
        circleTextEl.dataset.ctInit = "1";
    }

    // Testimonials Slide
    if (typeof Swiper !== "undefined") {
        const swiper2 = new Swiper(".testimonialsSlides", {
            slidesPerView: 1,
            loop: true,
        });

        var testimonialHeight = $(".testimonialSingle").outerHeight();
        $(".testimonialsSlides").css("height", testimonialHeight + "px");

        // Insta feed Slides
        if (typeof EffectMaterial !== "undefined") {
            const swiper = new Swiper(".swiper", {
                modules: [EffectMaterial],
                effect: "material",
                slidesPerView: 7,
                spaceBetween: 20,
            });
            const rClass = ["bottomSlide", "topSlide"];
            $(".swiper-material-content").each(function (index) {
                $(this).addClass(rClass[Math.floor(Math.random() * rClass.length)]);
            });
        }
    }

    // Scroll to sections
    $(".starta-menu li a").each(function (e) {
        const target = $(this).attr("href");
        const targetEl = $(target);
        if (targetEl.length === 0) return;
        const targetRect = targetEl.offset();

        $(this).off("click").on("click", function (e) {
            $(".menuIcon").removeClass("close");
            $(".starta-mobile-nav").removeClass("menu-active");
            e.preventDefault();

            if (typeof gsap !== "undefined" && ScrollArea) {
                gsap.to(scrollbar, {
                    scrollTo: targetRect.top - 120,
                    duration: 2.5,
                    ease: "power4.inOut",
                });
            }

            $(".starta-menu li a").removeClass("active");
            $(this).addClass("active");
        });
    });

    // Play video
    $(document).off("click", ".startplay").on("click", ".startplay", function () {
        const getParent = $(this).closest(".video-block");
        const getVideo = getParent.find("iframe");
        getVideo.css("display", "block");
        getParent.find(".close").css("display", "block");
        var symbol = getVideo[0].src.indexOf("?") > -1 ? "&" : "?";

        if (getVideo[0].src.indexOf("autoplay") === -1) {
            getVideo[0].src += symbol + "autoplay=1";
        } else {
            getVideo[0].src = getVideo[0].src.replace(/autoplay=0/, "autoplay=1");
        }
    });

    // Close video
    $(document).off("click", ".video-block .close").on("click", ".video-block .close", function () {
        const getParent = $(this).closest(".video-block");
        const getVideo = getParent.find("iframe");
        getVideo.css("display", "none");

        var symbol = getVideo[0].src.indexOf("?") > -1 ? "&" : "?";

        if (getVideo[0].src.indexOf("autoplay") === -1) {
            getVideo[0].src += symbol + "autoplay=0";
        } else {
            getVideo[0].src = getVideo[0].src.replace(/autoplay=1/, "autoplay=0");
        }

        $(this).css("display", "none");
    });

    // Testimonial circle
    $(".testimonialCircle").each(function () {
        $(this).css({
            width: $(this).parent().outerHeight() + "px",
            height: $(this).parent().outerHeight() + "px",
        });
    });

    // Инициализируем все анимации
    window.reinitAllAnimations();
});

// ========================
// BLAZOR SUPPORT
// ========================
if (window.Blazor) {
    window.Blazor.addEventListener("enhancedload", () => {
        console.log("Blazor enhanced load - reinitializing animations");
        window.reinitAllAnimations();
    });
}

// Инициализация при загрузке страницы
document.addEventListener("DOMContentLoaded", () => {
    console.log("DOM Content Loaded");
    window.reinitAllAnimations();
});


// 1. Проверяем HTML элемент
console.log('Mobile nav:', document.querySelector('.starta-mobile-nav'));

// 2. Проверяем CSS
const nav = document.querySelector('.starta-mobile-nav');
if (nav) {
    const computed = window.getComputedStyle(nav);
    console.log('Position:', computed.position);
    console.log('Left:', computed.left);
    console.log('Display:', computed.display);
    console.log('Z-index:', computed.zIndex);
}

// 3. Проверяем иконку
console.log('Menu icon:', document.querySelector('.menuIcon'));

// 4. Пробуем открыть меню вручную
document.querySelector('.starta-mobile-nav')?.classList.add('menu-active');
