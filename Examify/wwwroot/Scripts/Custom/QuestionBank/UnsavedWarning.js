// Unsaved Changes Warning
(function() {
    'use strict';

    var hasUnsavedChanges = false;
    var formSubmitting = false;
    var formInitialized = false;
    var cancelClicked = false;

    $(function() {
        // Wait 1 second before tracking changes (let form load)
        setTimeout(function() {
            formInitialized = true;
        }, 1000);

        // Track changes on form inputs (only after initialization)
        $('#questionForm').on('input change', 'input, textarea, select', function() {
            if (formInitialized) {
                hasUnsavedChanges = true;
            }
        });

        // Clear flag when form is submitted
        $('#questionForm').on('submit', function() {
            formSubmitting = true;
            hasUnsavedChanges = false;
        });

        // Warn before leaving page
        window.addEventListener('beforeunload', function(e) {
            if (hasUnsavedChanges && !formSubmitting) {
                e.preventDefault();
                e.returnValue = 'You have unsaved changes. Are you sure you want to leave?';
                return e.returnValue;
            }
        });

        // Cancel button - allow closing without warning
        $('#btnCancel').on('click', function() {
            cancelClicked = true;
        });

        // Warn before closing modal
        $('#questionModel').on('hide.bs.modal', function(e) {
            if (hasUnsavedChanges && !formSubmitting && !cancelClicked) {
                if (!confirm('You have unsaved changes. Are you sure you want to close?')) {
                    e.preventDefault();
                    return false;
                }
            }
        });

        // Reset cancel flag when modal is hidden
        $('#questionModel').on('hidden.bs.modal', function() {
            cancelClicked = false;
            hasUnsavedChanges = false;
            formInitialized = false;
        });

        console.log('✓ Unsaved changes warning initialized');
    });

})();
