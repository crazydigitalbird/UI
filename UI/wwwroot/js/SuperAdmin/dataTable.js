var $table = $('#superAdminTable');

$(function () {
    initTable();

    $(window).resize(function () {
        setTimeout(() => {
            $('#superAdminTable').bootstrapTable("refreshOptions", {
                stickyHeaderOffsetY: $('#navMenu').outerHeight(),
                height: getHeight()
            })
        }, 100);
    });
})

function initTable() {
    $("#tableDiv").removeClass('d-none');

    $table.bootstrapTable();

    setTimeout(() => {
        $table.bootstrapTable("refreshOptions", {
            stickyHeaderOffsetY: $('#navMenu').outerHeight(),
            height: getHeight()
        })
    }, 10);

    //$table.bootstrapTable('uncheckAll');
}

function getHeight() {
    var freeAreaHeight = $(window).height() - $('#navMenu').outerHeight() - $('#toolbar').outerHeight();
    return freeAreaHeight;
    //var initialTableHeight = $('#tableDiv').outerHeight() + 2;
}

function groupFormatter(value, row) {
    var $select = $('#groupsSelect').clone();
    $select.attr('onchange', `changeGroup(event, ${row['id']})`);
    $select.attr('onfocus', 'getValueSelect(event)');

    var $optionDefautl = $select.find('option[value="0"]').first();
    $optionDefautl.text('Please select Group');

    var $option = $select.find(`option[value="${value}"]`).first();
    $option.attr('selected', 'selected');

    return $select[0].outerHTML;
}

function shiftFormatter(value, row) {
    var $select = $('#shiftsSelect').clone();
    $select.attr('onchange', `changeShift(event, ${row['id']})`);
    $select.attr('onfocus', 'getOldValueShift(event)');

    var $optionDefautl = $select.find('option[value="0"]').first();
    $optionDefautl.text('Please select Shift');

    var $option = $select.find(`option[value="${value}"]`).first();
    $option.attr('selected', 'selected');

    return $select[0].outerHTML;
}

function cabinetFormatter(value, row) {
    var $select = $('#cabinetsSelect').clone();
    $select.attr('onchange', `changeCabinet(event, ${row['id']})`);
    $select.attr('onfocus', 'getOldValueCabinet(event)');

    var $optionDefautl = $select.find('option[value="0"]').first();
    $optionDefautl.text('Please select Cabinet');

    var $option = $select.find(`option[value="${value}"]`).first();
    $option.attr('selected', 'selected');

    return $select[0].outerHTML;
}