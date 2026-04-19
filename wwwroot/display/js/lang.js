// ============================================================
//  FIDS Language Cycling Engine  — TH / EN / ZH
//  cycle every 10s:  ไทย → English → 中文 → (repeat)
// ============================================================

// ---- UI text translations ----
const FIDS_I18N = {
    // Header titles
    DEPARTURES:   { th: 'ขาออก',              en: 'DEPARTURES',       zh: '出发'      },
    ARRIVALS:     { th: 'ขาเข้า',             en: 'ARRIVALS',         zh: '到达'      },
    GATE:         { th: 'ประตูขาออก',         en: 'DEPARTURE GATE',   zh: '登机口'    },
    BELT:         { th: 'รับสัมภาระ',         en: 'BAGGAGE CLAIM',    zh: '行李提取'  },
    CHECKIN:      { th: 'เคาน์เตอร์เช็คอิน', en: 'CHECK-IN',         zh: '值机柜台'  },

    // Column headers
    flight:       { th: 'เที่ยวบิน',          en: 'FLIGHT',           zh: '航班'      },
    airline:      { th: 'สายการบิน',          en: 'AIRLINE',          zh: '航空公司'  },
    destination:  { th: 'ปลายทาง',            en: 'DESTINATION',      zh: '目的地'    },
    origin:       { th: 'ต้นทาง',             en: 'ORIGIN',           zh: '出发地'    },
    time:         { th: 'เวลา',               en: 'TIME',             zh: '时间'      },
    gate:         { th: 'ประตู',              en: 'GATE',             zh: '登机口'    },
    belt:         { th: 'สายพาน',             en: 'BELT',             zh: '行李转盘'  },
    remarks:      { th: 'หมายเหตุ',           en: 'REMARKS',          zh: '备注'      },

    // Footer scrolling text
    dep:          { th: 'เที่ยวบินขาออก • FLIGHT DEPARTURES • 出发航班 •',
                    en: 'FLIGHT DEPARTURES • เที่ยวบินขาออก • 出发航班 •',
                    zh: '出发航班 • FLIGHT DEPARTURES • เที่ยวบินขาออก •' },
    arr:          { th: 'เที่ยวบินขาเข้า • FLIGHT ARRIVALS • 到达航班 •',
                    en: 'FLIGHT ARRIVALS • เที่ยวบินขาเข้า • 到达航班 •',
                    zh: '到达航班 • FLIGHT ARRIVALS • เที่ยวบินขาเข้า •' },
};

// ---- Status translations (keyed by canonical status key) ----
const FIDS_STATUS = {
    CHECKIN_OPEN: { th: 'เปิดให้เช็คอิน',        en: 'CHECK-IN OPEN',    zh: '值机开放'    },
    BOARDING:     { th: 'กำลังขึ้นเครื่อง',       en: 'BOARDING',         zh: '登机中'      },
    GATE_OPEN:    { th: 'ประตูเปิด',              en: 'GATE OPEN',        zh: '登机口开放'  },
    DELAYED:      { th: 'ล่าช้า',                 en: 'DELAYED',          zh: '航班延误'    },
    CANCELLED:    { th: 'ยกเลิก',                 en: 'CANCELLED',        zh: '航班取消'    },
    LANDED:       { th: 'ลงจอดแล้ว',              en: 'LANDED',           zh: '已降落'      },
    BAGGAGE:      { th: 'รับสัมภาระ',             en: 'BAGGAGE CLAIM',    zh: '行李提取'    },
    EXPECTED:     { th: 'คาดว่าจะถึง',            en: 'EXPECTED',         zh: '预计到达'    },
    DEPARTED:     { th: 'ออกเดินทางแล้ว',         en: 'DEPARTED',         zh: '已出发'      },
    ON_TIME:      { th: 'ตรงเวลา',                en: 'ON TIME',          zh: '准时'        },
};

