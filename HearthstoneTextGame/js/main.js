var updatePopover = function () {
    $('button[rel=popover]').popover({
        html: true,
        trigger: 'click',
        content: function () { return '<img src="' + $(this).data('img') + '" />'; }
    });
};

var notify = function (msg) {
    new PNotify({
        title: 'Notice',
        text: msg,
        delay: 1000
    });
};

var notifyInfo = function (msg) {
    new PNotify({
        title: 'Info',
        text: msg,
        delay: 1000,
        type: 'info'
    });
};

var notifySuccess = function (msg) {
    new PNotify({
        title: 'Success',
        text: msg,
        delay: 1000,
        type: 'success'
    });
};

var notifyError = function (msg) {
    new PNotify({
        title: 'Error',
        text: msg,
        delay: 1000,
        type: 'error'
    });
};