function setUserId() {
    var email = $('#email').val();
    if (email) {
        var $option = $('#users').find(`option[value="${email}"]`);
        if ($option.length > 0) {
            $('#userId').val($option.data('userid'));
        }
    }
}