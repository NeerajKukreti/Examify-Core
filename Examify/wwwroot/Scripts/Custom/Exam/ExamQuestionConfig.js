var ExamQuestionConfig = (function () {
    var availableTable;
    var selectedQuestions = [];
    var sortOrderCounter = 1;

    function init() {
        initAvailableQuestionsTable();
        loadExistingQuestions();
        bindEvents();
    }

    function initAvailableQuestionsTable() {
        availableTable = $('#AvailableQuestionsTable').DataTable({
            "ajax": {
                "url": getAvailableQuestionsUrl,
                "type": "GET",
                "data": { examId: examId },
                "dataSrc": function(json) {
                    populateFilters(json.data);
                    return json.data;
                }
            },
            "columns": [
                {
                    "data": "questionId",
                    "orderable": false,
                    "render": function (data, type, row) {
                        return '<input type="checkbox" class="question-checkbox" data-question-id="' + data + '" />';
                    }
                },
                {
                    "data": "questionEnglish",
                    "render": function (data, type, row) {
                        var text = data || row.questionHindi || '';
                        return text.length > 200 ? text.substring(0, 200) + '...' : text;
                    }
                },
                { "data": "subjectName" },
                { "data": "topicName" },
                { "data": "questionTypeName" },
                { "data": "difficultyLevel" }
            ],
            "pageLength": 10,
            "language": {
                "emptyTable": "No questions available"
            }
        });
    }

    function populateFilters(data) {
        var subjects = [...new Set(data.map(q => q.subjectName).filter(s => s))];
        var topics = [...new Set(data.map(q => q.topicName).filter(t => t))];
        var types = [...new Set(data.map(q => q.questionTypeName).filter(t => t))];

        $('#filterSubject').html('<option value="">All Subjects</option>' + 
            subjects.map(s => '<option value="' + s + '">' + s + '</option>').join(''));
        
        $('#filterTopic').html('<option value="">All Topics</option>' + 
            topics.map(t => '<option value="' + t + '">' + t + '</option>').join(''));
        
        $('#filterType').html('<option value="">All Types</option>' + 
            types.map(t => '<option value="' + t + '">' + t + '</option>').join(''));
    }

    function applyFilters() {
        var subject = $('#filterSubject').val();
        var topic = $('#filterTopic').val();
        var type = $('#filterType').val();
        var difficulty = $('#filterDifficulty').val();

        availableTable.column(2).search(subject).draw();
        availableTable.column(3).search(topic).draw();
        availableTable.column(4).search(type).draw();
        availableTable.column(5).search(difficulty).draw();
    }

    function loadExistingQuestions() {
        $.ajax({
            url: getExamQuestionsUrl,
            type: "GET",
            data: { examId: examId },
            success: function (response) {
                if (response.data && response.data.length > 0) {
                    selectedQuestions = response.data.map(function (q, index) {
                        return {
                            questionId: q.questionId,
                            questionEnglish: q.questionEnglish,
                            topicName: q.topicName,
                            marks: q.marks,
                            negativeMarks: q.negativeMarks,
                            sortOrder: q.sortOrder || (index + 1)
                        };
                    });
                    sortOrderCounter = selectedQuestions.length + 1;
                    renderSelectedQuestions();
                }
            }
        });
    }

    function bindEvents() {
        $('#filterSubject, #filterTopic, #filterType, #filterDifficulty').on('change', function() {
            applyFilters();
        });

        $(document).on('change', '.question-checkbox', function () {
            var $checkbox = $(this);
            var questionId = parseInt($checkbox.data('question-id'));
            
            if ($checkbox.is(':checked')) {
                var rowData = availableTable.row($checkbox.closest('tr')).data();
                addQuestion(rowData);
            } else {
                removeQuestion(questionId);
            }
        });

        $(document).on('click', '.btn-remove-question', function () {
            var questionId = parseInt($(this).data('question-id'));
            removeQuestion(questionId);
            uncheckQuestion(questionId);
        });

        $(document).on('change', '.marks-input, .negative-marks-input', function () {
            var questionId = parseInt($(this).closest('.question-card').data('question-id'));
            var question = selectedQuestions.find(q => q.questionId === questionId);
            
            if (question) {
                if ($(this).hasClass('marks-input')) {
                    question.marks = parseFloat($(this).val()) || 1.0;
                } else {
                    question.negativeMarks = parseFloat($(this).val()) || 0.0;
                }
            }
        });

        $('#btnSaveConfiguration').on('click', saveConfiguration);
    }

    function addQuestion(rowData) {
        var exists = selectedQuestions.some(q => q.questionId === rowData.questionId);
        if (exists) {
            toastr.warning('Question already added');
            return;
        }

        selectedQuestions.push({
            questionId: rowData.questionId,
            questionEnglish: rowData.questionEnglish,
            topicName: rowData.topicName,
            marks: 1.0,
            negativeMarks: 0.0,
            sortOrder: sortOrderCounter++
        });

        renderSelectedQuestions();
    }

    function removeQuestion(questionId) {
        selectedQuestions = selectedQuestions.filter(q => q.questionId !== questionId);
        renderSelectedQuestions();
    }

    function uncheckQuestion(questionId) {
        $('.question-checkbox[data-question-id="' + questionId + '"]').prop('checked', false);
    }

    function renderSelectedQuestions() {
        var $panel = $('#selectedQuestionsPanel');
        $('#selectedCount').text(selectedQuestions.length);

        if (selectedQuestions.length === 0) {
            $panel.html('<p class="text-muted text-center">No questions selected yet</p>');
            return;
        }

        var html = '';
        selectedQuestions.forEach(function (q, index) {
            html += '<div class="question-card" data-question-id="' + q.questionId + '">' +
                '<div class="d-flex justify-content-between align-items-start mb-2">' +
                '<div class="flex-grow-1 question-text">' +
                '<strong>Q' + (index + 1) + '.</strong> ' + (q.questionEnglish || '').substring(0, 70) + '...' +
                '<div class="question-meta">Topic: ' + (q.topicName || 'N/A') + '</div>' +
                '</div>' +
                '<button type="button" class="btn btn-sm btn-danger btn-remove-question" data-question-id="' + q.questionId + '">' +
                '<i class="fas fa-times"></i>' +
                '</button>' +
                '</div>' +
                '<div class="d-flex gap-2">' +
                '<div style="flex: 1;">' +
                '<label class="form-label">MARKS</label>' +
                '<input type="number" class="form-control marks-input" value="' + q.marks + '" min="0.01" step="0.25" />' +
                '</div>' +
                '<div style="flex: 1;">' +
                '<label class="form-label">NEGATIVE</label>' +
                '<input type="number" class="form-control negative-marks-input" value="' + q.negativeMarks + '" min="0" step="0.25" />' +
                '</div>' +
                '<div style="flex: 0.8;">' +
                '<label class="form-label">ORDER</label>' +
                '<input type="number" class="form-control" value="' + q.sortOrder + '" readonly />' +
                '</div>' +
                '</div>' +
                '</div>';
        });

        $panel.html(html);
    }

    function saveConfiguration() {
        if (selectedQuestions.length === 0) {
            toastr.warning('Please select at least one question');
            return;
        }

        var config = {
            examId: examId,
            questions: selectedQuestions.map(function (q) {
                return {
                    examId: examId,
                    questionId: q.questionId,
                    marks: q.marks,
                    negativeMarks: q.negativeMarks,
                    sortOrder: q.sortOrder
                };
            })
        };

        $.ajax({
            url: saveExamQuestionsUrl,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(config),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Configuration saved successfully!');
                    setTimeout(function () {
                        window.location.href = '/Exam';
                    }, 1500);
                } else {
                    toastr.error(response.message || 'Failed to save configuration');
                }
            },
            error: function () {
                toastr.error('An error occurred while saving');
            }
        });
    }

    return {
        init: init
    };
})();

$(document).ready(function () {
    ExamQuestionConfig.init();
});
