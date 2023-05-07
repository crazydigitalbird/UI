const popUpInformation = document.querySelector('.pop-up.information');
const popUpGallery = document.querySelector('.pop-up.gallery');

$(document).on('click', '#interlocutorAvatarChatHeader, #ownerAvatarChatHeader', function (event) {
    informationPerson(event.target);
});

$(document).on('click', '#attachment', function (event) {
    showGallery(event.target);
});

$(document).on('click', '.pop-up__close', function (event) {
    closePopup(event.target);
});

//Closing the popup when clicked outside of it 
window.addEventListener('click', (e) => {
    if (e.target == popUpInformation) {
        popUpInformation.classList.add('d-none')
    }
    if (e.target == popUpGallery) {
        popUpGallery.classList.add('d-none')
    }
})

function closePopup(e) {
    $(e).closest('.pop-up').addClass('d-none');
}

//Pop-up information
function informationPerson(e) {
    var sheetId = $('#manMessagesMails').data('sheet-id');
    var idUser;
    if (e.id === 'interlocutorAvatarChatHeader') {
        idUser = $('#interlocutorIdChatHeader').text();
    }
    else if (e.id === 'ownerAvatarChatHeader') {
        idUser = $('#ownerIdChatHeader').text();
    }
    if (sheetId && idUser) {
        var idUserPopup = popUpInformation.getAttribute('data-id-user');
        if (idUser === idUserPopup) {
            popUpInformation.classList.remove('d-none');
        }
        else {
            $.post('/Chats/InformationPerson', { sheetId: sheetId, idUser: idUser }, function (data) {
                $('#informationPersonBody').html(data);
                popUpInformation.setAttribute('data-id-user', idUser);
                popUpInformation.classList.remove('d-none');
            });
        }
    }
}