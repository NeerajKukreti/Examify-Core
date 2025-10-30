// Exam Interface Functionality
console.log('Exam.js file loaded successfully');

// API Configuration - will be set from window.examUrls
let API_BASE_URL = 'https://localhost:7271/api/Exam'; // fallback

let examData = null;
let allQuestions = [];
let currentSectionIndex = 0;
let currentQuestionIndex = 0;
let examResponses = {};
let questionStartTime = Date.now();
let examStartTime = Date.now();
let examId = 0;
let timeRemaining = 7200;
let autoSaveInterval = null;
let lastSavedTime = null;

// Anti-cheating variables
let tabSwitchCount = 0;
let isTabActive = true;
let fullscreenEnforced = false;
let timerStarted = false;
let timerInterval = null;
let isOnline = navigator.onLine;

const S = { UNVISITED: 'unvisited', NOT_ANSWERED: 'not-answered', ANSWERED: 'answered', MARKED: 'marked', MARKED_ANSWERED: 'marked-answered' };

async function loadExamData() {
    
    try {
        console.log('Loading exam data for examId:', examId);
        const apiUrl = `${API_BASE_URL}/${examId}/sessionquestions?userId=${window.currentUserId || 1}`;
        console.log('API URL:', apiUrl);
    
        const response = await fetch(apiUrl);
        console.log('API Response status:', response.status);

        if (!response.ok) {
            throw new Error(`API returned ${response.status}: ${response.statusText}`);
        }

        examData = await response.json();
        console.log('Raw exam data:', examData);
        console.log('API response structure:', JSON.stringify(examData, null, 2));

        // Handle API response structure
        if (!examData) {
            throw new Error('API returned null data');
        }

        // API returns ExamQuestionsResponse with capital 'Sections'
        if (!examData.Sections || !Array.isArray(examData.Sections)) {
            throw new Error('No Sections found in API response. Got: ' + Object.keys(examData).join(', '));
        }

        // Use the API structure directly
        examData.sections = examData.Sections;

        allQuestions = [];
        examData.sections.forEach((section, sectionIdx) => {
            if (section.Questions && Array.isArray(section.Questions)) {
                section.Questions.forEach((question) => {
                    allQuestions.push({
                        ...question,
                        sectionIndex: sectionIdx,
                        SectionName: section.SectionName,
                        globalIndex: allQuestions.length
                    });
                });
            }
        });

        // Initialize responses first
        allQuestions.forEach((q) => {
            examResponses[q.SessionQuestionId] = {
                SessionQuestionId: q.SessionQuestionId,
                SessionChoiceId: null,
                SessionChoiceIds: [],
                TimeSpent: 0,
                IsMarkedForReview: false,
                status: S.UNVISITED
            };
        });

        timeRemaining = examData.DurationMinutes * 60;
        
        // Try to restore from localStorage and merge
        const restored = loadFromLocalStorage();
        
        updateSectionTabs();
        renderQuestionGrid();
        showQuestion(restored ? currentQuestionIndex : 0);
        startTimer();
        startAutoSave();
        updateDebugPanel();
        
        if (restored) {
            alert('✅ Your previous progress has been restored!');
        }

    } catch (error) {
        console.error('Failed to load exam data:', error);
        $('#apiStatus').text('✗ Failed: ' + error.message).css('color', '#dc3545');
        $('#examDataStatus').text('✗ API Error').css('color', '#dc3545');
        $('#questionText').html(`<div style="color: red;">Error: ${error.message}</div>`);
        $('#qTitle').text('Error Loading Data');
    }
}

function updateSectionTabs() {
    const tabContainer = $('#sectionTabs');
    tabContainer.empty();

    examData.sections.forEach((section, index) => {
        const tab = $(`<button class="section-tab ${index === currentSectionIndex ? 'active' : ''}" data-section="${index}">${section.SectionName}</button>`);
        tabContainer.append(tab);
    });

    $('#sectionTitle').text(`Section: ${examData.sections[currentSectionIndex].SectionName}`);
}

function renderQuestionGrid() {
    const $grid = $('#qGrid').empty();

    // Show all questions from all sections
    allQuestions.forEach((question, index) => {
        const response = examResponses[question.SessionQuestionId];
        if (!response) {
            console.error('No response found for question:', question.SessionQuestionId);
            return;
        }
        const col = $('<div class="col"></div>');
        const btn = $(`<button type="button" class="qbtn q-${response.status}" data-global="${index}">${index + 1}</button>`);

        if (index === currentQuestionIndex) {
            btn.addClass('border border-dark');
        }

        col.append(btn);
        $grid.append(col);
    });

    updateStatusCounts();
}

