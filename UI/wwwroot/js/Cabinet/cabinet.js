$(function () {
    $.ajaxSetup({
        headers: {
            RequestVerificationToken: $('[name=__RequestVerificationToken]').first().val()
        }
    });
})

$('#addCabinet').click(() => {
    $('#formAddCabinet').validate().resetForm();
    $('#nameCabinet').removeClass('is-invalid is-valid');
    $('#nameCabinet').val('');
    $('#modalAddCabinet').modal('show');
});

function successAddCabinet(cabinet) {
    /*var cabinet = JSON.parse(data);*/
    $('#modalAddCabinet').modal('hide');
    $('#toastBody').html(`The ${cabinet.name} cabinet has been added successfully`)

    var toast = $('#toast');

    if (toast.hasClass('bg-danger')) {
        toast.removeClass('bg-danger');
    }

    if (!toast.hasClass('bg-cornflower_blue')) {
        toast.addClass('bg-cornflower_blue');
    }

    var $newRow = $(`<tr data-cabinet-id='${cabinet.id}'>
                        <td>C${cabinet.id}</td>
                        <td class="text-start">${cabinet.name}</td>
                        <td>0</td>
                        <td>0</td>
                    </tr>`);

    $('#tableCabinets').append($newRow);

    let $newCheckboxInModalBindCabinet = $(`<div class="form-check">
                                                <input class="form-check-input" type="checkbox" id="${cabinet.id}" name="cabinets" value="${cabinet.id}" />
                                                <label class="form-check-label text-white" for="${cabinet.id}">${cabinet.name}</label>
                                            </div>`);
    $('#modalBindCabinet').find('.modal-body').append($newCheckboxInModalBindCabinet);

    setTimeout(() => bootstrap.Toast.getOrCreateInstance(toast).show(), 500);
}

function failureAddCabinet(error) {
    $('#modalAddCabinet').modal('hide');
    toastShowError(error.responseText);
}

function searchTable(e, tableId) {
    var searchText = e.target.value.toLowerCase();
    $(`#${tableId} tbody`).find('tr').each(function () {
        if (searchText && !$(this).text().toLowerCase().includes(searchText)) {
            $(this).addClass('d-none');
        }
        else {
            $(this).removeClass('d-none')
        }
    });
}

function selectedCabinet(e) {
    $('#tableCabinets tbody').find('.tr-border').each(function () {
        $(this).removeClass('tr-border');
    });

    var $tr = $(e.target).closest('tr');
    $tr.addClass('tr-border');

    var cabinetId = $tr.data('cabinet-id');

    if (cabinetId != undefined) {
        $('#tableUsers tbody tr').each(function () {
            var dataCabinets = $(this).data('cabinets');
            if (dataCabinets) {
                var cabinets = dataCabinets.toString().split(',').map(Number);
                if (cabinets.includes(cabinetId)) {
                    if ($(this).hasClass('d-none')) {
                        $(this).removeClass('d-none');
                    }
                } else if (!$(this).hasClass('d-none')) {
                    $(this).addClass('d-none');
                }
            }
            else {
                $(this).addClass('d-none');
            }
        });
    } else { // All cabinets
        $('#tableUsers tbody tr.d-none').each(function () {
            $(this).removeClass('d-none');
        });
    }
}

function bindCabinetToUser(e, operatorId, userName) {

    $('#operatorId').val(operatorId);
    $('#userName').val(userName);

    var bindCabinets = getBindCabinets(e);

    $('#formBindCabinet .form-check-input').each(function () {
        var id = Number($(this).attr('id'));
        var checked = $(this).prop('checked');

        if (checked) {
            if (!bindCabinets.includes(id)) {
                this.checked = false;
                this.disabled = false;
            }
            else {
                this.disabled = true;
            }
        }
        else {
            if (bindCabinets.includes(id)) {
                this.checked = true;
                this.disabled = true;
            }
        }
    });

    $('#modalBindCabinet').modal('show');
}

