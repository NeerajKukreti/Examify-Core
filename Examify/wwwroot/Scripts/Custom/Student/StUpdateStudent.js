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

$(document).on('click', '#ChangePassword', function () {

    if ($("#frmChangeStudentPassword").valid()) {

        var frmData = new FormData(document.querySelector("#frmChangeStudentPassword"));

        $.ajax({
            url: changeStudentPassword,
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
                    document.getElementById("frmChangeStudentPassword").reset();
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
