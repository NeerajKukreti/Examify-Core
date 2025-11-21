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
    
    $('#examTable').DataTable({
        ajax: {
            url: window.API_ENDPOINTS.baseUrl + 'student/Exam/list',
            type: 'GET',
            dataSrc: function (json) {
                userExams = json.UserExams || [];
                // Filter out exams that have already been taken
                const takenExamIds = userExams.map(ue => ue.ExamId);
                return json.Data.filter(exam => !takenExamIds.includes(exam.ExamId));
            }
        },
        columns: [
            { data: 'ExamName' },
            { data: 'Description' },
            {
                data: 'DurationMinutes',
                render: function (data) {
                    return data + ' mins';
                }            },    
            { data: 'TotalQuestions' },
            { data: 'ExamType' },
            {
                data: 'ExamId', width:"115px",
                render: function (data) {
                    return `<button onclick="launchExam(${data}, this)" class="btn btn-sm btn-primary">Launch Exam</button>`;
                }
            }
        ],
        responsive: true,
        lengthChange: true,
        autoWidth: false
    });
});
