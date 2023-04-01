$('body').css('overflow', 'hidden');

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
})

function initTable() {
    $("#tableDiv").removeClass('d-none');

    $('#operatorTable').bootstrapTable();

    setHeight();

    //$table.bootstrapTable('uncheckAll');
}

function setHeight() {
    var freeAreaHeight = $(window).height() - $('#navMenu').outerHeight(true);
    var adminAgencyTableHeight = $('#operatorTable').outerHeight();
    if (adminAgencyTableHeight > freeAreaHeight) {
        setTimeout(() => {
            $('#operatorTable').bootstrapTable("refreshOptions", {
                stickyHeaderOffsetY: $('#navMenu').outerHeight(),
                height: freeAreaHeight
            })
        }, 10);
    }
    else {
        setTimeout(() => {
            $('#operatorTable').bootstrapTable("refreshOptions", {
                stickyHeaderOffsetY: $('#navMenu').outerHeight(),
                height: 0
            })
        }, 10);
    }
    //var initialTableHeight = $('#tableDiv').outerHeight() + 2;
}

//function initTable() {
//    $('#operatorTable').bootstrapTable();

//    setTimeout(() => {
//        $('#operatorTable').bootstrapTable("refreshOptions", {
//            stickyHeaderOffsetY: $('#navMenu').outerHeight(),
//            height: getHeight()
//        })
//    }, 10);
//}

//function getHeight() {
//    var freeAreaHeight = $(window).height() - $('#navMenu').outerHeight(true) - 16;
//    var opetatorTableHeight = $('#operatorTable').outerHeight(true) + 2;
//    if (freeAreaHeight > opetatorTableHeight) {
//        return opetatorTableHeight;
//    }
//    return freeAreaHeight;
//}

function balanceFormatter(value, row) {
    var balanceFormatHtml = `<div class="row justify-content-center">
                                <div class="col-6 text-end">
                                    ${value}$ 
                                </div>
                                <div class="col-auto ps-0">
                                    <i class="fa-solid fa-signal fa-signal-gradient"></i>
                                </div>
                             </div>`;
    return balanceFormatHtml;
}

function commentsFormatter(value, row) {
    var notViewComments = JSON.parse(value.toLowerCase());
    if (notViewComments) {
        return `<a href="#" role="button" class="position-relative" onclick="showComments(event, ${row['id']})">
                    <i class="fa-regular fa-note-sticky @*fa-clipboard*@ fa-xl"></i>
                    <span class="position-absolute top-0 translate-middle p-1 bg-danger border botder-light rounded-circle text" style="left: 90%">
                        <span class="visually-hidden">Unread comments</span>
                    </span>
                </a>`;
    } else {
        return `<a href="#" role="button" class="position-relative" onclick="showComments(event, ${row['id']})">
                    <i class="fa-regular fa-note-sticky fa-xl"></i>
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