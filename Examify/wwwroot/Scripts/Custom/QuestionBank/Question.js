var API_BASE_URL_QUESTION = 'https://localhost:7271/api/Question'; // fallback
var API_BASE_URL_SUBJECT = 'https://localhost:7271/api/Subject'; // fallback

// Question type IDs will be set dynamically when types are loaded
var MCQ_TYPE_ID; 
var TRUE_FALSE_TYPE_ID;

$(document).ready(function () {

    // Ensure dynamic options are cleared (back to initial markup) & MAX_OPTIONS resets
    if (window.QuestionOptions) {
        if (typeof window.QuestionOptions.resetOptions === 'function') {
            window.QuestionOptions.resetOptions();
        }
        if (typeof window.QuestionOptions.resetMax === 'function') {
            window.QuestionOptions.resetMax(10); // default
        }
    }

    // Load subjects and question types on page load
    loadSubjects();
    loadQuestionTypes();

    // Load topics when subject changes
    $(document).off('change.questionOptions', '#ddlSubject').on('change.questionOptions', '#ddlSubject', function () {
        var subjectId = $(this).val();
        loadTopics(subjectId);
    });

    $(document).off('click.panelToggle', '.panel-header').on('click.panelToggle', '.panel-header', function () {
        let $btn = $(this).find('.toggle-btn');
        let $panelBody = $btn.closest(".editor-panel").find(".panel-body");

        $panelBody.slideToggle(200);  // animate hide/show
        $btn.text($btn.text() === "−" ? "+" : "−"); // change symbol
    });

    // Handle multi-select toggle behavior
    initializeMultiSelectBehavior();
});

function initializeMultiSelectBehavior() {
    // Handle multi-select checkbox change
    $(document).off('change.multiSelect', '[asp-for="IsMultiSelect"]').on('change.multiSelect', '#IsMultiSelect', function () {
        var isMultiSelect = $(this).is(':checked');

        if (!isMultiSelect) {
            $('.option-checkbox').find('input').prop('checked', true);
        }
    });

    // Handle "Is Correct" checkbox changes
    $(document).off('change.correctAnswer', '#options-wrapper input[type="checkbox"]:not([asp-for="IsMultiSelect"])')
        .on('change.correctAnswer', '#options-wrapper input[type="checkbox"]:not([asp-for="IsMultiSelect"])', function () {
           
            var isMultiSelect = $('#IsMultiSelect').is(':checked');
            var $this = $(this);

            // Update hidden field immediately
            var $group = $this.closest('.option-group');
            var isChecked = $this.is(':checked');
            
            // Find or create hidden field for IsCorrect
            var $correctHidden = $group.find('.option-correct-hidden');
            if ($correctHidden.length === 0) {
                $correctHidden = $('<input type="hidden" class="option-correct-hidden" />').appendTo($group);
            }
            
            // Set the value based on checkbox state
            $correctHidden.val(isChecked ? 'true' : 'false');

            // Only enforce single selection when multi-select is NOT checked (disabled)
            if (!isMultiSelect && $this.is(':checked')) {
                // If multi-select is disabled and this checkbox is being checked
                // Uncheck all other "Is Correct" checkboxes
                $('#options-wrapper input[type="checkbox"]:not([asp-for="IsMultiSelect"])')
                    .not($this)
                    .prop('checked', false)
                    .each(function() {
                        // Update their hidden fields too
                        $(this).closest('.option-group').find('.option-correct-hidden').val('false');
                    });
            }
            
            // Update names for all options
            updateOptionFieldNames();
        });
} 

// Function to update option field names for consistent indexing
function updateOptionFieldNames() {
    $('#options-wrapper .option-group').each(function(i) {
        var $group = $(this);
        
        // Text field
        var $hidden = $group.find('.option-hidden-field');
        if ($hidden.length) {
            $hidden.attr('name', 'Options[' + i + '].Text');
        }
        
        // IsCorrect field
        var $correctHidden = $group.find('.option-correct-hidden');
        if ($correctHidden.length) {
            $correctHidden.attr('name', 'Options[' + i + '].IsCorrect');
        }
        
        // ChoiceId field
        var $choiceId = $group.find('.option-choiceid-field');
        if ($choiceId.length) {
            $choiceId.attr('name', 'Options[' + i + '].ChoiceId');
        }
    });
}

