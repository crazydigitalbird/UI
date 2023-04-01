function confirm(siteId, siteName) {
    $('.modal-body').text(`Delete the ${siteName} site?`);
    $('#btnDelete').on('click', function () { deleteSite(siteId, siteName); });
    $('#modalConfirm').modal('show');
}

function deleteSite(siteId, siteName) {
    window.location.href = `/Site/Delete?siteId=${siteId}&name=${siteName}`;
}