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

function updateCounterUsers() {
    $('tbody > tr:not(.tr-placeholder)').each(function (index) {
        $(this).find('input').attr('name', `Users[${index}].Id`);
        $(this).find('select').attr('name', `Users[${index}].Role`);
    });
}