// Language translations
const translations = {
    th: {
        destination: 'ท่าอากาศยานนานาชาติสุราษฎร์ธานี',
        status: 'เปิดให้เช็คอิน'
    },
    zh: {
        destination: '素叻他尼国际机场',
        status: '开放值机'
    },
    en: {
        destination: 'SURAT THANI INTERNATIONAL AIRPORT',
        status: 'CHECK-IN OPEN'
    }
};

// Language cycling variables
const languages = ['th', 'zh', 'en']; // Thai, Chinese, English
let currentLanguageIndex = 0;

// Elements
const destinationElement = document.getElementById('destination');
const statusElement = document.getElementById('status');
const timeElement = document.getElementById('current-time');

// Language switching function
function switchLanguage(lang) {
    // Add fade-out effect
    destinationElement.classList.add('fade-out');
    statusElement.classList.add('fade-out');
    
    setTimeout(() => {
        // Update text content
        destinationElement.textContent = translations[lang].destination;
        statusElement.textContent = translations[lang].status;
        
        // Remove fade-out and add fade-in
        destinationElement.classList.remove('fade-out');
        statusElement.classList.remove('fade-out');
        destinationElement.classList.add('fade-in');
        statusElement.classList.add('fade-in');
        
        // Remove fade-in class after animation
        setTimeout(() => {
            destinationElement.classList.remove('fade-in');
            statusElement.classList.remove('fade-in');
        }, 500);
        
    }, 250);
}

// Auto language cycling every 10 seconds
function startAutoLanguageCycle() {
    setInterval(() => {
        currentLanguageIndex = (currentLanguageIndex + 1) % languages.length;
        const nextLanguage = languages[currentLanguageIndex];
        switchLanguage(nextLanguage);
    }, 10000); // 10 seconds
}

// Real-time clock functionality
// function updateTime() {
//     const now = new Date();
//     const currentLang = languages[currentLanguageIndex];
//     let formattedTime;
    
//     switch (currentLang) {
//         case 'th':
//             const thaiMonths = ['ม.ค.', 'ก.พ.', 'มี.ค.', 'เม.ย.', 'พ.ค.', 'มิ.ย.',
//                               'ก.ค.', 'ส.ค.', 'ก.ย.', 'ต.ค.', 'พ.ย.', 'ธ.ค.'];
//             const thaiDays = ['อา', 'จ', 'อ', 'พ', 'พฤ', 'ศ', 'ส'];
//             formattedTime = `${thaiDays[now.getDay()]} ${now.getDate()} ${thaiMonths[now.getMonth()]} ${now.getFullYear()} ${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`;
//             break;
            
//         case 'zh':
//             const zhMonths = ['1月', '2月', '3月', '4月', '5月', '6月',
//                              '7月', '8月', '9月', '10月', '11月', '12月'];
//             const zhDays = ['周日', '周一', '周二', '周三', '周四', '周五', '周六'];
//             formattedTime = `${zhDays[now.getDay()]} ${now.getFullYear()}年${zhMonths[now.getMonth()]}${now.getDate()}日 ${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`;
//             break;
            
//         default: // English
//             const options = {
//                 weekday: 'short',
//                 year: 'numeric',
//                 month: 'short',
//                 day: 'numeric',
//                 hour: '2-digit',
//                 minute: '2-digit',
//                 hour12: false
//             };
//             formattedTime = now.toLocaleDateString('en-US', options)
//                 .replace(',', '')
//                 .toUpperCase()
//                 .replace(/(\d{1,2}):(\d{2})/, '$1:$2');
//     }
    
//     timeElement.textContent = formattedTime;
// }

// Background animation
function createBackgroundAnimation() {
    const canvas = document.createElement('canvas');
    canvas.style.position = 'fixed';
    canvas.style.top = '0';
    canvas.style.left = '0';
    canvas.style.width = '100%';
    canvas.style.height = '100%';
    canvas.style.zIndex = '-1';
    canvas.style.opacity = '0.05';
    document.body.appendChild(canvas);
    
    const ctx = canvas.getContext('2d');
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
    
    const particles = [];
    const particleCount = 30;
    
    for (let i = 0; i < particleCount; i++) {
        particles.push({
            x: Math.random() * canvas.width,
            y: Math.random() * canvas.height,
            vx: (Math.random() - 0.5) * 0.3,
            vy: (Math.random() - 0.5) * 0.3,
            size: Math.random() * 2 + 1
        });
    }
    
    function animate() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        
        particles.forEach(particle => {
            particle.x += particle.vx;
            particle.y += particle.vy;
            
            if (particle.x < 0 || particle.x > canvas.width) particle.vx *= -1;
            if (particle.y < 0 || particle.y > canvas.height) particle.vy *= -1;
            
            ctx.beginPath();
            ctx.arc(particle.x, particle.y, particle.size, 0, Math.PI * 2);
            ctx.fillStyle = 'rgba(255, 255, 255, 0.3)';
            ctx.fill();
        });
        
        requestAnimationFrame(animate);
    }
    
    animate();
    
    // Handle window resize
    window.addEventListener('resize', () => {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    });
}

// Initialize everything when the page loads
document.addEventListener('DOMContentLoaded', () => {
    // Start with Thai language
    switchLanguage('th');
    
    // Start auto language cycling after 10 seconds
    setTimeout(() => {
        startAutoLanguageCycle();
    }, 5000);
    
    // Update time every second
    updateTime();
    setInterval(updateTime, 1000);
    
    // Start background animation
    createBackgroundAnimation();
    
    // Add keyboard shortcuts for manual control
    document.addEventListener('keydown', (e) => {
        if (e.key === 'f' || e.key === 'F') {
            // Toggle fullscreen
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen();
            } else {
                document.exitFullscreen();
            }
        }
        
        // Manual language switching
        if (e.key === '1') {
            currentLanguageIndex = 0;
            switchLanguage('th');
        }
        if (e.key === '2') {
            currentLanguageIndex = 1;
            switchLanguage('zh');
        }
        if (e.key === '3') {
            currentLanguageIndex = 2;
            switchLanguage('en');
        }
    });
});