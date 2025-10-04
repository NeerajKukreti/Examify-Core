var QuestionBankTable = function () {
    var table;
    var QuestionBank = function () {
        table = $('#QuestionBankTable');
        table.dataTable({
            "order": [], // This disables the initial sorting
            "createdRow": function (row, data, dataIndex) {
                // Custom row styling if needed
            },
            "ajax": {
                "url": loadQuestionBankUrl,
                "type": "GET",
                "dataType": "json",   // correct spelling
                "dataSrc": "data"         // response is an array
            },
            "columns": [
                {
                    "title": "", "data": "questionId",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var str = ' <a class="btn" data-id=' + oData.questionId+' data-bs-target="#questionModel"  data-bs-toggle="modal" id="EditQuestion">' +
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
        setInterval(function () {
            table.DataTable().ajax.reload(null, false);
        }, 30000);
    }
    return {
        init: function () {
            if (!$().dataTable) {
                return;
            }
            QuestionBank();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false);
        }
    };
}();

$(document).ready(function () {
    QuestionBankTable.init(); 

    $(document).on('click', '#CreateQuestion', function () {
        
        $.ajax({
            url: createQuestionUrl,
            type: "GET",
            beforeSend: function () {

            },
            success: function (res) {
                $(".questionModelBody").empty().html(res);
                
                // Clear previous question data for new question creation
                if (window.modelData) {
                    // Reset modelData to ensure it's for a new question
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
                }
                
                // Clear all form fields explicitly
                setTimeout(function() {
                    // Clear dropdowns
                    $('#ddlSubject').val('');
                    $('#ddlQuestionType').val('');
                    $('#ddlTopic').val('');
                    $('#IsMultiSelect').prop('checked', false);
                    $('textarea[asp-for="Explanation"]').val('');
                    
                    // Clear Quill editors
                    if (window.__quillEditors) {
                        Object.keys(window.__quillEditors).forEach(function(editorId) {
                            if (window.__quillEditors[editorId]) {
                                window.__quillEditors[editorId].setContents([]);
                            }
                        });
                    }
                    
                    // Clear hidden fields
                    $('#hfQuestionEnglish, #hfQuestionHindi, #hfAdditionalEnglish, #hfAdditionalHindi').val('');
                    
                    // Reset all form sections to default state
                    $('#options-section, #true-false-section, #descriptive-section, #pairing-section, #ordering-section').hide();
                    $('#options-section').show(); // Show MCQ by default
                    $('.multi-select-container').hide();
                    
                    // Clear all checkboxes and form data
                    $('.tf-option').prop('checked', false);
                    $('.option-checkbox input[type="checkbox"]').prop('checked', false);
                    $('.option-correct-hidden').val('false');
                    $('.option-choiceid-field').val('');
                    
                    // Reset dynamic content sections
                    $('#descriptive-options-wrapper .descriptive-option-group:not(:first)').remove();
                    $('#pairs-wrapper .pair-group:not(:first)').remove();
                    $('#orders-wrapper .order-group:not(:first)').remove();
                    
                    // Clear the first remaining items
                    $('.descriptive-input').val('');
                    $('.pair-left-input, .pair-right-input').val('');
                    $('.order-item-input').val('');
                    
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
                    
                    console.log('✅ Form cleared for new question creation');
                }, 100);
            },
            error: function (err) {
                alert(err)
            },
            complete: function () {

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

            },
            error: function (err) {
                alert(err)

            },
            complete: function () {

            }
        });

    });
});


