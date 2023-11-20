let dateTypeChart = moment().startOf('month');
let dateBalancesAllMonth = moment().startOf('month');
//let dataTypeChart;
//let dataBalancesAllMonth;

/*$('body').css('overflow', 'hidden');*/

//$(window).resize(function () {
//    let typeChart = Chart.getChart('typeChart');
//    let balanceMonthChart = Chart.getChart('balanceMonthChart');
//    typeChart.destroy();
//    balanceMonthChart.destroy();
//    drawTypeChat(dataTypeChart);
//    drawBalanceMonthChat(dataBalancesAllMonth);
//});

$(function () {
    $('#operatorTable').on('reset-view.bs.table', function (e) {
        $('#dropDownMenuBalance').on('shown.bs.dropdown', function () {
            $('#dropdownMenuBalanceUl').addClass('dropdownMenuBootstrapTableFixed');
        });
    });

    initTable();

    $(window).resize(function () {
        setHeight();
    });

    var data = $('#initialBalancesByMessageType').val().split(';').map(Number);
    var initialBalancesMonth = $('#initialBalancesMonth').val().split(';').map(Number);

    drawTypeChat(data);
    drawBalanceMonthChat(initialBalancesMonth);
    var sum = initialBalancesMonth.reduce((a, b) => a + b * 100, 0) / 100;
    $('#balanceAllMonth').text(sum);
})

function initTable() {
    $("#tableDiv").removeClass('d-none');

    $('#operatorTable').bootstrapTable();

    setHeight();

    //$table.bootstrapTable('uncheckAll');
}

function setHeight() {
    var freeAreaHeight = $('#containerTable').outerHeight(true);
    var adminAgencyTableHeight = $('#operatorTable').outerHeight();
    if (adminAgencyTableHeight > freeAreaHeight) {
        setTimeout(() => {
            $('#operatorTable').bootstrapTable("refreshOptions", {
                /*stickyHeaderOffsetY: $('#navMenu').outerHeight(),*/
                height: freeAreaHeight
            })
        }, 10);
    }
}

function balanceFormatter(value, row) {
    var balanceFormatHtml = `<div class="d-flex align-items-center text-end">
                                <div class="col-6 pe-0">
                                    ${value}$ 
                                </div>
                                <div class="col-6 pe-2">
                                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                        <path d="M19.88 18.47C20.36 17.7 20.63 16.8 20.57 15.81C20.44 13.66 18.73 11.84 16.6 11.61C15.9694 11.5366 15.3304 11.5975 14.725 11.7887C14.1197 11.9798 13.5616 12.2969 13.0875 12.7191C12.6133 13.1413 12.2339 13.659 11.9741 14.2382C11.7143 14.8175 11.58 15.4451 11.58 16.08C11.58 18.57 13.59 20.58 16.07 20.58C16.95 20.58 17.77 20.32 18.46 19.88L20.87 22.29C21.26 22.68 21.9 22.68 22.29 22.29C22.68 21.9 22.68 21.26 22.29 20.87L19.88 18.47ZM16.08 18.58C15.417 18.58 14.7811 18.3166 14.3122 17.8478C13.8434 17.3789 13.58 16.743 13.58 16.08C13.58 15.417 13.8434 14.7811 14.3122 14.3122C14.7811 13.8434 15.417 13.58 16.08 13.58C16.743 13.58 17.3789 13.8434 17.8478 14.3122C18.3166 14.7811 18.58 15.417 18.58 16.08C18.58 16.743 18.3166 17.3789 17.8478 17.8478C17.3789 18.3166 16.743 18.58 16.08 18.58ZM15.72 10.08C14.98 10.1 14.27 10.26 13.62 10.53L13.07 9.69999L9.99 14.71C9.90771 14.8442 9.7949 14.957 9.66076 15.0394C9.52661 15.1218 9.37492 15.1713 9.21802 15.184C9.06112 15.1966 8.90345 15.1721 8.75783 15.1123C8.61221 15.0525 8.48276 14.9592 8.38 14.84L6.26 12.37L3.2 17.27C2.89 17.76 2.23 17.89 1.76 17.55C1.34 17.24 1.22 16.66 1.5 16.21L5.28 10.16C5.64 9.58999 6.45 9.52999 6.89 10.04L9 12.5L12.18 7.32999C12.2674 7.18712 12.3895 7.0686 12.5349 6.9854C12.6803 6.90221 12.8443 6.85703 13.0118 6.85403C13.1792 6.85104 13.3448 6.89034 13.493 6.96829C13.6413 7.04624 13.7675 7.16033 13.86 7.29999L15.72 10.08ZM18.31 10.58C17.67 10.3 16.98 10.13 16.26 10.09L20.8 2.89999C21.11 2.40999 21.77 2.28999 22.23 2.62999C22.66 2.93999 22.77 3.52999 22.49 3.96999L18.31 10.58Z" fill="#55DEA9"/>
                                    </svg>
                                </div>
                             </div>`;
    return balanceFormatHtml;
}

