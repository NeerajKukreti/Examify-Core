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
            url: 'https://localhost:7271/api/student/Exam/list',
            type: 'GET',
            dataSrc: function (json) {
                return json.Data;
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
                    return `<button onclick="launchExam(${data})" class="btn btn-sm btn-primary">Launch Exam</button>`;
                }
            }
        ],
        responsive: true,
        lengthChange: true,
        autoWidth: false
    });
});
