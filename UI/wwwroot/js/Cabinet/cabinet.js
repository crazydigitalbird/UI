$('#addCabinet').click(() => {
    $('#formAddCabinet').validate().resetForm();
    $('#nameGroup').removeClass('is-invalid is-valid');
    $('#name').val('');
    $('#modalAddCabinet').modal('show');
});

$(function () {
    setHeight();

    $(window).resize(function () {
        setHeight();
    });
})

function setHeight() {
    var freeAreaHeight = $(window).height() - $('#navMenu').outerHeight(true) - $('#toolbarTableCabinets').outerHeight(true) - 16;
    $(".tableFixedHead").css("max-height", freeAreaHeight);
}

function successAddCabinet(cabinet) {
    /*var cabinet = JSON.parse(data);*/
    $('#modalAddCabinet').modal('hide');
    $('#toastBody').html(`The ${cabinet.name} cabinet has been added successfully`)

    var toast = $('#toast');

    if (toast.hasClass('bg-danger')) {
        toast.removeClass('bg-danger');
    }

    if (!toast.hasClass('bg-success')) {
        toast.addClass('bg-success');
    }

    var $newRow = $(`<tr data-cabinet-id='${cabinet.id}'>
                        <td>C${cabinet.id}</td>
                        <td class="text-start">${cabinet.name}</td>
                        <td>0</td>
                        <td>0</td>
                    </tr>`);

    $('#tableCabinets').append($newRow);

    setTimeout(() => mdb.Toast.getInstance(toast).show(), 500);
}

function failureAddCabinet(error) {
    $('#modalAddCabinet').modal('hide');

    $('#toastBody').html(`Error adding the ${error.responseText} cabinet`)

    var toast = $('#toast');

    if (toast.hasClass('bg-success')) {
        toast.removeClass('bg-success');
    }

    if (!toast.hasClass('bg-danger')) {
        toast.addClass('bg-danger');
    }

    setTimeout(() => mdb.Toast.getInstance(toast).show(), 500);
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
                    if ($(this).hasClass('d-none-users')) {
                        $(this).removeClass('d-none-users');
                    }
                } else if (!$(this).hasClass('d-none-users')) {
                    $(this).addClass('d-none-users');
                }
            }
            else {
                $(this).addClass('d-none-users');
            }
        });
    } else { // All cabinets
        $('#tableUsers tbody tr.d-none-users').each(function () {
            $(this).removeClass('d-none-users');
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

function successBindCabinets(data) {
    var userCabinets = JSON.parse(data);

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


    if (userCabinets.cabinets.length > 4) {

        var liCabinets = '';
        for (var i = 0; i < userCabinets.cabinets.length; i++) {
            liCabinets = liCabinets + `<li id="container_${userCabinets.userId}_${userCabinets.cabinets[i]}">
                                        <div class="dropdown-item">
                                            <div class="row align-items-center">
                                                <div class="col">
                                                 C${userCabinets.cabinets[i]}
                                                </div>
                                                <div class="col-auto">
                                                    <button type="button" class="btn btn-danger btn-sm" onclick="unbindCabinetToUser(${userCabinets.cabinets[i]}, ${userCabinets.userId})">
                                                        <i class="fa-solid fa-trash"></i>
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                       </li>`;
        }

        var htmlCabinets = `<div class="row">
                            <div class="dropdown" style="max-width: 455px;">
                                <a role="button" href="#" id="${userCabinets.userId}" type="button" class="btn btn-sm btn-primary btn-rounded my-md-1 btn-block dropdown-toggle" data-mdb-toggle="dropdown" aria-expanded="false">Total Cabinets ${userCabinets.cabinets.length}</a>
                                <ul class="dropdown-menu w-100" aria-labelledby="${userCabinets.userId}">
                                    ${liCabinets}
                                    <li>
                                        <div class="dropdown-item">
                                            <div class="row align-items-center">
                                                <div class="col">Add cabinet</div>
                                                <div class="col-auto">
                                                    <button type="button" class="btn btn-success btn-sm" onclick="bindCabinetToUser(this, ${userCabinets.userId}, '${userCabinets.userName}')">
                                                        <i class="fa-solid fa-plus"></i>
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    </li>
                                </ul>
                            </div>
                           </div>`;

        $tdUserCabinets.append(htmlCabinets);
    }
    else {
        var btnCabinets = '';
        for (var i = 0; i < userCabinets.cabinets.length; i++) {
            btnCabinets = btnCabinets + `<div id="container_${userCabinets.userId}_${userCabinets.cabinets[i]}" class="btn-group" style="display: contents;">
                                <button id="${userCabinets.userId}_${userCabinets.cabinets[i]}" type="button" class="btn btn-sm btn-primary btn-rounded my-md-1 dropdown-toggle btn-size-fixed" data-mdb-toggle="dropdown" aria-expanded="false" data-mdb-ripple-color="primary">C${userCabinets.cabinets[i]}</button>
                                <ul class="dropdown-menu dropdown-menu-width" aria-labelledby="${userCabinets.userId}_${userCabinets.cabinets[i]}">
                                    <li class="btn-size-fixed bg-danger">
                                        <button class="dropdown-item btn bg-danger text-center" onclick="unbindCabinetToUser(${userCabinets.cabinets[i]}, ${userCabinets.userId})">
                                            <i class="fa-solid fa-trash"></i>
                                        </button>
                                    </li>
                                </ul>
                            </div>`;
        }
        var htmlCabinets = `<div class="d-grid d-md-block gap-1">
                                ${btnCabinets}
                                <button type="button" class="btn btn-sm btn-success btn-rounded my-md-1 btn-size-fixed" onclick="bindCabinetToUser(this, ${userCabinets.userId}, '${userCabinets.userName}')">
                                    <i class="fa-solid fa-plus"></i>
                                </button>
                            </div>`;
        $tdUserCabinets.append(htmlCabinets);
    }

    $tdUserCabinets.closest('tr').attr('data-cabinets', userCabinets.cabinets.join(','));

    $('#modalBindCabinet').modal('hide')
}

function failureBindCabinets(error) {
    $('#modalBindCabinet').modal('hide');

    $('#toastBody').html(error.responseText)

    var toast = $('#toast');

    if (toast.hasClass('bg-success')) {
        toast.removeClass('bg-success');
    }

    if (!toast.hasClass('bg-danger')) {
        toast.addClass('bg-danger');
    }

    setTimeout(() => mdb.Toast.getInstance(toast).show(), 500);
}

function unbindCabinetToUser(cabinetId, operatorId) {
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

            var $aDropDownToggle = $(`#button_dropdown_${operatorId}`);
            if ($aDropDownToggle) {
                $aDropDownToggle.text(`Total Cabinets ${newCabinets.length}`);
            }
        }

        $(`#container_${operatorId}_${cabinetId}`).remove();

    }).fail(function (error) {
        $('#toastBody').html(error.responseText)

        var toast = $('#toast');

        if (toast.hasClass('bg-success')) {
            toast.removeClass('bg-success');
        }

        if (!toast.hasClass('bg-danger')) {
            toast.addClass('bg-danger');
        }

        mdb.Toast.getInstance(toast).show();
    });
}