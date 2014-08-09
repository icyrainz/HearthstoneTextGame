var updatePopover = function () {
    $('button[rel=popover]').popover({
        html: true,
        trigger: 'click',
        content: function () { return '<img src="' + $(this).data('img') + '" />'; }
    });
};