function updateStatusCounts() {
    const counts = { answered: 0, marked: 0, notVisited: 0, notAnswered: 0 };

    Object.values(examResponses).forEach(response => {
        if (response.status === S.ANSWERED) counts.answered++;
        else if (response.status === S.MARKED_ANSWERED) counts.answered++;
        else if (response.status === S.MARKED) counts.notAnswered++;
        else if (response.status === S.UNVISITED) counts.notVisited++;
        else if (response.status === S.NOT_ANSWERED) counts.notAnswered++;
        
        if (response.status === S.MARKED || response.status === S.MARKED_ANSWERED) {
            counts.marked++;
        }
    });

    $('#answeredCount').text(counts.answered);
    $('#markedCount').text(counts.marked);
    $('#notVisitedCount').text(counts.notVisited);
    $('#notAnsweredCount').text(counts.notAnswered);
    
    updateProgressIndicator(counts.answered);
}

function updateProgressIndicator(answered) {
    const total = allQuestions.length;
    const percentage = total > 0 ? Math.round((answered / total) * 100) : 0;
    $('#progressText').text(`${answered}/${total} (${percentage}%)`);
    $('#progressBar').css('width', percentage + '%');
}

function getQuestionUiType(q) {
    switch (q.QuestionTypeId) {
        case 20: // MCQ
        case 21: // True/False
            return 'objective';
        case 22: // Descriptive
        case 25: // Coding
            return 'subjective';
        case 23: // Matching
            return 'pairing';
        case 24: // Ordering
            return 'ordering';
        default:
            return 'unknown';
    }
}

function showQuestion(globalIndex = 0) {
    if (currentQuestionIndex !== globalIndex && allQuestions[currentQuestionIndex]) {
        const timeSpent = Date.now() - questionStartTime;
        examResponses[allQuestions[currentQuestionIndex].SessionQuestionId].TimeSpent += timeSpent;
        // Save ordering and pairing before leaving
        saveCurrentResponse();
    }

    currentQuestionIndex = globalIndex;
    questionStartTime = Date.now();

    const question = allQuestions[globalIndex];
    const response = examResponses[question.SessionQuestionId];

    // Debug: Log question object
    console.log('Rendering question:', question);
    console.log('IsMultiSelect:', question.IsMultiSelect, 'QuestionTypeId:', question.QuestionTypeId);

    if (!question) {
        $('#questionText').html('<div class="alert alert-danger">Question data missing.</div>');
        return;
    }
    if (response.status === S.UNVISITED) {
        response.status = S.NOT_ANSWERED;
    }
    if (question.sectionIndex !== currentSectionIndex) {
        currentSectionIndex = question.sectionIndex;
        updateSectionTabs();
    }
    $('#qTitle').text(`Question No. ${globalIndex + 1}`);
    const topicName = question.TopicName || 'General';
    if ($('#topicName').length === 0) {
        $('#qTitle').after(`<div id="topicName" class="text-muted mb-2"><strong>Topic: </strong><span id="topicText">${topicName}</span></div>`);
    } else {
        $('#topicText').text(topicName);
    }
    $('#questionText').html(question.QuestionTextEnglish || question.QuestionText || '');
    const $form = $('#optionsForm').empty();
    $('#orderingContainer').hide();
    $('#pairingContainer').hide();
    $('#subjectiveContainer').hide();

    const uiType = getQuestionUiType(question);

    if (uiType === 'objective' && question.SessionChoices && question.SessionChoices.length > 0) {
        if (question.IsMultiSelect === true || question.IsMultiSelect === 'true') {
            // Render checkboxes
            question.SessionChoices.forEach((choice, index) => {
                const choiceId = choice.SessionChoiceId || choice.ChoiceId;
                const checked = Array.isArray(response.SessionChoiceIds) && response.SessionChoiceIds.includes(choiceId) ? 'checked' : '';
                const selectedClass = checked ? 'selected' : '';
                $form.append(`
                    <div class="option-item ${selectedClass}" data-choice-id="${choiceId}">
                        <label for="opt${index}">
                            <input type="checkbox" name="q_multi" value="${choiceId}" id="opt${index}" ${checked}>
                            ${choice.ChoiceTextEnglish || choice.ChoiceText || `Choice ${index + 1}`}
                        </label>
                    </div>
                `);
            });
            $form.off('click', '.option-item').on('click', '.option-item', function () {
                const $this = $(this);
                const $checkbox = $this.find('input[type="checkbox"]');
                $this.toggleClass('selected');
                $checkbox.prop('checked', !$checkbox.prop('checked'));
                $checkbox.trigger('change');
            });
        } else {
            // Render radio buttons (single select)
            question.SessionChoices.forEach((choice, index) => {
                const choiceId = choice.SessionChoiceId || choice.ChoiceId;
                const checked = response.SessionChoiceId == choiceId ? 'checked' : '';
                const selectedClass = checked ? 'selected' : '';
                $form.append(`
                    <div class="option-item ${selectedClass}" data-choice-id="${choiceId}">
                        <input type="radio" name="q" value="${choiceId}" id="opt${index}" ${checked}>
                        <label for="opt${index}">${choice.ChoiceTextEnglish || choice.ChoiceText || `Choice ${index + 1}`}</label>
                    </div>
                `);
            });
            $form.off('click', '.option-item').on('click', '.option-item', function () {
                const $this = $(this);
                const $radio = $this.find('input[type="radio"]');
                $form.find('.option-item').removeClass('selected');
                $this.addClass('selected');
                $radio.prop('checked', true);
                $radio.trigger('change');
            });
        }
    }
    else if (uiType === 'ordering' && question.SessionOrders && question.SessionOrders.length > 0) {
        $('#orderingContainer').show();
        const $list = $('#orderingList').empty();
        let orderIds;
        if (response.OrderedItems && response.OrderedItems.length > 0) {
            orderIds = response.OrderedItems;
        } else if (!response._randomizedOrder) {
            const shuffled = question.SessionOrders.map(o => o.CorrectOrder)
                .sort(() => Math.random() - 0.5);
            response._randomizedOrder = shuffled;
            orderIds = shuffled;
        } else {
            orderIds = response._randomizedOrder;
        }
        let orderedSessionOrders = orderIds.map(id => question.SessionOrders.find(o => o.CorrectOrder === id)).filter(Boolean);
        orderedSessionOrders.forEach((order, idx) => {
            $list.append(`<li class="list-group-item ordering-item" draggable="true" data-order-id="${order.SessionOrderId}" data-correct-order="${order.CorrectOrder}">${order.ItemText}</li>`);
        });
        // Enable drag-and-drop using HTML5
        setupOrderingDragAndDrop($list, response);
    }
    else if (uiType === 'pairing' && question.SessionPairs && question.SessionPairs.length > 0) {
        $('#pairingContainer').show();
        const $pairing = $('#pairingList').empty();
        const rightItems = question.SessionPairs.map(p => p.RightText);
        const shuffledRight = rightItems.sort(() => Math.random() - 0.5);
        question.SessionPairs.forEach((pair, idx) => {
            let selectedRight = '';
            if (response.PairedItems && response.PairedItems[idx] && response.PairedItems[idx].RightText) {
                selectedRight = response.PairedItems[idx].RightText;
            }
            $pairing.append(`
                <div class="row mb-2">
                    <div class="col-6">
                        <input type="text" class="form-control pair-left" value="${pair.LeftText}" readonly>
                    </div>
                    <div class="col-6">
                        <select class="form-control pair-right-select" data-pair-idx="${idx}">
                            <option value="">Select answer</option>
                            ${shuffledRight.map(rt => `<option value="${rt}" ${selectedRight === rt ? 'selected' : ''}>${rt}</option>`).join('')}
                        </select>
                    </div>
                </div>
            `);
        });
        // Save pairing selection immediately
        $('#pairingList .pair-right-select').on('change', function() {
            const idx = $(this).data('pair-idx');
            const left = $('#pairingList .pair-left').eq(idx).val();
            const right = $(this).val();
            response.PairedItems = response.PairedItems || [];
            response.PairedItems[idx] = { LeftText: left, RightText: right };
        });
    }
    else if (uiType === 'subjective') {
        // render textarea
        $('#subjectiveContainer').show();
        $('#subjectiveAnswer').val(response.ResponseText || '');
    }
    else {
        $form.append('<div class="alert alert-warning">No valid options for this question type.</div>');
    }
    renderQuestionGrid();
    updateDebugPanel();
}

