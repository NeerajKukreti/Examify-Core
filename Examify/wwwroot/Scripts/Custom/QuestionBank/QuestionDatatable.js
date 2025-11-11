var QuestionBankTable = function () {
    var table;
    var QuestionBank = function () {
        table = $('#QuestionBankTable').dataTable({
            "order": [],
            "createdRow": function (row, data, dataIndex) {
                // Custom row styling if needed
            },
            "ajax": function (data, callback, settings) {
                $.ajax({
                    url: loadQuestionBankUrl,
                    type: "GET",
                    dataType: "json",
                    success: function (response) {
                        callback({ data: response.data || [] });
                    }
                });
            },
            "columns": [
                {
                    "title": "", "data": "questionId",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var str = ' <a class="btn" data-id=' + oData.questionId +
                            ' data-bs-target="#questionModel"  data-bs-toggle="modal" id="EditQuestion">' +
                            '<span class="fas fa-edit"></span>' +
                            '</a>';
                        $(nTd).html(str);
                    }
                },
                {
                    "title": "English Text", "data": "questionEnglish"
                },
                { "title": "Hindi Text", "data": "questionHindi" },
                { "title": "Topic", "data": "topicName" },
                { "title": "Multi Select", "data": "isMultiSelect" }
            ]
        });
    }
    return {
        init: function () {
            if (!$().dataTable) {
                return;
            }
            QuestionBank();
        },
        reloadTable: function () {
            table.ajax.reload(null, false);
        }
    };
}();

