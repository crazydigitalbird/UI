function successAddSheet(sheet) {

    var userInfo = JSON.parse(sheet.info);

    var fullName = '';
    if (userInfo.name) {
        fullName = userInfo.name;
    }
    if (userInfo.lastName) {
        fullName = `${fullName} ${userInfo.lastName}`;
    }

    var newCard = $(`<div class="col">
                    <div class="card h-100 text-center">
                        <div class="bg-image hover-overlay ripple" data-mdb-ripple-color="light">
                            <img src="${userInfo.avatar}" class="img-fluid" />
                            <a href="#">
                                <div class="mask" style="background-color: rgba(251, 251, 251, 0.15);"></div>
                            </a>
                        </div>
                        <div class="card-body">
                            <h5 class="card-title">${fullName}</h5>
                            <div>
                                <button type="button" class="btn btn-primary btn-floating" onclick="editSheet(event, ${sheet.id})">
                                    <i class="fa-solid fa-user-pen mr-2"></i>
                                </button>
                                <button type="button" class="btn btn-danger btn-floating" onclick="removeSheet(event, ${sheet.id})">
                                    <i class="fa-solid fa-trash mr-2"></i>
                                </button>
                            </div>
                            <div class="row d-none">
                                <div class="form-outline col w-100">
                                    <input type="password" name="password${sheet.id}" id="password${sheet.id}" class="form-control" />
                                    <label class="form-label" for="password${sheet.id}">New Password</label>
                                </div>
                                <div class="col-auto pe-0">
                                    <button type="button" class="btn btn-success btn-floating" data-mdb-ripple-color="success" onclick="changePassword(event, ${sheet.id})">
                                        <i class="fa-solid fa-save fa-xl"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>`);
    $('#cardAddSheet').after(newCard);

    showToast('bg-success', 'bg-danger', `The  ${fullName} sheet has been added successfully`)

    $('#email').val('');
    $('#password').val('');
}

function failureAddSheet(error) {
    //showAlert('alert-danger', 'alert-success', error.responseText);
    showAlert('bg-danger', 'bg-success', error.responseText);
}

function removeSheet(e, sheetId) {
    var fullName = $(e).closest('div').siblings('.card-title').text();
    $.post("/User/DeleteSheet", { sheetId: sheetId }, function () {
        e.closest('div .col').remove();
        showToast('bg-success', 'bg-danger', `${fullName}'s sheet has been deleted`);
    }).fail(function () {
        showToast('bg-danger', 'bg-success', `Error deleting the sheet of ${fullName}`);
    });
}

function editSheet(e, sheetId) {
    var $divParent = $(e).closest('div');
    $divParent.addClass('d-none');
    $divParent.siblings('div').removeClass('d-none');
    mdb.Input.getInstance($(`#password${sheetId}`).closest('div')[0]).update();
}

function changePassword(e, sheetId, credentials) {
    var $divParent = $(e).closest('div .row');
    $divParent.addClass('d-none');
    $divParent.siblings('div').removeClass('d-none');

    var $inputPassword = $(`#password${sheetId}`);

    if ($inputPassword.val()) {
        var credentialsObj = JSON.parse(credentials);
        var fullName = $(e).closest('.card-body').find('.card-title').text();

        $.post("/User/ChangePassword", { sheetId: sheetId, login: credentialsObj.login, password: $inputPassword.val() }, function () {
            showToast('bg-success', 'bg-danger', `${fullName}'s sheet password has been changed`);
        }).fail(function () {
            showToast('bg-danger', 'bg-success', `Error change password the sheet of ${fullName}`);
        });
        $(`#password${sheetId}`).val('');
    }
}

function showAlert(addClass, removeClass, text) {
    var $alert = $('#alertAddSheet');
    if (!$alert.hasClass(addClass)) {
        $alert.addClass(addClass);
    }
    if ($alert.hasClass(removeClass)) {
        $alert.removeClass(removeClass);
    }
    if ($alert.hasClass('d-none')) {
        $alert.removeClass('d-none');
    }
    $alert.text(text);
}

function showToast(addClass, removeClass, text) {
    $('#toastBody').html(text)

    var toast = $('#toast');

    if (toast.hasClass(removeClass)) {
        toast.removeClass(removeClass);
    }

    if (!toast.hasClass(addClass)) {
        toast.addClass(addClass);
    }

    mdb.Toast.getInstance(toast).show();
}