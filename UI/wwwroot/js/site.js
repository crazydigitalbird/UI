// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function setTheme(e) {
    $navMenu = $('#navMenu');
    if (e.target.checked) {
        $('link[href="/lib/MDB5-STANDARD-UI-KIT-Free-6.0.1/css/mdb.min.css"]').attr('href', '/lib/MDB5-STANDARD-UI-KIT-Free-6.0.1/css/mdb.dark.min.css');
        let date = new Date(Date.now() + 86400e3);
        date = date.toUTCString();
        document.cookie = "dark=1; expires=" + date;
        if (!$navMenu.hasClass('bg-dark')) {
            $navMenu.removeClass('bg-light');
            $navMenu.addClass('bg-dark');
        }
    }
    else {
        $('link[href="/lib/MDB5-STANDARD-UI-KIT-Free-6.0.1/css/mdb.dark.min.css"]').attr('href', '/lib/MDB5-STANDARD-UI-KIT-Free-6.0.1/css/mdb.min.css');
        document.cookie = "dark=; expires=-1";
        if (!$navMenu.hasClass('bg-light')) {
            $navMenu.removeClass('bg-dark');
            $navMenu.addClass('bg-light');
        }
    }    
}