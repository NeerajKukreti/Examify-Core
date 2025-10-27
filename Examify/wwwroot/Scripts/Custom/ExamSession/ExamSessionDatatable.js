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
                data: 'examId',
                render: function (data) {
                    return `<a target="_blank" href="/ExamSession/StartExam?examId=${data}" class="btn btn-sm btn-primary">Launch Exam</a>`;
                }
            }
        ],
        responsive: true,
        lengthChange: true,
        autoWidth: false
    });
});
