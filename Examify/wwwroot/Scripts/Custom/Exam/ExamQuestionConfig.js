var ExamQuestionConfig = (function () {
    var availableTable;
    var selectedQuestions = [];
    var sortOrderCounter = 1;
    var advancedMode = false;

    function init() {
        initAvailableQuestionsTable();
        loadExistingQuestions();
        bindEvents();
        updateSummary();
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
                    "className": "text-center align-middle",
                    "render": function (data, type, row) {
                        return '<input type="checkbox" class="question-checkbox" data-question-id="' + data + '" />';
                    }
                },
                {
                    "data": "questionEnglish",
                    "className": "align-middle",
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
            "lengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]],
            "info": true,
            "paging": true,
            "dom": '<"d-flex justify-content-between align-items-center mb-2"<"d-flex gap-2"li><"ms-auto"p>>rt',
            "language": {
                "emptyTable": "No questions available",
                "info": "Showing _START_ to _END_ of _TOTAL_ entries",
                "infoEmpty": "Showing 0 to 0 of 0 entries",
                "infoFiltered": "(filtered from _MAX_ total entries)",
                "lengthMenu": "Show _MENU_ entries"
            },
            "drawCallback": function() {
                updateCheckboxStates();
            },
            "initComplete": function() {
                updateUnselectedCount();
            }
        });
    }

    function populateFilters(data) {
        var subjects = [...new Set(data.map(q => q.subjectName).filter(s => s))];
        var subjectsWithIds = {};
        data.forEach(function(q) {
            if (q.subjectName && q.subjectId) {
                subjectsWithIds[q.subjectName] = q.subjectId;
            }
        });
        var topics = [...new Set(data.map(q => q.topicName).filter(t => t))];
        var types = [...new Set(data.map(q => q.questionTypeName).filter(t => t))];

        $('#filterSubject').html('<option value="">All Subjects</option>' + 
            subjects.map(s => '<option value="' + s + '" data-subject-id="' + subjectsWithIds[s] + '">' + s + '</option>').join(''));
        
        $('#filterTopic').html('<option value="">All Topics</option>' + 
            topics.map(t => '<option value="' + t + '">' + t + '</option>').join(''));
        
        $('#filterType').html('<option value="">All Types</option>' + 
            types.map(t => '<option value="' + t + '">' + t + '</option>').join(''));
    }

    function loadTopicsBySubject(subjectId) {
        if (!subjectId) {
            $('#filterTopic').html('<option value="">All Topics</option>');
            return;
        }

        $.ajax({
            url: window.API_ENDPOINTS.baseUrl +"subject/"+ subjectId + '/topics',
            type: 'GET',
            success: function(response) {
                if (response.Success && response.Data) {
                    $('#filterTopic').html('<option value="">All Topics</option>' + 
                        response.Data.map(t => '<option value="' + t.TopicName + '">' + t.TopicName + '</option>').join(''));
                }
            },
            error: function() {
                $('#filterTopic').html('<option value="">All Topics</option>');
            }
        });
    }

    function applyFilters() {
        var subject = $('#filterSubject').val();
        var topic = $('#filterTopic').val();
        var type = $('#filterType').val();
        var difficulty = $('#filterDifficulty').val();

        availableTable.column(2).search(subject);
        availableTable.column(3).search(topic);
        availableTable.column(4).search(type);
        availableTable.column(5).search(difficulty);
        availableTable.draw();
    }

    function toggleUnselectedFilter() {
        $.fn.dataTable.ext.search = [];
        
        if ($('#toggleUnselectedOnly').is(':checked')) {
            $.fn.dataTable.ext.search.push(function(settings, data, dataIndex) {
                var row = availableTable.row(dataIndex);
                if (!row || !row.data()) return true;
                var questionId = row.data().questionId;
                return !selectedQuestions.some(q => q.questionId === questionId);
            });
        }
        
        availableTable.draw();
        updateUnselectedCount();
    }

    function updateUnselectedCount() {
        if (!availableTable) return;
        var totalQuestions = availableTable.data().count();
        var unselectedCount = totalQuestions - selectedQuestions.length;
        $('#unselectedCount').text(unselectedCount);
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
                    updateCheckboxStates();
                }
                updateUnselectedCount();
            }
        });
    }

    function bindEvents() {
        $('#btnSelectFiltered').on('click', selectFilteredQuestions);
        $('#btnClearSelection').on('click', clearAllSelections);
        $('#btnResetFilters').on('click', resetFilters);
        $('#toggleUnselectedOnly').on('change', toggleUnselectedFilter);

        $('#filterSubject').on('change', function() {
            var subjectId = $(this).find('option:selected').data('subject-id');
            if (subjectId) {
                loadTopicsBySubject(subjectId);
            }
            applyFilters();
        });

        $('#filterTopic, #filterType, #filterDifficulty').on('change', function() {
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

        // Advanced Settings Toggle
        $('#toggleAdvancedSettings').on('change', function() {
            advancedMode = $(this).is(':checked');
            if (advancedMode) {
                $('#globalSettings').hide();
            } else {
                $('#globalSettings').show();
            }
            renderSelectedQuestions();
        });

        // Global Marks/Negative Marks Change
        $('#globalMarks, #globalNegativeMarks').on('input', function() {
            var marks = parseFloat($('#globalMarks').val()) || 1.0;
            var negativeMarks = parseFloat($('#globalNegativeMarks').val()) || 0.0;
            
            selectedQuestions.forEach(function(q) {
                q.marks = marks;
                q.negativeMarks = negativeMarks;
            });
            
            if (advancedMode) {
                renderSelectedQuestions();
            }
        });

        $(document).on('click', '.btn-expand', function() {
            var $text = $(this).siblings('.question-text');
            $text.toggleClass('collapsed');
            $(this).html($text.hasClass('collapsed') ? '<i class="fas fa-chevron-down"></i>' : '<i class="fas fa-chevron-up"></i>');
        });
    }

    function selectFilteredQuestions() {
        var addedCount = 0;
        availableTable.rows({ search: 'applied' }).every(function() {
            var rowData = this.data();
            var $checkbox = $(this.node()).find('.question-checkbox');
            if (!$checkbox.is(':checked') && !$checkbox.is(':disabled')) {
                $checkbox.prop('checked', true);
                addQuestion(rowData, true);
                addedCount++;
            }
        });
        
        if (addedCount > 0) {
            toastr.success(addedCount + ' question(s) added successfully');
        } else {
            toastr.info('No new questions to add');
        }
    }

    function clearAllSelections() {
        if (selectedQuestions.length === 0) {
            toastr.info('No questions selected to clear');
            return;
        }
        
        if (!confirm('Are you sure you want to clear all ' + selectedQuestions.length + ' selected question(s)? This action cannot be undone.')) {
            return;
        }
        
        selectedQuestions = [];
        renderSelectedQuestions();
        updateCheckboxStates();
        updateUnselectedCount();
        toastr.success('All selections cleared');
    }

    function resetFilters() {
        $('#filterSubject').val('');
        $('#filterTopic').val('');
        $('#filterType').val('');
        $('#filterDifficulty').val('');
        $('#toggleUnselectedOnly').prop('checked', false);
        $.fn.dataTable.ext.search = [];
        applyFilters();
    }

    function addQuestion(rowData, silent) {
        var exists = selectedQuestions.some(q => q.questionId === rowData.questionId);
        if (exists) {
            if (!silent) {
                toastr.warning('Question already added');
            }
            return;
        }

        var marks = parseFloat($('#globalMarks').val()) || 1.0;
        var negativeMarks = parseFloat($('#globalNegativeMarks').val()) || 0.0;

        selectedQuestions.push({
            questionId: rowData.questionId,
            questionEnglish: rowData.questionEnglish,
            topicName: rowData.topicName,
            marks: marks,
            negativeMarks: negativeMarks,
            sortOrder: sortOrderCounter++
        });

        renderSelectedQuestions();
        updateCheckboxStates();
        updateUnselectedCount();
        
        if (!silent) {
            toastr.success('Question added successfully');
        }
    }

    function removeQuestion(questionId) {
        var $card = $('.question-card[data-question-id="' + questionId + '"]');
        $card.fadeOut(300, function() {
            selectedQuestions = selectedQuestions.filter(q => q.questionId !== questionId);
            renderSelectedQuestions();
            updateCheckboxStates();
            updateUnselectedCount();
            toastr.info('Question removed');
        });
    }

    function uncheckQuestion(questionId) {
        var $checkbox = $('.question-checkbox[data-question-id="' + questionId + '"]');
        $checkbox.prop('checked', false).prop('disabled', false);
    }

    function updateCheckboxStates() {
        if (!availableTable) return;
        
        availableTable.rows().every(function() {
            var rowData = this.data();
            var $checkbox = $(this.node()).find('.question-checkbox');
            var isSelected = selectedQuestions.some(q => q.questionId === rowData.questionId);
            
            if (isSelected) {
                $checkbox.prop('checked', true).prop('disabled', true);
            } else {
                $checkbox.prop('checked', false).prop('disabled', false);
            }
        });
    }

    function updateSummary() {
        var totalQuestions = selectedQuestions.length;
        var totalMarks = selectedQuestions.reduce((sum, q) => sum + (q.marks || 0), 0);
        var avgMarks = totalQuestions > 0 ? (totalMarks / totalQuestions).toFixed(2) : 0;
        
        $('#summaryTotalQuestions').text(totalQuestions);
        $('#summaryTotalMarks').text(totalMarks.toFixed(2));
        $('#summaryAvgMarks').text(avgMarks);
        
        // Get difficulty breakdown from available table
        var easyCount = 0, mediumCount = 0, hardCount = 0;
        selectedQuestions.forEach(function(q) {
            if (availableTable) {
                availableTable.rows().every(function() {
                    var rowData = this.data();
                    if (rowData.questionId === q.questionId) {
                        var difficulty = rowData.difficultyLevel;
                        if (difficulty === 'Easy') easyCount++;
                        else if (difficulty === 'Medium') mediumCount++;
                        else if (difficulty === 'Hard') hardCount++;
                    }
                });
            }
        });
        
        $('#summaryEasy').text(easyCount);
        $('#summaryMedium').text(mediumCount);
        $('#summaryHard').text(hardCount);
    }

    function renderSelectedQuestions() {
        var $panel = $('#selectedQuestionsPanel');
        $('#selectedCount').text(selectedQuestions.length);
        updateSummary();

        if (selectedQuestions.length === 0) {
            $panel.html('<p class="text-muted text-center">No questions selected yet</p>');
            return;
        }

        var html = '';
        selectedQuestions.forEach(function (q, index) {
            var questionText = q.questionEnglish || '';
            var needsExpand = questionText.length > 500;
            
            html += '<div class="question-card" data-question-id="' + q.questionId + '">' +
                '<div class="d-flex justify-content-between align-items-start mb-2">' +
                '<div class="flex-grow-1">' +
                '<div class="d-flex align-items-start">' +
                '<strong style="min-width: 30px;">Q' + (index + 1) + '.</strong>' +
                '<div class="question-text' + (needsExpand ? ' collapsed' : '') + '" style="flex: 1;">' + questionText + '</div>' +
                (needsExpand ? '<button type="button" class="btn btn-sm btn-outline-secondary btn-expand ms-2" style="flex-shrink: 0;"><i class="fas fa-chevron-down"></i></button>' : '') +
                '</div>' +
                '<div class="question-meta">Topic: ' + (q.topicName || 'N/A') + '</div>' +
                '</div>' +
                '<button type="button" class="btn btn-sm btn-danger btn-remove-question" data-question-id="' + q.questionId + '">' +
                '<i class="fas fa-times"></i>' +
                '</button>' +
                '</div>';
            
            // Show individual fields only in advanced mode
            if (advancedMode) {
                html += '<div class="d-flex gap-2">' +
                    '<div style="flex: 1;">' +
                    '<label class="form-label">MARKS</label>' +
                    '<input type="number" class="form-control marks-input" value="' + q.marks + '" min="0.01" step="0.25" />' +
                    '</div>' +
                    '<div style="flex: 1;">' +
                    '<label class="form-label">NEGATIVE</label>' +
                    '<input type="number" class="form-control negative-marks-input" value="' + q.negativeMarks + '" min="0" step="0.25" />' +
                    '</div>' +
                    '<div style="flex: 0.8;">' +
                    '<label class="form-label" style="display: none">ORDER</label>' +
                    '<input style="display: none" type="number" class="form-control" value="' + q.sortOrder + '" readonly />' +
                    '</div>' +
                    '</div>';
            }
            
            html += '</div>';
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

        $('#ajaxLoader').addClass('show');

        $.ajax({
            url: saveExamQuestionsUrl,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(config),
            success: function (response) {
                $('#ajaxLoader').removeClass('show');
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
                $('#ajaxLoader').removeClass('show');
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

    // Scroll to top functionality
    $(window).scroll(function() {
        if ($(this).scrollTop() > $(document).height() / 2) {
            $('#scrollToTop').fadeIn();
        } else {
            $('#scrollToTop').fadeOut();
        }
        
        // Show sticky save button when scrolled down
        if ($(this).scrollTop() > 300) {
            $('#stickyActionBar').addClass('show');
        } else {
            $('#stickyActionBar').removeClass('show');
        }
    });

    $('#scrollToTop').click(function() {
        $('html, body').animate({ scrollTop: 0 }, 600);
    });
    
    // Sticky save button click
    $('#btnStickyConfig').click(function() {
        $('#btnSaveConfiguration').click();
    });
    
    // Summary card collapse toggle
    $('#summaryToggle').click(function() {
        $('#summaryContent').slideToggle(300);
        $('#summaryIcon').toggleClass('fa-chevron-up fa-chevron-down');
    });
});