function saveCurrentResponse() {
    if (!allQuestions[currentQuestionIndex]) {
        console.warn('No current question to save');
        return;
    }
    
    const question = allQuestions[currentQuestionIndex];
    const response = examResponses[question.SessionQuestionId];
    
    if (!response) {
        console.error('No response object for question:', question.SessionQuestionId);
        return;
    }
    const uiType = getQuestionUiType(question);
    let answered = false;
    if (uiType === 'objective') {
        response.SessionChoiceIds = [];
        if (question.IsMultiSelect) {
            // Multi-select: store array of selected choices
            $('#optionsForm input[name="q_multi"]:checked').each(function () {
                response.SessionChoiceIds.push(parseInt($(this).val()));
            });
            answered = response.SessionChoiceIds.length > 0;
            // For multi-select, clear single value
            response.SessionChoiceId = null;
        } else {
            const selectedChoice = $('#optionsForm input[name="q"]:checked').val();
            if (selectedChoice) {
                response.SessionChoiceIds = [parseInt(selectedChoice)];
                response.SessionChoiceId = parseInt(selectedChoice); // <-- Add this line
                answered = true;
            } else {
                response.SessionChoiceId = null;
            }
        }
    }
    else if (uiType === 'ordering') {
        response.OrderedItems = [];
        $('#orderingList .ordering-item').each(function () {
            // Instead of SessionOrderId, use CorrectOrder
            const correctOrder = $(this).data('correct-order');
            response.OrderedItems.push(correctOrder);
        });
        answered = response.OrderedItems.length > 0;
    }
    else if (uiType === 'pairing') {
        response.PairedItems = [];
        let allSelected = true;
        $('#pairingList .pair-left').each(function (idx) {
            const left = $(this).val();
            const right = $('#pairingList .pair-right-select').eq(idx).val();
            if (!right) allSelected = false;
            response.PairedItems.push({ LeftText: left, RightText: right });
        });
        answered = allSelected && response.PairedItems.length > 0;
    }
    else if (uiType === 'subjective') {
        response.ResponseText = $('#subjectiveAnswer').val();
        answered = !!response.ResponseText && response.ResponseText.trim().length > 0;
    }
    // Status logic for all types
    if (response.IsMarkedForReview) {
        response.status = answered ? S.MARKED_ANSWERED : S.MARKED;
    } else {
        response.status = answered ? S.ANSWERED : S.NOT_ANSWERED;
    }
    saveToLocalStorage();
    updateDebugPanel();
}

