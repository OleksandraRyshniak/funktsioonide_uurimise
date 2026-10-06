// ================= LaTeX -> tekstvalem =================

function onAatom(s) {
    if (/^[A-Za-z0-9.]+$/.test(s)) return true;

    const m = s.match(/^[A-Za-z]*\(/);

    return !!m && sulguLopp(s, m[0].length - 1) === s.length;
}

function sulguLopp(s, avatud) {
    let sygavus = 0;

    for (let i = avatud; i < s.length; i++) {
        if (s[i] === '(') {
            sygavus++;
        }
        else if (s[i] === ')') {
            sygavus--;

            if (sygavus === 0) {
                return i + 1;
            }
        }
    }

    return s.length;
}

function sulgudesse(s) {
    if (onAatom(s)) {
        return s;
    }

    return '(' + s + ')';
}

function eemaldaValjasSulud(s) {
    while (s.startsWith('(') && sulguLopp(s, 0) === s.length) {
        s = s.slice(1, -1);
    }

    return s;
}

function latexValemiks(latex) {
    const t = [];

    for (let i = 0; i < latex.length;) {
        const c = latex[i];

        if (/\s/.test(c)) {
            i++;
            continue;
        }

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

    const funktsioonid = {
        '\\sin': 'Sin',
        '\\cos': 'Cos',
        '\\tan': 'Tan',
        '\\ln': 'Ln',
        '\\log': 'Log'
    };

    function aatomiAlgus(k) {
        if (k === undefined) {
            return false;
        }

        if (/^\d/.test(k)) {
            return true;
        }

        if (k === 'x' ||
            k === 'e' ||
            k === '\\exponentialE' ||
            k === '(' ||
            k === '{') {
            return true;
        }

        if (k === '\\pi' ||
            k === '\\left' ||
            k === '\\frac' ||
            k === '\\sqrt' ||
            k === '\\sin' ||
            k === '\\cos' ||
            k === '\\tan' ||
            k === '\\ln' ||
            k === '\\log') {
            return true;
        }

        return false;
    }

    let p = 0;

    function vaata() {
        return t[p];
    }

    function votta() {
        return t[p++];
    }

    function lopeb() {
        if (p >= t.length) {
            return true;
        }

        if (t[p] === '}' ||
            t[p] === ')' ||
            t[p] === ']' ||
            t[p] === '\\right') {
            return true;
        }

        return false;
    }

    function oodata(m) {
        if (votta() !== m) {
            throw new Error('Sulg puudub');
        }
    }

    function avaldis() {
        let s = '';

        if (vaata() === '-') {
            votta();
            s = '-';
        }
        else if (vaata() === '+') {
            votta();
        }

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

            if (k === '+' || k === '-') {
                break;
            }

            if (k === '\\cdot' || k === '\\times' || k === '*') {
                votta();
                s += '*' + tegur();
            }
            else if (k === '/' || k === '\\div') {
                votta();
                s += '/' + tegur();
            }
            else if (aatomiAlgus(k)) {
                s += '*' + tegur();
            }
            else {
                throw new Error('Tundmatu märk: ' + k);
            }
        }

        return s;
    }

    function tegur() {

        if (vaata() === '-') {
            votta();
            return '(-' + tegur() + ')';
        }

        if (vaata() === '+') {
            votta();
            return tegur();
        }

        let alus = aatom();

        while (vaata() === '^') {

            votta();

            let aste;

            if (vaata() === '{') {

                votta();

                aste = avaldis();

                oodata('}');
            }
            else {

                aste = aatom();
            }

            alus =
                sulgudesse(alus) +
                '^' +
                sulgudesse(aste);
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

        if (vaata() === '-') {
            votta();

            return '-' + aatom();
        }

        return aatom();
    }

    function aatom() {
        const k = votta();

        if (k === undefined) {
            throw new Error('Täida tühjad kohad');
        }

        if (/^\d/.test(k)) {
            return k;
        }

        if (k === 'x') {
            return 'x';
        }

        if (k === 'e' || k === '\\exponentialE') {
            return 'e';
        }

        if (k === '\\pi') {
            return 'Pi';
        }

        if (k === '\\placeholder') {
            throw new Error('Täida tühjad kohad');
        }

        if (k === '(') {
            const v = avaldis();

            oodata(')');

            return '(' + v + ')';
        }

        if (k === '{') {
            const v = avaldis();

            oodata('}');

            return '(' + v + ')';
        }

        if (k === '\\left') {
            const d = votta();
            const v = avaldis();

            oodata('\\right');
            votta();

            if (d === '(') {
                return '(' + v + ')';
            }

            if (d === '|') {
                return 'Abs(' + v + ')';
            }

            throw new Error('Tundmatu sulg');
        }

        if (k === '\\frac') {
            const a = plokk();
            const b = plokk();

            return '(' + sulgudesse(a) + '/' + sulgudesse(b) + ')';
        }

        if (k === '\\sqrt') {
            let n = null;

            if (vaata() === '[') {
                votta();

                n = avaldis();

                oodata(']');
            }

            const v = plokk();

            if (n === null) {
                return 'Sqrt(' + v + ')';
            }

            return sulgudesse(v) + '^(1/' + sulgudesse(n) + ')';
        }

        if (funktsioonid[k]) {
            const arg = tegur();

            if (arg.startsWith('(')) {
                return funktsioonid[k] + arg;
            }

            return funktsioonid[k] + '(' + arg + ')';
        }

        throw new Error('Tundmatu märk: ' + k);
    }

    const tulemus = avaldis();

    if (p < t.length) {
        throw new Error('Üleliigne märk: ' + t[p]);
    }

    return eemaldaValjasSulud(tulemus);
}


// ================= klaviatuuri nupud =================

const NUPUD = {

    pohi: [
        { s: '7', ins: '7' },
        { s: '8', ins: '8' },
        { s: '9', ins: '9' },
        { s: '÷', ins: '\\div ', k: 'op' },
        {
            s: '<span class="kl-murd"><span>a</span><span>b</span></span>',
            ins: '\\frac{#@}{#?}',
            k: 'fn'
        },
        { s: '⌫', cmd: 'deleteBackward', k: 'kl-punane' },

        { s: '4', ins: '4' },
        { s: '5', ins: '5' },
        { s: '6', ins: '6' },
        { s: '×', ins: '\\cdot ', k: 'op' },
        { s: 'x<sup>2</sup>', ins: 'x^{2}', k: 'fn' },
        { s: 'x<sup>n</sup>', ins: 'x^{#?}', k: 'fn' },

        { s: '1', ins: '1' },
        { s: '2', ins: '2' },
        { s: '3', ins: '3' },
        { s: '−', ins: '-', k: 'op' },
        { s: '√', ins: '\\sqrt{#0}', k: 'fn' },
        { s: '<sup>n</sup>√', ins: '\\sqrt[#?]{#0}', k: 'fn' },

        { s: '0', ins: '0' },
        { s: '.', ins: '.' },
        { s: 'π', ins: '\\pi ', k: 'fn' },
        { s: '+', ins: '+', k: 'op' },
        { s: '<i>x</i>', ins: 'x', k: 'fn' },
        { s: '|x|', ins: '\\left|#0\\right|', k: 'fn' },

        { s: '( )', ins: '\\left(#0\\right)', k: 'op', lai: 2 },
        { s: '<i>e</i>', ins: 'e', k: 'fn' },
        { s: '◀', cmd: 'moveToPreviousChar' },
        { s: '▶', cmd: 'moveToNextChar' },
        { s: 'Tühjenda', cmd: 'clear', k: 'kl-punane', lai: 3 }
    ],

    trig: [
        { s: 'sin', ins: '\\sin\\left(#?\\right)', k: 'fn' },
        { s: 'cos', ins: '\\cos\\left(#?\\right)', k: 'fn' },
        { s: 'tan', ins: '\\tan\\left(#?\\right)', k: 'fn' },

        { s: 'π', ins: '\\pi ', k: 'fn' },
        { s: '<i>x</i>', ins: 'x', k: 'fn' },
        { s: '( )', ins: '\\left(#0\\right)', k: 'op' },

        { s: '+', ins: '+', k: 'op' },
        { s: '−', ins: '-', k: 'op' },
        { s: '×', ins: '\\cdot ', k: 'op' },

        { s: '⌫', cmd: 'deleteBackward', k: 'kl-punane' },
        { s: '◀', cmd: 'moveToPreviousChar' },
        { s: '▶', cmd: 'moveToNextChar' }
    ],

    log: [
        { s: 'ln', ins: '\\ln\\left(#?\\right)', k: 'fn' },
        { s: 'log', ins: '\\log\\left(#?\\right)', k: 'fn' },

        { s: '<i>e</i>', ins: 'e', k: 'fn' },
        { s: '<i>x</i>', ins: 'x', k: 'fn' },

        { s: 'x<sup>2</sup>', ins: 'x^{2}', k: 'fn' },
        { s: 'x<sup>n</sup>', ins: 'x^{#?}', k: 'fn' },

        { s: '√', ins: '\\sqrt{#0}', k: 'fn' },
        { s: '( )', ins: '\\left(#0\\right)', k: 'op' },

        { s: '⌫', cmd: 'deleteBackward', k: 'kl-punane' },
        { s: '◀', cmd: 'moveToPreviousChar' },
        { s: '▶', cmd: 'moveToNextChar' }
    ],

    muu: [
        { s: '√', ins: '\\sqrt{#0}', k: 'fn' },
        { s: '<sup>n</sup>√', ins: '\\sqrt[#?]{#0}', k: 'fn' },

        {
            s: '<span class="kl-murd"><span>a</span><span>b</span></span>',
            ins: '\\frac{#@}{#?}',
            k: 'fn'
        },
        { s: 'x<sup>2</sup>', ins: 'x^{2}', k: 'fn' },
        { s: 'x<sup>n</sup>', ins: 'x^{#?}', k: 'fn' },
        { s: '|x|', ins: '\\left|#0\\right|', k: 'fn' },

        { s: 'π', ins: '\\pi ', k: 'fn' },
        { s: '<i>e</i>', ins: 'e', k: 'fn' },
        { s: '<i>x</i>', ins: 'x', k: 'fn' },

        { s: '( )', ins: '\\left(#0\\right)', k: 'op' },

        { s: '⌫', cmd: 'deleteBackward', k: 'kl-punane' },
        { s: 'Tühjenda', cmd: 'clear', k: 'kl-punane' }
    ]
};


// ================= MathLive =================

customElements.whenDefined('math-field').then(function () {

    const mf = document.getElementById('matemaatikaVali');
    const valem = document.getElementById('Valem');
    const viga = document.getElementById('valemiViga');
    const kl = document.getElementById('klaviatuur');

    if (!mf || !valem || !kl) {
        return;
    }

    if (window.MathfieldElement) {
        MathfieldElement.soundsDirectory = null;
    }

    mf.mathVirtualKeyboardPolicy = 'manual';


    // ================= klaviatuuri loomine =================

    function looNupud(nupud) {
        kl.innerHTML = '';

        nupud.forEach(function (n) {

            const b = document.createElement('button');

            b.type = 'button';
            b.innerHTML = n.s;

            if (n.k) {
                b.className = n.k;
            }

            if (n.lai) {
                b.style.gridColumn = 'span ' + n.lai;
            }

            b.addEventListener('click', function () {

                mf.focus();

                if (n.ins !== undefined) {
                    mf.insert(n.ins);
                }
                else if (n.cmd === 'clear') {
                    mf.setValue('');
                }
                else if (n.cmd) {
                    mf.executeCommand(n.cmd);
                }

                sync();
            });

            kl.appendChild(b);
        });
    }


    // vaikimisi põhiklaviatuur

    looNupud(NUPUD.pohi);


    // ================= kategooriate vahetamine =================

    const kategooriaNupud =
        document.querySelectorAll('[data-klaviatuur]');

    kategooriaNupud.forEach(function (nupp) {

        nupp.addEventListener('click', function () {

            const kategooria =
                nupp.getAttribute('data-klaviatuur');

            if (kategooria === 'pohi') {
                looNupud(NUPUD.pohi);
            }

            if (kategooria === 'trig') {
                looNupud(NUPUD.trig);
            }

            if (kategooria === 'log') {
                looNupud(NUPUD.log);
            }

            if (kategooria === 'muu') {
                looNupud(NUPUD.muu);
            }


            // eemaldame aktiivse klassi

            kategooriaNupud.forEach(function (teineNupp) {
                teineNupp.classList.remove('active');
            });


            // valitud kategooria aktiivseks

            nupp.classList.add('active');
        });
    });


    kl.addEventListener('mousedown', function (e) {
        e.preventDefault();
    });


    // ================= olemasolev valem =================

    if (valem.value.trim()) {
        try {
            mf.setValue(valemLatexiks(valem.value));
        }
        catch {
            mf.setValue(valem.value);
        }
    }


    // ================= valemi sünkroniseerimine =================

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

            taimer = setTimeout(function () {

                if (typeof uuendaGraafik === 'function') {
                    uuendaGraafik();
                }

            }, 300);
        }
        catch (e) {
            viga.textContent = e.message;
        }
    }


    mf.addEventListener('input', sync);
});