var Tabs = function () {

    var editele = null;
    //TODO:: bootstrap-tabdrop.js orignal js
    var Institute = function (ele) {

        editele = ele;

        $(ele).on('click', '.tabclose', function () {

            //activating previous tab
            $(this).parent().prev().find('a').trigger('click');
            $(this).parent().prev().find('span').addClass('active');

            var paneid = $(this).data('paneid');
            $("#" + paneid).remove();
            $(this).parent().remove();
        });
    }

    return {

        //main function to initiate the module
        init: function (ele) {
            Institute(ele);
        },
        addTab: function (id, name, htmlContent, isactive) {

            if ($(editele + " li#li" + id).length) {
                $("#li" + id + " a").trigger('click');
                return;  // existing from function when tab with same id exits
            }
            var active = isactive ? "active" : "";

            if (isactive) {
                $(editele + ".tabclose").hide();
                $(editele + " li.active").removeClass('active')
                $(editele + " .tab-pane.active").removeClass('active')
            }

            var li = '<li id="li' + id + '" class="' + active + '">' +
                '<a style="display: inline-block" href="#tab' + id + '" data-toggle="tab">' + name + '</a>' +
                '<span data-paneid="tab' + id + '" class="tabclose ' + active + '">x</span>' +
                '</li>';

            var tabpane = '<div class="tab-pane ' + active + '" id="tab' + id + '">' + htmlContent + '</div>';

            $(editele + ' .nav.nav-tabs').append(li);
            $(editele + ' .tab-content').append(tabpane);
        },
        activeTab: function (tabid) {
            $(tabid + " a").trigger('click');
        },
        addAjaxTab: function (id,name, isactive, url, type) {
            $.ajax({
                url: url,
                type: type,
                success: function (res) {
                    
                    Tabs.addTab(id, name, res, isactive);
                },
                error: function () {
                    alert("error while retrieving data")
                },
                complete: function () { }
            });
        }
    };

}();

