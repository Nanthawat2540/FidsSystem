// ============================================================
//  BOARD DRAWING — Departure / Arrival / Departures Gate / Gate
//  requires: C, rr  (template-draw.js)
// ============================================================

function drawHeader(ctx, w, h, title) {
    const hh = Math.max(h * .09, 12);
    ctx.fillStyle = C.header;
    ctx.fillRect(0, 0, w, hh);
    ctx.fillStyle = '#4FC3F7';
    ctx.fillRect(0, hh - 2, w, 2);
    ctx.fillStyle = C.text;
    ctx.font = `bold ${hh * .42}px Arial`;
    ctx.textAlign = 'left';
    ctx.fillText('✈  ' + title, w * .02, hh * .72);
    ctx.fillStyle = 'rgba(255,255,255,0.10)';
    rr(ctx, w * .55, hh * .12, w * .43, hh * .76, 4);
    ctx.fillStyle = C.text;
    ctx.font = `${hh * .26}px Arial`;
    ctx.textAlign = 'center';
    ctx.fillText('Surat Thani International Airport', w * .765, hh * .66);
    ctx.textAlign = 'left';
    return hh;
}

function drawColHeader(ctx, w, h, y) {
    const rh = Math.max(h * .052, 6);
    ctx.fillStyle = C.colHdr;
    ctx.fillRect(0, y, w, rh);
    ctx.fillStyle = 'rgba(255,255,255,0.45)';
    ctx.font = `bold ${rh * .52}px Arial`;
    ['FLIGHT', 'AIRLINE', 'ROUTE', 'TIME', 'GATE', 'STATUS'].forEach((c, i) => {
        const xs = [.02, .17, .31, .54, .64, .73];
        ctx.fillText(c, w * xs[i], y + rh * .76);
    });
    return rh;
}

function drawFlightRows(ctx, w, h, startY, count) {
    const fh    = h * .062;
    const avail = h - startY - fh;
    const rh    = avail / count;
    const fltN  = ['FD3210','SL412','DD9801','VZ155','FD3218','SL890','FD3225','VZ156','WE123','TG456'];
    const times = ['06:30','08:00','09:30','10:45','07:15','13:30','12:00','11:20','14:10','16:55'];
    const gates = ['A1','B1','B2','C1','A2','B3','A3','C1','B4','A4'];
    const routes = ['Bangkok','Phuket','Hat Yai','Suvar.','Chiang Mai','Udon','Krabi','BKK','Samui','Narathiwat'];
    const aaBg  = [C.airAsia,'#E53935',C.nokAir,C.airAsia,C.airAsia,'#E53935',C.airAsia,C.airAsia,'#1565C0','#6A1B9A'];
    const aaLbl = ['AA','LA','NK','VJ','AA','LA','AA','VJ','WE','TG'];

    for (let i = 0; i < count && i < fltN.length; i++) {
        const y  = startY + i * rh;
        const st = C.statuses[i % C.statuses.length];
        const fs = Math.max(rh * .30, 6);

        ctx.fillStyle = i % 2 === 0 ? C.row1 : C.row2;
        ctx.fillRect(0, y, w, rh);
        ctx.fillStyle = 'rgba(255,255,255,0.04)';
        ctx.fillRect(0, y + rh - 1, w, 1);

        ctx.fillStyle = C.blue;
        ctx.font = `bold ${fs}px Arial`;
        ctx.fillText(fltN[i], w * .02, y + rh * .66);

        ctx.fillStyle = aaBg[i];
        rr(ctx, w * .17, y + rh * .15, w * .09, rh * .70, 2);
        ctx.fillStyle = '#fff';
        ctx.font = `bold ${fs * .80}px Arial`;
        ctx.textAlign = 'center';
        ctx.fillText(aaLbl[i], w * .215, y + rh * .66);
        ctx.textAlign = 'left';

        ctx.fillStyle = C.text;
        ctx.font = `${fs * .88}px Arial`;
        ctx.fillText(routes[i], w * .31, y + rh * .66);

        ctx.fillStyle = '#FFE082';
        ctx.font = `bold ${fs}px Arial`;
        ctx.fillText(times[i], w * .54, y + rh * .66);

        ctx.fillStyle = '#1E88E5';
        rr(ctx, w * .635, y + rh * .15, w * .065, rh * .70, 2);
        ctx.fillStyle = '#fff';
        ctx.font = `bold ${fs * .76}px Arial`;
        ctx.textAlign = 'center';
        ctx.fillText(gates[i], w * .668, y + rh * .66);
        ctx.textAlign = 'left';

        ctx.fillStyle = st.bg;
        rr(ctx, w * .725, y + rh * .12, w * .265, rh * .76, 3);
        ctx.fillStyle = st.fg;
        ctx.font = `bold ${Math.max(fs * .72, 5)}px Arial`;
        ctx.textAlign = 'center';
        ctx.fillText(st.lbl, w * .858, y + rh * .65);
        ctx.textAlign = 'left';
    }
}

function drawBoardFooter(ctx, w, h, label) {
    const fh = Math.max(h * .062, 8);
    ctx.fillStyle = C.header;
    ctx.fillRect(0, h - fh, w, fh);
    ctx.fillStyle = 'rgba(255,255,255,0.6)';
    ctx.font = `${fh * .30}px Arial`;
    ctx.fillText(label, w * .02, h - fh * .24);
    ctx.fillStyle = '#091249';
    ctx.fillRect(w * .74, h - fh, w * .26, fh);
    ctx.fillStyle = '#4FC3F7';
    ctx.font = `bold ${fh * .36}px Arial`;
    ctx.textAlign = 'center';
    ctx.fillText('08:30 น.', w * .87, h - fh * .24);
    ctx.textAlign = 'left';
}

function drawBoard(ctx, w, h, isArrival) {
    ctx.fillStyle = C.bg;
    ctx.fillRect(0, 0, w, h);
    const hh  = drawHeader(ctx, w, h, isArrival ? 'ขาเข้า' : 'ขาออก');
    const chh = drawColHeader(ctx, w, h, hh);
    const fh  = h * .062;
    const rowH = h - hh - chh - fh;
    const cnt  = Math.max(1, Math.floor(rowH / Math.max(h * .065, 8)));
    drawFlightRows(ctx, w, h, hh + chh, Math.min(cnt, 10));
    drawBoardFooter(ctx, w, h, isArrival ? 'เที่ยวบินขาเข้า' : 'เที่ยวบินขาออก');
}
