// =============================================
// Tekstvalem -> LaTeX
// Näiteks:
// 2*x -> 2x
// x^2 -> x²
// 1/x -> murd
// Sqrt(x) -> ruutjuur
// Sin(x) -> sin(x)
// =============================================

function valemLatexiks(valem) {

    if (!valem) {
        return '';
    }

    const t =
        valem.match(/\d+(?:\.\d+)?|[A-Za-z]+|π|[^\s]/g) || [];

    let p = 0;


    function vaata() {
        return t[p];
    }


    function votta() {
        return t[p++];
    }


    function onFunktsioon(nimi) {

        nimi = nimi.toLowerCase();

        if (nimi === 'sqrt') return true;
        if (nimi === 'sin') return true;
        if (nimi === 'cos') return true;
        if (nimi === 'tan') return true;
        if (nimi === 'ln') return true;
        if (nimi === 'log') return true;
        if (nimi === 'abs') return true;

        return false;
    }


    // =============================================
    // Liitmine ja lahutamine
    // =============================================

    function avaldis() {

        let vasak = korrutis();

        while (vaata() === '+' || vaata() === '-') {

            const mark = votta();
            const parem = korrutis();

            if (mark === '+') {
                vasak = vasak + ' + ' + parem;
            }
            else {
                vasak = vasak + ' - ' + parem;
            }
        }

        return vasak;
    }


    // =============================================
    // Korrutamine ja jagamine
    // =============================================

    function korrutis() {

        let vasak = aste();

        while (vaata() === '*' || vaata() === '/') {

            const mark = votta();
            const parem = aste();

            if (mark === '*') {

                if (vajabKorrutusmarki(vasak, parem)) {
                    vasak = vasak + '\\cdot ' + parem;
                }
                else {
                    vasak = vasak + parem;
                }
            }
            else {
                vasak =
                    '\\frac{' +
                    eemaldaValisedSulud(vasak) +
                    '}{' +
                    eemaldaValisedSulud(parem) +
                    '}';
            }
        }

        return vasak;
    }


    // =============================================
    // Astmed
    // =============================================

    function aste() {

        if (vaata() === '-') {

            votta();

            return '-' + aste();
        }

        if (vaata() === '+') {

            votta();

            return aste();
        }

        let alus = aatom();

        if (vaata() === '^') {

            votta();

            let eksponent = aste();

            alus =
                alus +
                '^{' +
                eemaldaValisedSulud(eksponent) +
                '}';
        }

        return alus;
    }


    // =============================================
    // Arv, x, sulud, funktsioon
    // =============================================

    function aatom() {

        const k = votta();

        if (k === undefined) {
            throw new Error('Valem on pooleli');
        }


        // arv

        if (/^\d/.test(k)) {
            return k;
        }


        // sulud

        if (k === '(') {

            const sisu = avaldis();

            if (votta() !== ')') {
                throw new Error('Sulg puudub');
            }

            return '\\left(' + sisu + '\\right)';
        }


        const nimi = k.toLowerCase();


        // x

        if (nimi === 'x') {
            return 'x';
        }


        // e

        if (nimi === 'e') {
            return 'e';
        }


        // pi

        if (nimi === 'pi' || k === 'π') {
            return '\\pi';
        }


        // funktsioonid

        if (onFunktsioon(nimi)) {

            if (votta() !== '(') {
                throw new Error('Funktsioonil puuduvad sulud');
            }

            const sisu = avaldis();

            if (votta() !== ')') {
                throw new Error('Funktsioonil puudub sulg');
            }


            // ruutjuur

            if (nimi === 'sqrt') {

                return (
                    '\\sqrt{' +
                    eemaldaValisedSulud(sisu) +
                    '}'
                );
            }


            // absoluutväärtus

            if (nimi === 'abs') {

                return (
                    '\\left|' +
                    sisu +
                    '\\right|'
                );
            }


            // siinus

            if (nimi === 'sin') {

                return (
                    '\\sin\\left(' +
                    sisu +
                    '\\right)'
                );
            }


            // koosinus

            if (nimi === 'cos') {

                return (
                    '\\cos\\left(' +
                    sisu +
                    '\\right)'
                );
            }


            // tangens

            if (nimi === 'tan') {

                return (
                    '\\tan\\left(' +
                    sisu +
                    '\\right)'
                );
            }


            // naturaallogaritm

            if (nimi === 'ln') {

                return (
                    '\\ln\\left(' +
                    sisu +
                    '\\right)'
                );
            }


            // logaritm

            if (nimi === 'log') {

                return (
                    '\\log\\left(' +
                    sisu +
                    '\\right)'
                );
            }
        }


        throw new Error('Tundmatu märk: ' + k);
    }


    // =============================================
    // Kas korrutusmärk peab olema nähtav?
    // =============================================

    function vajabKorrutusmarki(vasak, parem) {

        // 2x
        if (/^\d/.test(vasak) && parem === 'x') {
            return false;
        }

        // 2e
        if (/^\d/.test(vasak) && parem === 'e') {
            return false;
        }

        // 2π
        if (/^\d/.test(vasak) && parem.startsWith('\\pi')) {
            return false;
        }

        // 2sin(x), 2cos(x) jne
        if (/^\d/.test(vasak) && parem.startsWith('\\')) {
            return false;
        }

        // x(...)
        if (vasak === 'x' && parem.startsWith('\\left(')) {
            return false;
        }

        return true;
    }


    // =============================================
    // Eemaldame ainult LaTeXi välised sulud
    // =============================================

    function eemaldaValisedSulud(tekst) {

        if (tekst.startsWith('\\left(') &&
            tekst.endsWith('\\right)')) {

            return tekst.substring(
                6,
                tekst.length - 7
            );
        }

        return tekst;
    }


    const tulemus = avaldis();

    if (p < t.length) {
        throw new Error(
            'Üleliigne märk: ' + t[p]
        );
    }

    return tulemus;
}


// =============================================
// Kõik data-valem elemendid
// =============================================

function naitaValemeid() {

    const elemendid =
        document.querySelectorAll('[data-valem]');


    elemendid.forEach(function (element) {

        const tekst =
            element.getAttribute('data-valem');


        if (!tekst) {
            return;
        }


        let latex;


        try {

            latex =
                valemLatexiks(tekst);

        }
        catch (e) {

            // Kui valemit ei saa teisendada,
            // jätame algse teksti nähtavaks.

            return;
        }


        const mf =
            document.createElement('math-field');


        mf.className =
            'valem-kuva';


        mf.setAttribute(
            'read-only',
            ''
        );


        mf.setAttribute(
            'math-virtual-keyboard-policy',
            'manual'
        );


        mf.setAttribute(
            'tabindex',
            '-1'
        );


        element.replaceChildren(mf);


        mf.setValue(latex);

    });
}


// =============================================
// Käivitame pärast MathLive laadimist
// =============================================

customElements
    .whenDefined('math-field')
    .then(function () {

        naitaValemeid();

    });