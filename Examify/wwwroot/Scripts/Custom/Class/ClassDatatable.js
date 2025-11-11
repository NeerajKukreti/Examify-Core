// Class DataTable initialization and management
var ClassTable = function () {
    var table;
    var ClassBank = function () {
        table = $('#ClassTable');
        table.dataTable({
            "order": [],
            "ajax": function (data, callback, settings) {
                $.ajax({
                    url: loadClassUrl,
                    type: "GET",
                    dataType: "json",
                    success: function (response) {
                        callback({ data: response.data || [] });
                    }
                });
            },
            "columns": [
                {
                    "title": "Class Name", "data": "className"
                },
                {
                    "title": "Batches", "data": "batches",
                    "render": function (data, type, row) {
                        if (data && data.length > 0) {
                            const batchNames = data.map(batch => batch.batchName).join(', ');
                            return '<span class="badge bg-info me-1">' + data.length + '</span>' + batchNames;
                        }
                        return '<span class="text-muted">No batches</span>';
                    }
                },
                {
                    "title": "Status", "data": "isActive",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var active = '<i class="fas fa-check-circle text-success"></i>';
                        var inactive = '<i class="fas fa-times-circle text-danger"></i>';

                        var str = '<div class="btn-group" role="group">' +
                            '<a title="Click to ' + (oData.isActive ? 'Deactivate' : 'Activate') + ' it" data-id="' + oData.classId + '" onclick="changeStatus(' + oData.classId + ')">' +
                            (oData.isActive ? active : inactive) +
                            '</a>' +
                            '</div>';
                        $(nTd).html(str);
                    }
                },
                {
                    "title": "Action", "data": "isActive",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        str = '<div class="btn-group" role="group">' +
                            '<a style="cursor:pointer" class="text-decoration-underline" data-id=' + oData.classId +
                            ' onclick="editClass(' + oData.classId + ')"> <i class="fas fa-edit"></i>' +
                            '</a>' +
                            '</div>';
                        $(nTd).html(str);
                    }
                }
            ]
        });

        setInterval(function () {
            table.DataTable().ajax.reload(null, false);
        }, 30000);
    }

    return {
        init: function () {
            if (!$().dataTable) {
                return;
            }
            ClassBank();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false);
        }
    };
}();

// Global function to reload table
window.reloadClassTable = function() {
    ClassTable.reloadTable();
};

// Class management functions
function editClass(classId) {
    $.ajax({
        url: editClassUrl + "?id=" + classId,
        type: "GET",
        success: function (response) {
            $('.classModalBody').empty().html(response);
            $('#classModel').modal('show');
        },
        error: function () {
            if (typeof toastr !== 'undefined') {
                toastr.error('Failed to load class data for editing');
            } else {
                alert('Failed to load class data for editing');
            }
        }
    });
}

function changeStatus(classId) {
    if (confirm("Are you sure you want to change the status of this class?")) {
        $.ajax({
            url: changeStatusUrl + "/" + classId,
            type: "POST",
            success: function (response) {
                if (response.success) {
                    ClassTable.reloadTable();
                    if (typeof toastr !== 'undefined') {
                        toastr.success(response.message || 'Status changed successfully!');
                    } else {
                        alert(response.message || 'Status changed successfully!');
                    }
                } else {
                    if (typeof toastr !== 'undefined') {
                        toastr.error(response.message || 'Failed to change status');
                    } else {
                        alert(response.message || 'Failed to change status');
                    }
                }
            },
            error: function () {
                if (typeof toastr !== 'undefined') {
                    toastr.error('An error occurred while changing the status');
                } else {
                    alert('An error occurred while changing the status');
                }
            }
        });
    }
}

// Document ready initialization
$(document).ready(function () {
    // Initialize DataTable
    ClassTable.init();
    
    // Handle Create Class button click
    $('#CreateClass').on('click', function () {
        $.ajax({
            url: createClassUrl,
            type: "GET",
            success: function (response) {
                $('.classModalBody').empty().html(response);
                $('#classModel').modal('show');
            },
            error: function () {
                if (typeof toastr !== 'undefined') {
                    toastr.error('Failed to load create class form');
                } else {
                    alert('Failed to load create class form');
                }
            }
        });
    });

    console.log('? Class DataTable initialized');
});