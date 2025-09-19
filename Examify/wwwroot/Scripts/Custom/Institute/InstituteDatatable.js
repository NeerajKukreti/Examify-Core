var instituteTable = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue 
    var table;
    var Institute = function () {

        table = $('#InstituteTable');

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
                "url": LoadInstitute,
                "type": "GET",
                "datatype": "json"
            },//
            "columns": [
                
                {
                    "title": "Institute", "data": "InstituteName",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var tabName = "";
                        //alert(oData.ShortName )
                        if (oData.ShortName !== null && oData.ShortName.length) {
                            tabName = oData.ShortName.substr(0, 15);
                        }
                        else tabName = oData.InstituteName.substr(0, 15);

                        var url = EditInstitute + "?InstituteId=" + oData.InstituteId;
                        var editLnk = 'Tabs.addAjaxTab("EditInstitute_' + oData.InstituteId + '", "' + tabName + '", true, "' + url + '","GET")';
                        $(nTd).html("<a href='javascript:" + editLnk + "'>" + oData.InstituteName + "</a>");
                    }
                },
                { "title": "Password", "data": "Password" },
                { "title": "Phone", "data": "Phone" },
                { "title": "Email", "data": "EmailId" },
                { "title": "Address", "data": "Address" },
                { "title": "City", "data": "City" },
                //{ "title": "Pincode", "data": "Pincode" },
                { "title": "Active", "data": "IsActive" },
                { "title": "Valid Upto", "data": "Validity" },
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

            "bStateSave": true, // save datatable state(pagination, sort, etc) in cookie.

           

            "lengthMenu": [
                [5, 15, 20, -1],
                [5, 15, 20, "All"]  
            ], 
            "pageLength": 5,
            "pagingType": "bootstrap_full_number",
             
            "order": [
                [1, "asc"]
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

            Institute();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false); // user paging is not reset on reload
        }
    };

}();

if (App.isAngularJsApp() === false) {
    jQuery(document).ready(function () {
        instituteTable.init();
    });
}