//document.getElementById('pwd').onfocus = function () {
//    document.getElementById('pwd').removeAttribute('readonly');
//}

const popUpAddUser = document.querySelector('.pop-up.addUser');

//Show pop-up addUser
$(document).on('click', '#btnAddUser', function (event) {
    popUpAddUser.classList.remove('d-none');
});

//Close pop-up addUser
$(document).on('click', '.pop-up__close', function (event) {
    closePopUpAddUser()
});

window.addEventListener('click', (e) => {
    if (e.target == popUpAddUser) {
        closePopUpAddUser()
    }
});

function closePopUpAddUser() {
    popUpAddUser.classList.add('d-none');
    clearFromAddUser();
}

function clearFromAddUser() {
    document.getElementById('formAddUser').reset();
    document.getElementById('formAddUser').classList.remove('was-validated');
}

function deleteUser(e, userId) {
    var tr = e.target.closest('tr');

    var option = document.createElement('option');
    $(option).attr('value', $(tr).find('td[name="email"]').text());
    $(option).text($(tr).find('td[name="email"]').text());
    $(option).attr('data-userid', $(tr).find('input').val());
    $('datalist').append(option);

    /*var $optionDatalist = $(`<option value="${}" data-userid=${}>${}</option>`);*/

    tr.remove();
    updateCounterUsers();
}

function addUser(e) {
    var $input = $('#newUser');/*$(e.target).siblings('input');*/
    if ($input.val() && !$input.hasClass('is-invalid') && $(`datalist option[value="${$input.val()}"]`).length > 0) {
        var $optionDatalist = $('datalist').find(`option[value="${$input.val()}"]`);
        var userId = $optionDatalist.data('userid');
        var index = $('tbody > tr:not(.tr-placeholder)').length;
        var $row = $(`<tr>
                    <input type="hidden" name="Users[${index}].Id" value="${userId}">
                            <td name="email">${$input.val()}</td>
                            <td class="w-25">
                                <select class="form-select form-select-sm" name="Users[${index}].Role">
                                    <option value="1">AdminAgency</option>
                                    <option value="2">Operator</option>
                                    <option value="3" selected="selected">User</option>
                                </select>
                            </td>
                            <td class="text-center">
                                <button type="button" class="btn btn-sm btn-danger" onclick="deleteUser(event, ${userId})">Delete</button>
                            </td>
                        </tr>`);
        $('table').append($row);
        $input.val('');
        $optionDatalist.remove();
        updateCounterUsers();
    }
}

function successAddUser(user) {
    clearFromAddUser();

    var index = $('tbody > tr:not(.tr-placeholder)').length;

    var originalUser = `<input type="hidden" name="originalUsers[${index}].Id" value="${user.id}" />
                        <input type="hidden" name="originalUsers[${index}].Role" value="${user.role}" />`;            

    $('#originalUsersDiv').append(originalUser);

    let selectedAdminAgency = user.role === 'AdminAgency' ? 'selected' : '';
    let selectedOperator = user.role === 'Operator' ? 'selected' : '';

    var $row = $(`<tr>
                    <input type="hidden" name="Users[${index}].Id" value="${user.id}">
                            <td name="email">${user.email}</td>
                            <td class="w-25">
                                <select class="form-select form-select-sm" name="Users[${index}].Role">
                                    <option ${selectedAdminAgency} value="1">AdminAgency</option>
                                    <option ${selectedOperator} value="2">Operator</option>
                                </select>
                            </td>
                            <td class="text-center">
                                <button type="button" class="btn btn-sm btn-danger" onclick="deleteUser(event, ${user.id})">Delete</button>
                            </td>
                        </tr>`);
    $('table').append($row);
    updateCounterUsers();

    closePopUpAddUser();

    $('#toastBody').html(`The ${user.email} has been added successfully`)

    var toast = $('#toast');

    if (toast.hasClass('bg-danger')) {
        toast.removeClass('bg-danger');
    }

    if (!toast.hasClass('bg-success')) {
        toast.addClass('bg-success');
    }

    setTimeout(() => mdb.Toast.getInstance(toast).show(), 500);
}

function failureAddUser(error) {
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

function updateCounterUsers() {
    $('tbody > tr:not(.tr-placeholder)').each(function (index) {
        $(this).find('input').attr('name', `Users[${index}].Id`);
        $(this).find('select').attr('name', `Users[${index}].Role`);
    });
}