// ============================================================
//  FIDS SHARED CONSTANTS + DRAWERS MAP
//  device-specific drawing → template-draw-board.js
//                          → template-draw-baggage-claim.js
//                          → template-draw-baggage-info.js
//                          → template-draw-checkin.js
// ============================================================

const C = {
    bg:      '#111118', header: '#0d1b5e', colHdr: '#162070',
    row1:    '#121220', row2:   '#16162a', text:   '#ffffff',
    dim:     'rgba(255,255,255,0.5)',      blue:   '#90CAF9',
    airAsia: '#DC143C', nokAir: '#FFB300', navy:   '#0d1b5e',
    statuses: [
        { bg:'#1A237E', fg:'#C5CAE9', lbl:'เปิดเช็คอิน'      },
        { bg:'#BF360C', fg:'#FFCCBC', lbl:'กำลังขึ้นเครื่อง' },
        { bg:'#0D47A1', fg:'#BBDEFB', lbl:'ประตูเปิด'         },
        { bg:'#F57F17', fg:'#FFF8E1', lbl:'ล่าช้า'            },
        { bg:'#B71C1C', fg:'#FFCDD2', lbl:'ยกเลิก'            },
        { bg:'#1B5E20', fg:'#C8E6C9', lbl:'ลงจอดแล้ว'        },
    ]
};

function rr(ctx, x, y, w, h, r) {
    r = Math.min(r, w / 2, h / 2);
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.arcTo(x + w, y,     x + w, y + h, r);
    ctx.arcTo(x + w, y + h, x,     y + h, r);
    ctx.arcTo(x,     y + h, x,     y,     r);
    ctx.arcTo(x,     y,     x + w, y,     r);
    ctx.closePath();
    ctx.fill();
}

// Arrow-function wrappers resolve at call-time → device scripts can load after
const DRAWERS = {
    'Baggage Claim':             (ctx, w, h) => drawBaggageClaim(ctx, w, h),
    'Baggage Claim Information': (ctx, w, h) => drawBaggageInfo(ctx, w, h),
    'Arrival Information':       (ctx, w, h) => drawBoard(ctx, w, h, true),
    'Check-in':                  (ctx, w, h) => drawCheckin(ctx, w, h),
    'Departures Information':    (ctx, w, h) => drawBoard(ctx, w, h, false),
    'Departures Gate':           (ctx, w, h) => drawBoard(ctx, w, h, false),
    'Gate':                      (ctx, w, h) => drawBoard(ctx, w, h, false),
};
