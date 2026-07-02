// ===== LANGUAGE TRANSLATIONS =====
const languages = {
    th: {
        status: 'เปิดให้เช็คอิน',
        from: {
            'U-TAPAO INTERNATIONAL AIRPORT': 'สนามบินนานาชาติอู่ตะเภา',
            'SURAT THANI INTERNATIONAL AIRPORT': 'สนามบินนานาชาติสุราษฎร์ธานี',
            'CHIANG MAI INTERNATIONAL AIRPORT': 'สนามบินนานาชาติเชียงใหม่',
            'PHUKET INTERNATIONAL AIRPORT': 'สนามบินนานาชาติภูเก็ต',
            'HAT YAI INTERNATIONAL AIRPORT': 'สนามบินนานาชาติหาดใหญ่',
            'KRABI INTERNATIONAL AIRPORT': 'สนามบินนานาชาติกระบี่',
            'UBON RATCHATHANI AIRPORT': 'สนามบินอุบลราชธานี',
            'KHON KAEN AIRPORT': 'สนามบินขอนแก่น'
        }
    },
    en: {
        status: 'CHECK-IN OPEN',
        from: {
            'U-TAPAO INTERNATIONAL AIRPORT': 'U-TAPAO INTERNATIONAL AIRPORT',
            'SURAT THANI INTERNATIONAL AIRPORT': 'SURAT THANI INTERNATIONAL AIRPORT',
            'CHIANG MAI INTERNATIONAL AIRPORT': 'CHIANG MAI INTERNATIONAL AIRPORT',
            'PHUKET INTERNATIONAL AIRPORT': 'PHUKET INTERNATIONAL AIRPORT',
            'HAT YAI INTERNATIONAL AIRPORT': 'HAT YAI INTERNATIONAL AIRPORT',
            'KRABI INTERNATIONAL AIRPORT': 'KRABI INTERNATIONAL AIRPORT',
            'UBON RATCHATHANI AIRPORT': 'UBON RATCHATHANI AIRPORT',
            'KHON KAEN AIRPORT': 'KHON KAEN AIRPORT'
        }
    },
    zh: {
        status: '值机开放',
        from: {
            'U-TAPAO INTERNATIONAL AIRPORT': '乌塔堡国际机场',
            'SURAT THANI INTERNATIONAL AIRPORT': '苏拉他尼国际机场',
            'CHIANG MAI INTERNATIONAL AIRPORT': '清迈国际机场',
            'PHUKET INTERNATIONAL AIRPORT': '普吉国际机场',
            'HAT YAI INTERNATIONAL AIRPORT': '合艾国际机场',
            'KRABI INTERNATIONAL AIRPORT': '甲米国际机场',
            'UBON RATCHATHANI AIRPORT': '乌汶叻差他尼机场',
            'KHON KAEN AIRPORT': '孔敬机场'
        }
    }
};

// ===== GLOBAL VARIABLES =====
let currentLanguage = 0;
const languageKeys = ['th', 'en', 'zh'];

// ===== LANGUAGE SWITCHING FUNCTION =====
function switchLanguage() {
    const lang = languageKeys[currentLanguage];
    
    // Update status for all flights
    for (let i = 1; i <= 15; i++) {
        const statusElement = document.getElementById(`status${i}`);
        if (statusElement) {
            statusElement.textContent = languages[lang].status;
        }
    }

    // Update airport names
    const fromElements = document.querySelectorAll('.from');
    fromElements.forEach(element => {
        const originalText = element.getAttribute('data-original') || element.textContent;
        if (!element.getAttribute('data-original')) {
            element.setAttribute('data-original', originalText);
        }
        
        if (languages[lang].from[originalText]) {
            element.textContent = languages[lang].from[originalText];
        }
    });

    currentLanguage = (currentLanguage + 1) % 3;
}

// ===== TIME UPDATE FUNCTION =====
function updateTime() {
    const now = new Date();
    const options = {
        weekday: 'short',
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
    };
    
    const timeString = now.toLocaleDateString('en-GB', options)
        .replace(',', '')
        .toUpperCase();
    
    // Update main time display
    document.querySelector('.current-time').textContent = timeString;
    
    // Update footer date
    const dateOptions = {
        weekday: 'short',
        day: '2-digit',
        month: 'short',
        year: 'numeric'
    };
    
    // const dateString = now.toLocaleDateString('en-GB', dateOptions)
    //     .replace(',', '')
    //     .toUpperCase();
    
    // document.getElementById('footerDate').textContent = dateString;
}

// ===== HOVER EFFECTS =====
function addHoverEffects() {
    const statusBadges = document.querySelectorAll('.status');
    statusBadges.forEach(badge => {
        badge.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.05)';
        });
        
        badge.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
        });
    });
}

// ===== INITIALIZATION =====
function init() {
    // Update time immediately
    updateTime();
    
    // Add hover effects
    addHoverEffects();
    
    // Set up intervals
    setInterval(updateTime, 1000);        // Update time every second
    setInterval(switchLanguage, 10000);   // Switch language every 10 seconds
}

// ===== START APPLICATION =====
document.addEventListener('DOMContentLoaded', init);