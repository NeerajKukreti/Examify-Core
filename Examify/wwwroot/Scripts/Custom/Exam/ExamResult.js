// ExamResult.js - Exam Result functionality
console.log('ExamResult.js loaded successfully');

// API Configuration
let RESULT_API_BASE = '/api/Exam';

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
    $('#studentName').text(resultData.StudentName || 'Student');
    $('#submissionDate').text(formatDateTime(resultData.SubmittedAt));
    
    // Score information
    $('#totalScore').text(resultData.TotalScore || 0);
    $('#maxScore').text(resultData.MaxPossibleScore || 0);
    $('#percentage').text(`${calculatePercentage(resultData.TotalScore, resultData.MaxPossibleScore)}%`);
    
    // Question statistics
    $('#totalQuestions').text(resultData.TotalQuestions || 0);
    $('#correctAnswers').text(resultData.CorrectAnswers || 0);
    $('#wrongAnswers').text(resultData.WrongAnswers || 0);
    $('#unanswered').text(resultData.UnansweredQuestions || 0);
    
    // Time information
    $('#timeTaken').text(formatDuration(resultData.TimeTaken));
    $('#timeAllowed').text(formatDuration(resultData.TimeAllowed));
    
    // Pass/Fail status
    const passed = resultData.TotalScore >= (resultData.PassingMarks || 0);
    const statusText = passed ? 'PASSED' : 'FAILED';
    const statusClass = passed ? 'text-success' : 'text-danger';
    
    $('#resultStatus').text(statusText).removeClass('text-success text-danger').addClass(statusClass);
    $('#passingMarks').text(resultData.PassingMarks || 0);
    
    // Show appropriate result card
    if (passed) {
        $('#passCard').show();
        $('#failCard').hide();
    } else {
        $('#passCard').hide();
        $('#failCard').show();
    }
}

// Render question-wise analysis
function renderQuestionAnalysis(resultData) {
    console.log('Rendering question analysis:', resultData);
    
    const analysisContainer = $('#questionAnalysis');
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
    
    let statusClass = 'border-secondary';
    let statusIcon = '⚪';
    let statusText = 'Not Attempted';
    
    if (isAttempted) {
        if (isCorrect) {
            statusClass = 'border-success';
            statusIcon = '✅';
            statusText = 'Correct';
        } else {
            statusClass = 'border-danger';
            statusIcon = '❌';
            statusText = 'Incorrect';
        }
    }
    
    return $(`
        <div class="card mb-3 ${statusClass}">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h6 class="mb-0">Question ${questionNumber}</h6>
                <span class="badge ${isCorrect ? 'bg-success' : (isAttempted ? 'bg-danger' : 'bg-secondary')}">
                    ${statusIcon} ${statusText}
                </span>
            </div>
            <div class="card-body">
                <p class="card-text">${question.QuestionText || 'Question text not available'}</p>
                
                ${isAttempted ? `
                    <div class="row">
                        <div class="col-md-6">
                            <strong>Your Answer:</strong>
                            <p class="text-${isCorrect ? 'success' : 'danger'}">${question.SelectedAnswer || 'N/A'}</p>
                        </div>
                        <div class="col-md-6">
                            <strong>Correct Answer:</strong>
                            <p class="text-success">${question.CorrectAnswer || 'N/A'}</p>
                        </div>
                    </div>
                ` : `
                    <div>
                        <strong>Correct Answer:</strong>
                        <p class="text-success">${question.CorrectAnswer || 'N/A'}</p>
                    </div>
                `}
                
                <div class="mt-2">
                    <small class="text-muted">
                        Topic: ${question.TopicName || 'General'} | 
                        Marks: ${question.Marks || 1} | 
                        Time Spent: ${formatDuration(question.TimeSpent) || '0s'}
                    </small>
                </div>
            </div>
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

// Share result
function shareResult() {
    if (navigator.share) {
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
        window.location.href = '/Exam/Selection';
    });
});