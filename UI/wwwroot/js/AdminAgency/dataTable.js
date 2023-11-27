$("body").css('overflow', 'hidden');
var $table = $('#adminAgencyTable');

$(function () {
    initTable();

    $(window).resize(function () {
        setHeight();
    });

    $.ajaxSetup({
        headers: {
            RequestVerificationToken: $('[name=__RequestVerificationToken]').first().val()
        }
    });
});

function initTable() {
    $("#tableDiv").removeClass('d-none');

    $table.bootstrapTable();

    setHeight();

    //$table.bootstrapTable('uncheckAll');
}

function setHeight() {
    var freeAreaHeight = $(window).height() - $('#navMenu').outerHeight() - $('#toolbar').outerHeight();
    var adminAgencyTableHeight = $('.fixed-table-toolbar').outerHeight() + $('#adminAgencyTable').outerHeight();
    if (adminAgencyTableHeight > freeAreaHeight) {
        setTimeout(() => {
            $table.bootstrapTable("refreshOptions", {
                stickyHeaderOffsetY: $('#navMenu').outerHeight(),
                height: freeAreaHeight
            })
        }, 10);
    }
    else {
        setTimeout(() => {
            $table.bootstrapTable("refreshOptions", {
                stickyHeaderOffsetY: $('#navMenu').outerHeight(),
                height: 0
            })
        }, 10);
    }
    //var initialTableHeight = $('#tableDiv').outerHeight() + 2;
}

function operatorsFormatter(value, row) {
    var operatorHtml = `<div class="row justify-content-center">
                    <div class="col-6 text-end pe-0">
                        ${value}
                    </div>
                    <div class="col-6 text-start pe-0">
                        <a role="button" title="<div class='row'><div name='popoverProfileId' class='d-none'>${row['id']}</div><div class='col-10'>${row['name']} ${row['lastName']}</div><div class='col-2 text-end'><a href='#' role='button' class='btn-close' aria-label='Close' onclick='popoverClose(event)'></a></div></div>" data-mdb-toggle="popover" data-id-field="${row['id']}">
                            <i class="fa-solid fa-circle-info text-primary"></i>
                        </a>
                    </div>
                </div>`;

    return operatorHtml;
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
