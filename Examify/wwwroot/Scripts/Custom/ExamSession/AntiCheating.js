// AntiCheating.js - Advanced anti-cheating measures
console.log('AntiCheating.js loaded successfully');

// Anti-cheating state
let antiCheatState = {
    violations: [],
    tabSwitches: 0,
    keyViolations: 0,
    mouseViolations: 0,
    focusLost: 0,
    devToolsDetected: false,
    startTime: Date.now(),
    isActive: true
};

// Initialize anti-cheating system
function initializeAntiCheating() {
    console.log('Initializing anti-cheating system...');
    
    blockKeyboardShortcuts();
    blockRightClick();
    blockTextSelection();
    monitorTabSwitching();
    monitorDevTools();
    blockPrintScreen();
    monitorWindowResize();
    preventCopyPaste();
    
    // Start monitoring
    startViolationMonitoring();
    
    console.log('Anti-cheating system activated');
}

// Block dangerous keyboard shortcuts
function blockKeyboardShortcuts() {
    const blockedKeys = [
        { key: 123, name: 'F12' },
        { key: 116, name: 'F5' },
        { key: 122, name: 'F11' },
        { key: 44, name: 'Print Screen' }
    ];
    
    const blockedCombinations = [
        { ctrl: true, key: 85, name: 'Ctrl+U' },
        { ctrl: true, key: 83, name: 'Ctrl+S' },
        { ctrl: true, key: 82, name: 'Ctrl+R' },
        { ctrl: true, key: 80, name: 'Ctrl+P' },
        { ctrl: true, shift: true, key: 73, name: 'Ctrl+Shift+I' },
        { ctrl: true, shift: true, key: 74, name: 'Ctrl+Shift+J' },
        { ctrl: true, shift: true, key: 67, name: 'Ctrl+Shift+C' },
        { alt: true, key: 115, name: 'Alt+F4' }
    ];
    
    document.addEventListener('keydown', function(e) {
        const keyCode = e.keyCode || e.which;
        const isInputField = e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA';
        
        // Check blocked keys
        const blockedKey = blockedKeys.find(k => k.key === keyCode);
        if (blockedKey) {
            e.preventDefault();
            e.stopPropagation();
            logViolation('KEYBOARD', `Blocked key: ${blockedKey.name}`);
            return false;
        }
        
        // Check blocked combinations (allow Ctrl+A, Ctrl+C, Ctrl+V in input fields)
        const blockedCombo = blockedCombinations.find(combo => 
            (!combo.ctrl || e.ctrlKey) &&
            (!combo.shift || e.shiftKey) &&
            (!combo.alt || e.altKey) &&
            combo.key === keyCode
        );
        
        if (blockedCombo) {
            e.preventDefault();
            e.stopPropagation();
            logViolation('KEYBOARD', `Blocked combination: ${blockedCombo.name}`);
            return false;
        }
    }, true);
}

// Block right-click context menu
function blockRightClick() {
    document.addEventListener('contextmenu', function(e) {
        e.preventDefault();
        logViolation('MOUSE', 'Right-click attempted');
        return false;
    });
}

// Block text selection
function blockTextSelection() {
    document.onselectstart = function(e) {
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
            return true; // Allow selection in form fields
        }
        // Allow selection in question text for reading
        if (e.target.closest('#questionText, .option-item')) {
            return true;
        }
        logViolation('MOUSE', 'Text selection attempted');
        return false;
    };
    
    document.onmousedown = function(e) {
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
            return true;
        }
        return false;
    };
    
    document.ondragstart = function() {
        logViolation('MOUSE', 'Drag operation attempted');
        return false;
    };
}

// Monitor tab switching and focus loss
function monitorTabSwitching() {
    // Window blur (focus lost)
    window.addEventListener('blur', function() {
        if (antiCheatState.isActive) {
            antiCheatState.focusLost++;
            logViolation('FOCUS', 'Window focus lost');
            handleFocusViolation();
        }
    });
    
    // Visibility change (tab switch)
    document.addEventListener('visibilitychange', function() {
        if (document.hidden && antiCheatState.isActive) {
            antiCheatState.tabSwitches++;
            logViolation('TAB_SWITCH', 'Tab switched or window minimized');
            handleTabSwitchViolation();
        }
    });
    
    // Page unload attempt
    window.addEventListener('beforeunload', function(e) {
        if (antiCheatState.isActive) {
            logViolation('NAVIGATION', 'Page unload attempted');
            e.preventDefault();
            e.returnValue = 'Are you sure you want to leave the exam?';
            return e.returnValue;
        }
    });
}

