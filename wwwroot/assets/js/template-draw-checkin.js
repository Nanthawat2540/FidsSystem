// ============================================================
//  CHECK-IN DRAWING  (landscape 16:9)
//  requires: C, rr  (template-draw.js)
// ============================================================

function drawCheckin(ctx, w, h) {
    ctx.fillStyle = '#1C1C1E';
    ctx.fillRect(0, 0, w, h);

    ctx.fillStyle = '#fff';
    ctx.font = `bold ${h * .068}px Arial`;
    ctx.textAlign = 'center';
    ctx.fillText('เคาน์เตอร์เช็คอิน', w * .5, h * .2);
    ctx.font = `${h * .052}px Arial`;
    ctx.fillText('ทุกสายการบิน', w * .5, h * .3);

    const cg = ctx.createLinearGradient(w * .1, h * .36, w * .9, h * .66);
    cg.addColorStop(0, '#1565C0'); cg.addColorStop(1, '#0a1a6e');
    ctx.fillStyle = cg;
    rr(ctx, w * .08, h * .36, w * .84, h * .31, 10);

    ctx.fillStyle = '#fff';
    ctx.font = `${h * .088}px Arial`;
    ctx.fillText('✈', w * .22, h * .58);
    ctx.font = `bold ${h * .072}px Arial`;
    ctx.fillText('ทุกสายการบิน', w * .55, h * .57);
    ctx.textAlign = 'left';

    ctx.fillStyle = C.navy;
    ctx.fillRect(0, h * .8, w, h * .2);
    ctx.fillStyle = '#fff';
    ctx.font = `${h * .044}px Arial`;
    ctx.fillText('Surat Thani Airport', w * .04, h * .93);
    ctx.fillStyle = 'rgba(255,255,255,0.7)';
    ctx.textAlign = 'right';
    ctx.fillText('08:30 น.', w * .96, h * .93);
    ctx.textAlign = 'left';
}
