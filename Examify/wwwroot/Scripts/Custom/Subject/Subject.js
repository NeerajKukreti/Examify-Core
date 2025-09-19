
$(document).on('click', '#btnAddNewSubject', function () {
    $.ajax({
        url: createSubjectUrl,
        type: 'GET',
        beforeSend: function () {
            App.blockUI({ // sample ui-blockui.js
                animate: true,
                cenrerY: true
            });
        },
        success: function (response) {
            Tabs.addTab('CreateSubject', 'Create Subject', response, true);
        },
        error: function () { App.unblockUI(); },
        complete: function () { App.unblockUI(); }
    });
});

$(document).on('click', '.btnSubjectFormSubmit', function () {

    var frmId = "#"+$(this).data("formid");

    if ($(frmId).valid()) {
        
        var frmData = new FormData(document.querySelector(frmId));
        var filebase = $("#files").get(0);
        var files = filebase.files;
        debugger;
        frmData.set("Topics", frmData.getAll("TopicList").join());

        if (filebase.files.length) {
            frmData.append(files[0].name, files[0]);
        }

        $.ajax({
            url: createSubjectUrl,
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
            success: function (Subjectid) {
                
                $(frmId + " input#SubjectId").val(Subjectid); 
                ShowNotification("success", "Record Saved", "Subject# " + Subjectid +" Saved Successfully");
                SubjectTable.reloadTable(); //reloads Subject datatable
            },
            error: function (err) {
                
                ShowNotification("error", "Error occured", err.status + " " + err.statusText);
                App.unblockUI(); 
            },
            complete: function () { App.unblockUI(); }
        });
    }
   
    
});

function ActivateSubject(ele) {
    var active = $(ele).data('active');
    
    $.ajax({
        url: activateSubjectUrl,
        type: 'GET',
        data: { Activate: active.toLowerCase() !== 'true', SubjectId: $(ele).data('subjectid') },
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

function DeleteSubject(ele) {
    var active = $(ele).data('delete');

    $.ajax({
        url: deleteSubjectUrl,
        type: 'GET',
        data: { Delete: active.toLowerCase() !== 'true', SubjectId: $(ele).data('subjectid') },
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