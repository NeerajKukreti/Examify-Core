var QuestionBankTable = function () {
    var table;
    var QuestionBank = function () {
        table = $('#QuestionBankTable');
        table.dataTable({
            "order": [], // This disables the initial sorting
            "createdRow": function (row, data, dataIndex) {
                // Custom row styling if needed
            },
            "ajax": {
                "url": loadQuestionBankUrl,
                "type": "GET",
                "dataType": "json",   // correct spelling
                "dataSrc": "data"         // response is an array
            },
            "columns": [
                {
                    "title": "", "data": "questionId",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var str = ' <a class="btn" data-id=' + oData.questionId+' data-bs-target="#questionModel"  data-bs-toggle="modal" id="EditQuestion">' +
                            '<span class="fas fa-edit"></span>' +
                            '</a>';
                        $(nTd).html(str);
                    }
                },
                {
                    "title": "English Text", "data": "questionEnglish"
                },
                { "title": "Hindi Text", "data": "questionHindi" },
                { "title": "Topic", "data": "topicName" },
                { "title": "Multi Select", "data": "isMultiSelect" }
            ]
        });
        setInterval(function () {
            table.DataTable().ajax.reload(null, false);
        }, 30000);
    }
    return {
        init: function () {
            if (!$().dataTable) {
                return;
            }
            QuestionBank();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false);
        }
    };
}();

$(document).ready(function () {
    QuestionBankTable.init(); 

    $(document).on('click', '#CreateQuestion', function () {
        
        $.ajax({
            url: createQuestionUrl,
            type: "GET",
            beforeSend: function () {

            },
            success: function (res) {
                $(".questionModelBody").empty().html(res);

            },
            error: function (err) {
                alert(err)

            },
            complete: function () {

            }
        });

    });

    $(document).on('click', '#EditQuestion', function () {
        var $this = $(this);
         

        $.ajax({
            url: editQuestionUrl + "?id=" + $this.data('id'),
            type: "GET",
            beforeSend: function () {

            },
            success: function (res) {
                $(".questionModelBody").empty().html(res);

            },
            error: function (err) {
                alert(err)

            },
            complete: function () {

            }
        });

    });
});