$(document).ready(function () {
    QuestionBankTable.init();

    // Listen for the custom event from _Create.cshtml
    $(document).on('questionFormReady', function(event) {
        console.log('📨 Received questionFormReady event:', event.detail);
        
        // Only run cleanup if this is a new question
        if (event.detail && event.detail.isNewQuestion) {
            console.log('🧹 Running cleanup for new question creation...');
            performNewQuestionCleanup();
        } else {
            console.log('📝 Edit mode detected - skipping cleanup');
        }
    });

    $(document).on('click', '#CreateQuestion', function () {

        $.ajax({
            url: createQuestionUrl,
            type: "GET",
            beforeSend: function () {

            },
            success: function (res) {
                $(".questionModelBody").empty().html(res);
                // Cleanup will now be triggered by the questionFormReady event
                console.log('✅ Create question form loaded - waiting for questionFormReady event');
            },
            error: function (err) {
                alert(err)
            },
            complete: function () {
                // Removed cleanup code from here - now handled by event listener
            }
        });

    });

    $(document).on('click', '#EditQuestion', function () {
        var $this = $(this);

        $.ajax({
            url: editQuestionUrl + "?id=" + $this.data('id'),
            type: "GET",
            beforeSend: function () {

            },
            success: function (res) {
                $(".questionModelBody").empty().html(res);
                console.log('✅ Edit question form loaded');
            },
            error: function (err) {
                alert(err)
            },
            complete: function () {

            }
        });

    });
    
    // Centralized cleanup function for new questions
    function performNewQuestionCleanup() {
        // Override modelData for new question
        window.modelData = {
            QuestionId: 0,
            SubjectId: null,
            QuestionTypeId: null,
            TopicId: null,
            IsMultiSelect: false,
            QuestionEnglish: '',
            QuestionHindi: '',
            AdditionalTextEnglish: '',
            AdditionalTextHindi: '',
            Explanation: '',
            Options: [],
            Pairs: [],
            Orders: []
        };

        // Clear basic form fields
        $('#ddlSubject').val('');
        $('#ddlQuestionType').val('');
        $('#ddlTopic').val('');
        $('#ddlDifficultyLevel').val('Easy');
        $('#IsMultiSelect').prop('checked', false).prop('disabled', false);
        $('textarea[asp-for="Explanation"]').val('');

        // Clear Quill editors if they exist
        if (window.__quillEditors) {
            Object.keys(window.__quillEditors).forEach(function (editorId) {
                if (window.__quillEditors[editorId]) {
                    window.__quillEditors[editorId].setContents([]);
                }
            });
        }

        // Clear hidden fields
        $('#hfQuestionEnglish, #hfQuestionHindi, #hfAdditionalEnglish, #hfAdditionalHindi').val('');

        // Reset all question type sections
        $('#options-section, #true-false-section, #descriptive-section, #pairing-section, #ordering-section').hide();
        $('#options-section').show(); // Show MCQ by default
        $('.multi-select-container').show();

        // Clear MCQ options
        $('.option-checkbox input[type="checkbox"]').prop('checked', false);
        $('.option-correct-hidden').val('false');
        $('.option-choiceid-field').val('');
        $('#options-wrapper .option-group').each(function () {
            var editorId = $(this).find('.editor-container').attr('id');
            if (window.__quillEditors && window.__quillEditors[editorId]) {
                window.__quillEditors[editorId].setContents([]);
            }
            $(this).find('.option-hidden-field').val('');
        });
        $('.ql-editor p').html('');

        // Clear True/False options
        $('.tf-option').prop('checked', false);
        $('.true-false-option .option-correct-hidden').val('false');
        $('.true-false-option .option-choiceid-field').val('');

        // Clear Descriptive options (remove extra, clear first)
        $('#descriptive-options-wrapper .descriptive-option-group:not(:first)').remove();
        $('#descriptive-options-wrapper .descriptive-option-group:first .descriptive-input').val('');
        $('#descriptive-options-wrapper .descriptive-option-group:first .option-hidden-field').val('');
        $('#descriptive-options-wrapper .descriptive-option-group:first .option-choiceid-field').val('');

        // Clear Pairing options (remove extra, clear first)
        $('#pairs-wrapper .pair-group:not(:first)').remove();
        $('#pairs-wrapper .pair-group:first .pair-left-input, #pairs-wrapper .pair-group:first .pair-right-input').val('');
        $('#pairs-wrapper .pair-group:first .pair-left-hidden, #pairs-wrapper .pair-group:first .pair-right-hidden').val('');
        $('#pairs-wrapper .pair-group:first .pair-id-hidden').val('');
        $('.form-control.pair-left-input').val('');
        $('.form-control.pair-right-input').val('');

        // Clear Ordering options (remove extra, clear first)
        $('#orders-wrapper .order-group:not(:first)').remove();
        $('#orders-wrapper .order-group:first .order-item-input').val('');
        $('#orders-wrapper .order-group:first .order-item-hidden').val('');
        $('#orders-wrapper .order-group:first .order-id-hidden').val('');
        $('#orders-wrapper .order-group:first .order-correct-hidden').val('1');
        $('.form-control.order-item-input').val('');

        // Reset global counters
        if (typeof descriptiveOptionCount !== 'undefined') {
            window.descriptiveOptionCount = 1;
        }
        if (typeof pairCount !== 'undefined') {
            window.pairCount = 1;
        }
        if (typeof orderItemCount !== 'undefined') {
            window.orderItemCount = 1;
        }

        // Reset QuestionOptions module if available
        if (window.QuestionOptions && typeof window.QuestionOptions.resetOptions === 'function') {
            window.QuestionOptions.resetOptions();
        }

        // Clear any validation errors
        $('.text-danger').empty();
        $('.error-message').remove();
        $('.error').removeClass('error');

        // Force QuestionForm to recognize this is a new question
        if (window.QuestionForm) {
            window.QuestionForm.modelData = window.modelData;
            
            // Reset initialization flag if it exists
            if (typeof window.QuestionForm.isInitializing !== 'undefined') {
                window.QuestionForm.isInitializing = true;
                setTimeout(function () {
                    window.QuestionForm.isInitializing = false;
                }, 200);
            }
            
            // Force hide all sections and show MCQ
            if (typeof window.QuestionForm.hideAllSections === 'function') {
                window.QuestionForm.hideAllSections();
                $('#options-section').show();
                $('.multi-select-container').show();
            }
        }

        console.log('✅ New question cleanup completed via event-driven approach');
        console.log('📊 Clean modelData:', window.modelData);
    }
});


