
$(document).on('click', '#btnAddNewInstitute', function () {
    $.ajax({
        url: createInstituteUrl,
        type:'GET',
        success: function (response) {
            Tabs.addTab('CreateInstitute', 'Create Institute', response, true);
        },
        error: function () { },
        complete: function () { }
    });
});



$(document).on('click', '.btnInstituteFormSubmit', function () {

    var frmId = "#"+$(this).data("formid");

    if ($(frmId).valid()) {
        
        var frmData = new FormData(document.querySelector(frmId));
        var filebase = $("#files").get(0);
        var files = filebase.files;

        if (filebase.files.length) {
            frmData.append(files[0].name, files[0]);
        }

        $.ajax({
            url: createInstituteUrl,
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
            success: function (instituteid) {
                
                $(frmId +" input#InstituteId").val(instituteid); 
                ShowNotification("success", "Record Saved", "Institute# " + instituteid +" Saved Successfully");
                instituteTable.reloadTable(); //reloads institute datatable
            },
            error: function (err) {
                
                ShowNotification("error", "Error occured", err.status + " " + err.statusText);
                App.unblockUI(); 
            },
            complete: function () { App.unblockUI(); }
        });
    }
   
    
});

function ActivateInstitute(ele) {
    var active = $(ele).data('active');
    
    $.ajax({
        url: activateInstituteUrl,
        type: 'GET',
        data: { Activate: active.toLowerCase() !== 'true', InstituteId: $(ele).data('instituteid') },
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

function DeleteInstitute(ele) {
    var active = $(ele).data('delete');

    $.ajax({
        url: deleteInstituteUrl,
        type: 'GET',
        data: { Delete: active.toLowerCase() !== 'true', InstituteId: $(ele).data('instituteid') },
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