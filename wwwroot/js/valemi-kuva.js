// tekstvalem -> LaTeX (õpikustiil)
function valemLatexiks(valem) {
    const t = valem.match(/\d+(?:\.\d+)?|[A-Za-z]+|[^\s]/g) || [];
    const funktsioonid = { sqrt: 1, sin: '\\sin', cos: '\\cos', tan: '\\tan', ln: '\\ln', abs: 1 };
    let p = 0;
    const vaata = () => t[p];
    const votta = () => t[p++];

    // 2x, 3\sin(x), x(x+1) -- ilma korrutusmärgita, nagu õpikus
    const lopebTegurina = s => /^-?(\d+(\.\d+)?|x|e)$/.test(s) || s.endsWith('\\right)') || s.endsWith('\\pi ')
        || (s.startsWith('\\frac{') && s.endsWith('}'));
    const algabTahega = s => /^(x|e|\\pi|\\sqrt|\\sin|\\cos|\\tan|\\ln|\\left\()/.test(s);

    function avaldis() {
        let s = '';
        if (vaata() === '-') { votta(); s = '-'; }
        else if (vaata() === '+') votta();
        s += korrutis().t;
        while (vaata() === '+' || vaata() === '-') {
            const op = votta();
            s += op + korrutis().t;
        }
        return s;
    }

    function korrutis() {
        let cur = aste();
        while (vaata() === '*' || vaata() === '/') {
            const op = votta();
            const r = aste();
            if (op === '*') {
                const mark = lopebTegurina(cur.t) && algabTahega(r.t) ? '' : '\\cdot ';
                cur = { t: cur.t + mark + r.t };
            } else {
                cur = { t: '\\frac{' + (cur.g ?? cur.t) + '}{' + (r.g ?? r.t) + '}' };
            }
        }
        return cur;
    }

    function aste() {
        if (vaata() === '-') { votta(); return { t: '-' + aste().t }; }
        const alus = aatom();
        if (vaata() === '^') {
            votta();
            let miinus = '';
            if (vaata() === '-') { votta(); miinus = '-'; }
            const e = aste();
            return { t: alus.t + '^{' + miinus + (e.g ?? e.t) + '}' };
        }
        return alus;
    }

    function aatom() {
        const k = votta();
        if (k === undefined) throw new Error('pooleli');
        if (/^\d/.test(k)) return { t: k };
        if (k === '(') {
            const v = avaldis();
            if (votta() !== ')') throw new Error('sulg');
            return { t: '\\left(' + v + '\\right)', g: v };
        }
        const nimi = k.toLowerCase();
        if (nimi === 'x') return { t: 'x' };
        if (nimi === 'e') return { t: 'e' };
        if (nimi === 'pi' || k === 'π') return { t: '\\pi ' };
        if (funktsioonid[nimi]) {
            if (votta() !== '(') throw new Error('sulg');
            const v = avaldis();
            if (votta() !== ')') throw new Error('sulg');
            if (nimi === 'sqrt') return { t: '\\sqrt{' + v + '}' };
            if (nimi === 'abs') return { t: '\\left|' + v + '\\right|' };
            return { t: funktsioonid[nimi] + '\\left(' + v + '\\right)' };
        }
        throw new Error('Tundmatu: ' + k);
    }

    const tulemus = avaldis();
    if (p < t.length) throw new Error('Üleliigne märk');
    return tulemus;
}

// kõik [data-valem] elemendid -> ilus valem
function naitaValemeid() {
    document.querySelectorAll('[data-valem]').forEach(el => {
        const tekst = el.dataset.valem;
        let latex;
        try { latex = valemLatexiks(tekst); }
        catch { return; }               // jääb tavaline tekst
        const mf = document.createElement('math-field');
        mf.className = 'valem-kuva';
        mf.setAttribute('read-only', '');
        mf.setAttribute('math-virtual-keyboard-policy', 'manual');
        el.replaceChildren(mf);
        mf.setValue(latex);
    });
}

customElements.whenDefined('math-field').then(naitaValemeid);