function loadSubjects() {
    var apiUrl = API_BASE_URL_SUBJECT + '/list?instituteId=3';
    $.get(apiUrl, function (data) {
        var $ddl = $('#ddlSubject');
        $ddl.empty().append('<option value="">Select Subject</option>');
        $.each(data, function (i, subject) {
            $ddl.append('<option value="' + subject.SubjectId + '">' + subject.SubjectName + '</option>');
        });
        $(document).trigger('subjectsLoaded'); // <-- Add this
    });
}

function loadQuestionTypes() {
    var apiUrl = API_BASE_URL_QUESTION + '/types';
    $.get(apiUrl, function (data) {
        var $ddl = $('#ddlQuestionType');
        $ddl.empty().append('<option value="">Select Type</option>');
        
        // Find the MCQ and True/False type IDs by their names
        $.each(data, function (i, type) {
            $ddl.append('<option value="' + type.QuestionTypeId + '">' + type.TypeName + '</option>');
            
            // Identify MCQ and True/False types by their names
            // Adjust these conditions based on the actual names in your database
            if (type.TypeName.toLowerCase().includes('mcq') || 
                type.TypeName.toLowerCase().includes('multiple choice')) {
                MCQ_TYPE_ID = type.QuestionTypeId;
            }
            else if (type.TypeName.toLowerCase().includes('true/false') || 
                     type.TypeName.toLowerCase().includes('true false') ||
                     type.TypeName.toLowerCase().includes('t/f')) {
                TRUE_FALSE_TYPE_ID = type.QuestionTypeId;
            }
        });
        
        // Set defaults if not found
        if (!MCQ_TYPE_ID && data.length > 0) MCQ_TYPE_ID = data[0].QuestionTypeId; // Default to first type
        if (!TRUE_FALSE_TYPE_ID && data.length > 1) TRUE_FALSE_TYPE_ID = data[1].QuestionTypeId; // Default to second type
        
        console.log("Question Types loaded - MCQ:", MCQ_TYPE_ID, "True/False:", TRUE_FALSE_TYPE_ID);
        
        $(document).trigger('questionTypesLoaded');
    });
}

// Update loadTopics to accept a callback
function loadTopics(subjectId, callback) {
    if (!subjectId) {
        $('#ddlTopic').empty().append('<option value="">Select Topic</option>');
        if (callback) callback();
        return;
    }
    var apiUrl = API_BASE_URL_SUBJECT + '/topics?instituteId=3&subjectId=' + subjectId;
    $.get(apiUrl, function (data) {
        var $ddl = $('#ddlTopic');
        $ddl.empty().append('<option value="">Select Topic</option>');
        $.each(data, function (i, topic) {
            $ddl.append('<option value="' + topic.TopicId + '">' + topic.Description + '</option>');
        });
        if (callback) callback();
    });
}

