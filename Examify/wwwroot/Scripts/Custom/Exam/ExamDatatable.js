
// API Configuration - will be set from window.examUrls
let API_BASE_URL = 'https://localhost:7271/api/Exam'; // fallback

var ExamTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue 
    var Exam = function () {

        var table = $('#ExamTable');

        //debugger;
        // begin first table
        table.dataTable({
             
            "ajax": {
                "url": `${API_BASE_URL}/list`,
                "type": "GET",
                "datatype": "json",
                "dataSrc": "",
                "error": function (xhr, error, thrown) {
                    console.error('DataTables AJAX error:', xhr.status, xhr.responseText);
                }
            },//
            "columns": [
                {
                    "title": "Exam", "data": "ExamName",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var url = "/Exam/Details?id=" + oData.ExamId;
                        $(nTd).html("<a href='" + url + "'>" + oData.ExamName + "</a>");
                    }
                },

                { "title": "Description", "data": "Description" },
            ],
             
          
        });

        //table.columns.adjust().draw();

        table.find('.group-checkable').change(function () {
            var set = jQuery(this).attr("data-set");
            var checked = jQuery(this).is(":checked");
            jQuery(set).each(function () {
                if (checked) {
                    $(this).prop("checked", true);
                    $(this).parents('tr').addClass("active");
                } else {
                    $(this).prop("checked", false);
                    $(this).parents('tr').removeClass("active");
                }
            });
            jQuery.uniform.update(set);
        });

        table.on('change', 'tbody tr .checkboxes', function () {
            $(this).parents('tr').toggleClass("active");
        });

        setInterval(function () {
            table.DataTable().ajax.reload(null, false); // user paging is not reset on reload
        }, 30000);
    }

    return {

        //main function to initiate the module
        init: function () {
            if (!jQuery().dataTable) {
                return;
            }

            Exam();
        }

    };

}();

if (App.isAngularJsApp() === false) {
    jQuery(document).ready(function () {
        ExamTables.init();
    });
}