function commentsFormatter(value, row) {
    var notViewComments = JSON.parse(value.toLowerCase());
    if (notViewComments) {
        return `<a href="#" role="button" class="position-relative me-1" onclick="showComments(event, ${row['id']})" title="Comments">
                    <i class="fa-solid fa-headset fa-xl text-warning"></i>
                    <span class="position-absolute top-0 translate-middle p-1 bg-danger border botder-light rounded-circle text" style="left: 90%">
                        <span class="visually-hidden">Unread comments</span>
                    </span>
                </a>
                <a href="/Chat/Index?sheetId=${row['id']}" role="button" title="Chats">
                    <i class="fa-regular fa-comments fa-xl"></i>
                </a>`;
    } else {
        return `<a href="#" role="button" class="position-relative me-1" onclick="showComments(event, ${row['id']})" title="Comments">
                    <i class="fa-solid fa-headset fa-xl text-warning"></i>
                </a>
                <a href="/Chat/Index?sheetId=${row['id']}" role="button" title="Chats">
                    <i class="fa-regular fa-comments fa-xl"></i>
                </a>`;
    }
}

function updateBalance(e, interval) {
    if (!$(e.target).hasClass('active')) {
        $.get('/Operator/Balance', { interval: interval }, function (data) {
            for (var key in data) {
                var row = $('#operatorTable').bootstrapTable('getRowByUniqueId', key);
                if (row) {
                    $('#operatorTable').bootstrapTable('updateCellByUniqueId', {
                        id: key,
                        field: 'balance',
                        value: data[key],
                        reinit: false
                    });
                }
            }

            $('.dropdown-menu').each(function () {
                $(this).find('a').each(function () {
                    if ($(this).text() != $(e.target).text() && $(this).hasClass('active')) {
                        $(this).removeClass('active');
                    }
                    if ($(this).text() === $(e.target).text()) {
                        $(this).addClass('active');
                    }
                });
            });

        }).fail(function (error) {
            failure(error.responseText);
        })
    }
}

$('#modalComments').on('shown.bs.modal', function () {
    $('.modal-body')[0].scrollTo(0, $('.list-unstyled').outerHeight());
});

function showComments(event, sheetId) {
    $.get('Operator/Comments', { sheetId: sheetId }, function (data) {
        $('#modalComments').find('.modal-content').html(data);
        var row = $('#operatorTable').bootstrapTable('getRowByUniqueId', sheetId);
        $('#modalComments').find('.modal-tittle').text(`${row['name']} ${row['lastName']}`);
        $('#modalComments').modal('show');

        if ($(event.target)) {
            $('#operatorTable').bootstrapTable('updateCellByUniqueId', {
                id: sheetId,
                field: 'comments',
                value: 'false',
                reinit: false
            });
        }

    }).fail(function (error) {
        failure(error.responseText);
    });
}

function addComment(sheetId) {
    var $textAreaComment = $('#textAreaComment');
    if ($textAreaComment.val()) {
        $.post('Operator/AddComment', { sheetId: sheetId, text: $textAreaComment.val() }, function (data) {
            /*${new Date(data.created).toLocaleDateString()}*/
            $('.list-unstyled').append(`<li classs="d-flex justify-content-between mb-4">
                                            <div class="mask-custom card w-100">
                                                <div class="card-header d-flex justify-content-between p-3" style="border-bottom: 1px solid rgba(255, 255, 255, .3);">
                                                    <p class="fw-bold mb-0"><i class="fa-solid fa-check text-success"></i> ${data.member.user.login}</p>
                                                    <p class="text-light small mb-0"><i class="fa-solid fa-clock"></i> now</p>
                                                </div>
                                                <div class="card-body">
                                                    <p class="mb-0">
                                                        ${data.content}
                                                    </p>
                                                </div>
                                            </div>
                                        </li>`);
            $textAreaComment.val('');
            $('.modal-body')[0].scrollTo(0, $('.list-unstyled').outerHeight());
            var noComments = $('#noComments');
            if (noComments) {
                noComments.remove();
            }
        }).fail(function (error) {
            failure(error.responseText);
        });
    }
}

