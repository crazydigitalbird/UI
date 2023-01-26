function successAddProfile(data) {
    var profile = JSON.parse(data);


    var newCard = $(`<div class="col">
                    <div class="card h-100 text-center">
                        <div class="bg-image hover-overlay ripple" data-mdb-ripple-color="light">
                            <img src="/image/city.jpg" class="img-fluid" />
                            <a href="#">
                                <div class="mask" style="background-color: rgba(251, 251, 251, 0.15);"></div>
                            </a>
                        </div>
                        <div class="card-body">
                            <h5 class="card-title">${profile.FirstName} ${profile.LastName}</h5>
                            <div>
                                <button type="button" class="btn btn-primary btn-floating" onclick="editProfile(event, ${profile.Id})">
                                    <i class="fa-solid fa-user-pen mr-2"></i>
                                </button>
                                <button type="button" class="btn btn-danger btn-floating" onclick="removeProfile(event, ${profile.Id})">
                                    <i class="fa-solid fa-trash mr-2"></i>
                                </button>
                            </div>
                            <div class="row d-none">
                                <div class="form-outline col w-100">
                                    <input type="password" name="password${profile.Id}" id="password${profile.Id}" class="form-control" />
                                    <label class="form-label" for="password${profile.Id}">New Password</label>
                                </div>
                                <div class="col-auto pe-0">
                                    <button type="button" class="btn btn-success btn-floating" data-mdb-ripple-color="success" onclick="changePassword(event, ${profile.Id})">
                                        <i class="fa-solid fa-save fa-xl"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>`);
    $('#cardAddProfile').after(newCard);

    //showAlert('alert-success', 'alert-danger', `Added profile ${profile.FirstName} ${profile.LastName}`);
    showToast('bg-success', 'bg-danger', `The  ${profile.FirstName} ${profile.LastName} profile has been added successfully`)

    $('#email').val('');
    $('#password').val('');
}

function failurAddProfile(error) {
    //showAlert('alert-danger', 'alert-success', error.responseText);
    showAlert('bg-danger', 'bg-success', error.responseText);
}

function removeProfile(e, profileId) {
    var fullName = $(e.target).siblings('.card-title').text();
    $.post("/User/DeleteProfile", { profileId: profileId }, function () {
        e.target.closest('div .col').remove();
        showToast('bg-success', 'bg-danger', `${fullName}'s profile has been deleted`);
    }).fail(function () {
        showToast('bg-danger', 'bg-success', `Error deleting the profile of ${fullName}`);
    });
}

function editProfile(e, profileId) {
    var $divParent = $(e.target).closest('div');
    $divParent.addClass('d-none');
    $divParent.siblings('div').removeClass('d-none');
    mdb.Input.getInstance($(`#password${profileId}`).closest('div')[0]).update();
}

function changePassword(e, profileId) {
    var $divParent = $(e.target).closest('div .row');
    $divParent.addClass('d-none');
    $divParent.siblings('div').removeClass('d-none');

    var $inputPassword = $(`#password${profileId}`);

    if ($inputPassword.val()) {
        var fullName = $(e.target).closest('.card-body').find('.card-title').text();

        $.post("/User/ChangePassword", { profileId: profileId, password: $inputPassword.val() }, function () {
            showToast('bg-success', 'bg-danger', `${fullName}'s profile password has been changed`);
        }).fail(function () {
            showToast('bg-danger', 'bg-success', `Error change password the profile of ${fullName}`);
        });
        $(`#password${profileId}`).val('');
    }

}

function showAlert(addClass, removeClass, text) {
    var $alert = $('#alertAddProfile');
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