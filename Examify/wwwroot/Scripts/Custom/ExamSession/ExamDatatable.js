var ExamTable = function () {
    var table;
    var Exam = function () {
        table = $('#ExamTable');
        table.dataTable({
            "order": [],
            "ajax": {
                "url": loadExamUrl,
                "type": "GET",
                "dataType": "json",
                "dataSrc": "data"
            },
            "columns": [
                {
                    "title": "Exam Name", "data": "examName",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var str = '<div class="btn-group" role="group">' +
                            '<a style="cursor:pointer" class="text-decoration-underline" data-id=' + oData.examId +
                            ' data-bs-target="#examModel" data-bs-toggle="modal" id="EditExam">' +
                            oData.examName +
                            '</a>' +
                            '</div>';
                        $(nTd).html(str);
                    }
                },
                {
                    "title": "Description", "data": "description",
                    "render": function (data, type, row) {
                        return data || '-';
                    }
                },
                {
                    "title": "Duration", "data": "durationMinutes",
                    "render": function (data, type, row) {
                        return data + ' mins';
                    }
                },
                {
                    "title": "Questions", "data": "totalQuestions"
                },
                {
                    "title": "Type", "data": "examType",
                    "render": function (data, type, row) {
                        return data || '-';
                    }
                },
                {
                    "title": "Status", "data": "isActive",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var active = '<i class="fas fa-check-circle text-success"></i>';
                        var inactive = '<i class="fas fa-times-circle text-danger"></i>';
                        var str = '<div class="btn-group" role="group">' +
                            '<a title="Click to ' + (oData.isActive ? 'Deactivate' : 'Activate') + ' it" data-id="' + oData.examId + '" id="ActivateExam">' +
                            (oData.isActive ? active : inactive) +
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
            Exam();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false);
        }
    };
}();

window.reloadExamTable = function () {
    if (ExamTable && typeof ExamTable.reloadTable === 'function') {
        ExamTable.reloadTable();
        console.log('✅ Exam table reloaded');
    }
};

$(document).ready(function () {
    ExamTable.init();

    $(document).on('click', '#btnAddNewExam', function () {
        $.ajax({
            url: createExamUrl,
            type: "GET",
            success: function (res) {
                $(".examModalBody").empty().html(res);
                $('#examModel').modal('show');
                console.log('✅ Create exam form loaded');
            },
            error: function (err) {
                if (typeof toastr !== 'undefined') {
                    toastr.error('Failed to load create exam form');
                } else {
                    alert('Failed to load create exam form');
                }
            }
        });
    });

    $(document).on('click', '#EditExam', function () {
        var examId = $(this).data('id');
        $.ajax({
            url: editExamUrl + "?id=" + examId,
            type: "GET",
            beforeSend: function () {
                $(".examModalBody").html('<div class="d-flex justify-content-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
            },
            success: function (res) {
                $(".examModalBody").empty().html(res);
                console.log('✅ Edit exam form loaded for ID:', examId);
            },
            error: function (xhr, status, error) {
                if (typeof toastr !== 'undefined') {
                    toastr.error('Failed to load edit exam form: ' + error);
                } else {
                    alert('Failed to load edit exam form: ' + error);
                }
            }
        });
    });

    $(document).on('click', '#ActivateExam', function () {
        var $this = $(this);
        var examId = $this.data('id');

        if ($this.find('i').hasClass('text-success')) {
            let userConfirmation = confirm("Are you sure you want to deactivate this exam?");
            if (!userConfirmation) return;
        }

        $.ajax({
            url: changeStatusUrl + "/" + examId,
            type: "POST",
            success: function (response) {
                if (response.success) {
                    ExamTable.reloadTable();
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
