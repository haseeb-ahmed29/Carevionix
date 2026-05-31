(function () {
  const loader = document.getElementById("pageLoader");
  const liveClock = document.getElementById("liveClock");
  const initialLoaderDuration = 450;
  const initialLoaderKey = "carevionixInitialLoaderShown";

  const showLoader = () => {
    if (!loader) return;
    loader.classList.add("is-active");
  };

  const hideLoader = () => {
    if (!loader) return;
    loader.classList.remove("is-active");
  };

  let hasShownInitialLoader = false;
  try {
    hasShownInitialLoader = window.sessionStorage.getItem(initialLoaderKey) === "true";
    window.sessionStorage.setItem(initialLoaderKey, "true");
  } catch {
    hasShownInitialLoader = false;
  }

  if (!hasShownInitialLoader) {
    showLoader();
    window.setTimeout(hideLoader, initialLoaderDuration);
  }

  window.addEventListener("load", hideLoader);
  window.addEventListener("pageshow", function (event) {
    if (event.persisted) hideLoader();
  });
  window.addEventListener("focus", hideLoader);

  const updateClock = () => {
    if (!liveClock) return;
    liveClock.textContent = new Intl.DateTimeFormat(undefined, {
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
      weekday: "short"
    }).format(new Date());
  };

  updateClock();
  window.setInterval(updateClock, 1000);

  const iconPaths = {
    archive: '<path d="M3 7h18"/><path d="M5 7v12h14V7"/><path d="M8 3h8l2 4H6z"/><path d="M10 11h4"/>',
    "bar-chart-3": '<path d="M3 3v18h18"/><path d="M7 16V9"/><path d="M12 16V5"/><path d="M17 16v-3"/>',
    "calendar-check": '<path d="M8 2v4"/><path d="M16 2v4"/><rect x="3" y="4" width="18" height="18" rx="2"/><path d="M3 10h18"/><path d="m9 16 2 2 4-5"/>',
    "calendar-days": '<path d="M8 2v4"/><path d="M16 2v4"/><rect x="3" y="4" width="18" height="18" rx="2"/><path d="M3 10h18"/><path d="M8 14h.01"/><path d="M12 14h.01"/><path d="M16 14h.01"/><path d="M8 18h.01"/><path d="M12 18h.01"/><path d="M16 18h.01"/>',
    "calendar-plus": '<path d="M8 2v4"/><path d="M16 2v4"/><rect x="3" y="4" width="18" height="18" rx="2"/><path d="M3 10h18"/><path d="M12 14v5"/><path d="M9.5 16.5h5"/>',
    check: '<path d="m20 6-11 11-5-5"/>',
    "check-circle-2": '<circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-5"/>',
    "clipboard-pen-line": '<path d="M9 5h6"/><path d="M9 3h6v4H9z"/><path d="M8 5H6a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h5"/><path d="M16 5h2a2 2 0 0 1 2 2v5"/><path d="m14 20 6-6 2 2-6 6h-2z"/>',
    clock: '<circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/>',
    cookie: '<path d="M12 2a10 10 0 1 0 10 10 4 4 0 0 1-4-4 4 4 0 0 1-4-4 4 4 0 0 1-2-2"/><path d="M8.5 8.5h.01"/><path d="M16 15.5h.01"/><path d="M11 13h.01"/>',
    "credit-card": '<rect x="2" y="5" width="20" height="14" rx="2"/><path d="M2 10h20"/><path d="M6 15h4"/>',
    database: '<ellipse cx="12" cy="5" rx="8" ry="3"/><path d="M4 5v14c0 1.7 3.6 3 8 3s8-1.3 8-3V5"/><path d="M4 12c0 1.7 3.6 3 8 3s8-1.3 8-3"/>',
    "file-heart": '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/><path d="M12 18s-4-2.2-4-5a2.3 2.3 0 0 1 4-1.5A2.3 2.3 0 0 1 16 13c0 2.8-4 5-4 5z"/>',
    "file-text": '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/><path d="M8 13h8"/><path d="M8 17h8"/><path d="M8 9h2"/>',
    folder: '<path d="M3 7a2 2 0 0 1 2-2h5l2 2h7a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>',
    "folder-lock": '<path d="M3 7a2 2 0 0 1 2-2h5l2 2h7a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><rect x="9" y="12" width="6" height="5" rx="1"/><path d="M10 12v-1a2 2 0 0 1 4 0v1"/>',
    "heart-pulse": '<path d="M19 14c1.5-1.5 3-3.2 3-5.5A5.5 5.5 0 0 0 12 5a5.5 5.5 0 0 0-10 3.5c0 5.2 10 11.5 10 11.5s2-1.2 4.2-3"/><path d="M3 13h4l2-3 4 7 2-4h6"/>',
    history: '<path d="M3 12a9 9 0 1 0 3-6.7"/><path d="M3 3v6h6"/><path d="M12 7v5l3 2"/>',
    house: '<path d="m3 11 9-8 9 8"/><path d="M5 10v10h14V10"/><path d="M9 20v-6h6v6"/>',
    info: '<circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/>',
    "layout-dashboard": '<rect x="3" y="3" width="7" height="9" rx="1"/><rect x="14" y="3" width="7" height="5" rx="1"/><rect x="14" y="12" width="7" height="9" rx="1"/><rect x="3" y="16" width="7" height="5" rx="1"/>',
    "lock-keyhole": '<rect x="4" y="10" width="16" height="10" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/><path d="M12 14v2"/>',
    "log-in": '<path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"/><path d="m10 17 5-5-5-5"/><path d="M15 12H3"/>',
    "log-out": '<path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><path d="m16 17 5-5-5-5"/><path d="M21 12H9"/>',
    mail: '<rect x="3" y="5" width="18" height="14" rx="2"/><path d="m3 7 9 6 9-6"/>',
    "messages-square": '<path d="M21 15a2 2 0 0 1-2 2H8l-5 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/><path d="M8 9h8"/><path d="M8 13h5"/>',
    "pie-chart": '<path d="M21 12A9 9 0 1 1 12 3v9z"/><path d="M12 3a9 9 0 0 1 9 9h-9z"/>',
    "receipt-text": '<path d="M4 2v20l2-1 2 1 2-1 2 1 2-1 2 1 2-1 2 1V2l-2 1-2-1-2 1-2-1-2 1-2-1-2 1z"/><path d="M8 8h8"/><path d="M8 12h8"/><path d="M8 16h5"/>',
    "scan-line": '<path d="M4 7V5a1 1 0 0 1 1-1h2"/><path d="M17 4h2a1 1 0 0 1 1 1v2"/><path d="M20 17v2a1 1 0 0 1-1 1h-2"/><path d="M7 20H5a1 1 0 0 1-1-1v-2"/><path d="M7 12h10"/>',
    search: '<circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/>',
    settings: '<path d="M12 15.5a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7z"/><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.9l.1.1-2 3.4-.2-.1a1.7 1.7 0 0 0-2 .2l-.2.1a1.7 1.7 0 0 0-.9 1.5V22h-4v-.2a1.7 1.7 0 0 0-.9-1.5l-.2-.1a1.7 1.7 0 0 0-2-.2l-.2.1-2-3.4.1-.1A1.7 1.7 0 0 0 4.6 15v-.2A1.7 1.7 0 0 0 3.1 14H3v-4h.1a1.7 1.7 0 0 0 1.5-.9V9a1.7 1.7 0 0 0-.3-1.9l-.1-.1 2-3.4.2.1a1.7 1.7 0 0 0 2-.2l.2-.1A1.7 1.7 0 0 0 9.5 2h4a1.7 1.7 0 0 0 .9 1.5l.2.1a1.7 1.7 0 0 0 2 .2l.2-.1 2 3.4-.1.1A1.7 1.7 0 0 0 19.4 9v.2a1.7 1.7 0 0 0 1.5.8h.1v4h-.1a1.7 1.7 0 0 0-1.5 1z"/>',
    "shield-check": '<path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/><path d="m9 12 2 2 4-5"/>',
    stethoscope: '<path d="M6 3v5a4 4 0 0 0 8 0V3"/><path d="M10 12v3a4 4 0 0 0 8 0v-2"/><circle cx="18" cy="13" r="2"/><path d="M4 3h4"/><path d="M12 3h4"/>',
    "user-check": '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="m16 11 2 2 4-5"/>',
    "user-plus": '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M19 8v6"/><path d="M16 11h6"/>',
    "user-round": '<circle cx="12" cy="8" r="5"/><path d="M20 21a8 8 0 0 0-16 0"/>',
    users: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.9"/><path d="M16 3.1a4 4 0 0 1 0 7.8"/>',
    "users-round": '<path d="M18 21a6 6 0 0 0-12 0"/><circle cx="12" cy="8" r="5"/><path d="M22 21a4.8 4.8 0 0 0-4-4.7"/><path d="M16 3.1a5 5 0 0 1 0 9.8"/>',
    video: '<path d="M15 10.5V6a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h9a2 2 0 0 0 2-2v-4.5l7 4.5v-12z"/>'
  };

  document.querySelectorAll("[data-lucide]").forEach((icon) => {
    const name = icon.getAttribute("data-lucide");
    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svg.setAttribute("viewBox", "0 0 24 24");
    svg.setAttribute("fill", "none");
    svg.setAttribute("stroke", "currentColor");
    svg.setAttribute("stroke-width", "2");
    svg.setAttribute("stroke-linecap", "round");
    svg.setAttribute("stroke-linejoin", "round");
    svg.setAttribute("aria-hidden", "true");
    svg.setAttribute("class", icon.getAttribute("class") || "");
    svg.innerHTML = iconPaths[name] || '<circle cx="12" cy="12" r="9"/><path d="M12 8v8"/><path d="M8 12h8"/>';
    icon.replaceWith(svg);
  });

  const heroSlider = document.querySelector(".hero-image-slider");
  if (heroSlider) {
    const slides = Array.from(heroSlider.querySelectorAll(".hero-slide"));
    let activeSlideIndex = slides.findIndex((slide) => slide.classList.contains("is-active"));

    if (slides.length > 1) {
      activeSlideIndex = activeSlideIndex >= 0 ? activeSlideIndex : 0;
      slides.forEach((slide, index) => {
        slide.classList.toggle("is-active", index === activeSlideIndex);
        slide.setAttribute("aria-hidden", index === activeSlideIndex ? "false" : "true");
      });

      window.setInterval(() => {
        slides[activeSlideIndex].classList.remove("is-active");
        slides[activeSlideIndex].setAttribute("aria-hidden", "true");

        activeSlideIndex = (activeSlideIndex + 1) % slides.length;

        slides[activeSlideIndex].classList.add("is-active");
        slides[activeSlideIndex].setAttribute("aria-hidden", "false");
      }, 2000);
    }
  }

})();
