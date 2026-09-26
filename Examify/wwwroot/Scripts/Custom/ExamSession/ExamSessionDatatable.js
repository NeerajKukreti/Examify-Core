let examWindows = {};

function launchExam(examId, btn) {
    debugger
    btn.disabled = true;
    btn.textContent = 'Launching...';
    
    // Check if window already exists and is open
    if (examWindows[examId] && !examWindows[examId].closed) {
        examWindows[examId].focus();
        btn.disabled = false;
        btn.textContent = 'Launch Exam';
        return;
    }
    
    const features = `popup,width=${screen.availWidth},height=${screen.availHeight},left=0,top=0`;
    const w = window.open('/ExamSession/Details?id=' + examId, '_blank', features);
    if (w) {
        examWindows[examId] = w;
        w.focus();
        btn.disabled = false;
        btn.textContent = 'Launch Exam';
    } else {
        alert('Popup blocked. Please allow popups for this site.');
        btn.disabled = false;
        btn.textContent = 'Launch Exam';
    }
}

$(document).ready(function () {
    let userExams = [];
    const base = window.API_ENDPOINTS.baseUrl;
    const listUrl = base.endsWith('/') ? base + 'student/Exam/list' : base + '/student/Exam/list';
    
    $('#examTable').DataTable({
        ajax: {
            url: listUrl,
            type: 'GET',
            dataSrc: function (json) {
                userExams = json.UserExams || [];
                const submittedExamIds = userExams.filter(ue => ue.Status === 'Submit').map(ue => ue.ExamId);
                const allExams = json.Data || [];
                return allExams.map(exam => {
                    exam.IsCompleted = submittedExamIds.includes(exam.ExamId);
                    return exam;
                });
            }
        },
        columns: [
            { data: 'ExamName' },
            { data: 'Description' },
            {
                data: 'DurationMinutes',
                render: function (data) {
                    return (data || 0) + ' mins';
                }
            },    
            { data: 'TotalQuestions' },
            { data: 'ExamType' },
            {
                data: 'ExamId', width:"130px",
                render: function (data, type, row) {
                    if (row.IsCompleted && row.ExamType === 'Practice') {
                        return `<button onclick="launchExam(${data}, this)" class="btn btn-sm btn-outline-primary"><i class="fas fa-redo me-1"></i> Retake</button>`;
                    } else if (row.IsCompleted) {
                        return `<span class="badge bg-secondary p-2">Completed</span>`;
                    } else {
                        return `<button onclick="launchExam(${data}, this)" class="btn btn-sm btn-primary">Launch Exam</button>`;
                    }
                }
            }
        ],
        responsive: true,
        lengthChange: true,
        autoWidth: false
    });
});