// question.js - dynamic option addition & removal for question create page
// Requires: jQuery, Quill, base editors already initialized (they create per-editor toolbars)
(function (window, $) {
    'use strict';

    var MAX_OPTIONS = 10; // internal state (reset via resetMax)
    var initialOptionCount = 0; // captured on first run
    var EVENTS_NS_ADD = 'click.questionOptionsAdd';
    var EVENTS_NS_REMOVE = 'click.questionOptionsRemove';

    function captureInitialCount() {
        if (initialOptionCount === 0) {
            initialOptionCount = $('#options-wrapper .option-group').length; // expected 4
        }
    }

    function resetMax(val) {
        MAX_OPTIONS = (typeof val === 'number' && val > 0) ? val : 10;
    }

    // Remove any dynamically added options beyond the original initialOptionCount
    function resetOptions() {
        captureInitialCount();
        $('#options-wrapper .option-group').each(function () {
            var idx = parseInt($(this).attr('data-option-index'), 10);
            if (idx > initialOptionCount) {
                var editorId = $(this).find('.editor-container').attr('id');
                if (window.__quillEditors && window.__quillEditors[editorId]) {
                    delete window.__quillEditors[editorId];
                }
                // remove associated toolbar if present
                $('#toolbar-for-' + editorId).remove();
                $(this).remove();
            }
        });
    }

    function nextOptionIndex() {
        var max = 0;
        $('#options-wrapper .option-group').each(function () {
            var v = parseInt($(this).attr('data-option-index'), 10);
            if (!isNaN(v) && v > max) max = v;
        });
        return max + 1;
    }

    function getToolbarTemplateClone() {
        // Pick the first existing generated toolbar (created for initial editors)
        ///var $anyToolbar = $('[id^="toolbar-for-editor-option"], [id^="toolbar-for-editor-question"], [id^="toolbar-for-editor-additional"]').first();
        var $anyToolbar = $('[class="toolbar2"]').first();
        if (!$anyToolbar.length) return null;
        var $clone = $anyToolbar.clone(false); // shallow clone; handlers not needed (Quill will bind new ones)
        $clone.removeAttr('aria-label role');
        $clone.find('*').removeAttr('aria-controls aria-describedby');
        return $clone;
    }

    function initQuillFor(editorId, displayName) {
     
        if (typeof Quill === 'undefined') return;
        var $editor = $('#' + editorId);
        if (!$editor.length) return;

        var toolbarId = 'toolbar-for-' + editorId;
        var $toolbarClone = getToolbarTemplateClone();
        if ($toolbarClone) {
            $toolbarClone.attr('id', toolbarId);
            $toolbarClone.css('display', ''); // Remove display: none
            $toolbarClone.insertBefore($editor); 
        } else {
            console.error('No toolbar template found for dynamic option editor');
        }

        var modulesConfig = { toolbar: '#' + toolbarId };
        if (Quill.imports['modules/imageResize']) {
            modulesConfig.imageResize = { modules: ['Resize', 'DisplaySize', 'Toolbar'], preserveRatio: false };
        }

        var q = new Quill('#' + editorId, {
            theme: 'snow',
            placeholder: 'Compose a ' + (displayName || 'option'),
            modules: modulesConfig
        });

        q.getModule('toolbar').addHandler('image', function () {
            var input = document.createElement('input');
            input.type = 'file';
            input.accept = 'image/*';
            input.click();
            input.onchange = async function () {
                var file = input.files[0];
                if (!file) return;
                var formData = new FormData();
                formData.append('file', file);
                try {
                    var resp = await fetch('/Question/uploads', { method: 'POST', body: formData });
                    var data = await resp.json();
                    var range = q.getSelection(true);
                    q.insertEmbed(range.index, 'image', data.url);
                    q.setSelection(range.index + 1);
                } catch (e) { console.error('Upload failed', e); }
            };
        });

        window.__quillEditors = window.__quillEditors || {};
        window.__quillEditors[editorId] = q;
    }

    function addOption() {
        captureInitialCount();
        var idx = nextOptionIndex();
        if (idx > MAX_OPTIONS) {
            alert('Maximum options (' + MAX_OPTIONS + ') reached');
            return;
        }
        var editorId = 'editor-option' + idx;
        var html = [
            '<div class="form-group option-group" data-option-index="' + idx + '">',
            '  <div class="option-label">Option ' + idx + '</div>',
            '  <div class="row">',
            '    <div class="col-md-10">',
            '      <div class="editor-container" style="height: 50px !important" id="' + editorId + '" data-name="Option ' + idx + '"></div>',
            '       <input type="hidden" class="option-hidden-field" data-option-index="'+idx+'" />',
            '       <input type="hidden" class="option-choiceid-field" data-option-index="'+idx+'" />',
            '       <input type="hidden" class="option-correct-hidden" value="false" />',
            '       <span class="text-danger option-error" id="valOption' + idx + '"></span>',
            '    </div>',
            '    <div class="col-md-2 option-controls">',
            '      <div class="option-checkbox checkbox">',
            '        <label>',
            '          <input type="checkbox" /> Is Correct',
            '        </label>',
            '      </div>',
            '      <div class="option-remove-btn" style="">',
            '        <button type="button" class="btn btn-danger btn-xs btn-remove-option" title="Remove Option">',
            '          <i class="glyphicon glyphicon-trash"></i> Remove',
            '        </button>',
            '      </div>',
            '    </div>',
            '  </div>',
            '</div>'
        ].join('');

        $('#options-wrapper').append(html);
        initQuillFor(editorId, 'Option ' + idx);

        // Re-initialize multi-select behavior for new checkboxes
        if (typeof initializeMultiSelectBehavior === 'function') {
            initializeMultiSelectBehavior();
        }
        
        // Update hidden field names
        updateOptionFieldNames();
    }

    function removeOption(btn) {
        var $group = $(btn).closest('.option-group');
        var editorId = $group.find('.editor-container').attr('id');
        if (window.__quillEditors && window.__quillEditors[editorId]) {
            delete window.__quillEditors[editorId];
        }
        $('#toolbar-for-' + editorId).remove();
        $group.fadeOut(200, function () {
            $(this).remove();
            // Update hidden field names after removal
            updateOptionFieldNames();
        });
    }

    // Ensure old handlers are removed before binding (prevents multiple option additions)
    $(document).off(EVENTS_NS_ADD, '#btnAddOption').on(EVENTS_NS_ADD, '#btnAddOption', addOption);
    $(document).off(EVENTS_NS_REMOVE, '.btn-remove-option').on(EVENTS_NS_REMOVE, '.btn-remove-option', function () { removeOption(this); });

    // Public API for external use
    window.QuestionOptions = { add: addOption, resetMax: resetMax, resetOptions: resetOptions };

    // Capture baseline on load
    captureInitialCount();

})(window, jQuery);

