//var popOverSettings = {
//    html: true,
//    trigger: 'focus',
//    selector: '[data-toggle="popover"]', //Sepcify the selector here
//    content: function () {
//        return '<img src="' + $(this).data('img') + '" />';
//    }
//}

//var setupPopover = function () {
//    $('body').popover(popOverSettings)
//}

//var updatePopover = function () {
//    $('button[rel=popover]').popover({
//        html: true,
//        trigger: 'focus',
//        content: function () { return '<img src="' + $(this).data('img') + '" />'; }
//    });
//};

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

var notifyImage = function (url) {
    new PNotify({
        title: 'Card Img',
        text: '<img class="center-block" src="' + url + '" />'
    });
}


var showModal = function (e) {
    $('#askModal').modal('show');
};

var hideModal = function (e) {
    $('#askModal').modal('hide');
    //$('body').removeClass('modal-open');
    //$('.modal-backdrop').remove();
};

var startTimerBar = function (e) {
    $("#timerBar").progressTimer({
        timeLimit: 75,
        warningThreshold: 15,
        baseStyle: 'progress-bar-warning',
        warningStyle: 'progress-bar-danger',
        completeStyle: 'progress-bar-info'
    });
};