// Monitor developer tools
function monitorDevTools() {
    let devtools = { open: false };
    
    setInterval(function() {
        const threshold = 160;
        
        if (window.outerHeight - window.innerHeight > threshold || 
            window.outerWidth - window.innerWidth > threshold) {
            
            if (!devtools.open) {
                devtools.open = true;
                antiCheatState.devToolsDetected = true;
                logViolation('DEV_TOOLS', 'Developer tools opened');
                handleDevToolsViolation();
            }
        } else {
            devtools.open = false;
        }
    }, 1000);
}

// Block Print Screen
function blockPrintScreen() {
    document.addEventListener('keyup', function(e) {
        if (e.keyCode === 44) {
            logViolation('PRINT_SCREEN', 'Print Screen key pressed');
        }
    });
}

// Monitor window resize (potential fullscreen exit)
function monitorWindowResize() {
    window.addEventListener('resize', function() {
        if (antiCheatState.isActive) {
            logViolation('WINDOW', 'Window resized');
        }
    });
}

// Prevent copy/paste operations
function preventCopyPaste() {
    ['copy', 'paste', 'cut'].forEach(event => {
        document.addEventListener(event, function(e) {
            if (e.target.tagName !== 'INPUT' && e.target.tagName !== 'TEXTAREA') {
                e.preventDefault();
                logViolation('CLIPBOARD', `${event.toUpperCase()} operation blocked`);
            }
        });
    });
}

// Log violation
function logViolation(type, description) {
    const violation = {
        type: type,
        description: description,
        timestamp: new Date().toISOString(),
        timeFromStart: Date.now() - antiCheatState.startTime,
        url: window.location.href,
        userAgent: navigator.userAgent
    };
    
    antiCheatState.violations.push(violation);
    console.warn('Anti-cheat violation:', violation);
    
    // Update counters
    switch(type) {
        case 'KEYBOARD':
            antiCheatState.keyViolations++;
            break;
        case 'MOUSE':
            antiCheatState.mouseViolations++;
            break;
    }
    
    // Send to server if needed
    sendViolationToServer(violation);
}

// Handle focus violation
function handleFocusViolation() {
    if (antiCheatState.focusLost >= 3) {
        showViolationWarning('Multiple focus violations detected. Exam may be terminated.');
    }
}

// Handle tab switch violation
function handleTabSwitchViolation() {
    if (antiCheatState.tabSwitches >= 2) {
        showCriticalViolation('Tab switching detected. Exam will be submitted automatically.');
        setTimeout(() => {
            if (typeof forceSubmitExam === 'function') {
                forceSubmitExam();
            }
        }, 3000);
    } else {
        showViolationWarning(`Tab switching detected. Warning ${antiCheatState.tabSwitches}/2`);
    }
}

// Handle developer tools violation
function handleDevToolsViolation() {
    showCriticalViolation('Developer tools detected. Please close them immediately.');
}

// Show violation warning
function showViolationWarning(message) {
    alert(`⚠️ VIOLATION WARNING\n\n${message}`);
}

// Show critical violation
function showCriticalViolation(message) {
    alert(`🚨 CRITICAL VIOLATION\n\n${message}`);
}

// Send violation to server
function sendViolationToServer(violation) {
    // Add exam context to violation
    const violationData = {
        ...violation,
        userId: window.currentUserId || null,
        examId: window.examId || null,
        sessionId: window.examData?.SessionId || null
    };
    
    const apiUrl = window.examUrls?.apiBaseUrl || window.API_ENDPOINTS.baseUrl.replace('/', '');
    
    fetch(`${apiUrl}/violation`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(violationData)
    }).catch(err => console.error('Failed to send violation:', err));
}

// Start violation monitoring
function startViolationMonitoring() {
    setInterval(function() {
        const totalViolations = antiCheatState.violations.length;
        
        if (totalViolations >= 10) {
            showCriticalViolation('Too many violations detected. Exam will be terminated.');
            if (typeof forceSubmitExam === 'function') {
                forceSubmitExam();
            }
        }
    }, 5000);
}

// Get violation report
function getViolationReport() {
    return {
        totalViolations: antiCheatState.violations.length,
        tabSwitches: antiCheatState.tabSwitches,
        keyViolations: antiCheatState.keyViolations,
        mouseViolations: antiCheatState.mouseViolations,
        focusLost: antiCheatState.focusLost,
        devToolsDetected: antiCheatState.devToolsDetected,
        violations: antiCheatState.violations,
        examDuration: Date.now() - antiCheatState.startTime
    };
}

// Disable anti-cheating (call when exam is submitted)
function disableAntiCheating() {
    antiCheatState.isActive = false;
    console.log('Anti-cheating system disabled');
}

// Export functions for external use
window.AntiCheat = {
    initialize: initializeAntiCheating,
    getReport: getViolationReport,
    disable: disableAntiCheating
};