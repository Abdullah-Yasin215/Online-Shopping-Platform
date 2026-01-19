// ========================================
// Scroll Animations
// ========================================

// Intersection Observer for scroll-triggered animations
const initScrollAnimations = () => {
    const observer = new IntersectionObserver(
        (entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');
                    // Optionally unobserve after animation
                    // observer.unobserve(entry.target);
                }
            });
        },
        {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px',
        }
    );

    // Observe all elements with animate-on-scroll class
    document.querySelectorAll('.animate-on-scroll').forEach((el) => {
        observer.observe(el);
    });
};

// ========================================
// Header Scroll Effect
// ========================================

const initHeaderScroll = () => {
    const header = document.querySelector('.bw-header');
    if (!header) return;

    let lastScroll = 0;

    window.addEventListener('scroll', () => {
        const currentScroll = window.pageYOffset;

        if (currentScroll > 100) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }

        lastScroll = currentScroll;
    });
};

// ========================================
// Smooth Page Transitions
// ========================================

const initPageTransitions = () => {
    // Create transition overlay if it doesn't exist
    if (!document.querySelector('.page-transition')) {
        const overlay = document.createElement('div');
        overlay.className = 'page-transition';
        document.body.appendChild(overlay);
    }

    // Fade in on page load
    window.addEventListener('load', () => {
        document.body.style.opacity = '0';
        setTimeout(() => {
            document.body.style.transition = 'opacity 300ms';
            document.body.style.opacity = '1';
        }, 10);
    });
};

// ========================================
// Initialize All
// ========================================

document.addEventListener('DOMContentLoaded', () => {
    initScrollAnimations();
    initHeaderScroll();
    initPageTransitions();
});

// Export for use in other scripts
window.StreetFluxAnimations = {
    initScrollAnimations,
    initHeaderScroll,
    initPageTransitions,
};