function failure(error) {

    $('#toastBody').html(error)

    var toast = $('#toast');

    if (toast.hasClass('bg-success')) {
        toast.removeClass('bg-success');
    }

    if (!toast.hasClass('bg-danger')) {
        toast.addClass('bg-danger');
    }

    setTimeout(() => mdb.Toast.getInstance(toast).show(), 500);
}

function drawTypeChat(data) {
    //dataTypeChart = data;
    const ctx = document.getElementById('typeChart');

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Чаты', 'Чаты фото', 'Чаты видео', 'Чаты аудио', 'Стикеры', 'Письма', 'Другое'],
            datasets: [{
                data: data,
                backgroundColor: ['#7D6AF0', '#55DEA9', '#57A2FB', '#FFCA41', '#388B15', '#D4681B', '#EB5858'],
                borderWidth: 1
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
                    },
                    ticks: {
                        color: 'white'
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                chartAreaBorder: {
                    borderColor: '#454563',
                    borderWidth: 1,
                    borderDash: [2, 4]
                }
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

function drawBalanceMonthChat(balancesMonth) {
    //dataBalancesAllMonth = balancesMonth;
    var labels = Array.from({ length: balancesMonth.length }, (_, i) => i + 1);
    const ctx = document.getElementById('balanceMonthChart').getContext('2d');

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                data: balancesMonth,
                cubicInterpolationMode: 'monotone',
                fill: true,
                borderColor: '#7D6AF0',
                backgroundColor: 'rgba(125, 106, 240, 0.1)',
                borderWidth: 4,
                pointRadius: 2,
                pointHoverRadius: 9,
                pointHoverBackgroundColor: '#0E132F',
                pointHoverBorderWidth: 4,
                pointHoverBorderColor: '#FFF'
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
                legend: {
                    display: false
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
                            var date = dateBalancesAllMonth.locale('ru').format('D MMMM').split(' ');
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

function downDateTypeChart() {
    downDate(dateTypeChart);
    updateTypeChart();
}

function upDateTypeChart() {
    upDate(dateTypeChart);
    updateTypeChart();
}

function updateTypeChart() {
    $('#dateTypeChart').text(dateTypeChart.locale('ru').format('MMMM YYYY'));

    var month = dateTypeChart.locale('ru').format('M');
    var year = dateTypeChart.locale('ru').format('YYYY')

    $.get('/Operator/BalancesType', { month, year }, function (data) {
        var currentMonth = dateTypeChart.locale('ru').format('M');
        if (currentMonth === month) {
            var chart = Chart.getChart('typeChart');
            //dataTypeChart = data;
            chart.data.datasets[0].data = data;
            chart.update();
        }
    });
}

function downDateBalancesAllMonthChart() {
    downDate(dateBalancesAllMonth);
    updateBalancesAllMonthChart();
}

function upDateBalancesAllMonthChart() {
    upDate(dateBalancesAllMonth);
    updateBalancesAllMonthChart();
}

function updateBalancesAllMonthChart() {
    $('#dateBalanceAllMonth').text(dateBalancesAllMonth.locale('ru').format('MMMM YYYY'));

    var month = dateBalancesAllMonth.locale('ru').format('M');
    var year = dateBalancesAllMonth.locale('ru').format('YYYY')

    $.get('/Operator/BalancesAllMonth', { month, year }, function (data) {
        var currentMonth = dateBalancesAllMonth.locale('ru').format('M');
        if (currentMonth === month) {

            var sum = data.reduce((a, b) => a + b * 100, 0) / 100;
            $('#balanceAllMonth').text(sum);

            var chart = Chart.getChart('balanceMonthChart');
            var labels = Array.from({ length: data.length }, (_, i) => i + 1);
            chart.data.labels = labels;
            //dataBalancesAllMonth = data;
            chart.data.datasets[0].data = data;
            chart.update();
        }
    });
}

function downDate(date) {
    date = date.subtract(1, 'month');
}

function upDate(date) {
    date = date.add(1, 'M');
}