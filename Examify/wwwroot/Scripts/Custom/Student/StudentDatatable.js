var StudentBankTable = function () {
    var table;
    var StudentBank = function () {
        table = $('#StudentTable'); // Fixed: matches the HTML table ID
        table.dataTable({
            "order": [], // This disables the initial sorting
            "createdRow": function (row, data, dataIndex) {
                // Custom row styling if needed
            },
            "ajax": function (data, callback, settings) {
                $.ajax({
                    url: loadStudentUrl,
                    type: "GET",
                    dataType: "json",
                    success: function (response) {
                        callback({ data: response.data || [] });
                    }
                });
            },
            "columns": [
                {
                    "title": "Student Name", "data": "studentName" 
                },
                {
                    "title": "Father Name", "data": "fatherName",
                    "render": function (data, type, row) {
                        return data || '-';
                    }
                },
                {
                    "title": "Mobile", "data": "mobile"
                },
                {
                    "title": "Email", "data": "email",
                    "render": function (data, type, row) {
                        return data || '-';
                    }
                },
                {
                    "title": "State", "data": "stateName",
                    "render": function (data, type, row) {
                        return data || '-';
                    }
                },
                {
                    "title": "Category", "data": "category",
                    "render": function (data, type, row) {
                        return data || '-';
                    }
                }, 
                {
                    "title": "Status", "data": "isActive", "width": "30px",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var active = '<i class="fas fa-check-circle text-success"></i>';
                        var inactive = '<i class="fas fa-times-circle text-danger" ></i>';

                        var str = '<div class="btn-group" role="group">' +
                            '<a title="Click to ' + (active ? 'Deactivate' : 'Activate') + ' it" data-id="' + oData.studentId + '" id="ActivateStudent">' +
                            (oData.isActive ? active : inactive) +
                            '</a>' +
                            '</div>';
                        $(nTd).html(str);
                    }
                },
                {
                    "data": "studentName", "width": "30px", "orderable": false, 
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        str = '<div class="btn-group" role="group">' +
                            '<a style="cursor:pointer" class="text-decoration-underline" data-id=' + oData.studentId +
                            ' data-bs-target="#studentModel" data-bs-toggle="modal" id="EditStudent">' + // Fixed: matches HTML modal ID
                            '<i class="fas fa-edit"></i>' +
                            '</a>' +
                            '</div>';

                        $(nTd).html(str);
                    }
                },
            ]
        });

        // Auto-refresh every 30 seconds
        setInterval(function () {
            table.DataTable().ajax.reload(null, false);
        }, 30000);
    }

    return {
        init: function () {
            if (!$().dataTable) {
                return;
            }
            StudentBank();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false);
        }
    };
}();

// Expose reloadStudentTable function globally for form success callbacks
window.reloadStudentTable = function () {
    if (StudentBankTable && typeof StudentBankTable.reloadTable === 'function') {
        StudentBankTable.reloadTable();
        console.log('✅ Student table reloaded');
    }
};

$(document).ready(function () {
    StudentBankTable.init();

    // Create Student Handler
    $(document).on('click', '#CreateStudent', function () {
        $.ajax({
            url: createStudentUrl,
            type: "GET",
            beforeSend: function () {
                // Show loading if needed
            },
            success: function (res) {
                $(".studentModalBody").empty().html(res);
                console.log('✅ Create student form loaded');
            },
            error: function (err) {
                if (typeof toastr !== 'undefined') {
                    toastr.error('Failed to load create student form');
                } else {
                    alert('Failed to load create student form');
                }
            }
        });
    });

    // Edit Student Handler
    $(document).on('click', '#EditStudent', function () {
        var $this = $(this);
        var studentId = $this.data('id');

        console.log('🔍 Edit clicked for student ID:', studentId);
        console.log('🔍 Edit URL:', editStudentUrl + "?id=" + studentId);

        $.ajax({
            url: editStudentUrl + "?id=" + studentId,
            type: "GET",
            beforeSend: function () {
                console.log('🔄 Loading edit form...');
                // Show loading in modal
                $(".studentModalBody").html('<div class="d-flex justify-content-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
            },
            success: function (res) {
                console.log('✅ Edit form loaded successfully');
                $(".studentModalBody").empty().html(res);
                console.log('✅ Edit student form loaded for ID:', studentId);
            },
            error: function (xhr, status, error) {
                console.error('❌ Edit student error:', error);
                console.error('❌ Status:', status);
                console.error('❌ Response:', xhr.responseText);

                if (typeof toastr !== 'undefined') {
                    toastr.error('Failed to load edit student form: ' + error);
                } else {
                    alert('Failed to load edit student form: ' + error);
                }
            }
        });
    });

    // Delete Student Handler
    $(document).on('click', '#ActivateStudent', function () {

        var $this = $(this);
        var studentId = $this.data('id'); 

        if ($this.find('i').hasClass('text-success')) {
            let userConfirmation = confirm("Are you sure you want to deactivate this user?");

            if (!userConfirmation) return;
        }

        $.ajax({
            url: changeStatusUrl + "/" + studentId,
            type: "POST",
            beforeSend: function () {
                // Show loading if needed
            },
            success: function (response) {
                if (response.success) {
                    // Reload the table
                    StudentBankTable.reloadTable();

                    // Show success message
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
            error: function (err) {
                if (typeof toastr !== 'undefined') {
                    toastr.error('An error occurred while changing the status');
                } else {
                    alert('An error occurred while changing the status');
                }
            }
        });

    });
});