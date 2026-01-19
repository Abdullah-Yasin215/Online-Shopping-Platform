// ========================================
// Micro-interactions & UI Enhancements
// ========================================

// ========================================
// Button Ripple Effect
// ========================================

const initButtonRipples = () => {
    document.querySelectorAll('.btn-ripple').forEach((button) => {
        button.addEventListener('click', function (e) {
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;

            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';
            ripple.classList.add('ripple-effect');

            this.appendChild(ripple);

            setTimeout(() => ripple.remove(), 600);
        });
    });
};

// ========================================
// Image Lazy Loading Enhancement
// ========================================

const enhanceImageLoading = () => {
    const images = document.querySelectorAll('img[loading="lazy"]');

    images.forEach((img) => {
        img.addEventListener('load', function () {
            this.classList.add('loaded');
        });

        // Add loading class if not loaded
        if (!img.complete) {
            img.classList.add('loading');
        }
    });
};

// ========================================
// Quantity Input Enhancement
// ========================================

const enhanceQuantityInputs = () => {
    document.querySelectorAll('.quantity-control').forEach((control) => {
        const input = control.querySelector('input[type="number"]');
        const decreaseBtn = control.querySelector('.quantity-decrease');
        const increaseBtn = control.querySelector('.quantity-increase');

        if (decreaseBtn) {
            decreaseBtn.addEventListener('click', () => {
                const min = parseInt(input.min) || 1;
                const current = parseInt(input.value) || min;
                if (current > min) {
                    input.value = current - 1;
                    input.dispatchEvent(new Event('change', { bubbles: true }));
                }
            });
        }

        if (increaseBtn) {
            increaseBtn.addEventListener('click', () => {
                const max = parseInt(input.max) || Infinity;
                const current = parseInt(input.value) || 0;
                if (current < max) {
                    input.value = current + 1;
                    input.dispatchEvent(new Event('change', { bubbles: true }));
                }
            });
        }
    });
};

// ========================================
// Form Field Animations
// ========================================

const enhanceFormFields = () => {
    document.querySelectorAll('.form-control, .form-select').forEach((field) => {
        // Add focus class
        field.addEventListener('focus', function () {
            this.parentElement.classList.add('field-focused');
        });

        field.addEventListener('blur', function () {
            this.parentElement.classList.remove('field-focused');
        });

        // Add filled class if has value
        const checkValue = () => {
            if (field.value) {
                field.parentElement.classList.add('field-filled');
            } else {
                field.parentElement.classList.remove('field-filled');
            }
        };

        field.addEventListener('input', checkValue);
        checkValue(); // Initial check
    });
};

// ========================================
// Toast Notifications Enhancement
// ========================================

const enhanceToasts = () => {
    // Add animation classes to Bootstrap toasts
    document.querySelectorAll('.toast').forEach((toast) => {
        toast.classList.add('animate-slide-in-right');

        toast.addEventListener('hidden.bs.toast', function () {
            this.classList.add('animate-fade-out');
        });
    });
};

// ========================================
// Card Hover Effects
// ========================================

const enhanceCardHovers = () => {
    document.querySelectorAll('.product-card').forEach((card) => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-8px)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
};

// ========================================
// Initialize All Micro-interactions
// ========================================

document.addEventListener('DOMContentLoaded', () => {
    initButtonRipples();
    enhanceImageLoading();
    enhanceQuantityInputs();
    enhanceFormFields();
    enhanceToasts();
    enhanceCardHovers();
});

// Export for use in other scripts
window.StreetFluxMicroInteractions = {
    initButtonRipples,
    enhanceImageLoading,
    enhanceQuantityInputs,
    enhanceFormFields,
    enhanceToasts,
    enhanceCardHovers,
};
