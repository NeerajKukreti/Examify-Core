// ExamTimer.js - Timer functionality for exams
console.log('ExamTimer.js loaded successfully');

// Timer variables
let examTimer = null;
let timeLeft = 0;
let isPaused = false;
let warningShown = false;

// Initialize timer
function initializeTimer(durationMinutes) {
    timeLeft = durationMinutes * 60;
    updateTimerDisplay();
    startTimer();
}

// Start the timer
function startTimer() {
    if (examTimer) clearInterval(examTimer);
    
    examTimer = setInterval(function() {
        if (!isPaused && timeLeft > 0) {
            timeLeft--;
            updateTimerDisplay();
            
            // Show warning at 5 minutes
            if (timeLeft === 300 && !warningShown) {
                showTimeWarning();
                warningShown = true;
            }
            
            // Auto-submit when time is up
            if (timeLeft <= 0) {
                timeUp();
            }
        }
    }, 1000);
}

// Update timer display
function updateTimerDisplay() {
    const hours = Math.floor(timeLeft / 3600);
    const minutes = Math.floor((timeLeft % 3600) / 60);
    const seconds = timeLeft % 60;
    
    const timeString = 
        (hours < 10 ? '0' : '') + hours + ':' +
        (minutes < 10 ? '0' : '') + minutes + ':' +
        (seconds < 10 ? '0' : '') + seconds;
    
    $('#timer').text(timeString);
    
    // Change color when time is running low
    if (timeLeft <= 300) { // 5 minutes
        $('#timer').addClass('text-danger');
    } else if (timeLeft <= 600) { // 10 minutes
        $('#timer').addClass('text-warning');
    }
}

// Pause timer
function pauseTimer() {
    isPaused = true;
    $('#pauseBtn').text('Resume');
}

// Resume timer
function resumeTimer() {
    isPaused = false;
    $('#pauseBtn').text('Pause');
}

// Show time warning
function showTimeWarning() {
    alert('Warning: Only 5 minutes remaining!');
}

// Time up - auto submit
function timeUp() {
    clearInterval(examTimer);
    alert('Time is up! Your exam will be submitted automatically.');
    if (typeof submitExam === 'function') {
        submitExam(true);
    }
}

// Get remaining time in seconds
function getRemainingTime() {
    return timeLeft;
}

// Add time (for extensions)
function addTime(minutes) {
    timeLeft += minutes * 60;
    updateTimerDisplay();
}

// Event handlers
$(document).ready(function() {
    $('#pauseBtn').on('click', function() {
        if (isPaused) {
            resumeTimer();
        } else {
            pauseTimer();
        }
    });
});