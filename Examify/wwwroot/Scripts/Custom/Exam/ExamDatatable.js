var ExamTable = function () {
    var table;
    var Exam = function () {
        table = $('#ExamTable');
        table.dataTable({
            "order": [],             
            "ajax": function (data, callback, settings) {
                $.ajax({
                    url: loadExamUrl,
                    type: "GET",
                    dataType: "json",
                    success: function (response) {
                        callback({ data: response.data || [] });
                    }
                });
            },
            "columns": [
                {
                    "title": "Exam Name", "data": "examName" 
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
                ,
                {
                    "title": "Edit", "data": "examId", "orderable": false,
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var str = '<div class="btn-group" role="group">' +
                            '<a style="cursor:pointer" class="text-decoration-underline" data-id=' + oData.examId +
                            ' data-bs-target="#examModel" data-bs-toggle="modal" id="EditExam">' +
                            '<i class="fas fa-edit"></i>' +
                            '</a>' +
                            '</div>';
                        $(nTd).html(str);
                    }
                },
                {
                    "title": "Configure", "data": "examId", "orderable": false,
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var str = '<a href="/Exam/ConfigureQuestions/' + oData.examId + '" class="btn btn-sm btn-primary" title="Configure Exam">' +
                            '<i class="fas fa-cog"></i> ' +
                            '</a>';
                        $(nTd).html(str);
                    }
                },
                {
                    "title": "Publish", "data": "examId", "orderable": false,
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var btnClass = oData.isPublished ? 'btn-success' : 'btn-outline-secondary';
                        var iconClass = oData.isPublished ? 'fas fa-paper-plane' : 'fas fa-paper-plane text-muted';
                        var str = '<button class="btn btn-sm ' + btnClass + '" data-id="' + oData.examId + '" id="PublishExam" title="' + (oData.isPublished ? 'Unpublish' : 'Publish') + ' Exam">' +
                            '<i class="' + iconClass + '"></i>' +
                            '</button>';
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

    $(document).on('click', '#PublishExam', function () {
        var examId = $(this).data('id');
        var isPublished = $(this).hasClass('btn-success');
        var action = isPublished ? 'unpublish' : 'publish';
        
        if (confirm('Are you sure you want to ' + action + ' this exam?')) {
            $.ajax({
                url: window.API_ENDPOINTS.baseUrl + 'Exam/'+examId+'/publish',
                //url: PublishExamUrl.replace("{id}", examId),
                type: 'POST',
                success: function (response) {
                    if (response.Success) {
                        ExamTable.reloadTable();
                        if (typeof toastr !== 'undefined') {
                            toastr.success(response.message || 'Exam published successfully!');
                        } else {
                            alert(response.message || 'Exam published successfully!');
                        }
                    } else {
                        if (typeof toastr !== 'undefined') {
                            toastr.error(response.message || 'Failed to publish exam');
                        } else {
                            alert(response.message || 'Failed to publish exam');
                        }
                    }
                },
                error: function (err) {
                    if (typeof toastr !== 'undefined') {
                        toastr.error('An error occurred while publishing the exam');
                    } else {
                        alert('An error occurred while publishing the exam');
                    }
                }
            });
        }
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
