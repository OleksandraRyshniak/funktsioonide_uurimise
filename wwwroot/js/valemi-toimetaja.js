// ================= LaTeX -> tekstvalem =================
function onAatom(s) {
    if (/^[A-Za-z0-9.]+$/.test(s)) return true;
    const m = s.match(/^[A-Za-z]*\(/);
    return !!m && sulguLopp(s, m[0].length - 1) === s.length;
}

function sulguLopp(s, avatud) {
    let sygavus = 0;
    for (let i = avatud; i < s.length; i++) {
        if (s[i] === '(') sygavus++;
        else if (s[i] === ')') {
            sygavus--;
            if (sygavus === 0) return i + 1;
        }
    }
    return s.length;
}

const sulgudesse = s => (onAatom(s) ? s : '(' + s + ')');

function eemaldaValjasSulud(s) {
    while (s.startsWith('(') && sulguLopp(s, 0) === s.length) s = s.slice(1, -1);
    return s;
}

function latexValemiks(latex) {
    const t = [];
    for (let i = 0; i < latex.length;) {
        const c = latex[i];
        if (/\s/.test(c)) { i++; continue; }
        if (c === '\\') {
            const m = latex.slice(i).match(/^\\([A-Za-z]+|.)/);
            t.push('\\' + m[1]);
            i += m[0].length;
            continue;
        }
        if (/\d/.test(c)) {
            const m = latex.slice(i).match(/^\d+(?:[.,]\d+)?/);
            t.push(m[0].replace(',', '.'));
            i += m[0].length;
            continue;
        }
        t.push(c);
        i++;
    }

    const funktsioonid = { '\\sin': 'Sin', '\\cos': 'Cos', '\\tan': 'Tan', '\\ln': 'Ln' };
    const aatomiAlgus = k => k !== undefined && (/^\d/.test(k) || k === 'x' || k === '(' || k === '{' ||
        ['\\pi', '\\left', '\\frac', '\\sqrt', '\\sin', '\\cos', '\\tan', '\\ln'].includes(k));

    let p = 0;
    const vaata = () => t[p];
    const votta = () => t[p++];
    const lopeb = () => p >= t.length || ['}', ')', ']', '\\right'].includes(t[p]);
    const oodata = m => { if (votta() !== m) throw new Error('Sulg puudub'); };

    function avaldis() {
        let s = '';
        if (vaata() === '-') { votta(); s = '-'; }
        else if (vaata() === '+') votta();
        s += korrutis();
        while (!lopeb() && (vaata() === '+' || vaata() === '-')) {
            const op = votta();
            s += ' ' + op + ' ' + korrutis();
        }
        return s;
    }

    function korrutis() {
        let s = tegur();
        while (!lopeb()) {
            const k = vaata();
            if (k === '+' || k === '-') break;
            if (k === '\\cdot' || k === '\\times' || k === '*') { votta(); s += '*' + tegur(); }
            else if (k === '/' || k === '\\div') { votta(); s += '/' + tegur(); }
            else if (aatomiAlgus(k)) { s += '*' + tegur(); }
            else throw new Error('Tundmatu märk: ' + k);
        }
        return s;
    }

    function tegur() {
        if (vaata() === '-') { votta(); return '(-' + tegur() + ')'; }
        if (vaata() === '+') { votta(); return tegur(); }
        let alus = aatom();
        if (vaata() === '^') {
            votta();
            alus = sulgudesse(alus) + '^' + sulgudesse(plokk());
        }
        return alus;
    }

    function plokk() {
        if (vaata() === '{') {
            votta();
            const v = avaldis();
            oodata('}');
            return v;
        }
        if (vaata() === '-') { votta(); return '-' + aatom(); }
        return aatom();
    }

    function aatom() {
        const k = votta();
        if (k === undefined) throw new Error('Täida tühjad kohad');
        if (/^\d/.test(k)) return k;
        if (k === 'x') return 'x';
        if (k === '\\pi') return 'Pi';
        if (k === '\\placeholder') throw new Error('Täida tühjad kohad');
        if (k === '(') { const v = avaldis(); oodata(')'); return '(' + v + ')'; }
        if (k === '{') { const v = avaldis(); oodata('}'); return '(' + v + ')'; }
        if (k === '\\left') {
            const d = votta();
            const v = avaldis();
            oodata('\\right');
            votta();
            if (d === '(') return '(' + v + ')';
            if (d === '|') return 'Abs(' + v + ')';
            throw new Error('Tundmatu sulg');
        }
        if (k === '\\frac') {
            const a = plokk();
            const b = plokk();
            return '(' + sulgudesse(a) + '/' + sulgudesse(b) + ')';
        }
        if (k === '\\sqrt') {
            let n = null;
            if (vaata() === '[') { votta(); n = avaldis(); oodata(']'); }
            const v = plokk();
            return n === null ? 'Sqrt(' + v + ')' : sulgudesse(v) + '^(1/' + sulgudesse(n) + ')';
        }
        if (funktsioonid[k]) {
            const arg = tegur();
            return funktsioonid[k] + (arg.startsWith('(') ? arg : '(' + arg + ')');
        }
        throw new Error('Tundmatu märk: ' + k);
    }

    const tulemus = avaldis();
    if (p < t.length) throw new Error('Üleliigne märk: ' + t[p]);
    return eemaldaValjasSulud(tulemus);
}




// ================= klaviatuur =================
const NUPUD = [
    { s: '7', ins: '7' }, { s: '8', ins: '8' }, { s: '9', ins: '9' },
    { s: '÷', ins: '\\div ', k: 'op' },
    { s: '<span class="kl-murd"><span>a</span><span>b</span></span>', ins: '\\frac{#@}{#?}', k: 'fn' },
    { s: '⌫', cmd: 'deleteBackward', k: 'kl-punane' },

    { s: '4', ins: '4' }, { s: '5', ins: '5' }, { s: '6', ins: '6' },
    { s: '×', ins: '\\cdot ', k: 'op' },
    { s: 'x<sup>2</sup>', ins: '#@^2', k: 'fn' },
    { s: 'x<sup>n</sup>', ins: '#@^{#?}', k: 'fn' },

    { s: '1', ins: '1' }, { s: '2', ins: '2' }, { s: '3', ins: '3' },
    { s: '−', ins: '-', k: 'op' },
    { s: '√', ins: '\\sqrt{#0}', k: 'fn' },
    { s: '<sup>n</sup>√', ins: '\\sqrt[#?]{#0}', k: 'fn' },

    { s: '0', ins: '0' }, { s: '.', ins: '.' },
    { s: 'π', ins: '\\pi ', k: 'fn' },
    { s: '+', ins: '+', k: 'op' },
    { s: '<i>x</i>', ins: 'x', k: 'fn' },
    { s: '|x|', ins: '\\left|#0\\right|', k: 'fn' },

    { s: 'sin', ins: '\\sin\\left(#?\\right)', k: 'fn' },
    { s: 'cos', ins: '\\cos\\left(#?\\right)', k: 'fn' },
    { s: 'tan', ins: '\\tan\\left(#?\\right)', k: 'fn' },
    { s: 'ln', ins: '\\ln\\left(#?\\right)', k: 'fn' },
    { s: '◀', cmd: 'moveToPreviousChar' },
    { s: '▶', cmd: 'moveToNextChar' },

    { s: '( )', ins: '\\left(#0\\right)', k: 'op', lai: 3 },
    { s: 'Tühjenda', cmd: 'clear', k: 'kl-punane', lai: 3 }
];

customElements.whenDefined('math-field').then(() => {
    const mf = document.getElementById('matemaatikaVali');
    const valem = document.getElementById('Valem');
    const viga = document.getElementById('valemiViga');
    const kl = document.getElementById('klaviatuur');
    if (!mf || !valem || !kl) return;

    if (window.MathfieldElement) MathfieldElement.soundsDirectory = null;
    mf.mathVirtualKeyboardPolicy = 'manual';

    // klaviatuuri nupud
    NUPUD.forEach(n => {
        const b = document.createElement('button');
        b.type = 'button';
        b.innerHTML = n.s;
        if (n.k) b.className = n.k;
        if (n.lai) b.style.gridColumn = 'span ' + n.lai;
        b.addEventListener('click', () => {
            mf.focus();
            if (n.ins !== undefined) mf.insert(n.ins);
            else if (n.cmd === 'clear') mf.setValue('');
            else if (n.cmd) mf.executeCommand(n.cmd);
            sync();
        });
        kl.appendChild(b);
    });
    kl.addEventListener('mousedown', e => e.preventDefault());

    // olemasolev valem (muutmine / vigane vorm)
    if (valem.value.trim()) {
        try { mf.setValue(valemLatexiks(valem.value)); }
        catch { mf.setValue(valem.value); }
    }

    let taimer;
    function sync() {
        const latex = mf.getValue('latex');
        if (!latex.trim()) {
            valem.value = '';
            viga.textContent = '';
            return;
        }
        try {
            valem.value = latexValemiks(latex);
            viga.textContent = '';
            clearTimeout(taimer);
            taimer = setTimeout(() => {
                if (typeof uuendaGraafik === 'function') uuendaGraafik();
            }, 300);
        } catch (e) {
            viga.textContent = e.message;
        }
    }
    mf.addEventListener('input', sync);
});