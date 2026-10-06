let graafik = null;
let praeguneValem = '';


// ================= Ilus samm teljel =================

function ilusSamm(vahemik) {

    const toores = vahemik / 10;
    const aste =
        Math.pow(10, Math.floor(Math.log10(toores)));

    const n = toores / aste;

    let kordaja = 10;

    if (n <= 1) {
        kordaja = 1;
    }
    else if (n <= 2) {
        kordaja = 2;
    }
    else if (n <= 5) {
        kordaja = 5;
    }

    return kordaja * aste;
}


// ================= Arvu kuvamine =================

function fmt(v) {

    return String(
        Math.round(v * 100) / 100
    );
}


// ================= Graafiku lisad =================

const koolipluginad = {

    id: 'koolistiil',

    afterDatasetsDraw(chart) {

        const ctx = chart.ctx;
        const a = chart.chartArea;
        const x = chart.scales.x;
        const y = chart.scales.y;

        const x0 = x.getPixelForValue(0);
        const y0 = y.getPixelForValue(0);

        ctx.save();


        // ================= Ekstreemumid =================

        const ekstr =
            chart.data.datasets[1].data;

        ctx.font = '13px sans-serif';
        ctx.fillStyle = '#000';
        ctx.strokeStyle = '#666';
        ctx.lineWidth = 1;

        ctx.setLineDash([4, 4]);


        ekstr.forEach(function (p) {

            const px =
                x.getPixelForValue(p.x);

            const py =
                y.getPixelForValue(p.y);


            if (px < a.left ||
                px > a.right ||
                py < a.top ||
                py > a.bottom) {

                return;
            }


            ctx.beginPath();


            // joon y-teljeni ainult siis,
            // kui y-telg on nähtav

            if (x0 >= a.left &&
                x0 <= a.right) {

                ctx.moveTo(px, py);
                ctx.lineTo(x0, py);
            }


            // joon x-teljeni ainult siis,
            // kui x-telg on nähtav

            if (y0 >= a.top &&
                y0 <= a.bottom) {

                ctx.moveTo(px, py);
                ctx.lineTo(px, y0);
            }


            ctx.stroke();
        });


        ctx.setLineDash([]);


        // Ekstreemumi koordinaadid

        ekstr.forEach(function (p) {

            const px =
                x.getPixelForValue(p.x);

            const py =
                y.getPixelForValue(p.y);


            if (px < a.left ||
                px > a.right ||
                py < a.top ||
                py > a.bottom) {

                return;
            }


            ctx.textAlign = 'left';

            if (p.max) {
                ctx.textBaseline = 'bottom';
            }
            else {
                ctx.textBaseline = 'top';
            }


            let nihkeY = 6;

            if (p.max) {
                nihkeY = -6;
            }


            ctx.fillText(
                '(' +
                fmt(p.x) +
                '; ' +
                fmt(p.y) +
                ')',
                px + 8,
                py + nihkeY
            );
        });


        // ================= Telgede nooled =================

        ctx.strokeStyle = '#000';
        ctx.fillStyle = '#000';
        ctx.lineWidth = 2;


        function nool(tx, ty, nurk) {

            ctx.beginPath();

            ctx.moveTo(tx, ty);

            ctx.lineTo(
                tx - 11 * Math.cos(nurk - 0.4),
                ty - 11 * Math.sin(nurk - 0.4)
            );

            ctx.lineTo(
                tx - 11 * Math.cos(nurk + 0.4),
                ty - 11 * Math.sin(nurk + 0.4)
            );

            ctx.closePath();

            ctx.fill();
        }


        // X-telje nool

        if (y0 >= a.top &&
            y0 <= a.bottom) {

            nool(
                a.right + 4,
                y0,
                0
            );
        }


        // Y-telje nool

        if (x0 >= a.left &&
            x0 <= a.right) {

            nool(
                x0,
                a.top - 4,
                -Math.PI / 2
            );
        }


        // ================= x ja y tähised =================

        ctx.font = 'italic 17px serif';


        // x

        if (y0 >= a.top &&
            y0 <= a.bottom) {

            ctx.textAlign = 'right';
            ctx.textBaseline = 'bottom';

            ctx.fillText(
                'x',
                a.right - 4,
                y0 - 8
            );
        }


        // y

        if (x0 >= a.left &&
            x0 <= a.right) {

            ctx.textAlign = 'left';
            ctx.textBaseline = 'top';

            ctx.fillText(
                'y',
                x0 + 8,
                a.top + 2
            );
        }


        // ================= O =================

        if (x0 >= a.left &&
            x0 <= a.right &&
            y0 >= a.top &&
            y0 <= a.bottom) {

            ctx.font = '15px serif';

            ctx.textAlign = 'right';
            ctx.textBaseline = 'top';

            ctx.fillText(
                'O',
                x0 - 5,
                y0 + 4
            );
        }


        ctx.restore();
    }
};


