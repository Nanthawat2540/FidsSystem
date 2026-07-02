// ============================================================
//  BAGGAGE CLAIM INFO DRAWING — belt list display (portrait 9:16)
//  requires: C, rr  (template-draw.js)
// ============================================================

function drawBaggageInfo(ctx, w, h) {
    ctx.fillStyle = '#111';
    ctx.fillRect(0, 0, w, h);
    const bh = h * .3;

    ctx.fillStyle = '#00BFFF';
    ctx.fillRect(0, 0, w, bh);
    ctx.fillStyle = '#000';
    ctx.font = `bold ${bh * .14}px Arial`;
    ctx.textAlign = 'center';
    ctx.fillText('BELT NO.', w * .5, bh * .3);
    ctx.font = `bold ${bh * .44}px Arial`;
    ctx.fillText('1', w * .5, bh * .88);
    ctx.textAlign = 'left';

    const rowH = (h * .68) / 3;
    const stBg = ['#00CC66', '#00CC66', '#FF3333'];
    ['FD3210', 'SL412', 'DD980'].forEach((f, i) => {
        const y = bh + i * rowH;
        ctx.fillStyle = '#111';
        ctx.fillRect(0, y, w, rowH - 1);
        ctx.strokeStyle = '#2a2a2a'; ctx.lineWidth = 1;
        ctx.strokeRect(0, y, w, rowH - 1);

        ctx.fillStyle = C.airAsia;
        rr(ctx, w * .02, y + rowH * .12, w * .13, rowH * .76, 2);
        ctx.fillStyle = '#fff';
        ctx.font = `bold ${rowH * .24}px Arial`;
        ctx.textAlign = 'center';
        ctx.fillText('AA', w * .085, y + rowH * .65);
        ctx.textAlign = 'left';

        ctx.fillStyle = '#fff';
        ctx.font = `bold ${rowH * .26}px Arial`;
        ctx.fillText(f, w * .18, y + rowH * .5);
        ctx.fillStyle = '#ccc';
        ctx.font = `${rowH * .20}px Arial`;
        ctx.fillText('Bangkok', w * .18, y + rowH * .78);

        ctx.fillStyle = stBg[i];
        rr(ctx, w * .7, y + rowH * .18, w * .27, rowH * .65, 4);
        ctx.fillStyle = '#000';
        ctx.font = `bold ${rowH * .20}px Arial`;
        ctx.textAlign = 'center';
        ctx.fillText(i < 2 ? 'OPEN' : 'CLOSED', w * .835, y + rowH * .62);
        ctx.textAlign = 'left';
    });

    ctx.fillStyle = '#00BFFF';
    ctx.fillRect(0, h - h * .075, w, h * .075);
    ctx.fillStyle = '#000';
    ctx.font = `bold ${h * .032}px Arial`;
    ctx.textAlign = 'center';
    ctx.fillText('BAGGAGE CLAIM', w * .5, h * .975);
    ctx.textAlign = 'left';
}