function getBindCabinets(e) {
    var userDataCabinets = $(e).closest('tr').attr('data-cabinets').toString();
    var bindCabinets = new Array();
    if (userDataCabinets) {
        bindCabinets = userDataCabinets.split(',').map(Number);
    }
    return bindCabinets;
}

function successBindCabinets(userCabinets) {
    $('#modalBindCabinet').modal('hide');

    if (userCabinets) {
        var $tdUserCabinets = $(`#tableUsers #tr_${userCabinets.userId} td[name=cabinets]`);

        for (var i = 0; i < userCabinets.cabinets.length; i++) {
            var $tdCabinetTotalUsers = $(`#tableCabinets tr[data-cabinet-id=${userCabinets.cabinets[i]}] td`).last();
            if ($tdCabinetTotalUsers) {
                cabinetTotalUsers = Number($tdCabinetTotalUsers.text()) + 1;
                $tdCabinetTotalUsers.text(cabinetTotalUsers);
            }
        }

        var oldUserCabinets = getBindCabinets($tdUserCabinets);
        if (oldUserCabinets) {
            userCabinets.cabinets = oldUserCabinets.concat(userCabinets.cabinets);
        }

        $tdUserCabinets.empty();

        var btnCabinets = '';
        for (var i = 0; i < userCabinets.cabinets.length; i++) {
            btnCabinets = btnCabinets +
                `<button id="${userCabinets.userId}_${userCabinets.cabinets[i]}" type="button" class="btn btn-sm btn-outline-success btn-trash text-white my-1" onclick="unbindCabinetToUser(${userCabinets.cabinets[i]}, ${userCabinets.userId})">
                <span>C${userCabinets.cabinets[i]}
            </button>\r`;
        }
        var htmlCabinets = `${btnCabinets}
            <button type="button" class="btn btn-sm btn-outline-success my-1" onclick="bindCabinetToUser(this, ${userCabinets.userId}, '${userCabinets.userName}')">
                <i class="fa-solid fa-plus"></i>
            </button>`;
        $tdUserCabinets.append(htmlCabinets);


        $tdUserCabinets.closest('tr').removeData('cabinets', undefined).attr('data-cabinets', userCabinets.cabinets.join(','));

        if (userCabinets.errorBindCabinets.length > 0) {
            toastShowError(``);
        }
    }
}

function failureBindCabinets(error) {
    $('#modalBindCabinet').modal('hide');
    toastShowError(error.responseText);
}

function unbindCabinetToUser(cabinetId, operatorId) {
    $(`#${operatorId}_${cabinetId}`).addClass('d-none');
    $.post('/Cabinet/UnbindCabinetToUser', { cabinetId: cabinetId, operatorId: operatorId }, function () {

        var $tdCabinetTotalUsers = $(`#tableCabinets tr[data-cabinet-id=${cabinetId}] td`).last();
        if ($tdCabinetTotalUsers) {
            cabinetTotalUsers = Number($tdCabinetTotalUsers.text()) - 1;
            $tdCabinetTotalUsers.text(cabinetTotalUsers);
        }

        var $tr = $(`#tr_${operatorId}`);
        var cabinets = $tr.attr('data-cabinets').toString().split(',').map(Number);
        if (cabinets) {
            var newCabinets = $.grep(cabinets, function (c) {
                return c != cabinetId;
            });

            $tr.removeData('cabinets', undefined).attr('data-cabinets', newCabinets.join(','));
        }

        $(`#${operatorId}_${cabinetId}`).remove();

    }).fail(function (error) {
        $(`#${operatorId}_${cabinetId}`).removeClass('d-none');
        toastShowError(error.responseText);
    });
}

function toastShowError(message) {
    $('#toastBody').html(message)

    var toast = $('#toast');

    if (toast.hasClass('bg-cornflower_blue')) {
        toast.removeClass('bg-cornflower_blue');
    }

    if (!toast.hasClass('bg-danger')) {
        toast.addClass('bg-danger');
    }

    bootstrap.Toast.getOrCreateInstance(toast).show();
}