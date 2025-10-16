// ExamValidation.js - Exam validation functionality
console.log('ExamValidation.js loaded successfully');

// Form validation functions
function validateExamForm() {
    let isValid = true;
    const errors = [];
    
    // Validate exam title
    const title = $('#examTitle').val();
    if (!title || title.trim().length < 3) {
        errors.push('Exam title must be at least 3 characters long');
        isValid = false;
    }
    
    // Validate duration
    const duration = $('#examDuration').val();
    if (!duration || duration < 1) {
        errors.push('Exam duration must be at least 1 minute');
        isValid = false;
    }
    
    // Validate questions
    const questionCount = $('.question-item').length;
    if (questionCount < 1) {
        errors.push('Exam must have at least 1 question');
        isValid = false;
    }
    
    // Display errors
    if (!isValid) {
        displayValidationErrors(errors);
    }
    
    return isValid;
}

function displayValidationErrors(errors) {
    const errorContainer = $('#validationErrors');
    errorContainer.empty();
    
    if (errors.length > 0) {
        const errorList = $('<ul class="list-unstyled"></ul>');
        errors.forEach(error => {
            errorList.append(`<li class="text-danger"><i class="fas fa-exclamation-circle"></i> ${error}</li>`);
        });
        errorContainer.append(errorList).show();
    }
}

function clearValidationErrors() {
    $('#validationErrors').empty().hide();
}

// Real-time validation
$(document).ready(function() {
    $('#examForm').on('submit', function(e) {
        if (!validateExamForm()) {
            e.preventDefault();
            return false;
        }
    });
    
    // Clear errors on input change
    $('#examForm input, #examForm select, #examForm textarea').on('change input', function() {
        clearValidationErrors();
    });
});