// ---- City / Airport name translations ----
//  key = canonical English name (uppercase) used in DB or as partial match
const FIDS_CITY = {
    // Thailand airports — Thai names (primary)
    'SURAT THANI':          { th: 'สุราษฎร์ธานี',       en: 'Surat Thani',           zh: '素叻他尼'         },
    'SUVARNABHUMI':         { th: 'สุวรรณภูมิ',         en: 'Suvarnabhumi (BKK)',     zh: '素万那普'         },
    'DON MUEANG':           { th: 'ดอนเมือง',           en: 'Don Mueang (DMK)',       zh: '廊曼'             },
    'BANGKOK':              { th: 'กรุงเทพฯ',           en: 'Bangkok',                zh: '曼谷'             },
    'PHUKET':               { th: 'ภูเก็ต',             en: 'Phuket (HKT)',           zh: '普吉'             },
    'CHIANG MAI':           { th: 'เชียงใหม่',          en: 'Chiang Mai (CNX)',       zh: '清迈'             },
    'HAT YAI':              { th: 'หาดใหญ่',            en: 'Hat Yai (HDY)',          zh: '合艾'             },
    'KRABI':                { th: 'กระบี่',             en: 'Krabi (KBV)',            zh: '甲米'             },
    'KOH SAMUI':            { th: 'เกาะสมุย',           en: 'Koh Samui (USM)',        zh: '苏梅岛'           },
    'SAMUI':                { th: 'เกาะสมุย',           en: 'Koh Samui (USM)',        zh: '苏梅岛'           },
    'U-TAPAO':              { th: 'อู่ตะเภา',           en: 'U-Tapao (UTP)',          zh: '乌塔堡'           },
    'UDON THANI':           { th: 'อุดรธานี',           en: 'Udon Thani (UTH)',       zh: '乌隆他尼'         },
    'NAKHON SI THAMMARAT':  { th: 'นครศรีธรรมราช',     en: 'Nakhon Si Thammarat',   zh: '洛坤'             },
    'UBON RATCHATHANI':     { th: 'อุบลราชธานี',        en: 'Ubon Ratchathani (UBP)',zh: '乌汶叻差他尼'     },
    'KHON KAEN':            { th: 'ขอนแก่น',            en: 'Khon Kaen (KKC)',        zh: '孔敬'             },
    'TRANG':                { th: 'ตรัง',               en: 'Trang (TST)',            zh: '董里'             },
    'NARATHIWAT':           { th: 'นราธิวาส',           en: 'Narathiwat (NAW)',       zh: '那拉提瓦'         },
    'BURIRAM':              { th: 'บุรีรัมย์',           en: 'Buriram (BFV)',          zh: '武里南'           },
    'MAE SOT':              { th: 'แม่สอด',             en: 'Mae Sot (MAQ)',          zh: '美索'             },
    'PHITSANULOK':          { th: 'พิษณุโลก',           en: 'Phitsanulok (PHS)',      zh: '彭世洛'           },
    'NAKHON RATCHASIMA':    { th: 'นครราชสีมา',         en: 'Nakhon Ratchasima',     zh: '呵叻'             },
    'ROI ET':               { th: 'ร้อยเอ็ด',           en: 'Roi Et (ROI)',           zh: '廊开'             },
    // IATA codes
    'BKK':  { th: 'สุวรรณภูมิ',  en: 'Suvarnabhumi',   zh: '素万那普' },
    'DMK':  { th: 'ดอนเมือง',    en: 'Don Mueang',     zh: '廊曼'     },
    'HKT':  { th: 'ภูเก็ต',      en: 'Phuket',         zh: '普吉'     },
    'CNX':  { th: 'เชียงใหม่',   en: 'Chiang Mai',     zh: '清迈'     },
    'HDY':  { th: 'หาดใหญ่',     en: 'Hat Yai',        zh: '合艾'     },
    'KBV':  { th: 'กระบี่',      en: 'Krabi',          zh: '甲米'     },
    'USM':  { th: 'เกาะสมุย',    en: 'Koh Samui',      zh: '苏梅岛'   },
    'UTH':  { th: 'อุดรธานี',    en: 'Udon Thani',     zh: '乌隆他尼' },
    'UBP':  { th: 'อุบลราชธานี', en: 'Ubon Ratchathani',zh: '乌汶叻差他尼'},
    'KKC':  { th: 'ขอนแก่น',     en: 'Khon Kaen',      zh: '孔敬'     },
    'UTP':  { th: 'อู่ตะเภา',    en: 'U-Tapao',        zh: '乌塔堡'   },
    'SNO':  { th: 'สกลนคร',      en: 'Sakon Nakhon',   zh: '色军那空' },
    // Thai names as keys (for lookup when DB stores Thai)
    'กรุงเทพฯ':         { th: 'กรุงเทพฯ',       en: 'Bangkok',          zh: '曼谷'         },
    'สุวรรณภูมิ':       { th: 'สุวรรณภูมิ',     en: 'Suvarnabhumi',     zh: '素万那普'     },
    'ดอนเมือง':         { th: 'ดอนเมือง',       en: 'Don Mueang',       zh: '廊曼'         },
    'ภูเก็ต':           { th: 'ภูเก็ต',         en: 'Phuket',           zh: '普吉'         },
    'เชียงใหม่':        { th: 'เชียงใหม่',      en: 'Chiang Mai',       zh: '清迈'         },
    'หาดใหญ่':          { th: 'หาดใหญ่',        en: 'Hat Yai',          zh: '合艾'         },
    'กระบี่':           { th: 'กระบี่',         en: 'Krabi',            zh: '甲米'         },
    'สุราษฎร์ธานี':     { th: 'สุราษฎร์ธานี',   en: 'Surat Thani',      zh: '素叻他尼'     },
    'อุดรธานี':         { th: 'อุดรธานี',       en: 'Udon Thani',       zh: '乌隆他尼'     },
    'อุบลราชธานี':      { th: 'อุบลราชธานี',    en: 'Ubon Ratchathani', zh: '乌汶叻差他尼' },
    'ขอนแก่น':          { th: 'ขอนแก่น',        en: 'Khon Kaen',        zh: '孔敬'         },
    'นครศรีธรรมราช':    { th: 'นครศรีธรรมราช',  en: 'Nakhon Si Thammarat', zh: '洛坤'      },
};

