//function setHeight() {
//    var $areaTops = $('.area-top');
//    let heightContainer = $('.first-container').outerHeight(true);
//    let heightStatRow = $('.first-container .row').first().outerHeight(true);

//    $areaTops.each(function () {
//        let reduceHeight = heightContainer - heightStatRow - $(this).outerHeight(true);
//        if (reduceHeight < 0) {
//            let divTable = $(this).find('div');
//            let newHeightTable = divTable.outerHeight(true) + reduceHeight;
//            divTable.height(newHeightTable);
//        }
//    });

//}

//$(document).ready(function () {
//    setHeight();
//});

//$(window).resize(function () {
//    setHeight();
//});
let dateBalancesCurrentMonth = moment().startOf('month');
let dateBalancesLastMonth = moment().startOf('month').subtract(1, 'month');

$(function () {
    var dataLastMonth = $('#initialBalancesLastMonth').val().split(';').map(Number);
    var dataCurrentMonth = $('#initialBalancesCurrentMonth').val().split(';').map(Number);
    drawBalanceMonthsChats(dataLastMonth, dataCurrentMonth);
})

function drawBalanceMonthsChats(dataLastMonth, dataCurrentMonth) {
    let currentDate = new Date();
    currentDate.setDate(0);
    let currentMonthDays = daysInMonth(currentDate.getMonth(), currentDate.getFullYear());
    currentDate.setMonth(currentDate.getMonth() - 1);
    let lastMonthDays = daysInMonth(currentDate.getMonth(), currentDate.getFullYear());
    let maxDays = currentMonthDays >= lastMonthDays ? currentMonthDays : lastMonthDays;
    var labels = Array.from({ length: maxDays }, (_, i) => i + 1);
    const ctx = document.getElementById('balanceMonthsChart').getContext('2d');

    var gradient = ctx.createLinearGradient(0, 0, 0, 700);
    gradient.addColorStop(0, 'rgba(255, 202, 65, 1)');
    gradient.addColorStop(1, 'rgba(255, 202, 65, 0.3)');

    var gradientLast = ctx.createLinearGradient(0, 0, 0, 700);
    gradientLast.addColorStop(0, 'rgba(85, 222, 9, 1)');
    gradientLast.addColorStop(1, 'rgba(85, 222, 9, 0.3)');

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Текущий',
                data: dataCurrentMonth,
                cubicInterpolationMode: 'monotone',
                fill: true,
                borderColor: '#FFCA41',
                backgroundColor: gradient,/*'rgba(255, 202, 65, 0.1)',*/
                borderWidth: 4,
                //pointRadius: 2,
                //pointHoverRadius: 9,
                //pointHoverBackgroundColor: '#0E132F',
                //pointHoverBorderWidth: 4,
                //pointHoverBorderColor: '#FFF'
            },
            {
                label: 'Прошлый',
                data: dataLastMonth,
                cubicInterpolationMode: 'monotone',
                fill: true,
                borderColor: '#55DE09',
                backgroundColor: gradientLast/*'rgba(85, 222, 9, 0.1)'*/,
                //borderWidth: 4,
                //pointRadius: 2,
                //pointHoverRadius: 9,
                //pointHoverBackgroundColor: '#0E132F',
                //pointHoverBorderWidth: 4,
                //pointHoverBorderColor: '#FFF'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,

            scales: {
                y: {
                    beginAtZeto: true,
                    grid: {
                        color: '#454563',
                    },
                    border: {
                        dash: [2, 4],
                    }
                },
                x: {
                    grid: {
                        color: '#454563',
                    },
                    border: {
                        dash: [2, 4],
                    }
                }
            },

            plugins: {
                title: {
                    display: true,
                    align: 'start',
                    padding: {
                        top: 10,
                        bottom: 0
                    },
                    color: 'white',
                    text: 'Заработок относительно прошлого месяца'
                },

                legend: {
                    display: true,
                    align: 'end',
                    labels: {
                        color: 'white',
                        usePointStyle: true,
                        pointStyle: 'circle',
                    }
                },

                tooltip: {
                    xAlign: 'center',
                    yAlign: 'bottom',
                    intersect: false,
                    backgroundColor: '#131334',
                    displayColors: false,

                    callbacks: {
                        title: function (context) {
                            return '';
                        },
                        label: function (context) {
                            var date;
                            if (context.dataset.label == 'Текущий') {
                                date = dateBalancesCurrentMonth.locale('ru').format('D MMMM').split(' ');
                            }
                            else {
                                date = dateBalancesLastMonth.locale('ru').format('D MMMM').split(' ');
                            }
                            var month = date[1];
                            return context.label + ' ' + month + ' $' + context.formattedValue;
                        }
                    }
                },

                chartAreaBorder: {
                    borderColor: '#454563',
                    borderWidth: 1,
                    borderDash: [2, 4]
                }
            },

            interaction: {
                mode: 'index',
                intersect: false
            }
        },

        plugins: [chartAreaBorder]
    });
}

const chartAreaBorder = {
    id: 'chartAreaBorder',
    beforeDraw(chart, args, options) {
        const { ctx, chartArea: { left, top, width, height } } = chart;
        ctx.save();
        ctx.strokeStyle = options.borderColor;
        ctx.lineWidth = options.borderWidth;
        ctx.setLineDash(options.borderDash || []);
        ctx.lineDashOffset = options.borderDashOffset;
        ctx.strokeRect(left, top, width, height);
        ctx.restore();
    }
};

function daysInMonth(month, year) {
    return new Date(year, month, 0).getDate();
}