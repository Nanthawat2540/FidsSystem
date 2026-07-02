// ============================================================
//  BAGGAGE CLAIM DRAWING — single flight display (landscape 16:9)
//  requires: C, rr  (template-draw.js)
// ============================================================

function drawBaggageClaim(ctx, w, h) {
    ctx.fillStyle = '#111';
    ctx.fillRect(0, 0, w, h);
    const topH = h * .28;

    ctx.fillStyle = C.airAsia;
    ctx.fillRect(0, 0, w * .38, topH);
    ctx.fillStyle = '#fff';
    ctx.font = `bold ${topH * .22}px Arial`;
    ctx.textAlign = 'center';
    ctx.fillText('AirAsia', w * .19, topH * .62);

    ctx.fillStyle = '#1a1a1a';
    ctx.fillRect(w * .38, 0, w * .62, topH);
    ctx.fillStyle = 'rgba(255,255,255,0.38)';
    ctx.font = `${topH * .12}px Arial`;
    ctx.fillText('เที่ยวบิน', w * .69, topH * .3);
    ctx.fillStyle = '#fff';
    ctx.font = `bold ${topH * .30}px Arial`;
    ctx.fillText('FD3210', w * .69, topH * .72);
    ctx.textAlign = 'left';

    const g = ctx.createLinearGradient(0, topH, 0, h * .8);
    g.addColorStop(0, '#1565C0'); g.addColorStop(1, '#0a1a6e');
    ctx.fillStyle = g;
    ctx.fillRect(0, topH, w, h * .52);

    ctx.fillStyle = 'rgba(255,255,255,0.4)';
    ctx.font = `${h * .048}px Arial`;
    ctx.textAlign = 'center';
    ctx.fillText('จาก', w * .5, topH + h * .14);
    ctx.fillStyle = '#fff';
    ctx.font = `bold ${h * .11}px Arial`;
    ctx.fillText('ดอนเมือง', w * .5, topH + h * .36);
    ctx.textAlign = 'left';

    ctx.fillStyle = C.navy;
    ctx.fillRect(0, h * .8, w, h * .2);
    ctx.fillStyle = '#A5D6A7';
    ctx.font = `bold ${h * .052}px Arial`;
    ctx.fillText('รับสัมภาระครบแล้ว', w * .04, h * .925);
}