// ---- Translate city name: try exact match then partial match ----
function translateCity(raw, lang) {
    if (!raw || raw === '-') return raw;
    const trimmed = raw.trim();
    const up = trimmed.toUpperCase();

    // 1. Exact key match (case-insensitive)
    for (const [key, val] of Object.entries(FIDS_CITY)) {
        if (up === key.toUpperCase()) return val[lang] ?? trimmed;
    }
    // 2. Partial match — key found inside the raw string
    for (const [key, val] of Object.entries(FIDS_CITY)) {
        if (up.includes(key.toUpperCase())) return val[lang] ?? trimmed;
    }
    return trimmed;  // fallback: show as-is
}

// ============================================================
//  Cycling Engine
// ============================================================
const FIDS_LANGS   = ['th', 'en', 'zh'];
let   fidsCurLang  = 0;

function fidsApplyLang(lang) {
    // 1. UI text (header title, col headers, footer)
    document.querySelectorAll('[data-lang-key]').forEach(el => {
        const key = el.dataset.langKey;
        if (FIDS_I18N[key]) el.textContent = FIDS_I18N[key][lang] ?? el.textContent;
    });

    // 2. Status badges
    document.querySelectorAll('[data-status-key]').forEach(el => {
        const key = el.dataset.statusKey;
        if (FIDS_STATUS[key]) el.textContent = FIDS_STATUS[key][lang] ?? el.textContent;
    });

    // 3. City names
    document.querySelectorAll('[data-city]').forEach(el => {
        const city = el.dataset.city;
        el.textContent = translateCity(city, lang);
    });

    // 4. Lang indicator badge (optional)
    const badge = document.getElementById('langBadge');
    if (badge) badge.textContent = lang.toUpperCase();
}

function fidsCycle() {
    fidsCurLang = (fidsCurLang + 1) % FIDS_LANGS.length;
    fidsApplyLang(FIDS_LANGS[fidsCurLang]);
}

document.addEventListener('DOMContentLoaded', () => {
    fidsApplyLang(FIDS_LANGS[fidsCurLang]);   // apply TH on load
    setInterval(fidsCycle, 10000);             // cycle every 10s
});