// ================= Graafiku joonistamine =================

async function joonistaGraafik(valem) {

    if (valem) {
        praeguneValem = valem;
    }


    if (!praeguneValem) {
        return;
    }


    // ================= X vahemik =================

    const alates =
        parseFloat(
            document.getElementById('alates').value
        );

    const kuni =
        parseFloat(
            document.getElementById('kuni').value
        );


    // ================= Y vahemik =================

    const yAlates =
        parseFloat(
            document.getElementById('yAlates').value
        );

    const yKuni =
        parseFloat(
            document.getElementById('yKuni').value
        );


    // ================= Kontroll =================

    if (isNaN(alates) ||
        isNaN(kuni) ||
        kuni <= alates) {

        return;
    }


    if (isNaN(yAlates) ||
        isNaN(yKuni) ||
        yKuni <= yAlates) {

        return;
    }


    // ================= Punktide samm =================

    const samm =
        Math.max(
            (kuni - alates) / 800,
            0.001
        );


    // ================= Andmed serverist =================

    const url =
        '/Funktsioonid/Punktid?valem=' +
        encodeURIComponent(praeguneValem) +
        '&alates=' +
        alates +
        '&kuni=' +
        kuni +
        '&samm=' +
        samm;


    const vastus =
        await fetch(url);


    if (!vastus.ok) {
        return;
    }


    const andmed =
        await vastus.json();


    // ================= Funktsiooni punktid =================

    const fPunktid =
        andmed.x.map(function (x, i) {

            const y =
                andmed.f[i];


            if (y === null) {

                return {
                    x: x,
                    y: null
                };
            }


            return {
                x: x,
                y: y
            };
        });


    // ================= Ekstreemumid =================

    const ekstr =
        andmed.ekstreemid.map(function (e) {

            return {
                x: e.x,
                y: e.y,
                max: e.max
            };
        });


    // ================= Telgede sammud =================

    const xSpan =
        kuni - alates;

    const ySpan =
        yKuni - yAlates;


    const xSamm =
        ilusSamm(xSpan);

    const ySamm =
        ilusSamm(ySpan);


    // Kasutame täpselt kasutaja sisestatud vahemikku

    const xMin = alates;
    const xMax = kuni;

    const yMin = yAlates;
    const yMax = yKuni;


    // ================= Ruudustik =================

    function rasterVarv(c) {

        if (c.tick.value === 0) {
            return '#000';
        }

        return '#d0d7de';
    }


    function rasterLaius(c) {

        if (c.tick.value === 0) {
            return 2;
        }

        return 1;
    }


    function ilmaNullita(v) {

        if (v === 0) {
            return '';
        }

        return v;
    }


    // ================= Vana graafiku eemaldamine =================

    if (graafik) {
        graafik.destroy();
    }


    // ================= Uus graafik =================

    graafik =
        new Chart(
            document.getElementById(
                'funktsiooniGraafik'
            ),
            {

                type: 'line',

                data: {

                    datasets: [

                        // Funktsioon

                        {
                            data: fPunktid,

                            borderColor: '#1a56db',

                            borderWidth: 2.5,

                            pointRadius: 0,

                            spanGaps: false
                        },


                        // Ekstreemumid

                        {
                            data: ekstr,

                            type: 'scatter',

                            showLine: false,

                            pointRadius: 5,

                            pointBackgroundColor: '#fff',

                            pointBorderColor: '#000',

                            pointBorderWidth: 2
                        }
                    ]
                },


                options: {

                    animation: false,

                    parsing: false,

                    aspectRatio: 1.3,

                    layout: {

                        padding: {
                            top: 16,
                            right: 16
                        }
                    },


                    plugins: {

                        legend: {
                            display: false
                        },

                        tooltip: {
                            enabled: false
                        }
                    },


                    scales: {

                        // ================= X =================

                        x: {

                            type: 'linear',

                            min: xMin,

                            max: xMax,

                            position: {
                                y: 0
                            },

                            grid: {
                                color: rasterVarv,
                                lineWidth: rasterLaius
                            },

                            border: {
                                display: false
                            },

                            ticks: {

                                stepSize: xSamm,

                                callback: ilmaNullita,

                                color: '#000',

                                padding: 4
                            }
                        },


                        // ================= Y =================

                        y: {

                            type: 'linear',

                            min: yMin,

                            max: yMax,

                            position: {
                                x: 0
                            },

                            grid: {
                                color: rasterVarv,
                                lineWidth: rasterLaius
                            },

                            border: {
                                display: false
                            },

                            ticks: {

                                stepSize: ySamm,

                                callback: ilmaNullita,

                                color: '#000',

                                padding: 4
                            }
                        }
                    }
                },


                plugins: [
                    koolipluginad
                ]
            }
        );
}


// ================= Graafiku uuendamine =================

function uuendaGraafik() {

    joonistaGraafik(
        praeguneValem
    );
}