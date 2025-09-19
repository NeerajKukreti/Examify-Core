//orignal script form-validation.js


var form = $('#frmCreateStudent');
var error = $('.alert-danger', form);
var success = $('.alert-success', form);

form.validate({
    
    errorElement: 'span', //default input error message container
    errorClass: 'help-block help-block-error', // default input error message class
    focusInvalid: false, // do not focus the last invalid input
    ignore: "", // validate all fields including form hidden input
    rules: {
        Name: {
            //minlength: 50,
            required: true
        },
        PhoneNumber: {
            minlength: 10,
            required: false
        },
        Password: {
            minlength: 8,
            required: false
        },
    },

    messages: { // custom messages for radio buttons and checkboxes
        membership: {
            required: "Please select a Membership type"
        },
        service: {
            required: "Please select  at least 2 types of Service",
            minlength: jQuery.validator.format("Please select  at least {0} types of Service")
        }
    },

    

    invalidHandler: function (event, validator) { //display error alert on form submit   
        success.hide();
        error.show();
        App.scrollTo(error, -200);
    },

    highlight: function (element) { // hightlight error inputs
        $(element)
            .closest('.form-group').addClass('has-error'); // set error class to the control group
    },

    unhighlight: function (element) { // revert the change done by hightlight
        $(element)
            .closest('.form-group').removeClass('has-error'); // set error class to the control group
    },

    success: function (label) {
        label
            .closest('.form-group').removeClass('has-error'); // set success class to the control group
    },


});



$("#frmChangeStudentPassword").validate({
    
    errorElement: 'span', //default input error message container
    errorClass: 'help-block help-block-error', // default input error message class
    focusInvalid: false, // do not focus the last invalid input
    ignore: "", // validate all fields including form hidden input
    rules: {
        Password: {
            minlength: 8,
            required: true
        },
        NewPassword: {
            minlength: 8,
            required: true
        },
        ConfirmPassword: {
            equalTo: "#NewPassword"
        }

    },

    messages: { // custom messages for radio buttons and checkboxes
        CurrentPassword: "Enter Current Password",
        NewPassword: {
            required: "Enter a new password",
            minlength: "Your password must be at least 8 characters long"
        },
        ConfirmPassword: " Enter Confirm Password Same as New Password"
    },



    invalidHandler: function (event, validator) { //display error alert on form submit   
        success.hide();
        error.show();
        App.scrollTo(error, -200);
    },

    highlight: function (element) { // hightlight error inputs
        $(element)
            .closest('.form-group').addClass('has-error'); // set error class to the control group
    },

    unhighlight: function (element) { // revert the change done by hightlight
        $(element)
            .closest('.form-group').removeClass('has-error'); // set error class to the control group
    },

    success: function (label) {
        label
            .closest('.form-group').removeClass('has-error'); // set success class to the control group
    },

});