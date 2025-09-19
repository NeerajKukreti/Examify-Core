
$(document).on('click', '#btnAddNewStudent', function () {
    $.ajax({
        url: createStudentUrl,
        type:'GET',
        success: function (response) {
            Tabs.addTab('CreateStudent', 'Create Student', response, true);
        },
        error: function () { },
        complete: function () { }
    });
});

$(document).on('click', '.btnStudentFormSubmit', function () {

    var frmId = "#"+$(this).data("formid");

    if ($(frmId).valid()) {
        
        var frmData = new FormData(document.querySelector(frmId));
        var filebase = $("#files").get(0);
        var files = filebase.files;

        if (filebase.files.length) {
            frmData.append(files[0].name, files[0]);
        }

        $.ajax({
            url: createStudentUrl,
            type: "POST",
            contentType: false,
            processData: false,
            data: frmData,
            beforeSend: function () {
                
                App.blockUI({ // sample ui-blockui.js
                    
                    animate: true,
                    cenrerY: true,
                    
                });
            },
            success: function (Studentid) {
                
                $(frmId +" input#StudentId").val(Studentid); 
                ShowNotification("success", "Record Saved", "Student# " + Studentid +" Saved Successfully");
                StudentTable.reloadTable(); //reloads Student datatable
            },
            error: function (err) {
                
                ShowNotification("error", "Error occured", err.status + " " + err.statusText);
                App.unblockUI(); 
            },
            complete: function () { App.unblockUI(); }
        });
    }
   
    
});

function ActivateStudent(ele) {
    var active = $(ele).data('active');
    
    $.ajax({
        url: activateStudentUrl,
        type: 'GET',
        data: { Activate: active.toLowerCase() !== 'true', StudentId: $(ele).data('studentid') },
        success: function () {
            if (active.toLowerCase() !== 'true') {
                $(ele).html('<i class="fa fa-pencil"></i> Deactivate');
                $(ele).data('active', 'true');
            }
            else {
                $(ele).html('<i class="fa fa-pencil"></i> Activate');
                $(ele).data('active', 'false');
            }
        },
        error: function () {
        },
        complete: function () {
        }
    });
}

function DeleteStudent(ele) {
    var active = $(ele).data('delete');

    $.ajax({
        url: deleteStudentUrl,
        type: 'GET',
        data: { Delete: active.toLowerCase() !== 'true', StudentId: $(ele).data('studentid') },
        success: function () {
            if (active.toLowerCase() !== 'true') {
                $(ele).html('<i class="fa fa-trash-o"></i> Restore');
                $(ele).data('delete', 'true');
            }
            else {
                $(ele).html('<i class="fa fa-trash-o"></i> Delete');
                $(ele).data('delete', 'false');
            }
        },
        error: function () {
        },
        complete: function () {
        }
    });

}