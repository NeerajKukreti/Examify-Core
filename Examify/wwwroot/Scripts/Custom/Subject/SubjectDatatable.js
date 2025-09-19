var SubjectTable = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue 
    var table;
    var Subject = function () {

        table = $('#SubjectTable');

        // begin first table
        table.DataTable({
            "createdRow": function (row, data, dataIndex) {

                if (!data.IsActive) {
                    $('td', row).eq(0).addClass('ActiveRow');
                }

                if (data.IsDeleted) {
                    $('td', row).eq(0).addClass('DeletedRow');
                }
                //
            },
            "ajax": {
                "url": LoadSubject,
                "type": "GET",
                "datatype": "json"
            },//
            "columns": [
                //{
                //    "title": "Id", "data": "SubjectId"
                //},
                {
                    "title": "Subject", "data": "SubjectName",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var tabName = "";
                        //alert(oData.ShortName )

                        tabName = oData.SubjectName.substr(0, 15);

                        var url = EditSubject + "?SubjectId=" + oData.SubjectId;
                        var editLnk = 'Tabs.addAjaxTab("EditSubject_' + oData.SubjectId + '", "' + tabName + '", true, "' + url + '","GET")';
                        $(nTd).html("<a href='javascript:" + editLnk + "'>" + oData.SubjectName + "</a>");
                    }
                },
                { "title": "Description", "data": "Description" }
                
            ],

            // Internationalisation. For more info refer to http://datatables.net/manual/i18n
            "language": {
                "aria": {
                    "sortAscending": ": activate to sort column ascending",
                    "sortDescending": ": activate to sort column descending"
                },
                "emptyTable": "No data available in table",
                "info": "Showing _START_ to _END_ of _TOTAL_ records",
                "infoEmpty": "No records found",
                "infoFiltered": "(filtered1 from _MAX_ total records)",
                "lengthMenu": "Show _MENU_",
                "search": "Search:",
                "zeroRecords": "No matching records found",
                "paginate": {
                    "previous": "Prev",
                    "next": "Next",
                    "last": "Last",
                    "first": "First"
                }
            },

            // Or you can use remote translation file
            //"language": {
            //   url: '//cdn.datatables.net/plug-ins/3cfcc339e89/i18n/Portuguese.json'
            //},

            // Uncomment below line("dom" parameter) to fix the dropdown overflow issue in the datatable cells. The default datatable layout
            // setup uses scrollable div(table-scrollable) with overflow:auto to enable vertical scroll(see: assets/global/plugins/datatables/plugins/bootstrap/dataTables.bootstrap.js). 
            // So when dropdowns used the scrollable div should be removed. 
            //"dom": "<'row'<'col-md-6 col-sm-12'l><'col-md-6 col-sm-12'f>r>t<'row'<'col-md-5 col-sm-12'i><'col-md-7 col-sm-12'p>>",

            "bStateSave": true, // save datatable state(pagination, sort, etc) in cookie.

           

            "lengthMenu": [
                [5, 15, 20, -1],
                [5, 15, 20, "All"] // change per page values here
            ],
            // set the initial value
            "pageLength": 5,
            "pagingType": "bootstrap_full_number",
            "columnDefs": [
                //{  // set default column settings
                //'orderable': false,
                //'targets': [0]
                //},
                {
                    "targets": 0,
                    "className": "text-left",
                }
            ],
            "order": [
                [1, "asc"]
            ], // set first column as a default sort by asc

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

            Subject();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false); // user paging is not reset on reload
        }
    };

}();

if (App.isAngularJsApp() === false) {
    jQuery(document).ready(function () {
        SubjectTable.init();
    });
}