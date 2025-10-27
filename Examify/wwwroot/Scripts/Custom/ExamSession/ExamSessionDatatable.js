function launchExam(examId) {
    const features = `popup,width=${screen.availWidth},height=${screen.availHeight},left=0,top=0`;
    const w = window.open('/ExamSession/Details?id=' + examId, '_blank', features);
    if (w) {
        w.focus();
    } else {
        alert('Popup blocked. Please allow popups for this site.');
    }
}

$(document).ready(function () {
    $('#examTable').DataTable({
        ajax: {
            url: 'https://localhost:7271/api/Exam/list',
            type: 'GET',
            dataSrc: function (json) {
                return json.Data.filter(exam => exam.TotalQuestions > 0);
            }
        },
        columns: [
            { data: 'ExamName' },
            { data: 'Description' },
            { data: 'DurationMinutes' },    
            { data: 'TotalQuestions' },
            { data: 'ExamType' },
            {
                data: 'ExamId',
                render: function (data) {
                    return `<button onclick="launchExam(${data})" class="btn btn-sm btn-primary">Launch Exam</button>`;
                }
            }
        ],
        responsive: true,
        lengthChange: true,
        autoWidth: false
    });
});
