// ============================================================
//  CREATE PAGE UI  (requires template-draw.js + device scripts)
// ============================================================

// ---- canvas thumbnail per card ----
function drawCard(card) {
    const canvas = card.querySelector('.dt-canvas');
    const wrap   = card.querySelector('.dt-canvas-wrap');
    if (!canvas || !wrap) return;

    const ratio      = card.dataset.ratio || '16:9';
    const isPortrait = ratio === '9:16';
    const wrapW = wrap.clientWidth  || 160;
    const wrapH = wrap.clientHeight || 110;

    let cw, ch;
    if (isPortrait) {
        ch = wrapH;
        cw = Math.round(ch * 9 / 16);
    } else {
        cw = wrapW;
        ch = Math.round(cw * 9 / 16);
        if (ch > wrapH) { ch = wrapH; cw = Math.round(ch * 16 / 9); }
    }
    if (cw <= 0 || ch <= 0) return;

    canvas.width  = cw;
    canvas.height = ch;
    canvas.style.width  = cw + 'px';
    canvas.style.height = ch + 'px';

    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, cw, ch);
    const type = card.dataset.type;
    if (DRAWERS[type]) DRAWERS[type](ctx, cw, ch);
}

function drawAllCards() {
    document.querySelectorAll('.device-type-card').forEach(card => drawCard(card));
}

// ---- ratio selection ----
const ratiosByType = {
    'Baggage Claim':             [{ lbl:'Horizontal 16:9', val:'16:9', cls:'ratio-box-h' }, { lbl:'Vertical 9:16', val:'9:16', cls:'ratio-box-v' }],
    'Baggage Claim Information': [{ lbl:'Vertical 9:16',   val:'9:16', cls:'ratio-box-v' }, { lbl:'Horizontal 16:9', val:'16:9', cls:'ratio-box-h' }],
    'Arrival Information':       [{ lbl:'Horizontal 16:9', val:'16:9', cls:'ratio-box-h' }, { lbl:'Vertical 9:16', val:'9:16', cls:'ratio-box-v' }],
    'Check-in':                  [{ lbl:'Horizontal 16:9', val:'16:9', cls:'ratio-box-h' }, { lbl:'Vertical 9:16', val:'9:16', cls:'ratio-box-v' }],
    'Departures Information':    [{ lbl:'Horizontal 16:9', val:'16:9', cls:'ratio-box-h' }, { lbl:'Vertical 9:16', val:'9:16', cls:'ratio-box-v' }],
    'Departures Gate':           [{ lbl:'Vertical 9:16',   val:'9:16', cls:'ratio-box-v' }, { lbl:'Horizontal 16:9', val:'16:9', cls:'ratio-box-h' }],
    'Gate':                      [{ lbl:'Vertical 9:16',   val:'9:16', cls:'ratio-box-v' }, { lbl:'Horizontal 16:9', val:'16:9', cls:'ratio-box-h' }],
};

function renderRatios(type) {
    const ratios = ratiosByType[type] || [];
    const grid   = document.getElementById('ratioGrid');
    if (!grid) return;
    grid.innerHTML = '';
    ratios.forEach((r, i) => {
        const label = document.createElement('label');
        label.className = 'ratio-card' + (i === 0 ? ' selected' : '');
        label.innerHTML = `
            <div class="${r.cls}"></div>
            <input type="radio" name="Ratio" value="${r.val}" ${i === 0 ? 'checked' : ''} required style="display:none;">
            <div class="ratio-label">${r.lbl}</div>`;
        label.addEventListener('click', () => {
            document.querySelectorAll('.ratio-card').forEach(c => c.classList.remove('selected'));
            label.classList.add('selected');
        });
        grid.appendChild(label);
    });
    const section = document.getElementById('ratioSection');
    if (section) section.style.display = ratios.length ? 'block' : 'none';
}

// ---- card click ----
document.querySelectorAll('.device-type-card').forEach(card => {
    card.addEventListener('click', function () {
        document.querySelectorAll('.device-type-card').forEach(c => c.classList.remove('selected'));
        this.classList.add('selected');
        const radio = this.querySelector('input[type="radio"]');
        if (radio) radio.checked = true;
        renderRatios(this.dataset.type);
    });
});

// ---- init: double-RAF ensures layout is complete ----
function init() {
    requestAnimationFrame(() => requestAnimationFrame(() => drawAllCards()));
}
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
} else {
    init();
}
