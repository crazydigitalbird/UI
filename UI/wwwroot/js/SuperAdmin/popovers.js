$(function () {
    var myDefaultAllowList = mdb.Tooltip.Default.allowList;

    var ARIA_ATTRIBUTE_PATTERN = /^aria-[\w-]*$/i
    myDefaultAllowList['*'].push(ARIA_ATTRIBUTE_PATTERN);

    myDefaultAllowList.table = ['name'];
    myDefaultAllowList.thead = [];
    myDefaultAllowList.tbody = [];
    myDefaultAllowList.tr = ['onclick'];
    myDefaultAllowList.th = [];
    myDefaultAllowList.td = ['name', 'colspan'];

    myDefaultAllowList.ul = ['aria-labelledby'];
    myDefaultAllowList.li = [];
    myDefaultAllowList.input = ['onkeyup', 'placeholder', 'type', 'value', 'name'];

    myDefaultAllowList.button = ['data-mdb-toggle'];

    myDefaultAllowList['a'].push('onclick');
});

//Closing the popover when clicked outside of it 
$(document).on('click', function (e) {
    $('[data-mdb-toggle="popover"],[data-original-title]').each(function () {
        if (!$(this).is(e.target) && $(this).has(e.target).length === 0 && $('.popover').has(e.target).length === 0) {
            if ($(this).attr('popover-show') == 1) {
                $(this).popover('toggle');
            }
        }
    })
})

//It fires when resetting the view of the table ( resize, sort, search, filter ...)
$('#superAdminTable').on('reset-view.bs.table', function (e) {
    //Deleting all popovers. Eliminates the unpinned popover effect
    $('.popover').remove();

    //Initialize all popovers on a page would be to select them data-mdb-toggle attribute
    $('[data-mdb-toggle="popover"]').popover({
        html: true
    }).on('show.bs.popover', function (event) { // This event fires immediately the show instance method is called

        //Hide all popover
        $('[data-mdb-toggle="popover"]').popover('hide');

        //Get a popover associated with an element
        var popover = mdb.Popover.getInstance(event.target);
        var id = $(event.target).data('idField');

        //Check installed content in popover
        if (!popover._config.content) {

            //Get popover content
            var content;
            $.ajax({
                async: false,
                method: "POST",
                url: '/SuperAdmin/GetOperators',
                data: { profileId: id },
            })
                .done(function (data) {
                    content = data;
                })
                .fail(function () {
                    content = 'fail';
                });

            //Installing popover content
            popover._config.content = content;
        }
    }).on('shown.bs.popover', function (event) {
        $(event.target).attr('popover-show', '1');
    }).on('hide.bs.popover', function (event) {
        var profileId = $('input[name=popoverProfileId]').val();
        var operators = $('table[name=operators] tbody tr:not(.tr-placeholder)').length;
        $('#superAdminTable').bootstrapTable('updateCellByUniqueId', {
            id: profileId,
            field: 'operators',
            value: operators,
            reinit: false
            });
    }).on('hidden.bs.popover', function (event) {
        $(event.target).attr('popover-show', '0');
    });
});

//Handler for clicking on the btn-close in popover
function popoverClose(e) {
    mdb.Popover.getInstance($('[popover-show=1]')).hide();
}

//Deleting an operator from the list and adding an operator to the list free operators
function deleteOperator(e, operatorId) {
    var profileId = $(e.target.closest('div')).find('input[name="popoverProfileId"]').val();
    $.post('/SuperAdmin/DeleteOperatorFromProfile', { operatorId: operatorId, profileId: profileId }, function () {
        setTimeout(() => {
            var $deleteRow = $(e.target.closest('tr'));
            $deleteRow.find('td[name]').each(function () {
                this.remove();
            });
            var $newRow = $deleteRow.clone();
            $newRow.attr('onclick', 'addOperator(event)');
            var $freeOperatorsBody = $deleteRow.closest('div').find('table[name="freeOperators"]');
            $freeOperatorsBody.append($newRow);
            $deleteRow.remove();
        }, 10);
    }).fail(function (error) {
        failurePopover(error.responseText);
    });
}

//Search operators in the list free operators
function searchOperator(e) {
    var searchText = e.target.value;
    $(e.target).parent().find('tr').each(function () {
        if (searchText && !$(this).text().includes(searchText)) {
            $(this).addClass('d-none');
        }
        else {
            $(this).removeClass('d-none')
        }
    });
}

//Adding an operator from the list free operators
function addOperator(e) {
    var $row = $(e.target.closest('tr'));
    var operatorId = $row.find('td').first().text();
    var profileId = $(e.target.closest('#dropdownOperators')).siblings('input[name="popoverProfileId"]').val();

    $.post('/SuperAdmin/AddOperatorFromProfile', { operatorId: operatorId, profileId: profileId }, function (data) {
        var team = data;

        $addRow = $row.clone();
        $addRow.removeAttr('onclick');
        $addRow.find('td').last().before(`<td name="Team">${team}</td>`);
        $addRow.find('td').last().after(`<td name="Action">
                            <a role="button" class="btn btn-sm btn-info" onclick="addTempOperator(event)">
                                <i class="fa-solid fa-clock fa-xl"></i>
                            </a>
                            <a role="button" class="btn btn-sm btn-danger" onclick="deleteOperator(event, ${operatorId})">
                                <i class="fa-solid fa-trash fa-xl"></i>
                            </a>
                          </td>`);
        var $operatorsBody = $(e.target.closest('#dropdownOperators')).siblings('table[name="operators"]');
        $operatorsBody.append($addRow);
        setTimeout(() => $row.remove(), 10);
    }).fail(function (error) {
        failurePopover(error.responseText);
    });

}

//Addint an temp operator from the list operators
function addTempOperator(e) {
}

function failurePopover(error) {

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