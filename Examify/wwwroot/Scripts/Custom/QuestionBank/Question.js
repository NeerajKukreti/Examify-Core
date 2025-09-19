let API_BASE_URL_QUESTION = 'https://localhost:7271/api/Question'; // fallback
let API_BASE_URL_SUBJECT = 'https://localhost:7271/api/Subject'; // fallback

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

            // Only enforce single selection when multi-select is NOT checked (disabled)
            if (!isMultiSelect && $this.is(':checked')) {
                // If multi-select is disabled and this checkbox is being checked
                // Uncheck all other "Is Correct" checkboxes
                $('#options-wrapper input[type="checkbox"]:not([asp-for="IsMultiSelect"])')
                    .not($this)
                    .prop('checked', false); 
            }
        });
}



function loadQuestionTypes() {
    var apiUrl = API_BASE_URL_QUESTION + '/types';
    $.get(apiUrl, function (data) {
        var $ddl = $('#ddlQuestionType');
        $ddl.empty().append('<option value="">Select Type</option>');
        $.each(data, function (i, type) {
            $ddl.append('<option value="' + type.QuestionTypeId + '">' + type.TypeName + '</option>');
        });
    });
}

function loadSubjects() {
    var apiUrl = API_BASE_URL_SUBJECT + '/list?instituteId=3'; // Change instituteId as needed
    $.get(apiUrl, function (data) {
        var $ddl = $('#ddlSubject');
        $ddl.empty().append('<option value="">Select Subject</option>');
        $.each(data, function (i, subject) {
            $ddl.append('<option value="' + subject.SubjectId + '">' + subject.SubjectName + '</option>');
        });
    });
}

function loadTopics(subjectId) {
    if (!subjectId) {
        $('#ddlTopic').empty().append('<option value="">Select Topic</option>');
        return;
    }
    var apiUrl = API_BASE_URL_SUBJECT + '/topics?instituteId=3&subjectId=' + subjectId; // Change instituteId as needed
    $.get(apiUrl, function (data) {
        var $ddl = $('#ddlTopic');
        $ddl.empty().append('<option value="">Select Topic</option>');
        $.each(data, function (i, topic) {
            $ddl.append('<option value="' + topic.TopicId + '">' + topic.Description + '</option>');
        });
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
        var $anyToolbar = $('[id^="toolbar-for-editor-option"], [id^="toolbar-for-editor-question"], [id^="toolbar-for-editor-additional"]').first();
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
            '    </div>',
            '    <div class="col-md-2 option-controls">',
            '      <div class="option-checkbox checkbox">',
            '        <label>',
            '          <input type="checkbox" /> Is Correct',
            '        </label>',
            '      </div>',
            '      <div class="option-remove-btn" style="margin-top: 8px;    margin-left: -12px;">',
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