function startTimer() {
    if (timerStarted) return;

    timerStarted = true;
    timerInterval = setInterval(function () {
        if (timeRemaining > 0) {
            timeRemaining--;
            const hours = Math.floor(timeRemaining / 3600);
            const minutes = Math.floor((timeRemaining % 3600) / 60);
            const seconds = timeRemaining % 60;

            const timerEl = $('#timer');
            timerEl.text(
                (hours < 10 ? '0' : '') + hours + ':' +
                (minutes < 10 ? '0' : '') + minutes + ':' +
                (seconds < 10 ? '0' : '') + seconds
            );

            // Time warnings
            if (timeRemaining === 600) {
                alert('⏰ 10 minutes remaining!');
            } else if (timeRemaining === 300) {
                alert('⏰ 5 minutes remaining!');
            } else if (timeRemaining === 120) {
                timerEl.addClass('text-danger fw-bold');
                alert('⚠️ Only 2 minutes left!');
            }

            updateDebugPanel();
        } else {
            alert('Time is up! Exam will be submitted automatically.');
            finalSubmit();
        }
    }, 1000);
}

function startAutoSave() {
    autoSaveInterval = setInterval(function() {
        saveCurrentResponse();
        saveToLocalStorage();
        lastSavedTime = new Date();
        console.log('Auto-saved at:', lastSavedTime.toLocaleTimeString());
    }, 30000); // Auto-save every 30 seconds
}

function saveToLocalStorage() {
    try {
        const saveData = {
            examId: examId,
            userId: window.currentUserId,
            responses: examResponses,
            currentQuestionIndex: currentQuestionIndex,
            timeRemaining: timeRemaining,
            timestamp: Date.now()
        };
        localStorage.setItem(`exam_${examId}_${window.currentUserId}`, JSON.stringify(saveData));
        console.log('Saved to localStorage');
    } catch (e) {
        console.error('LocalStorage save failed:', e);
    }
}

function loadFromLocalStorage() {
    try {
        const saved = localStorage.getItem(`exam_${examId}_${window.currentUserId}`);
        if (saved) {
            const saveData = JSON.parse(saved);
            
            // Merge saved responses with initialized responses
            if (saveData.responses) {
                Object.keys(saveData.responses).forEach(key => {
                    if (examResponses[key]) {
                        examResponses[key] = { ...examResponses[key], ...saveData.responses[key] };
                    }
                });
            }
            
            currentQuestionIndex = saveData.currentQuestionIndex || 0;
            timeRemaining = saveData.timeRemaining || timeRemaining;
            console.log('Restored from localStorage:', new Date(saveData.timestamp).toLocaleString());
            return true;
        }
    } catch (e) {
        console.error('LocalStorage load failed:', e);
    }
    return false;
}

function clearLocalStorage() {
    try {
        localStorage.removeItem(`exam_${examId}_${window.currentUserId}`);
        console.log('Cleared localStorage');
    } catch (e) {
        console.error('LocalStorage clear failed:', e);
    }
}

let summaryFilter = 'all';
let summarySectionFilter = null;

// Show summary modal
function showSummary() {
    saveCurrentResponse();
    summaryFilter = 'all';
    summarySectionFilter = null;
    
    const counts = { answered: 0, marked: 0, notVisited: 0, notAnswered: 0 };
    Object.values(examResponses).forEach(r => {
        if (r.status === S.ANSWERED) counts.answered++;
        else if (r.status === S.MARKED_ANSWERED) counts.answered++;
        else if (r.status === S.MARKED) counts.notAnswered++;
        else if (r.status === S.UNVISITED) counts.notVisited++;
        else if (r.status === S.NOT_ANSWERED) counts.notAnswered++;
        
        if (r.status === S.MARKED || r.status === S.MARKED_ANSWERED) {
            counts.marked++;
        }
    });

    $('#summaryAnswered').text(counts.answered);
    $('#summaryNotAnswered').text(counts.notAnswered);
    $('#summaryMarked').text(counts.marked);
    $('#summaryNotVisited').text(counts.notVisited);

    renderSummarySectionTabs();
    renderSummaryQuestions();

    new bootstrap.Modal(document.getElementById('summaryModal')).show();
}

