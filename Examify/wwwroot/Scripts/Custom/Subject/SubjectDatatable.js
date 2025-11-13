var SubjectTable = function () {
    var table;
    var Subject = function () {
        table = $('#SubjectTable');
        table.dataTable({
            "order": [],             
            "ajax": function (data, callback, settings) {
                $.ajax({
                    url: loadSubjectUrl,
                    type: "GET",
                    dataType: "json",
                    success: function (response) {
                        callback({ data: response.data || [] });
                    }
                });
            },
            "columns": [
                {
                    "title": "Subject Name", "data": "subjectName" 
                },
                {
                    "title": "Description", "data": "description",
                    "render": function (data, type, row) {
                        return data || '-';
                    }
                },
                {
                    "title": "Status", "data": "isActive", "width": "30px",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var active = '<i class="fas fa-check-circle text-success"></i>';
                        var inactive = '<i class="fas fa-times-circle text-danger"></i>';
                        var str = '<div class="btn-group" role="group">' +
                            '<a title="Click to ' + (oData.isActive ? 'Deactivate' : 'Activate') + ' it" data-id="' + oData.subjectId + '" id="ActivateSubject">' +
                            (oData.isActive ? active : inactive) +
                            '</a>' +
                            '</div>';
                        $(nTd).html(str);
                    }
                },
                {
                    "data": "subjectName", "width": "10px", "orderable": false,  
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var str = '<div class="btn-group" role="group">' +
                            '<a style="cursor:pointer" class="text-decoration-underline" data-id=' + oData.subjectId +
                            ' data-bs-target="#subjectModel" data-bs-toggle="modal" id="EditSubject">' +
                            '<i class="fas fa-edit text-primary"></i>' +
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
            Subject();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false);
        }
    };
}();

window.reloadSubjectTable = function () {
    if (SubjectTable && typeof SubjectTable.reloadTable === 'function') {
        SubjectTable.reloadTable();
        console.log('✅ Subject table reloaded');
    }
};

$(document).ready(function () {
    SubjectTable.init();

    $(document).on('click', '#btnAddNewSubject', function () {
        $.ajax({
            url: createSubjectUrl,
            type: "GET",
            success: function (res) {
                $(".subjectModalBody").empty().html(res);
                $('#subjectModel').modal('show');
                console.log('✅ Create subject form loaded');
            },
            error: function (err) {
                if (typeof toastr !== 'undefined') {
                    toastr.error('Failed to load create subject form');
                } else {
                    alert('Failed to load create subject form');
                }
            }
        });
    });

    $(document).on('click', '#EditSubject', function () {
        var subjectId = $(this).data('id');
        $.ajax({
            url: editSubjectUrl + "?id=" + subjectId,
            type: "GET",
            beforeSend: function () {
                $(".subjectModalBody").html('<div class="d-flex justify-content-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
            },
            success: function (res) {
                $(".subjectModalBody").empty().html(res);
                console.log('✅ Edit subject form loaded for ID:', subjectId);
            },
            error: function (xhr, status, error) {
                if (typeof toastr !== 'undefined') {
                    toastr.error('Failed to load edit subject form: ' + error);
                } else {
                    alert('Failed to load edit subject form: ' + error);
                }
            }
        });
    });

    $(document).on('click', '#ActivateSubject', function () {
        var $this = $(this);
        var subjectId = $this.data('id');

        if ($this.find('i').hasClass('text-success')) {
            let userConfirmation = confirm("Are you sure you want to deactivate this subject?");
            if (!userConfirmation) return;
        }

        $.ajax({
            url: changeStatusUrl + "/" + subjectId,
            type: "POST",
            success: function (response) {
                if (response.success) {
                    SubjectTable.reloadTable();
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
