
//For Update Institute Profile From Institute Login

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
                
                //$(frmId +" input#InstituteId").val(instituteid); 
                ShowNotification("success", "Record Saved", "Record Updated Successfully");
                //instituteTable.reloadTable(); //reloads institute datatable
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

$(document).on('click', '#ChangePassword', function () {

    if ($("#frmChangeInstitutePassword").valid()) {

        var frmData = new FormData(document.querySelector("#frmChangeInstitutePassword"));

        $.ajax({
            url: changeInstitutePassword,
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
            success: function (StatusCode) {

                //alert(StatusCode);
                if (StatusCode == 1) {
                    ShowNotification("success", "Record Saved", "Password Changed Successfully");
                    document.getElementById("frmChangeInstitutePassword").reset();
                }
                else {
                    ShowNotification("error", "Error occured", "Current Password is Wrong");
                }
            },
            error: function (err) {

                ShowNotification("error", "Error occured", err.status + " " + err.statusText);
                App.unblockUI();
            },
            complete: function () { App.unblockUI(); }
        });
    }
    

});

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