function renderSummarySectionTabs() {
    const $tabs = $('#summarySectionTabs').empty();
    $tabs.append(`<li class="nav-item"><a class="nav-link ${summarySectionFilter === null ? 'active' : ''}" href="#" onclick="filterSummarySection(null); return false;">All Sections</a></li>`);
    examData.sections.forEach((section, idx) => {
        $tabs.append(`<li class="nav-item"><a class="nav-link ${summarySectionFilter === idx ? 'active' : ''}" href="#" onclick="filterSummarySection(${idx}); return false;">${section.SectionName}</a></li>`);
    });
}

function filterSummarySection(sectionIdx) {
    summarySectionFilter = sectionIdx;
    renderSummarySectionTabs();
    renderSummaryQuestions();
}

function filterSummary(filter) {
    summaryFilter = filter;
    $('#summaryStatusTabs .nav-link').removeClass('active');
    $('#summaryStatusTabs .nav-link').each(function() {
        const href = $(this).attr('onclick');
        if (href && href.includes(`'${filter}'`)) {
            $(this).addClass('active');
        }
    });
    renderSummaryQuestions();
}

function renderSummaryQuestions() {
    const $list = $('#summaryQuestionList').empty();
    
    allQuestions.forEach((q, idx) => {
        const r = examResponses[q.SessionQuestionId];
        
        // Section filter
        if (summarySectionFilter !== null && q.sectionIndex !== summarySectionFilter) return;
        
        // Status filter
        if (summaryFilter === 'answered' && r.status !== S.ANSWERED && r.status !== S.MARKED_ANSWERED) return;
        if (summaryFilter === 'notAnswered' && r.status !== S.NOT_ANSWERED && r.status !== S.MARKED) return;
        if (summaryFilter === 'marked' && r.status !== S.MARKED && r.status !== S.MARKED_ANSWERED) return;
        if (summaryFilter === 'notVisited' && r.status !== S.UNVISITED) return;
        
        let statusClass = '', statusText = '', statusLabel = '';
        
        if (r.status === S.MARKED_ANSWERED) {
            statusClass = 'bg-success'; statusText = '✓M'; statusLabel = ' (Marked)';
        } else if (r.status === S.ANSWERED) {
            statusClass = 'bg-success'; statusText = '✓';
        } else if (r.status === S.MARKED) {
            statusClass = 'bg-purple'; statusText = 'M'; statusLabel = ' (Marked)';
        } else if (r.status === S.NOT_ANSWERED) {
            statusClass = 'bg-danger'; statusText = '✗';
        } else {
            statusClass = 'bg-secondary'; statusText = '-';
        }

        const questionText = (q.QuestionTextEnglish || q.QuestionText || '').replace(/<[^>]*>/g, '').substring(0, 80);
        
        $list.append(`
            <div class="summary-question" onclick="jumpToQuestion(${idx})">
                <span class="summary-status ${statusClass}">${statusText}</span>
                <strong>Q${idx + 1}.</strong> ${questionText}...${statusLabel}
                <span class="float-end text-muted small">+${q.Marks} / -${q.NegativeMarks}</span>
            </div>
        `);
    });
}

function jumpToQuestion(index) {
    bootstrap.Modal.getInstance(document.getElementById('summaryModal')).hide();
    showQuestion(index);
}

function confirmSubmit() {
    bootstrap.Modal.getInstance(document.getElementById('summaryModal')).hide();
    
    const unanswered = Object.values(examResponses).filter(r => 
        r.status === S.NOT_ANSWERED || r.status === S.UNVISITED
    ).length;
    
    if (unanswered > 0) {
        $('#unansweredWarning').text(`⚠️ You have ${unanswered} unanswered question(s).`);
    } else {
        $('#unansweredWarning').text('');
    }
    
    new bootstrap.Modal(document.getElementById('confirmModal')).show();
}

function submitExam() {
    showSummary();
}

