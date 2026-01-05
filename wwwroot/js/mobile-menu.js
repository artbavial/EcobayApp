window.mobileMenu = {
    init: function () {
        const menuIcon = document.querySelector(".menuIcon");
        const mobileNav = document.querySelector(".starta-mobile-nav");

        if (!menuIcon || !mobileNav) {
            console.warn("mobileMenu.init: elements not found");
            return;
        }

        // Сброс старых обработчиков
        const newMenuIcon = menuIcon.cloneNode(true);
        menuIcon.parentNode.replaceChild(newMenuIcon, menuIcon);

        newMenuIcon.addEventListener("click", (e) => {
            e.stopPropagation();
            newMenuIcon.classList.toggle("close");
            mobileNav.classList.toggle("menu-active");
        });

        mobileNav.querySelectorAll("a").forEach(a => {
            a.addEventListener("click", () => {
                newMenuIcon.classList.remove("close");
                mobileNav.classList.remove("menu-active");
            });
        });
    }
};
