// ExamSecurity.js - Security measures for exam
console.log('ExamSecurity.js loaded successfully');

// Security variables
let tabSwitchCount = 0;
let isTabActive = true;
let securityViolations = [];

// Initialize security measures
function initializeSecurity() {
    disableRightClick();
    disableKeyboardShortcuts();
    preventTextSelection();
    monitorTabSwitching();
    preventCopyPaste();
    disableDevTools();
}

// Disable right-click context menu
function disableRightClick() {
    document.addEventListener('contextmenu', function(e) {
        e.preventDefault();
        logSecurityViolation('Right-click attempted');
        return false;
    });
}

// Disable keyboard shortcuts
function disableKeyboardShortcuts() {
    document.addEventListener('keydown', function(e) {
        const keyCode = e.keyCode || e.which;
        
        // Block dangerous key combinations
        const blockedKeys = [
            123, // F12
            116, // F5
            122  // F11
        ];
        
        const blockedCombinations = [
            (e.ctrlKey && keyCode === 85), // Ctrl+U
            (e.ctrlKey && keyCode === 83), // Ctrl+S
            (e.ctrlKey && keyCode === 65), // Ctrl+A
            (e.ctrlKey && keyCode === 67), // Ctrl+C
            (e.ctrlKey && keyCode === 86), // Ctrl+V
            (e.ctrlKey && keyCode === 82), // Ctrl+R
            (e.ctrlKey && e.shiftKey && keyCode === 73), // Ctrl+Shift+I
            (e.ctrlKey && e.shiftKey && keyCode === 74), // Ctrl+Shift+J
            (e.ctrlKey && e.shiftKey && keyCode === 67)  // Ctrl+Shift+C
        ];
        
        if (blockedKeys.includes(keyCode) || blockedCombinations.some(combo => combo)) {
            e.preventDefault();
            e.stopPropagation();
            logSecurityViolation(`Blocked key combination: ${keyCode}`);
            return false;
        }
    });
}

// Prevent text selection
function preventTextSelection() {
    document.onselectstart = function() { 
        logSecurityViolation('Text selection attempted');
        return false; 
    };
    document.onmousedown = function() { return false; };
    document.ondragstart = function() { return false; };
}

// Monitor tab switching
function monitorTabSwitching() {
    // Window blur event
    window.addEventListener('blur', function() {
        if (isTabActive) {
            tabSwitchCount++;
            isTabActive = false;
            logSecurityViolation('Tab switch detected');
            handleTabSwitch();
        }
    });
    
    // Window focus event
    window.addEventListener('focus', function() {
        isTabActive = true;
    });
    
    // Visibility change event
    document.addEventListener('visibilitychange', function() {
        if (document.hidden) {
            tabSwitchCount++;
            logSecurityViolation('Page visibility changed');
            handleTabSwitch();
        }
    });
}

// Handle tab switch violation
function handleTabSwitch() {
    if (tabSwitchCount >= 3) {
        alert('Multiple tab switches detected. Your exam will be submitted automatically.');
        if (typeof submitExam === 'function') {
            submitExam(true, true);
        }
    } else {
        alert(`Warning: Tab switching is not allowed. Violation ${tabSwitchCount}/3`);
    }
}

// Prevent copy-paste
function preventCopyPaste() {
    document.addEventListener('copy', function(e) {
        e.preventDefault();
        logSecurityViolation('Copy attempted');
    });
    
    document.addEventListener('paste', function(e) {
        e.preventDefault();
        logSecurityViolation('Paste attempted');
    });
    
    document.addEventListener('cut', function(e) {
        e.preventDefault();
        logSecurityViolation('Cut attempted');
    });
}

// Disable developer tools
function disableDevTools() {
    // Detect if dev tools are open
    let devtools = {
        open: false,
        orientation: null
    };
    
    setInterval(function() {
        if (window.outerHeight - window.innerHeight > 200 || 
            window.outerWidth - window.innerWidth > 200) {
            if (!devtools.open) {
                devtools.open = true;
                logSecurityViolation('Developer tools opened');
                alert('Developer tools detected. Please close them to continue.');
            }
        } else {
            devtools.open = false;
        }
    }, 500);
}

// Log security violations
function logSecurityViolation(violation) {
    const timestamp = new Date().toISOString();
    securityViolations.push({
        violation: violation,
        timestamp: timestamp,
        userAgent: navigator.userAgent
    });
    
    console.warn('Security violation:', violation);
    
    // Send to server if needed
    // sendSecurityLog(violation, timestamp);
}

// Get security report
function getSecurityReport() {
    return {
        tabSwitchCount: tabSwitchCount,
        violations: securityViolations,
        totalViolations: securityViolations.length
    };
}

// Initialize on page load
$(document).ready(function() {
    initializeSecurity();
});