// Quill editor initialization for all .editor-container elements in the question create page
$(function () {
    if (typeof Quill === 'undefined') {
        console.error('Quill is not loaded. Include quill.js before this script.');
        return;
    }
    // Map for editor instances
    var quillEditors = window.__quillEditors = {};
    // Robustly detect resize module and register it (if present)
    var resizeCandidate = (window.ImageResize && window.ImageResize.default) || window.ImageResize || window.QuillImageResizeModule || window.QuillImageResize || null;
    try {
        if (resizeCandidate) {
            Quill.register('modules/imageResize', resizeCandidate);
            console.log('imageResize registered');
        } else {
            console.log('imageResize not found; CSS fallback will apply.');
        }
    } catch (e) {
        console.warn('imageResize registration failed:', e);
        resizeCandidate = null;
    }
    // Find the toolbar template (first .toolbar1)
    var $template = $('.toolbar1').first();
    var $template2 = $('.toolbar2').first();
    if (!$template.length) {
        console.error('No .toolbar1 toolbar template found — please add one.');
        return;
    }
    // Remove template from DOM (we'll clone it and insert before each editor)
    //$template.detach();
    //$template2.detach();
    // Initialize each editor with its own toolbar clone placed immediately above it
    $('.editor-container').each(function () {
        var $editor = $(this);
        var editorId = $editor.attr('id');
        if (!editorId) {
            console.warn('editor container missing id — skipping', $editor);
            return;
        }
        
        // clone template and give unique id
        var toolbarId = 'toolbar-for-' + editorId;

        if (toolbarId.indexOf("option") != -1) {
            var $clone = $template2.clone(true).removeClass('toolbar1').attr('id', toolbarId);
        }
        else {
            var $clone = $template.clone(true).removeClass('toolbar1').attr('id', toolbarId);
        }
        
        $clone.css('display', ''); // Remove display: none
        $clone.insertBefore($editor);
        // build modules config
        var modulesConfig = { toolbar: '#' + toolbarId };
        if (resizeCandidate) {
            modulesConfig.imageResize = { modules: ['Resize', 'DisplaySize', 'Toolbar'], preserveRatio: false };
        }
        // init Quill for this editor
        var q = new Quill('#' + editorId, {
            theme: 'snow',
            placeholder: 'Compose a ' + $editor.data('name'),
            modules: modulesConfig
        });
        // Attach image upload handler for THIS editor
        q.getModule('toolbar').addHandler('image', function () {
            const input = document.createElement('input');
            input.setAttribute('type', 'file');
            input.setAttribute('accept', 'image/*');
            input.click();
            input.onchange = async () => {
                const file = input.files[0];
                if (file) {
                    const formData = new FormData();
                    formData.append('file', file);
                    try {
                        const response = await fetch('/Question/uploads', {
                            method: 'POST',
                            body: formData
                        });
                        const data = await response.json();
                        // Use THIS editor instance (q)
                        const range = q.getSelection(true);
                        q.insertEmbed(range.index, 'image', data.url);
                        q.setSelection(range.index + 1);
                    } catch (err) {
                        console.error('Upload failed:', err);
                    }
                }
            };
        });
        quillEditors[editorId] = q;
    });
    // OPTIONAL: use CSS fallback so images are resizable if JS module missing
    if (!resizeCandidate) {
        $('<style>')
            .prop('type', 'text/css')
            .html('.ql-editor img { resize: both; overflow: auto; max-width:100%; height:auto; }')
            .appendTo('head');
    }
    console.log('Initialized editors:', Object.keys(quillEditors));
});

