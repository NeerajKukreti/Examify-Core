$(document).ready(function () {
    $('#userExamTable').DataTable({
        ajax: {
            url: window.API_ENDPOINTS.baseUrl + `Exam/user/${userId}`,
            type: 'GET',
            dataSrc: function (json) {
                debugger;
                return json.Data;
            }
        },
        columns: [
            { data: 'ExamName' },
            { data: 'ExamType' },
            { data: 'Status' },
            {
                data: 'StartTime',
                render: function (data) {
                    return data ? new Date(data).toLocaleString() : '';
                }
            },
            {
                data: 'SubmitTime',
                render: function (data) {
                    return data ? new Date(data).toLocaleString() : '';
                }
            },
            {
                data: 'Percentage',
                render: function (data) {
                    return data ? data.toFixed(2) + '%' : '0%';
                }
            },
            {
                data: 'UserExamSessionId',
                render: function (data) {
                    return `<a target="_blank" href="/ExamSession/ExamResult?sessionId=${data}" class="btn btn-sm btn-info">View Result</a>`;
                }
            }
        ],
        responsive: true,
        lengthChange: true,
        autoWidth: false,
        order: [[4, 'desc']]
    });
});
