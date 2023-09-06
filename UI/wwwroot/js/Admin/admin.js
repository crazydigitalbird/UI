﻿function confirm(agencyId, agency) {
    $('.modal-body').text(`Delete the ${agency} agency?`);
    $('#btnDelete').on('click', function () { deleteAgency(agencyId); });
    $('#modalConfirm').modal('show');
}

function deleteAgency(agencyId) {
    window.location.href = `/Admin/DeleteAgency?agencyId=${agencyId}`;
}
