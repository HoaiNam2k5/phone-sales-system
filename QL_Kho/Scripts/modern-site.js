// ==================== Scroll to Top ====================
const scrollToTopBtn = document.getElementById('scrollToTop');

window.addEventListener('scroll', () => {
    if (window.pageYOffset > 300) {
        scrollToTopBtn.classList.add('show');
    } else {
        scrollToTopBtn.classList.remove('show');
    }
});

if (scrollToTopBtn) {
    scrollToTopBtn.addEventListener('click', () => {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
}

// ==================== Navbar Scroll Effect ====================
let lastScroll = 0;
const navbar = document.querySelector('.main-navbar');

window.addEventListener('scroll', () => {
    const currentScroll = window.pageYOffset;

    if (currentScroll > 100) {
        navbar.style.boxShadow = '0 5px 30px rgba(0,0,0,0.15)';
    } else {
        navbar.style.boxShadow = '0 2px 20px rgba(0,0,0,0.08)';
    }

    lastScroll = currentScroll;
});

// ==================== Mobile Menu Toggle ====================
const mobileToggle = document.querySelector('.mobile-toggle');
const navbarCollapse = document.querySelector('.navbar-collapse');

if (mobileToggle) {
    mobileToggle.addEventListener('click', () => {
        mobileToggle.classList.toggle('active');
        navbarCollapse.classList.toggle('show');
    });
}

// ==================== Dropdown Menu ====================
const dropdownItems = document.querySelectorAll('.dropdown-modern');

dropdownItems.forEach(item => {
    const link = item.querySelector('.nav-link-modern');
    const menu = item.querySelector('.dropdown-menu-modern');

    if (window.innerWidth <= 992) {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            menu.style.display = menu.style.display === 'block' ? 'none' : 'block';
        });
    }
});

// ==================== Chatbox ====================
function toggleChat() {
    const chatBox = document.getElementById('chatBox');
    chatBox.classList.toggle('active');
}

function sendMessage() {
    const input = document.getElementById('chatInput');
    const message = input.value.trim();

    if (message) {
        // Add user message
        addMessage(message, 'user');
        input.value = '';

        // Simulate bot response
        setTimeout(() => {
            const responses = [
                'Cảm ơn bạn đã liên hệ! Chúng tôi sẽ trả lời trong giây lát.',
                'Bạn cần hỗ trợ gì về sản phẩm không?',
                'Chúng tôi có nhiều ưu đãi hấp dẫn. Bạn quan tâm sản phẩm nào?',
                'Để tư vấn chi tiết, vui lòng gọi hotline: 0909 123 456'
            ];
            const randomResponse = responses[Math.floor(Math.random() * responses.length)];
            addMessage(randomResponse, 'bot');
        }, 1000);
    }
}

function addMessage(text, sender) {
    const chatBody = document.getElementById('chatBody');
    const messageDiv = document.createElement('div');
    messageDiv.className = `chat-message ${sender}`;

    messageDiv.innerHTML = `
        <div class="message-bubble">
            ${text}
        </div>
    `;

    chatBody.appendChild(messageDiv);
    chatBody.scrollTop = chatBody.scrollHeight;
}

function handleChatEnter(event) {
    if (event.key === 'Enter') {
        sendMessage();
    }
}

// ==================== Smooth Scroll ====================
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// ==================== Product Card Hover Effect ====================
const productCards = document.querySelectorAll('.product-card-modern');

productCards.forEach(card => {
    card.addEventListener('mouseenter', function () {
        this.style.zIndex = '10';
    });

    card.addEventListener('mouseleave', function () {
        this.style.zIndex = '1';
    });
});

// ==================== Category Card Animation ====================
const categoryCards = document.querySelectorAll('.category-card-modern');

const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -100px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry, index) => {
        if (entry.isIntersecting) {
            setTimeout(() => {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }, index * 100);
        }
    });
}, observerOptions);

categoryCards.forEach(card => {
    card.style.opacity = '0';
    card.style.transform = 'translateY(30px)';
    card.style.transition = 'all 0.5s ease';
    observer.observe(card);
});

// ==================== Newsletter Form ====================
const newsletterForm = document.querySelector('.newsletter-form');

if (newsletterForm) {
    newsletterForm.addEventListener('submit', (e) => {
        e.preventDefault();
        const email = newsletterForm.querySelector('input[type="email"]').value;

        if (email) {
            showNotification('Đăng ký thành công! Cảm ơn bạn đã quan tâm.', 'success');
            newsletterForm.reset();
        }
    });
}