async function finalSubmit() {
    bootstrap.Modal.getInstance(document.getElementById('confirmModal'))?.hide();
    
    if (typeof disableAntiCheating === 'function') {
        disableAntiCheating();
    }
    
    clearInterval(timerInterval);
    clearInterval(autoSaveInterval);
    if (allQuestions[currentQuestionIndex]) {
        const timeSpent = Date.now() - questionStartTime;
        examResponses[allQuestions[currentQuestionIndex].SessionQuestionId].TimeSpent += timeSpent;
    }
    const submission = {
        ExamId: examId,
        UserId: parseInt(window.currentUserId) || 1,
        SubmittedAt: new Date().toISOString(),
        TotalTimeSpent: Math.floor((Date.now() - examStartTime) / 1000),
        Responses: Object.values(examResponses).map(r => {
            const resp = {
                SessionQuestionId: r.SessionQuestionId,
                SessionChoiceIds: r.SessionChoiceIds || [],
                TimeSpent: r.TimeSpent,
                ResponseText: r.ResponseText || '',
                OrderedItems: [],
                PairedItems: []
            };
            // Ordering
            if (r.OrderedItems && r.OrderedItems.length > 0) {
                resp.OrderedItems = r.OrderedItems.map((correctOrder, idx) => ({
                    ResponseOrderId: 0,
                    ResponseId: 0,
                    UserOrder: correctOrder
                }));
            }
            // Pairing
            if (r.PairedItems && r.PairedItems.length > 0) {
                resp.PairedItems = r.PairedItems.map(pair => ({
                    ResponsePairId: 0,
                    ResponseId: 0,
                    LeftText: pair.LeftText,
                    RightText: pair.RightText
                }));
            }
            return resp;
        }),
        SessionId: examData.SessionId
    };

    try {
        const response = await fetch(`${API_BASE_URL}/${examId}/submit`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(submission)
        });

        if (response.ok) {
            const result = await response.json();
            if (result.Success && result.SessionId) {
                clearLocalStorage();
                const examResultUrl = window.examUrls?.examResultUrl || '/ExamSession/ExamResult';
                alert('✅ Exam submitted successfully!');
                window.location.href = `${examResultUrl}?sessionId=${result.SessionId}`;
            } else {
                alert('Failed to submit exam: ' + result.Message);
            }
        } else {
            alert('Failed to submit exam. Please try again.');
        }
    } catch (error) {
        console.error('Submission error:', error);
        alert('❌ Failed to submit exam. Please check your connection and try again.');
    }
}

// Handle forced submission (anti-cheat)
async function forceSubmitExam() {
    if (typeof disableAntiCheating === 'function') {
        disableAntiCheating();
    }
    clearInterval(timerInterval);
    clearInterval(autoSaveInterval);
    
    if (allQuestions[currentQuestionIndex]) {
        const timeSpent = Date.now() - questionStartTime;
        examResponses[allQuestions[currentQuestionIndex].SessionQuestionId].TimeSpent += timeSpent;
    }
    
    const submission = {
        ExamId: examId,
        UserId: parseInt(window.currentUserId) || 1,
        SubmittedAt: new Date().toISOString(),
        TotalTimeSpent: Math.floor((Date.now() - examStartTime) / 1000),
        Responses: Object.values(examResponses).map(r => ({
            SessionQuestionId: r.SessionQuestionId,
            SessionChoiceIds: r.SessionChoiceIds || [],
            TimeSpent: r.TimeSpent,
            ResponseText: r.ResponseText || '',
            OrderedItems: (r.OrderedItems || []).map((correctOrder, idx) => ({
                ResponseOrderId: 0,
                ResponseId: 0,
                UserOrder: correctOrder
            })),
            PairedItems: (r.PairedItems || []).map(pair => ({
                ResponsePairId: 0,
                ResponseId: 0,
                LeftText: pair.LeftText,
                RightText: pair.RightText
            }))
        })),
        SessionId: examData.SessionId
    };

    try {
        await fetch(`${API_BASE_URL}/${examId}/submit`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(submission)
        });
        clearLocalStorage();
        $('body').html('<div style="display: flex; align-items: center; justify-content: center; height: 100vh; background: #f8f9fa; font-family: Arial, sans-serif;"><div style="text-align: center; padding: 40px; background: white; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"><h2 style="color: #dc3545; margin-bottom: 20px;">🚨 Exam Terminated</h2><p style="color: #6c757d; margin-bottom: 0;">Your exam has been submitted due to violation of exam rules.</p></div></div>');
    } catch (error) {
        console.error('Force submission error:', error);
    }
}

