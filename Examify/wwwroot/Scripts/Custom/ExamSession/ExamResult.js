// ExamResult.js - Exam Result functionality
console.log('ExamResult.js loaded successfully');

// API Configuration
let RESULT_API_BASE = 'https://localhost:7271/api/Exam';

// Load and display exam result
async function displayExamResult(sessionId) {
    try {
        console.log('Loading exam result for session:', sessionId);
        
        const resultData = await loadResultData(sessionId);
        if (resultData) {
            renderResultSummary(resultData);
            renderQuestionAnalysis(resultData);
            renderPerformanceChart(resultData);
        }
        
    } catch (error) {
        console.error('Error displaying exam result:', error);
        showErrorMessage('Failed to load exam results. Please try again.');
    }
}

// Load result data from API
async function loadResultData(sessionId) {
    try {
        const response = await fetch(`${RESULT_API_BASE}/result/${sessionId}`);
        
        if (!response.ok) {
            throw new Error(`API returned ${response.status}: ${response.statusText}`);
        }
        
        const data = await response.json();
        console.log('Result data loaded:', data);
        return data;
        
    } catch (error) {
        console.error('Error loading result data:', error);
        throw error;
    }
}

// Render result summary
function renderResultSummary(resultData) {
    console.log('Rendering result summary:', resultData);
    
    // Basic exam info
    $('#examName').text(resultData.ExamName || 'Exam Result');
    
    // Score information
    const obtainedMarks = resultData.ObtainedMarks || 0;
    const totalMarks = resultData.TotalMarks || 0;
    const percentage = resultData.Percentage || 0;
    
    $('#percentageText').text(`${Math.round(percentage)}%`);
    $('#marksText').text(`${obtainedMarks} / ${totalMarks} Marks`);
    
    // Set score circle color based on percentage
    const scoreCircle = $('#scoreCircle');
    scoreCircle.removeClass('score-excellent score-good score-average score-poor');
    if (percentage >= 75) scoreCircle.addClass('score-excellent');
    else if (percentage >= 60) scoreCircle.addClass('score-good');
    else if (percentage >= 40) scoreCircle.addClass('score-average');
    else scoreCircle.addClass('score-poor');
    
    // Question statistics
    $('#totalQuestions').text(resultData.TotalQuestions || 0);
    $('#correctCount').text(resultData.CorrectAnswers || 0);
    $('#wrongCount').text(resultData.WrongAnswers || 0);
    $('#unattemptedCount').text(resultData.UnattemptedQuestions || 0);
}

// Render question-wise analysis
function renderQuestionAnalysis(resultData) {
    console.log('Rendering question analysis:', resultData);
    
    const analysisContainer = $('#questionsList');
    analysisContainer.empty();
    
    if (!resultData.QuestionResults || resultData.QuestionResults.length === 0) {
        analysisContainer.html('<p class="text-muted">No question analysis available.</p>');
        return;
    }
    
    resultData.QuestionResults.forEach((question, index) => {
        const questionCard = createQuestionCard(question, index + 1);
        analysisContainer.append(questionCard);
    });
}

// Create question analysis card
function createQuestionCard(question, questionNumber) {
    const isCorrect = question.IsCorrect;
    const isAttempted = question.IsAttempted;
    const questionType = question.QuestionType;
    
    let statusClass = isCorrect ? 'correct' : (isAttempted ? 'wrong' : 'unattempted');
    let statusBadge = isCorrect ? 'status-correct' : (isAttempted ? 'status-wrong' : 'status-unattempted');
    let statusText = isCorrect ? 'Correct' : (isAttempted ? 'Wrong' : 'Unattempted');
    
    let choicesHtml = '';
    
    // Render choices for MCQ/True-False
    if ((questionType === 'MCQ' || questionType === 'True/False') && question.AllChoices && question.AllChoices.length > 0) {
        choicesHtml = '<div class="mt-3"><strong>Options:</strong>';
        question.AllChoices.forEach(choice => {
            const isSelected = question.SessionChoiceId === choice.SessionChoiceId;
            const isCorrectChoice = choice.IsCorrect;
            let choiceClass = '';
            if (isCorrectChoice) choiceClass = 'choice-correct';
            else if (isSelected) choiceClass = 'choice-selected-wrong';
            
            choicesHtml += `<div class="choice-item ${choiceClass}">
                <span class="choice-icon">${isCorrectChoice ? '✓' : (isSelected ? '✗' : '○')}</span>
                ${choice.ChoiceText}
            </div>`;
        });
        choicesHtml += '</div>';
    }
    
    // Render response text for Descriptive
    if (questionType === 'Descriptive' && question.ResponseText) {
        choicesHtml = `<div class="mt-3"><strong>Your Answer:</strong><p class="text-muted">${question.ResponseText}</p></div>`;
    }
    
    // Render pairs for Matching
    if (questionType === 'Matching' && question.ResponsePairs && question.ResponsePairs.length > 0) {
        choicesHtml = '<div class="mt-3"><strong>Your Matches:</strong>';
        const uniquePairs = question.ResponsePairs.filter((pair, index, self) => 
            index === self.findIndex(p => p.LeftText === pair.LeftText && p.RightText === pair.RightText)
        );
        uniquePairs.forEach(pair => {
            choicesHtml += `<div class="choice-item"><strong>${pair.LeftText}</strong> → ${pair.RightText}</div>`;
        });
        choicesHtml += '</div>';
    }
    
    const marksAwarded = question.MarksAwarded || 0;
    const marksClass = marksAwarded > 0 ? 'marks-positive' : (marksAwarded < 0 ? 'marks-negative' : 'marks-zero');
    
    return $(`
        <div class="question-card ${statusClass}">
            <div class="question-header">
                <div>
                    <strong>Question ${questionNumber}</strong>
                    <span class="topic-badge">${questionType}</span>
                </div>
                <div>
                    <span class="status-badge ${statusBadge}">${statusText}</span>
                    <div class="marks-display ${marksClass}">Marks: ${marksAwarded > 0 ? '+' : ''}${marksAwarded}</div>
                </div>
            </div>
            <div class="question-text">${question.QuestionText || 'Question text not available'}</div>
            ${choicesHtml}
        </div>
    `);
}