$(function () {

    // Remove previous errors
    function clearErrors() {
        $('.error-message').remove();
        $('.error').removeClass('error');
    }

    // Helper to show errors
    function showError(selector, message) {
        $(selector).addClass('error').after('<span class="error-message" style="color:brown;">' + message + '</span>');
    }

    // Handle True/False checkbox behavior
    $(document).off('change.tfOption', '.tf-option').on('change.tfOption', '.tf-option', function() {
        if ($(this).is(':checked')) {
            // Get the selected option value (true or false)
            const selectedValue = $(this).data('option-value');
            console.log("Selected True/False option:", selectedValue);
            
            // Uncheck other options
            $('.tf-option').not(this).prop('checked', false);
            
            // Update all option values correctly
            $('.true-false-option').each(function(i) {
                const thisValue = $(this).find('.tf-option').data('option-value');
                const isSelected = thisValue === selectedValue;
                
                // Set correct text value - always maintain proper format
                const optionText = thisValue === 'true' ? '<p>True</p>' : '<p>False</p>';
                $(this).find('.option-hidden-field').val(optionText);
                
                // Set IsCorrect based on whether this is the selected option
                $(this).find('.option-correct-hidden').val(isSelected ? 'true' : 'false');
                
                // Ensure field names are set properly
                $(this).find('.option-hidden-field').attr('name', 'Options[' + i + '].Text');
                $(this).find('.option-correct-hidden').attr('name', 'Options[' + i + '].IsCorrect');
                $(this).find('.option-choiceid-field').attr('name', 'Options[' + i + '].ChoiceId');
            });
        }
    });

    // AJAX submit for question create form
    $(document).off('submit.ajaxCreate', '#questionForm').on('submit.ajaxCreate', '#questionForm', function (e) {
        e.preventDefault(); // Prevent normal form submit
        clearErrors();
        let valid = true;

        // Subject
        if (!$('#ddlSubject').val()) {
            showError('#ddlSubject', 'Subject is required');
            valid = false;
        }
        
        // Question Type
        if (!$('#ddlQuestionType').val() && !$('#hiddenQuestionTypeId').val()) {
            showError('#ddlQuestionType', 'Question type is required');
            valid = false;
        }
        
        // Topic
        if (!$('#ddlTopic').val()) {
            showError('#ddlTopic', 'Topic is required');
            valid = false;
        }
        
        // Question English (Quill)
        let qEng = window.__quillEditors && window.__quillEditors['editor-question-english'];
        let qEngText = qEng ? $(qEng.root).text().trim() : '';
        if (!qEngText) {
            showError('#valQuestionEnglish', 'Question English is required');
            valid = false;
        }
        
        // Sync Quill HTML to hidden field
        if (qEng) $('#hfQuestionEnglish').val(qEng.root.innerHTML);
        // Question Hindi
        let qHindi = window.__quillEditors && window.__quillEditors['editor-question-hindi'];
        if (qHindi) $('#hfQuestionHindi').val(qHindi.root.innerHTML);
        // Additional Text English
        let qAddEng = window.__quillEditors && window.__quillEditors['editor-additional-english'];
        if (qAddEng) $('#hfAdditionalEnglish').val(qAddEng.root.innerHTML);
        // Additional Text Hindi
        let qAddHindi = window.__quillEditors && window.__quillEditors['editor-additional-hindi'];
        if (qAddHindi) $('#hfAdditionalHindi').val(qAddHindi.root.innerHTML);

        // Get the question type - use let instead of const so we can modify it if needed
        let questionTypeId = parseInt($('#ddlQuestionType').val() || '0');
        
        // If the QuestionType dropdown is disabled (for editing), get value from hidden field
        if (($('#ddlQuestionType').prop('disabled') || questionTypeId === 0) && $('#hiddenQuestionTypeId').length) {
            questionTypeId = parseInt($('#hiddenQuestionTypeId').val() || '0');
        }
        
        // Create form data from form values
        var formData = new FormData(this);
        
        // Check if it's a True/False question
        if (questionTypeId === TRUE_FALSE_TYPE_ID) {
            // Remove all option fields from the form data first
            // to avoid including MCQ options and True/False options together
            const keysToRemove = [];
            for (let key of formData.keys()) {
                if (key.startsWith('Options[')) {
                    keysToRemove.push(key);
                }
            }
            keysToRemove.forEach(key => formData.delete(key));
            
            // Validate True/False options
            let tfCorrectChecked = false;
            $('.tf-option:checked').each(function() {
                tfCorrectChecked = true;
            });
            
            // Ensure at least one True/False option is selected
            if (!tfCorrectChecked) {
                $('#tfOptionsValidation').html('<span class="error-message" style="color:brown;">Please select either True or False as the correct answer</span>');
                valid = false;
            } else {
                $('#tfOptionsValidation').empty();
                
                // Only include the True/False options (exactly 2)
                
                // Get ChoiceId for True option
                let trueChoiceId = $('.true-false-option').eq(0).find('.option-choiceid-field').val() || '';
                let trueIsSelected = $('.tf-option[data-option-value="true"]').is(':checked');
                
                // Add True option
                formData.append('Options[0].Text', '<p>True</p>');
                formData.append('Options[0].IsCorrect', trueIsSelected ? 'true' : 'false');
                formData.append('Options[0].ChoiceId', trueChoiceId);
                
                // Get ChoiceId for False option
                let falseChoiceId = $('.true-false-option').eq(1).find('.option-choiceid-field').val() || '';
                let falseIsSelected = $('.tf-option[data-option-value="false"]').is(':checked');
                
                // Add False option
                formData.append('Options[1].Text', '<p>False</p>');
                formData.append('Options[1].IsCorrect', falseIsSelected ? 'true' : 'false');
                formData.append('Options[1].ChoiceId', falseChoiceId);
                
                // Debug output
                console.log("True option - ChoiceId:", trueChoiceId, "IsCorrect:", trueIsSelected);
                console.log("False option - ChoiceId:", falseChoiceId, "IsCorrect:", falseIsSelected);
            }
        } else {
            // Options (static and dynamic) for MCQ
            let correctChecked = false;
            
            // Remove existing option fields (to rebuild them in proper order)
            const keysToRemove = [];
            for (let key of formData.keys()) {
                if (key.startsWith('Options[')) {
                    keysToRemove.push(key);
                }
            }
            keysToRemove.forEach(key => formData.delete(key));
            
            // Gather MCQ options and add them back to formData
            $('#options-wrapper .option-group').each(function (i) {
                var $group = $(this);
                var $editor = $group.find('.editor-container');
                var editorId = $editor.attr('id');
                var qOpt = window.__quillEditors && window.__quillEditors[editorId];
                
                // Get text from editor
                var optionText = qOpt ? qOpt.root.innerHTML : '';
                
                // Get IsCorrect value
                var isChecked = $group.find('input[type="checkbox"]').is(':checked');
                if (isChecked) correctChecked = true;
                
                // Get ChoiceId if it exists
                var choiceId = $group.find('.option-choiceid-field').val() || '';
                
                // Add to form data
                formData.append(`Options[${i}].Text`, optionText);
                formData.append(`Options[${i}].IsCorrect`, isChecked ? 'true' : 'false');
                formData.append(`Options[${i}].ChoiceId`, choiceId);
            });

            // At least one correct option
            if (!correctChecked) {
                $('#optionsValidation').html('<span class="error-message" style="color:brown;">At least one option must be marked as correct</span>');
                valid = false;
            } else {
                $('#optionsValidation').empty();
            }
        }
        
        if (!valid) return;

        // Make sure QuestionTypeId is included in form data
        if (!formData.has('QuestionTypeId')) {
            formData.append('QuestionTypeId', questionTypeId.toString());
        }
        
        // Log form data for debugging
        console.log("Question Type ID being submitted:", questionTypeId);
        
        // Debug all form data
        const formDataEntries = [];
        for (let [key, value] of formData.entries()) {
            formDataEntries.push(`${key}: ${value}`);
        }
        console.log("Form data entries:", formDataEntries);
        
        // Convert FormData to URL-encoded string
        var serializedData = new URLSearchParams(formData).toString();
        
        $.ajax({
            url: $('#questionForm').attr('action'),
            type: 'POST',
            data: serializedData,
            contentType: 'application/x-www-form-urlencoded',
            success: function (response) {
                toastr.success('Question saved', 'Alert');

                if (response && response.success) {
                    // Close modal and refresh question list (customize as needed)
                    $('#questionModel').modal('hide');
                    if (typeof reloadQuestionTable === 'function') reloadQuestionTable();
                } else {
                    // Show error (customize as needed)
                    console.error("Save failed:", response);
                    alert(response.error || 'Failed to create question.');
                }
            },
            error: function (xhr, status, error) {
                console.error("AJAX Error:", xhr.responseText);
                toastr.error('An error occurred while saving the question', 'Alert');
            }
        });
    });

});