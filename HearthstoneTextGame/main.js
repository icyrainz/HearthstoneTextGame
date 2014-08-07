var updatePopover = function () {
    $('button[rel=popover]').popover({
        html: true,
        trigger: 'focus',
        content: function () { return '<img src="' + $(this).data('img') + '" />'; }
    });
};