// Render performance chart
function renderPerformanceChart(resultData) {
    const chartContainer = $('#performanceChart');
    
    if (!chartContainer.length) return;
    
    const correct = resultData.CorrectAnswers || 0;
    const wrong = resultData.WrongAnswers || 0;
    const unanswered = resultData.UnansweredQuestions || 0;
    
    // Simple text-based chart for now
    chartContainer.html(`
        <div class="row text-center">
            <div class="col-md-4">
                <div class="card bg-success text-white">
                    <div class="card-body">
                        <h3>${correct}</h3>
                        <p>Correct</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card bg-danger text-white">
                    <div class="card-body">
                        <h3>${wrong}</h3>
                        <p>Wrong</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card bg-secondary text-white">
                    <div class="card-body">
                        <h3>${unanswered}</h3>
                        <p>Unanswered</p>
                    </div>
                </div>
            </div>
        </div>
    `);
}

// Calculate percentage
function calculatePercentage(score, maxScore) {
    if (!maxScore || maxScore === 0) return 0;
    return Math.round((score / maxScore) * 100);
}

// Format date and time
function formatDateTime(dateString) {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
}

// Format duration in seconds to readable format
function formatDuration(seconds) {
    if (!seconds || seconds === 0) return '0s';
    
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    
    let result = '';
    if (hours > 0) result += `${hours}h `;
    if (minutes > 0) result += `${minutes}m `;
    if (secs > 0 || result === '') result += `${secs}s`;
    
    return result.trim();
}

// Show error message
function showErrorMessage(message) {
    $('#errorMessage').text(message).show();
    setTimeout(() => $('#errorMessage').hide(), 5000);
}

// Print result
function printResult() {
    window.print();
}

// Download result as PDF
function downloadResultPDF() {
    // Implementation depends on PDF library
    alert('PDF download feature will be implemented');
}

// Filter questions by status
function filterQuestions(filter) {
    $('.filter-buttons .btn').removeClass('active');
    $(event.target).closest('.btn').addClass('active');
    
    if (filter === 'all') {
        $('.question-card').show();
    } else {
        $('.question-card').hide();
        $(`.question-card.${filter}`).show();
    }
}

// Initialize on page load
$(document).ready(function() {
    if (window.sessionId) {
        displayExamResult(window.sessionId);
    } else {
        showErrorMessage('Session ID not found');
    }
});

// Share result
function shareResult() {
    alert('Share feature will be implemented');

    if (sessionId) {
    navigator.share({
            title: 'Exam Result',
            text: `I scored ${$('#totalScore').text()} out of ${$('#maxScore').text()} in the exam!`,
            url: window.location.href
        });
    } else {
        // Fallback - copy to clipboard
        const resultText = `Exam Result: ${$('#totalScore').text()}/${$('#maxScore').text()} (${$('#percentage').text()})`;
        navigator.clipboard.writeText(resultText).then(() => {
            alert('Result copied to clipboard!');
        });
    }
}

// Initialize result page
$(document).ready(function() {
    // Get session ID from URL
    const urlParams = new URLSearchParams(window.location.search);
    const sessionId = urlParams.get('sessionId');
    
    if (sessionId) {
        displayExamResult(sessionId);
    } else {
        showErrorMessage('No session ID provided. Cannot load results.');
    }
    
    // Event handlers
    $('#printBtn').on('click', printResult);
    $('#downloadBtn').on('click', downloadResultPDF);
    $('#shareBtn').on('click', shareResult);
    
    // Back to exams button
    $('#backToExamsBtn').on('click', function() {
        window.location.href = '/ExamSession/Selection';
    });
});