// wwwroot/js/graafik.js
let graafik = null;
let praeguneValem = '';

function ilusSamm(vahemik) {
    const toores = vahemik / 10;
    const aste = Math.pow(10, Math.floor(Math.log10(toores)));
    const n = toores / aste;
    const kordaja = n <= 1 ? 1 : n <= 2 ? 2 : n <= 5 ? 5 : 10;
    return kordaja * aste;
}

const fmt = v => String(Math.round(v * 100) / 100);

// telgede nooled, tähised ja ekstreemumite projektsioonid
const koolipluginad = {
    id: 'koolistiil',
    afterDatasetsDraw(chart) {
        const { ctx, chartArea: a, scales: { x, y } } = chart;
        const x0 = x.getPixelForValue(0);
        const y0 = y.getPixelForValue(0);
        ctx.save();

        // ekstreemumid: punktiirjooned telgedeni ja koordinaadid
        const ekstr = chart.data.datasets[1].data;
        ctx.font = '13px sans-serif';
        ctx.fillStyle = '#000';
        ctx.strokeStyle = '#666';
        ctx.lineWidth = 1;
        ctx.setLineDash([4, 4]);
        ekstr.forEach(p => {
            const px = x.getPixelForValue(p.x);
            const py = y.getPixelForValue(p.y);
            if (py < a.top || py > a.bottom) return;
            ctx.beginPath();
            ctx.moveTo(px, py); ctx.lineTo(px, y0);
            ctx.moveTo(px, py); ctx.lineTo(x0, py);
            ctx.stroke();
        });
        ctx.setLineDash([]);
        ekstr.forEach(p => {
            const px = x.getPixelForValue(p.x);
            const py = y.getPixelForValue(p.y);
            if (py < a.top || py > a.bottom) return;
            ctx.textAlign = 'left';
            ctx.textBaseline = p.max ? 'bottom' : 'top';
            ctx.fillText('(' + fmt(p.x) + '; ' + fmt(p.y) + ')', px + 8, py + (p.max ? -6 : 6));
        });

        // nooled telgede otstes
        ctx.strokeStyle = ctx.fillStyle = '#000';
        ctx.lineWidth = 2;
        const nool = (tx, ty, nurk) => {
            ctx.beginPath();
            ctx.moveTo(tx, ty);
            ctx.lineTo(tx - 11 * Math.cos(nurk - 0.4), ty - 11 * Math.sin(nurk - 0.4));
            ctx.lineTo(tx - 11 * Math.cos(nurk + 0.4), ty - 11 * Math.sin(nurk + 0.4));
            ctx.closePath();
            ctx.fill();
        };
        nool(a.right + 4, y0, 0);
        nool(x0, a.top - 4, -Math.PI / 2);

        // tähised x, y, O
        ctx.font = 'italic 17px serif';
        ctx.textAlign = 'right';
        ctx.textBaseline = 'bottom';
        ctx.fillText('x', a.right - 4, y0 - 8);
        ctx.textAlign = 'left';
        ctx.textBaseline = 'top';
        ctx.fillText('y', x0 + 8, a.top + 2);
        ctx.font = '15px serif';
        ctx.textAlign = 'right';
        ctx.textBaseline = 'top';
        ctx.fillText('O', x0 - 5, y0 + 4);

        ctx.restore();
    }
};

async function joonistaGraafik(valem) {
    if (valem) praeguneValem = valem;
    if (!praeguneValem) return;

    const alates = parseFloat(document.getElementById('alates').value);
    const kuni = parseFloat(document.getElementById('kuni').value);
    if (isNaN(alates) || isNaN(kuni) || kuni <= alates) return;

    const samm = Math.max((kuni - alates) / 800, 0.001);
    const url = '/Funktsioonid/Punktid?valem=' + encodeURIComponent(praeguneValem) +
        '&alates=' + alates + '&kuni=' + kuni + '&samm=' + samm;

    const vastus = await fetch(url);
    if (!vastus.ok) return;
    const andmed = await vastus.json();

    const xSpan = kuni - alates;
    const yPiir = 5 * xSpan;

    const fPunktid = andmed.x.map((x, i) => {
        const y = andmed.f[i];
        return { x, y: (y === null || Math.abs(y) > 1e3) ? null : y };
    });
    const ekstr = andmed.ekstreemid.map(e => ({ x: e.x, y: e.y, max: e.max }));

    // y-telje vahemik: sisaldab nulli, kärbitud mõistlikuks
    const ys = fPunktid.map(p => p.y).filter(v => v !== null).concat(ekstr.map(e => e.y));
    let yMin = Math.min(0, ...ys), yMax = Math.max(0, ...ys);
    yMin = Math.max(yMin, -yPiir); yMax = Math.min(yMax, yPiir);
    const pad = (yMax - yMin) * 0.1 || 1;
    yMin -= pad; yMax += pad;

    const xSamm = ilusSamm(xSpan);
    const ySamm = ilusSamm(yMax - yMin);
    const xMin = Math.floor(Math.min(alates, 0) / xSamm) * xSamm;
    const xMax = Math.ceil(Math.max(kuni, 0) / xSamm) * xSamm;
    yMin = Math.floor(yMin / ySamm) * ySamm;
    yMax = Math.ceil(yMax / ySamm) * ySamm;

    const rasterVarv = c => c.tick.value === 0 ? '#000' : '#d0d7de';
    const rasterLaius = c => c.tick.value === 0 ? 2 : 1;
    const ilmaNullita = v => v === 0 ? '' : v;

    if (graafik) graafik.destroy();
    graafik = new Chart(document.getElementById('funktsiooniGraafik'), {
        type: 'line',
        data: {
            datasets: [
                { data: fPunktid, borderColor: '#1a56db', borderWidth: 2.5, pointRadius: 0, spanGaps: false },
                {
                    data: ekstr, type: 'scatter', showLine: false, pointRadius: 5,
                    pointBackgroundColor: '#fff', pointBorderColor: '#000', pointBorderWidth: 2
                }
            ]
        },
        options: {
            animation: false,
            parsing: false,
            aspectRatio: 1.3,
            layout: { padding: { top: 16, right: 16 } },
            plugins: { legend: { display: false }, tooltip: { enabled: false } },
            scales: {
                x: {
                    type: 'linear', min: xMin, max: xMax, position: { y: 0 },
                    grid: { color: rasterVarv, lineWidth: rasterLaius },
                    border: { display: false },
                    ticks: { stepSize: xSamm, callback: ilmaNullita, color: '#000', padding: 4 }
                },
                y: {
                    type: 'linear', min: yMin, max: yMax, position: { x: 0 },
                    grid: { color: rasterVarv, lineWidth: rasterLaius },
                    border: { display: false },
                    ticks: { stepSize: ySamm, callback: ilmaNullita, color: '#000', padding: 4 }
                }
            }
        },
        plugins: [koolipluginad]
    });
}

function uuendaGraafik() {
    joonistaGraafik(praeguneValem);
}