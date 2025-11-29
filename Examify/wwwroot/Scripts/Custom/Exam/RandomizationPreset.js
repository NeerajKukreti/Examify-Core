var PresetManager = (function () {
    var table;
    var subjects = [];
    var topics = [];
    var questionTypes = [];

    function init() {
        loadMasterData();
        initTable();
        bindEvents();
    }

    function loadMasterData() {
        $.get(window.API_ENDPOINTS.baseUrl + 'Subject/' + instituteId + '/0', function (res) {
            subjects = res.Data || [];
        });
        $.get(window.API_ENDPOINTS.baseUrl + 'Question/types', function (res) {
            debugger;
            questionTypes = res.Data || res || [];
        });
    }

    function initTable() {
        table = $('#presetsTable').DataTable({
            ajax: {
                url: '/RandomizationPreset/GetAll',
                dataSrc: 'data'
            },
            columns: [
                { data: 'presetName' },
                { data: 'description' },
                {
                    data: 'details',
                    render: function (data) {
                        return data ? data.length + ' rule(s)' : '0 rules';
                    }
                },
                {
                    data: 'createdDate',
                    render: function (data) {
                        return new Date(data).toLocaleDateString();
                    }
                },
                {
                    data: null,
                    orderable: false,
                    render: function (data) {
                        return `
                            <button class="btn btn-sm btn-info btn-edit" data-id="${data.presetId}"><i class="fas fa-edit"></i></button>
                            <button class="btn btn-sm btn-danger btn-delete" data-id="${data.presetId}"><i class="fas fa-trash"></i></button>
                        `;
                    }
                }
            ]
        });
    }

    function bindEvents() {
        $('#btnNewPreset').click(() => openModal());
        $('#btnAddRule').click(addRuleRow);
        $('#btnSavePreset').click(savePreset);

        $(document).on('click', '.btn-edit', function () {
            var id = $(this).data('id');
            editPreset(id);
        });

        $(document).on('click', '.btn-delete', function () {
            var id = $(this).data('id');
            deletePreset(id);
        });

        $(document).on('click', '.btn-remove-rule', function () {
            $(this).closest('.rule-row').remove();
        });

        $(document).on('change', '.subject-select', function () {
            var $row = $(this).closest('.rule-row');
            var subjectId = $(this).val();
            loadTopics(subjectId, $row.find('.topic-select'));
        });
    }

    function openModal(preset = null) {
        $('#presetId').val(preset ? preset.presetId : 0);
        $('#presetName').val(preset ? preset.presetName : '');
        $('#presetDescription').val(preset ? preset.description : '');
        $('#rulesContainer').empty();

        if (preset && preset.details) {
            preset.details.forEach(d => addRuleRow(d));
        } else {
            addRuleRow();
        }

        $('#modalTitle').text(preset ? 'Edit Preset' : 'New Preset');
        new bootstrap.Modal($('#presetModal')).show();
    }

    function addRuleRow(detail = null) {
        var subjectOptions = subjects.map(s => `<option value="${s.SubjectId}">${s.SubjectName}</option>`).join('');
        var typeOptions = questionTypes.map(t => `<option value="${t.QuestionTypeId}">${t.TypeName}</option>`).join('');

        var html = `
            <div class="rule-row mb-3 p-3 border rounded">
                <div class="row g-2">aa
                    <div class="col-md-3">
                        <label>Subject <span class="text-danger">*</span></label>
                        <select class="form-select subject-select" required>
                            <option value="">Select Subject</option>${subjectOptions}
                        </select>
                    </div>
                    <div class="col-md-3">
                        <label>Topic <span class="text-danger">*</span></label>
                        <select class="form-select topic-select" required>
                            <option value="">Select Topic</option>
                        </select>
                    </div>
                    <div class="col-md-2">
                        <label>Difficulty</label>
                        <select class="form-select difficulty-select">
                            <option value="">All</option>
                            <option value="Easy">Easy</option>
                            <option value="Medium">Medium</option>
                            <option value="Hard">Hard</option>
                        </select>
                    </div>
                    <div class="col-md-2">
                        <label>Type</label>
                        <select class="form-select type-select">
                            <option value="">All</option>${typeOptions}
                        </select>
                    </div>
                    <div class="col-md-1">
                        <label>Pick</label>
                        <input type="number" class="form-control pick-count" min="1" value="5">
                    </div>
                    <div class="col-md-1">
                        <label>&nbsp;</label>
                        <button type="button" class="btn btn-danger btn-sm w-100 btn-remove-rule"><i class="fas fa-times"></i></button>
                    </div>
                </div>
            </div>
        `;

        var $row = $(html);
        $('#rulesContainer').append($row);

        $row.find('.subject-select, .topic-select').select2({
            dropdownParent: $('#presetModal'),
            width: '100%'
        });

        if (detail) {
            $row.find('.subject-select').val(detail.subjectId || '').trigger('change');
            $row.find('.difficulty-select').val(detail.difficultyLevel || '');
            $row.find('.type-select').val(detail.questionTypeId || '');
            $row.find('.pick-count').val(detail.pickCount);

            if (detail.subjectId) {
                loadTopics(detail.subjectId, $row.find('.topic-select'), detail.topicId);
            }
        }
    }

    function loadTopics(subjectId, $select, selectedTopicId = null) {
        if (!subjectId) {
            $select.html('<option value="">Select Topic</option>').trigger('change');
            return;
        }

        $.get(window.API_ENDPOINTS.baseUrl + 'subject/' + subjectId + '/topics', function (res) {
            var options = '<option value="">Select Topic</option>';
            if (res.Success && res.Data) {
                options += res.Data.map(t => `<option value="${t.TopicId}">${t.TopicName}</option>`).join('');
            }
            $select.html(options);
            if (selectedTopicId) $select.val(selectedTopicId);
            $select.trigger('change');
        });
    }

    function savePreset() {
        var preset = {
            presetId: parseInt($('#presetId').val()),
            presetName: $('#presetName').val().trim(),
            description: $('#presetDescription').val().trim(),
            instituteId: instituteId,
            isActive: true,
            details: []
        };

        if (!preset.presetName) {
            toastr.error('Preset name is required');
            return;
        }

        var isValid = true;
        $('.rule-row').each(function () {
            var $row = $(this);
            var subjectId = $row.find('.subject-select').val();
            var topicId = $row.find('.topic-select').val();
            
            if (!subjectId) {
                toastr.error('Subject is required for all rules');
                isValid = false;
                return false;
            }
            if (!topicId) {
                toastr.error('Topic is required for all rules');
                isValid = false;
                return false;
            }
            
            var detail = {
                subjectId: parseInt(subjectId),
                topicId: parseInt(topicId),
                difficultyLevel: $row.find('.difficulty-select').val() || null,
                questionTypeId: $row.find('.type-select').val() || null,
                pickCount: parseInt($row.find('.pick-count').val()) || 1
            };
            preset.details.push(detail);
        });
        
        if (!isValid) return;

        if (preset.details.length === 0) {
            toastr.error('Add at least one rule');
            return;
        }

        var url = preset.presetId > 0 ? '/RandomizationPreset/Update' : '/RandomizationPreset/Create';

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(preset),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    bootstrap.Modal.getInstance($('#presetModal')).hide();
                    table.ajax.reload();
                } else {
                    toastr.error(res.message);
                }
            }
        });
    }

    function editPreset(id) {
        $.get('/RandomizationPreset/GetById?id=' + id, function (res) {
            if (res.success) {
                openModal(res.data);
            }
        });
    }

    function deletePreset(id) {
        if (!confirm('Delete this preset?')) return;

        $.post('/RandomizationPreset/Delete', { id: id }, function (res) {
            if (res.success) {
                toastr.success(res.message);
                table.ajax.reload();
            } else {
                toastr.error(res.message);
            }
        });
    }

    return { init: init };
})();

$(document).ready(function () {
    PresetManager.init();
});