function openSecureExamWindow(examId) {
    const startExamUrl = window.examUrls?.startExamUrl || '/ExamSession/StartExam';

    const newWindow = window.open(
        `${startExamUrl}?examId=${examId}`,
        'secureExamWindow',
        'location=no' +
        ',menubar=no' +
        ',status=no' +
        ',toolbar=no' +
        ',scrollbars=no' +
        ',resizable=no' +
        ',maximized=yes'
    );
  
    if (newWindow) {
        newWindow.onload = () => {
            try {
                // Try move and resize
                newWindow.moveTo(0, 0);
                newWindow.resizeTo(screen.availWidth, screen.availHeight);

                const docEl = newWindow.document.documentElement;
                if (docEl.requestFullscreen) {
                    docEl.requestFullscreen();
                } else if (docEl.mozRequestFullScreen) {
                    docEl.mozRequestFullScreen();
                } else if (docEl.webkitRequestFullscreen) {
                    docEl.webkitRequestFullscreen();
                } else if (docEl.msRequestFullscreen) {
                    docEl.msRequestFullscreen();
                }
            } catch (e) {
                console.log("Browser restrictions prevented full maximization:", e);
            }
        };
    }
    //if (newWindow) {
    //    // Focus the window and attempt to maximize
    //    setTimeout(function () {
    //        try {
    //            newWindow.focus();
    //            // Try to maximize using window state
    //            if (newWindow.screen) {
    //                newWindow.moveTo(0, 0);
    //                newWindow.resizeTo(screen.availWidth, screen.availHeight);
    //            }
    //        } catch (e) {
    //            console.log('Window maximization handled by browser:', e);
    //        }
    //    }, 100);

    //    // Close parent window after opening secure window
    //    window.close();
    //} else {
    //    alert('Please allow popups for this site to take the exam.');
    //}
}

function initializeExam(id) {
    examId = id;
    examStartTime = Date.now();
    console.log('initializeExam function called with ID:', id);
    
    initializeOfflineDetection();

    // Update API_BASE_URL from configuration
    if (window.examUrls?.apiBaseUrl) {
        API_BASE_URL = window.examUrls.apiBaseUrl;
    }

    // Show fullscreen prompt and disable interface
    setupFullscreenListeners();
    showFullscreenPrompt();

    $(document).on('click', '.section-tab', function () {
        const sectionIndex = parseInt($(this).data('section'));
        currentSectionIndex = sectionIndex;
        updateSectionTabs();
        renderQuestionGrid();

        const firstQuestionInSection = allQuestions.find(q => q.sectionIndex === sectionIndex);
        if (firstQuestionInSection) {
            showQuestion(firstQuestionInSection.globalIndex);
        }
    });

    $(document).on('click', '.qbtn', function () {
        const globalIndex = parseInt($(this).data('global'));
        saveCurrentResponse();
        showQuestion(globalIndex);
    });

    $(document).on('change', '#optionsForm input[name="q"]', function () {
        saveCurrentResponse();
        renderQuestionGrid();
    });

    $(document).on('change', '#optionsForm input[name="q_multi"]', function () {
        saveCurrentResponse();
        renderQuestionGrid();
    });

    $('#btnReview').on('click', function (e) {
        e.preventDefault();
        const response = examResponses[allQuestions[currentQuestionIndex].SessionQuestionId];
        response.IsMarkedForReview = true;
        saveCurrentResponse();

        if (currentQuestionIndex < allQuestions.length - 1) {
            showQuestion(currentQuestionIndex + 1);
        }
    });

    $('#btnClear').on('click', function (e) {
        e.preventDefault();
        $('#optionsForm input[name="q"]').prop('checked', false);
        $('#optionsForm input[name="q_multi"]').prop('checked', false);
        saveCurrentResponse();
        renderQuestionGrid();
    });

    $('#btnNext').on('click', function (e) {
        e.preventDefault();
        saveCurrentResponse();

        if (currentQuestionIndex < allQuestions.length - 1) {
            showQuestion(currentQuestionIndex + 1);
        }
    });

    $('#btnPrevious').on('click', function (e) {
        e.preventDefault();
        saveCurrentResponse();

        if (currentQuestionIndex > 0) {
            showQuestion(currentQuestionIndex - 1);
        }
    });

    // Keyboard shortcuts
    $(document).on('keydown', function(e) {
        // Ignore if typing in input/textarea
        if ($(e.target).is('input, textarea, select')) return;
        
        const key = e.key.toLowerCase();
        
        if (key === 'n') {
            e.preventDefault();
            $('#btnNext').click();
        } else if (key === 'p') {
            e.preventDefault();
            $('#btnPrevious').click();
        } else if (key === 'm') {
            e.preventDefault();
            $('#btnReview').click();
        } else if (key === 'c') {
            e.preventDefault();
            $('#btnClear').click();
        }
    });
}

function initializeOfflineDetection() {
    window.addEventListener('online', function() {
        isOnline = true;
        $('#offlineIndicator').hide();
        console.log('Connection restored');
        alert('✅ Internet connection restored!');
    });
    
    window.addEventListener('offline', function() {
        isOnline = false;
        $('#offlineIndicator').show();
        console.log('Connection lost');
        alert('⚠️ Internet connection lost! Your answers are saved locally and will be submitted when connection is restored.');
    });
    
    if (!navigator.onLine) {
        $('#offlineIndicator').show();
    }
}

function setupFullscreen() {
    const fullscreenBtn = document.getElementById("fullscreenBtn");
    if (fullscreenBtn) {
        fullscreenBtn.style.display = 'none'; // Hide manual fullscreen button during exam
    }
}

function setupMobileToggle() {
    $(document).on('click', '#mobileToggle', function () {
        $('#sidebar').toggleClass('show');
    });
}

