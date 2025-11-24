/**
 * Exam UX Enhancements
 * Additional features to improve user experience during exams
 */

(function() {
    'use strict';

    // 1. AUTO-SAVE INDICATOR
    function showAutoSaveIndicator() {
        const indicator = $('<div class="auto-save-indicator">✓ Saved</div>');
        indicator.css({
            position: 'fixed',
            top: '70px',
            right: '20px',
            background: '#28a745',
            color: 'white',
            padding: '8px 16px',
            borderRadius: '20px',
            fontSize: '13px',
            fontWeight: '600',
            zIndex: '9999',
            opacity: '0',
            transition: 'opacity 0.3s'
        });
        
        $('body').append(indicator);
        setTimeout(() => indicator.css('opacity', '1'), 10);
        setTimeout(() => {
            indicator.css('opacity', '0');
            setTimeout(() => indicator.remove(), 300);
        }, 2000);
    }

    // 2. QUESTION NAVIGATION SHORTCUTS
    function setupQuestionShortcuts() {
        $(document).on('keydown', function(e) {
            if ($(e.target).is('input, textarea, select')) return;
            const key = e.key.toLowerCase();
            if (key === 'n') { e.preventDefault(); $('#btnNext').click(); }
            else if (key === 'p') { e.preventDefault(); $('#btnPrevious').click(); }
            else if (key === 'm') { e.preventDefault(); $('#btnReview').click(); }
            else if (key === 'c') { e.preventDefault(); $('#btnClear').click(); }
        });
    }

    // 3. QUESTION TIMER WITH WARNINGS
    let questionTimeWarningShown = false;
    
    function monitorQuestionTime() {
        setInterval(function() {
            const timeSpent = Math.floor((Date.now() - questionStartTime) / 1000);
            
            // Warn if spending too long on one question (5 minutes)
            if (timeSpent > 300 && !questionTimeWarningShown) {
                questionTimeWarningShown = true;
                showToast('⏰ You\'ve spent 5+ minutes on this question', 'warning', 4000);
            }
        }, 10000); // Check every 10 seconds
    }

    // 4. ANSWER CONFIRMATION
    function setupAnswerConfirmation() {
        // Disabled - checkmark animation removed
    }

    // 5. PROGRESS MILESTONES
    function checkProgressMilestones() {
        const answered = Object.values(examResponses).filter(r => 
            r.status === 'answered' || r.status === 'marked-answered'
        ).length;
        const total = allQuestions.length;
        const percentage = Math.round((answered / total) * 100);
        
        // Show milestone notifications
        if (percentage === 25 && !window.milestone25) {
            window.milestone25 = true;
            showToast('🎯 25% Complete! Keep going!', 'success', 3000);
        } else if (percentage === 50 && !window.milestone50) {
            window.milestone50 = true;
            showToast('🎯 Halfway there! You\'re doing great!', 'success', 3000);
        } else if (percentage === 75 && !window.milestone75) {
            window.milestone75 = true;
            showToast('🎯 75% Complete! Almost done!', 'success', 3000);
        } else if (percentage === 100 && !window.milestone100) {
            window.milestone100 = true;
            showToast('🎉 All questions answered! Review before submitting.', 'success', 4000);
        }
    }

    // 6. SMART NAVIGATION HINTS
    function showNavigationHints() {
        // Show hints for first-time users
        if (!localStorage.getItem('examHintsShown')) {
            setTimeout(() => {
                showToast('💡 Tip: Use keyboard shortcuts (N=Next, P=Previous, M=Mark)', 'info', 5000);
                localStorage.setItem('examHintsShown', 'true');
            }, 3000);
        }
    }

    // 7. UNANSWERED QUESTION REMINDER
    function setupUnansweredReminder() {
        // Remind about unanswered questions when navigating
        $(document).on('click', '.qbtn', function() {
            const currentResponse = examResponses[allQuestions[currentQuestionIndex].SessionQuestionId];
            
            if (currentResponse && currentResponse.status === 'not-answered') {
                const $reminder = $('<div class="unanswered-reminder">Question not answered</div>');
                $reminder.css({
                    position: 'fixed',
                    bottom: '80px',
                    left: '50%',
                    transform: 'translateX(-50%)',
                    background: '#ffc107',
                    color: '#000',
                    padding: '10px 20px',
                    borderRadius: '20px',
                    fontSize: '14px',
                    fontWeight: '600',
                    zIndex: '9999',
                    boxShadow: '0 4px 12px rgba(0,0,0,0.2)'
                });
                
                $('body').append($reminder);
                setTimeout(() => $reminder.fadeOut(300, function() { $(this).remove(); }), 2000);
            }
        });
    }

    // 8. ENHANCED CLEAR CONFIRMATION
    function setupClearConfirmation() {
        $('#btnClear').off('click').on('click', function(e) {
            e.preventDefault();
            
            const question = allQuestions[currentQuestionIndex];
            const response = examResponses[question.SessionQuestionId];
            
            // Only confirm if there's an answer to clear
            if (response.status === 'answered' || response.status === 'marked-answered') {
                if (confirm('Are you sure you want to clear your answer?')) {
                    clearCurrentAnswer();
                    showToast('Answer cleared', 'info', 2000);
                }
            } else {
                showToast('No answer to clear', 'info', 1500);
            }
        });
    }

    function clearCurrentAnswer() {
        const question = allQuestions[currentQuestionIndex];
        const response = examResponses[question.SessionQuestionId];
        const uiType = getQuestionUiType(question);

        $('#optionsForm input[name="q"]').prop('checked', false);
        $('#optionsForm input[name="q_multi"]').prop('checked', false);
        $('#optionsForm .option-item').removeClass('selected');
        $('#subjectiveAnswer').val('');

        if (uiType === 'ordering') {
            const $list = $('#orderingList').empty();
            if (response._randomizedOrder) {
                response._randomizedOrder.forEach(correctOrder => {
                    const order = question.SessionOrders.find(o => o.CorrectOrder === correctOrder);
                    if (order) {
                        const $item = $('<li class="list-group-item ordering-item" draggable="true"></li>')
                            .attr('data-order-id', order.SessionOrderId)
                            .attr('data-correct-order', order.CorrectOrder)
                            .html(order.ItemText);
                        $list.append($item);
                    }
                });
                setupOrderingDragAndDrop($list, response);
            }
            response.OrderedItems = [];
        } else if (uiType === 'pairing') {
            response.PairedItems = [];
            $('#pairingList .pair-right-select').val('');
        }

        saveCurrentResponse();
        renderQuestionGrid();
    }

    // 9. QUESTION TOOLTIPS
    function addQuestionTooltips() {
        $(document).on('mouseenter', '.qbtn', function() {
            const index = parseInt($(this).data('global'));
            const question = allQuestions[index];
            const response = examResponses[question.SessionQuestionId];
            
            let status = 'Not visited';
            if (response.status === 'answered') status = 'Answered';
            else if (response.status === 'marked-answered') status = 'Answered & Marked';
            else if (response.status === 'marked') status = 'Marked for review';
            else if (response.status === 'not-answered') status = 'Not answered';
            
            $(this).attr('data-tooltip', `Q${index + 1}: ${status}`);
        });
    }

    // 10. SMOOTH SCROLL TO TOP ON QUESTION CHANGE
    function setupSmoothScroll() {
        const originalShowQuestion = window.showQuestion;
        window.showQuestion = function(index) {
            originalShowQuestion(index);
            $('html, body').animate({ scrollTop: 0 }, 300);
        };
    }

    // 11. DOUBLE-CLICK PROTECTION ON SUBMIT
    let submitInProgress = false;
    
    function protectDoubleSubmit() {
        const originalFinalSubmit = window.finalSubmit;
        window.finalSubmit = async function() {
            if (submitInProgress) {
                showToast('⚠️ Submission in progress...', 'warning', 2000);
                return;
            }
            
            submitInProgress = true;
            try {
                await originalFinalSubmit();
            } finally {
                submitInProgress = false;
            }
        };
    }

    // 12. VISUAL FEEDBACK FOR ACTIONS
    function addButtonFeedback() {
        $('.btn').on('click', function() {
            const $btn = $(this);
            $btn.addClass('btn-clicked');
            
            if (!$('#btnClickAnimation').length) {
                $('head').append(`
                    <style id="btnClickAnimation">
                        .btn-clicked {
                            animation: btnClick 0.3s ease;
                        }
                        @keyframes btnClick {
                            0% { transform: scale(1); }
                            50% { transform: scale(0.95); }
                            100% { transform: scale(1); }
                        }
                    </style>
                `);
            }
            
            setTimeout(() => $btn.removeClass('btn-clicked'), 300);
        });
    }

    // 13. ENHANCED PROGRESS TRACKING
    function enhanceProgressTracking() {
        const originalSaveCurrentResponse = window.saveCurrentResponse;
        window.saveCurrentResponse = function() {
            originalSaveCurrentResponse();
            checkProgressMilestones();
            showAutoSaveIndicator();
        };
    }

    // 14. ACCESSIBILITY: ANNOUNCE QUESTION CHANGES
    function announceQuestionChange() {
        if (!$('#ariaLiveRegion').length) {
            $('body').append('<div id="ariaLiveRegion" aria-live="polite" aria-atomic="true" class="sr-only"></div>');
        }
        
        const originalShowQuestion = window.showQuestion;
        window.showQuestion = function(index) {
            originalShowQuestion(index);
            const question = allQuestions[index];
            $('#ariaLiveRegion').text(`Question ${index + 1} of ${allQuestions.length}. ${question.TopicName || ''}`);
        };
    }

    // 15. INITIALIZE ALL ENHANCEMENTS
    function initializeUXEnhancements() {
        console.log('Initializing UX enhancements...');
        
        setupQuestionShortcuts();
        monitorQuestionTime();
        setupAnswerConfirmation();
        showNavigationHints()
        setupUnansweredReminder();
        setupClearConfirmation();
        addQuestionTooltips();
        setupSmoothScroll();
        protectDoubleSubmit();
        addButtonFeedback();
        enhanceProgressTracking();
        announceQuestionChange();
        
        console.log('✓ UX enhancements initialized');
    }

    // Auto-initialize when exam loads
    $(document).ready(function() {
        // Wait for exam to be initialized
        setTimeout(initializeUXEnhancements, 1000);
    });

    // Expose for manual initialization if needed
    window.initializeUXEnhancements = initializeUXEnhancements;

})();
