const popUpGallery = document.querySelector('.pop-up.gallery'),
    popUpInformation = document.querySelector('.pop-up.information'),
    //popUpInformation = document.getElementById('information'),
    popUpVideo = document.querySelector('.pop-up.vidoe-player'),
    popUpPreviewPost = document.querySelector('.pop-up.preview'),
    popUpMail = document.querySelector('.pop-up.chatAndMail'),
    popUps = document.querySelectorAll('.pop-up');

//Information Person
$(document).on('click', '#interlocutorAvatarChatHeader, #ownerAvatarChatHeader', function (event) {
    informationPerson(event.target);
});

//Comment
$(document).on('click', '#commentBtn', function (event) {
    showComments();
});


//Media Gallery
$(document).on('click', '#attachment', function (event) {
    showGallery(false);
});

//Media Gallery Mail
$(document).on('click', '#attachmentMail', function (event) {
    showGallery(false);
});

//Midaia Gallery Post
$(document).on('click', '#attachmentPost', function (event) {
    showGallery(true);
});

//Mail
$(document).on('click', '#createMail', function (event) {
    showMail();
});

//Close pop-up
$(document).on('click', '.pop-up__close', function (event) {
    popUpVideoPause(event.target);
    closePopup(event.target);
});

//Closing the popup when clicked outside of it
//window.addEventListener('click', (e) => {
//    if (e.target == popUpInformation) {
//        popUpInformation.classList.add('d-none')
//    }
//    if (e.target == popUpGallery) {
//        popUpGallery.classList.add('d-none')
//    }
//    if (e.target == popUpVideo) {
//        popUpVideo.classList.add('d-none')
//    }
//})

window.addEventListener('click', (e) => {
    popUps.forEach(popUp => {
        if (e.target == popUp) {
            popUpVideoPause(popUp);
            popUp.classList.add('d-none')
        }
    })
});

function closePopup(e) {
    $(e).closest('.pop-up').addClass('d-none');
}

//Pop-up video player pause
function popUpVideoPause(popUp) {
    if ($(popUp).closest('.pop-up').is(popUpVideo)) {
        $(popUpVideo).find('video')[0].pause();
    }
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
        //var informationModal = new bootstrap.Modal(popUpInformation);
        if (idUser === idUserPopup) {
            popUpInformation.classList.remove('d-none');
            //informationModal.show();
        }
        else {
            $.post('/Chats/InformationPerson', { sheetId: sheetId, idUser: idUser }, function (data) {
                $('#informationPersonBody').html(data);
                popUpInformation.setAttribute('data-id-user', idUser);
                popUpInformation.classList.remove('d-none');
                //informationModal.show();
            });
        }
    }
}