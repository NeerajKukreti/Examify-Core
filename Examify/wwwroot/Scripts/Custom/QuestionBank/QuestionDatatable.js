var QuestionBankTable = function () {
    var table;
    var QuestionBank = function () {
        table = $('#QuestionBankTable');
        table.dataTable({
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
                        var str = ' <a class="btn" data-target="#questionModel" data-toggle="modal">' +
                         '<i class="fa fa-edit"  id="CreateQuestion"></i>' +
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
            if (!jQuery().dataTable) {
                return;
            }
            QuestionBank();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false);
        }
    };
}();

if (App.isAngularJsApp() === false) {
    jQuery(document).ready(function () {
        QuestionBankTable.init();

        $(document).on('click', '.ancQuestionEdit', function () {
            $.ajax({
                url: editQuestionUrl + "?Id=100", /*//" + $(this).data('questionid'),*/
                type: 'GET',
                success: function (res) {
                    //$(".addQuestionModal").empty();
                    //$(".addQuestionModal").html(res);
                    Tabs.addTab('ConfigureTest', 'Configure Test', res, true);
                }
            });
        });

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
    });
}

