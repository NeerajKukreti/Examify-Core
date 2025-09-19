// Details.js - Exam Details functionality
console.log('Details.js loaded successfully');

// API Configuration
let DETAILS_API_BASE = '/api/Exam';

// Load exam details from API
async function loadExamDetails(examId) {
    try {
        console.log('Loading exam details for ID:', examId);
        
        const response = await fetch(`${window.examUrls.apiBaseUrl}/GetExamById/${examId}`);
        if (!response.ok) {
            throw new Error(`Failed to load exam details: ${response.status}`);
        }
        
        const examData = await response.json();
        displayExamInfo(examData);
        return examData;
        
    } catch (error) {
        console.error('Error loading exam details:', error);
        showErrorMessage('Failed to load exam details. Please try again.');
    }
}

// Display exam information on the page
function displayExamInfo(examData) {
    console.log('Displaying exam info:', examData);
    
    // Update exam title
    $('#examTitle').text(examData.ExamName || 'Exam Details');
    
    // Update exam details
    $('#examDescription').text(examData.Description || 'No description available');
    $('#examDuration').text(`${examData.DurationMinutes || 0} minutes`);
    $('#totalQuestions').text(examData.TotalQuestions || 0);
    $('#marksPerQuestion').text(examData.MarksPerQuestion || 1);
    $('#negativeMarks').text(examData.NegativeMarks || 0);
    $('#passingMarks').text(examData.PassingMarks || 0);
    
    // Update exam status
    const status = examData.IsActive ? 'Active' : 'Inactive';
    $('#examStatus').text(status).removeClass('text-success text-danger')
        .addClass(examData.IsActive ? 'text-success' : 'text-danger');
    
    // Update dates
    if (examData.StartDate) {
        $('#startDate').text(formatDate(examData.StartDate));
    }
    if (examData.EndDate) {
        $('#endDate').text(formatDate(examData.EndDate));
    }
    
    // Update instructions
    if (examData.Instructions) {
        $('#examInstructions').html(examData.Instructions);
    }
    
    // Show/hide start exam button based on status
    if (examData.IsActive && isExamAvailable(examData)) {
        $('#startExamBtn').show().attr('data-exam-id', examData.ExamId);
    } else {
        $('#startExamBtn').hide();
    }
}

// Check if exam is available to take
function isExamAvailable(examData) {
    const now = new Date();
    const startDate = examData.StartDate ? new Date(examData.StartDate) : null;
    const endDate = examData.EndDate ? new Date(examData.EndDate) : null;
    
    if (startDate && now < startDate) {
        showInfoMessage(`Exam will be available from ${formatDate(startDate)}`);
        return false;
    }
    
    if (endDate && now > endDate) {
        showInfoMessage('Exam period has ended');
        return false;
    }
    
    return true;
}

// Format date for display
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
}

// Show error message
function showErrorMessage(message) {
    $('#errorMessage').text(message).show();
    setTimeout(() => $('#errorMessage').hide(), 5000);
}

// Show info message
function showInfoMessage(message) {
    $('#infoMessage').text(message).show();
    setTimeout(() => $('#infoMessage').hide(), 5000);
}

// Start exam function
function startExam(examId) {
    if (confirm('Are you ready to start the exam? Once started, the timer will begin.')) {
        window.location.href = `/Exam/StartExam?examId=${examId}`;
    }
}

// Load exam statistics
async function loadExamStatistics(examId) {
    try {
        const response = await fetch(`${DETAILS_API_BASE}/${examId}/statistics`);
        if (response.ok) {
            const stats = await response.json();
            displayExamStatistics(stats);
        }
    } catch (error) {
        console.error('Error loading exam statistics:', error);
    }
}

// Display exam statistics
function displayExamStatistics(stats) {
    $('#totalAttempts').text(stats.TotalAttempts || 0);
    $('#averageScore').text(stats.AverageScore || 0);
    $('#highestScore').text(stats.HighestScore || 0);
    $('#passRate').text(`${stats.PassRate || 0}%`);
}

// Initialize page
$(document).ready(function() {
    // Get exam ID from URL or data attribute
    const urlParams = new URLSearchParams(window.location.search);
    const examId = urlParams.get('id') || $('#examDetails').data('exam-id');
     
    
    // Start exam button click handler
    $(document).on('click', '.startExam', function() {
        const examId = window.examId;
        if (examId) {
            openSecureExamWindow(examId);
        }
    });
    
    // Refresh details button
    $('#refreshBtn').on('click', function() {
        if (examId) {
            loadExamDetails(examId);
        }
    });
});