// ==================== Notification System ====================
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;

    const icons = {
        success: 'fa-check-circle',
        error: 'fa-exclamation-circle',
        warning: 'fa-exclamation-triangle',
        info: 'fa-info-circle'
    };

    const colors = {
        success: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
        error: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
        warning: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
        info: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
    };

    notification.style.cssText = `
        position: fixed;
        top: 100px;
        right: 30px;
        padding: 20px 25px;
        border-radius: 15px;
        background: ${colors[type]};
        color: white;
        box-shadow: 0 10px 30px rgba(0,0,0,0.2);
        display: flex;
        align-items: center;
        gap: 12px;
        z-index: 10000;
        animation: slideInRight 0.5s ease;
        font-weight: 600;
    `;

    notification.innerHTML = `
        <i class="fas ${icons[type]}" style="font-size: 1.5rem;"></i>
        <span>${message}</span>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.5s ease';
        setTimeout(() => {
            notification.remove();
        }, 500);
    }, 3000);
}

// ==================== Lazy Loading Images ====================
const images = document.querySelectorAll('img[data-src]');

const imageObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            const img = entry.target;
            img.src = img.dataset.src;
            img.removeAttribute('data-src');
            imageObserver.unobserve(img);
        }
    });
});

images.forEach(img => imageObserver.observe(img));

// ==================== Add to Cart Animation ====================
window.addToCartAnimation = function (button) {
    const cart = document.querySelector('.cart-icon');
    const buttonRect = button.getBoundingClientRect();
    const cartRect = cart.getBoundingClientRect();

    const flyingItem = document.createElement('div');
    flyingItem.style.cssText = `
        position: fixed;
        width: 30px;
        height: 30px;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        border-radius: 50%;
        z-index: 9999;
        left: ${buttonRect.left}px;
        top: ${buttonRect.top}px;
        pointer-events: none;
        transition: all 0.8s cubic-bezier(0.4, 0, 0.2, 1);
    `;

    document.body.appendChild(flyingItem);

    setTimeout(() => {
        flyingItem.style.left = cartRect.left + 'px';
        flyingItem.style.top = cartRect.top + 'px';
        flyingItem.style.transform = 'scale(0)';
        flyingItem.style.opacity = '0';
    }, 10);

    setTimeout(() => {
        flyingItem.remove();
        // Animate cart badge
        const badge = cart.querySelector('.cart-badge');
        if (badge) {
            badge.style.animation = 'bounce 0.5s ease';
            setTimeout(() => {
                badge.style.animation = '';
            }, 500);
        }
    }, 800);
};

// ==================== Product Quick View ====================
window.quickView = function (productId) {
    showNotification('Đang tải thông tin sản phẩm...', 'info');
    // Implement quick view modal here
};

// ==================== Price Format ====================
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

// ==================== Search Suggestion ====================
const searchInput = document.querySelector('.search-bar-modern input');

if (searchInput) {
    let searchTimeout;

    searchInput.addEventListener('input', (e) => {
        clearTimeout(searchTimeout);
        const query = e.target.value;

        if (query.length > 2) {
            searchTimeout = setTimeout(() => {
                // Implement search suggestions here
                console.log('Searching for:', query);
            }, 300);
        }
    });
}

// ==================== Countdown Timer (for promo) ====================
function startCountdown(endTime) {
    const timer = setInterval(() => {
        const now = new Date().getTime();
        const distance = endTime - now;

        if (distance < 0) {
            clearInterval(timer);
            return;
        }

        const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((distance % (1000 * 60)) / 1000);

        document.getElementById('hours').textContent = String(hours).padStart(2, '0');
        document.getElementById('minutes').textContent = String(minutes).padStart(2, '0');
        document.getElementById('seconds').textContent = String(seconds).padStart(2, '0');
    }, 1000);
}

// Start countdown - ends in 24 hours
const tomorrow = new Date();
tomorrow.setHours(tomorrow.getHours() + 24);
if (document.getElementById('hours')) {
    startCountdown(tomorrow.getTime());
}

// ==================== Add CSS Animations ====================
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
    
    @keyframes bounce {
        0%, 100% {
            transform: scale(1);
        }
        50% {
            transform: scale(1.3);
        }
    }
`;
document.head.appendChild(style);

// ==================== Initialize on Load ====================
window.addEventListener('load', () => {
    // Add loaded class to body for animations
    document.body.classList.add('loaded');

    // Initialize tooltips if using Bootstrap
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
});

// ==================== Console Welcome Message ====================
console.log('%c🎉 Welcome to Kho Điện Thoại! 📱', 'font-size: 20px; font-weight: bold; color: #667eea;');
console.log('%cWebsite designed with ❤️', 'font-size: 14px; color: #764ba2;');