function showFullscreenPrompt() {
    // Create overlay to block interface
    const overlay = $(`
        <div id="fullscreenOverlay" style="
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.9);
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 18px;
            text-align: center;
        ">
            <div>
                <h3>Fullscreen Mode Required</h3>
                <p>This exam must be taken in fullscreen mode for security.</p>
                <button id="enterFullscreenBtn" class="btn btn-primary btn-lg">Enter Fullscreen & Start Exam</button>
            </div>
        </div>
    `);

    $('body').append(overlay);

    // Handle fullscreen button click using event delegation
    $(document).off('click', '#enterFullscreenBtn').on('click', '#enterFullscreenBtn', function () {
        const elem = document.documentElement;
        if (elem.requestFullscreen) {
            elem.requestFullscreen();
        } else if (elem.webkitRequestFullscreen) {
            elem.webkitRequestFullscreen();
        } else if (elem.msRequestFullscreen) {
            elem.msRequestFullscreen();
        } else if (elem.mozRequestFullScreen) {
            elem.mozRequestFullScreen();
        }
    });
}

function setupFullscreenListeners() {
    // Listen for fullscreen change
    const fullscreenEvents = ['fullscreenchange', 'webkitfullscreenchange', 'mozfullscreenchange', 'MSFullscreenChange'];
    fullscreenEvents.forEach(event => {
        document.addEventListener(event, function () {
            const isFullscreen = document.fullscreenElement || document.webkitFullscreenElement || document.msFullscreenElement || document.mozFullScreenElement;
            if (isFullscreen) {
                // Always remove overlay when entering fullscreen
                $('#fullscreenOverlay').remove();
                fullscreenEnforced = true;

                // Start exam only first time
                if (!timerStarted) {
                    loadExamData();
                }

                // Hide browser fullscreen exit button with CSS
                const style = document.createElement('style');
                style.innerHTML = `
                    ::-webkit-full-screen-ancestor { display: none !important; }
                    ::-webkit-full-screen-ancestor * { display: none !important; }
                    :-webkit-full-screen { background: white !important; }
                    :-moz-full-screen { background: white !important; }
                    :fullscreen { background: white !important; }
                `;
                document.head.appendChild(style);
            } else if (fullscreenEnforced) {
                // Show overlay again if user exits fullscreen (but don't restart exam)
                if (!$('#fullscreenOverlay').length) {
                    showFullscreenPrompt();
                }
                fullscreenEnforced = false;
            }
        });
    });
}

// Fullscreen enforcement disabled
// function enforceFullscreen() { ... }

function updateDebugPanel() {
    $('#apiStatus').text('✓ Loaded').css('color', '#28a745');
    $('#examDataStatus').text(examData ? '✓ Loaded' : '✗ Failed').css('color', examData ? '#28a745' : '#dc3545');
    $('#questionsLoaded').text(allQuestions.length);
    $('#currentQuestionDebug').text(currentQuestionIndex + 1);

    const answeredCount = Object.values(examResponses).filter(r => r.SessionChoiceId !== null).length;
    $('#responsesSaved').text(answeredCount);
    
    // Update marks display based on current question
    if (allQuestions.length > 0 && currentQuestionIndex >= 0 && currentQuestionIndex < allQuestions.length) {
        const currentQuestion = allQuestions[currentQuestionIndex];
        const marksPerQuestion = currentQuestion?.Marks;
        const negativeMarks = currentQuestion?.NegativeMarks;

        $('#currentMarks').text(`+${marksPerQuestion}`);
        $('#negativeMarks').text(`-${negativeMarks}`);
    }
   
    // Update question timer
    const questionTimeSpent = Math.floor((Date.now() - questionStartTime) / 1000);
    const minutes = Math.floor(questionTimeSpent / 60);
    const seconds = questionTimeSpent % 60;
    $('#questionTime').text(`Time ${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`);
}

// Helper to rebind drag events after DOM update
function setupOrderingDragAndDrop($list, response) {
    // Remove any previous delegated handlers
    $list.off('.orderingDrag');

    $list.on('dragstart.orderingDrag', '.ordering-item', function(e) {
        e.originalEvent.dataTransfer.setData('text/plain', $(this).index());
    });

    $list.on('dragover.orderingDrag', '.ordering-item', function(e) {
        e.preventDefault();
    });

    $list.on('drop.orderingDrag', '.ordering-item', function(e) {
        e.preventDefault();
        const fromIdx = parseInt(e.originalEvent.dataTransfer.getData('text/plain'), 10);
        const toIdx = $(this).index();
        const items = $list.children().toArray();
        const moved = items.splice(fromIdx, 1)[0];
        items.splice(toIdx, 0, moved);
        $list.empty().append(items);
        response.OrderedItems = items.map(item => $(item).data('correct-order'));
        // No need to rebind, delegation handles new DOM
    });
}