/**
 * QuestionForm.js - Main form initialization and coordination
 * Orchestrates the question creation/editing form functionality
 */
(function(window) {
    'use strict';
    
    const QuestionForm = {
        // Form initialization
        initialize: function(modelData) {
            this.modelData = modelData;
            this.isInitializing = true; // Flag to prevent unnecessary triggers during init
            this.setupEventHandlers();
            this.loadInitialData();
            
            if (modelData && modelData.QuestionId > 0) {
                // Defer population until after types are loaded
                $(document).on('questionTypesLoaded', () => {
                    this.populateEditData(modelData);
                });
            } else {
                // Set default question type for new questions
                $(document).on('questionTypesLoaded', () => {
                    this.setDefaultQuestionType();
                });
            }
        },
        
        // Setup centralized event handlers
        setupEventHandlers: function() {
            // Remove any existing handlers first to prevent duplicates
            $(document).off('change.questionType', '#ddlQuestionType');
            $(document).off('change.tfOption', '.tf-option');
            $(document).off('click.panelToggle', '.panel-header');
            $(document).off('click.cancelBtn', '#btnCancel');
            $(document).off('change.subjectChange', '#ddlSubject');
            
            // Question type change handler with debouncing
            $(document).on('change.questionType', '#ddlQuestionType', this.debounce((e) => {
                const questionTypeId = parseInt($(e.target).val());
                if (questionTypeId && !this.isInitializing) {
                    this.handleQuestionTypeChange(questionTypeId);
                }
            }, 100));
            
            // True/False checkbox behavior
            $(document).on('change.tfOption', '.tf-option', this.handleTrueFalseChange.bind(this));
            
            // Panel toggle functionality
            $(document).on('click.panelToggle', '.panel-header', this.handlePanelToggle);
            
            // Cancel button handler
            $(document).on('click.cancelBtn', '#btnCancel', this.handleCancel.bind(this));
            
            // Subject change handler
            $(document).on('change.subjectChange', '#ddlSubject', (e) => {
                const subjectId = $(e.target).val();
                if (typeof loadTopics === 'function') {
                    loadTopics(subjectId);
                }
            });
            
            // Event listeners for when data is loaded
            $(document).on('subjectsLoaded', this.onSubjectsLoaded.bind(this));
            $(document).on('questionTypesLoaded', this.onQuestionTypesLoaded.bind(this));
        },
        
        // Debounce utility to prevent rapid successive calls
        debounce: function(func, wait) {
            let timeout;
            return function executedFunction(...args) {
                const later = () => {
                    clearTimeout(timeout);
                    func(...args);
                };
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
            };
        },
        
        // Centralized question type change handler (no longer calls QuestionTypeManager directly)
        handleQuestionTypeChange: function (questionTypeId) {
            const questionType = window.QuestionTypeManager ? 
                window.QuestionTypeManager.getQuestionTypeById(questionTypeId) : null;
            
            if (!questionType) return;
            
            // Update hidden field if in edit mode
            if ($('#hiddenQuestionTypeId').length) {
                $('#hiddenQuestionTypeId').val(questionTypeId);
            }
            
            // Show appropriate section directly (bypass QuestionTypeManager to avoid double processing)
            this.showSectionByType(questionType);
            
            console.log('Question type changed to:', questionType.TypeName);
        },
        
        // Direct section management (moved from QuestionTypeManager to reduce calls)
        showSectionByType: function(questionType) {
            this.hideAllSections();
            
            if (window.QuestionTypeManager.isTrueFalseType(questionType)) {
                $('#true-false-section').show();
                $('#IsMultiSelect').prop('checked', false).prop('disabled', true);
                $('.multi-select-container').hide();
            } else if (window.QuestionTypeManager.isDescriptiveType(questionType)) {
                $('#descriptive-section').show();
                $('#IsMultiSelect').prop('checked', false).prop('disabled', true);
                $('.multi-select-container').hide();
            } else if (window.QuestionTypeManager.isPairingType(questionType)) {
                $('#pairing-section').show();
                $('#IsMultiSelect').prop('checked', false).prop('disabled', true);
                $('.multi-select-container').hide();
            } else if (window.QuestionTypeManager.isOrderingType(questionType)) {
                $('#ordering-section').show();
                $('#IsMultiSelect').prop('checked', false).prop('disabled', true);
                $('.multi-select-container').hide();
            } else {
                // Default to MCQ/options section
                $('#options-section').show();
                $('#IsMultiSelect').prop('disabled', false);
                $('.multi-select-container').show();
            }
        },
        
        hideAllSections: function() {
            $('#options-section, #true-false-section, #descriptive-section, #pairing-section, #ordering-section').hide();
            $('.multi-select-container').hide();
        },
        
        // Load initial data
        loadInitialData: function() {
            // These functions are defined in question.js
            if (typeof loadSubjects === 'function') {
                loadSubjects();
            }
            if (typeof loadQuestionTypes === 'function') {
                loadQuestionTypes();
            }
        },
        
        // Handle True/False checkbox changes
        handleTrueFalseChange: function (e) {
            debugger;
            const $checkbox = $(e.target);
            if ($checkbox.is(':checked')) {
                // Uncheck other True/False options
                $('.tf-option').not($checkbox).prop('checked', false);
                
                // Update hidden field values
                $('.true-false-option').each(function(i) {
                    const isChecked = $(this).find('.tf-option').is(':checked');
                    $(this).find('.option-correct-hidden').val(isChecked ? 'true' : 'false');
                });
                
                // Update field names
                if (window.FieldManager) {
                    window.FieldManager.updateTrueFalseFieldNames();
                }
            }
        },
        
        // Handle panel toggle
        handlePanelToggle: function(e) {
            const $btn = $(this).find('.toggle-btn');
            const $panelBody = $btn.closest('.editor-panel').find('.panel-body');
            
            $panelBody.slideToggle(200);
            $btn.text($btn.text() === '−' ? '+' : '−');
        },
        
        // Handle cancel button click
        handleCancel: function(e) {
            e.preventDefault();
            
            // Check if this is being opened in a modal
            if ($('#questionModel').length && $('#questionModel').hasClass('show')) {
                // Close modal without saving
                $('#questionModel').modal('hide');
                
                // Show appropriate message
                if (typeof toastr !== 'undefined') {
                    toastr.info('Question creation cancelled', 'Info');
                }
            } else {
                // If not in modal, could redirect back or close tab/window
                // For now, show a confirmation and optionally redirect
                if (confirm('Are you sure you want to cancel? Any unsaved changes will be lost.')) {
                    // Option 1: Go back in history
                    window.history.back();
                    
                    // Option 2: Redirect to a specific page (uncomment if needed)
                    // window.location.href = '/Question/Index'; // or wherever questions are listed
                    
                    // Show cancelled message
                    if (typeof toastr !== 'undefined') {
                        toastr.info('Question creation cancelled', 'Info');
                    }
                }
            }
        },
        
        // Set default question type for new questions (without triggering change event)
        setDefaultQuestionType: function() {
            if (!this.modelData || !this.modelData.QuestionId || this.modelData.QuestionId === 0) {
                const defaultType = window.QuestionTypeManager ? 
                    window.QuestionTypeManager.getDefaultQuestionType() : null;
                if (defaultType) {
                    // Set value directly without triggering change event
                    $('#ddlQuestionType').val(defaultType.QuestionTypeId);
                    // Handle the change manually
                    this.handleQuestionTypeChange(defaultType.QuestionTypeId);
                }
            }
            // Mark initialization as complete
            setTimeout(() => {
                this.isInitializing = false;
            }, 200);
        },
        
        // Handle subjects loaded event
        onSubjectsLoaded: function() {
            if (this.modelData && this.modelData.SubjectId) {
                $('#ddlSubject').val(this.modelData.SubjectId);
                // Load topics for the selected subject
                if (typeof loadTopics === 'function') {
                    loadTopics(this.modelData.SubjectId, () => {
                        $('#ddlTopic').val(this.modelData.TopicId);
                    });
                }
            }
        },
        
        // Handle question types loaded event (optimized to avoid unnecessary triggers)
        onQuestionTypesLoaded: function() {
            if (this.modelData && this.modelData.QuestionId > 0) {
                // Editing mode - set value directly without triggering change
                $('#ddlQuestionType').val(this.modelData.QuestionTypeId);
                // Handle the change manually
                const questionTypeId = this.modelData.QuestionTypeId;
                if (questionTypeId) {
                    this.handleQuestionTypeChange(questionTypeId);
                }
            } else {
                // New question mode
                this.setDefaultQuestionType();
            }
        },
        
        // Populate form with edit data
        populateEditData: function(modelData) {
            const questionType = window.QuestionTypeManager ? 
                window.QuestionTypeManager.getQuestionTypeById(modelData.QuestionTypeId) : null;
            
            // Populate basic form fields
            this.populateBasicFields(modelData);
            
            // Populate Quill editors
            this.populateQuillEditors(modelData);
            
            // Populate question type specific data
            if (questionType && window.QuestionTypeManager.isTrueFalseType(questionType)) {
                this.populateTrueFalseData(modelData);
            } else if (questionType && window.QuestionTypeManager.isDescriptiveType(questionType)) {
                this.populateDescriptiveData(modelData);
            } else if (questionType && window.QuestionTypeManager.isPairingType(questionType)) {
                this.populatePairingData(modelData);
            } else if (questionType && window.QuestionTypeManager.isOrderingType(questionType)) {
                this.populateOrderingData(modelData);
            } else {
                this.populateMCQData(modelData);
            }
        },
        
        // Populate basic form fields
        populateBasicFields: function(modelData) {
            $('#ddlSubject').val(modelData.SubjectId || '');
            $('#ddlTopic').val(modelData.TopicId || '');
            $('#IsMultiSelect').prop('checked', !!modelData.IsMultiSelect);
            $('textarea[asp-for="Explanation"]').val(modelData.Explanation || '');
        },
        
        // Populate Quill editors
        populateQuillEditors: function(modelData) {
            if (!window.__quillEditors) return;
            
            const editorMappings = {
                'editor-question-english': modelData.QuestionEnglish,
                'editor-question-hindi': modelData.QuestionHindi,
                'editor-additional-english': modelData.AdditionalTextEnglish,
                'editor-additional-hindi': modelData.AdditionalTextHindi
            };
            
            Object.keys(editorMappings).forEach(editorId => {
                const editor = window.__quillEditors[editorId];
                const content = editorMappings[editorId];
                if (editor && content) {
                    editor.root.innerHTML = content;
                }
            });
        },
        
        // Populate True/False question data
        populateTrueFalseData: function (modelData) {
            if (!modelData.Options || modelData.Options.length === 0) return;

            // First, uncheck all options
            $('.tf-option').prop('checked', false);

            let trueOption = null;
            let falseOption = null;

            // Find True and False options
            modelData.Options.forEach(opt => {
                const optText = (opt.Text || '').toLowerCase();
                if (optText.includes('true')) {
                    trueOption = opt;
                } else if (optText.includes('false')) {
                    falseOption = opt;
                }
            });

            // Set ChoiceIds
            if (trueOption) {
                $('.true-false-option').eq(0).find('.option-choiceid-field').val(trueOption.ChoiceId || '');
            }
            if (falseOption) {
                $('.true-false-option').eq(1).find('.option-choiceid-field').val(falseOption.ChoiceId || '');
            }

            // Check only the correct option
            if (trueOption && trueOption.IsCorrect) {
                $('.tf-option[data-option-value="true"]').prop('checked', true);
            } else if (falseOption && falseOption.IsCorrect) {
                $('.tf-option[data-option-value="false"]').prop('checked', true);
            }

            // Update hidden values
            $('.true-false-option').each(function () {
                const isChecked = $(this).find('.tf-option').is(':checked');
                $(this).find('.option-correct-hidden').val(isChecked ? 'true' : 'false');
            });
        },
        
        // Populate MCQ data
        populateMCQData: function(modelData) {
            if (!modelData.Options || modelData.Options.length === 0) return;
            
            // Reset MCQ options to default 4 if needed
            if (window.QuestionOptions && typeof window.QuestionOptions.resetOptions === 'function') {
                window.QuestionOptions.resetOptions();
            }
            
            // Enforce maximum of 4 options for editing (to match original behavior)
            const maxAllowedOptions = 4;
            const optionsToProcess = modelData.Options.slice(0, maxAllowedOptions);
            
            // Only add extra options if we have fewer than the required options (up to 4 max)
            const currentOptionCount = $('#options-wrapper .option-group').length;
            const neededOptions = Math.min(optionsToProcess.length, maxAllowedOptions);
            
            // Add additional option groups only if we need more than the current count
            if (neededOptions > currentOptionCount) {
                const optionsToAdd = neededOptions - currentOptionCount;
                for (let i = 0; i < optionsToAdd; i++) {
                    if (window.QuestionOptions && typeof window.QuestionOptions.add === 'function') {
                        window.QuestionOptions.add();
                    }
                }
            }
            
            // Populate existing options (maximum 4)
            $('#options-wrapper .option-group').each(function(i) {
                const opt = optionsToProcess[i];
                if (opt && i < maxAllowedOptions) {
                    const editorId = $(this).find('.editor-container').attr('id');
                    if (window.__quillEditors && window.__quillEditors[editorId]) {
                        window.__quillEditors[editorId].root.innerHTML = opt.Text || '';
                    }
                    $(this).find('.option-hidden-field').val(opt.Text || '');
                    $(this).find('input[type="checkbox"]').prop('checked', !!opt.IsCorrect);
                    $(this).find('.option-correct-hidden').val(opt.IsCorrect ? 'true' : 'false');
                    $(this).find('.option-choiceid-field').val(opt.ChoiceId || '');
                } else {
                    // Clear unused options
                    const editorId = $(this).find('.editor-container').attr('id');
                    if (window.__quillEditors && window.__quillEditors[editorId]) {
                        window.__quillEditors[editorId].root.innerHTML = '';
                    }
                    $(this).find('.option-hidden-field').val('');
                    $(this).find('input[type="checkbox"]').prop('checked', false);
                    $(this).find('.option-correct-hidden').val('false');
                    $(this).find('.option-choiceid-field').val('');
                }
            });
            
            // Log warning if more than 4 options were provided
            if (modelData.Options.length > maxAllowedOptions) {
                console.warn(`Question has ${modelData.Options.length} options, but only ${maxAllowedOptions} will be loaded for editing.`);
            }
        },
        
        // Populate descriptive data
        populateDescriptiveData: function(modelData) {
            if (!modelData.Options || modelData.Options.length === 0) return;
            
            // CLEAR existing descriptive options except the first one before populating
            $('#descriptive-options-wrapper .descriptive-option-group:not(:first)').remove();
            
            // Enforce maximum of 4 options for descriptive questions (to match MAX_DESCRIPTIVE_OPTIONS)
            const maxAllowedOptions = MAX_DESCRIPTIVE_OPTIONS;
            const optionsToProcess = modelData.Options.slice(0, maxAllowedOptions);
            
            optionsToProcess.forEach((opt, i) => {
                if (i >= maxAllowedOptions) return; // Skip if beyond limit
                
                let $optionGroup;
                
                if (i === 0) {
                    // Use the existing first descriptive option
                    $optionGroup = $('#descriptive-options-wrapper .descriptive-option-group').first();
                } else {
                    // Create new descriptive option directly (bypass button click to avoid alerts)
                    $optionGroup = this.createDescriptiveOption(i + 1);
                }
                
                if ($optionGroup.length) {
                    const plainText = window.FieldManager.htmlToPlainText(opt.Text || '');
                    
                    $optionGroup.find('.descriptive-input').val(plainText);
                    $optionGroup.find('.option-hidden-field').val(opt.Text || '');
                    $optionGroup.find('.option-choiceid-field').val(opt.ChoiceId || '');
                    
                    // Trigger input event to sync hidden field
                    $optionGroup.find('.descriptive-input').trigger('input');
                }
            });
            
            // Update global counter to match actual count
            if (typeof descriptiveOptionCount !== 'undefined') {
                window.descriptiveOptionCount = optionsToProcess.length;
            }
            
            // Log warning if more than 4 options were provided
            if (modelData.Options.length > maxAllowedOptions) {
                console.warn(`Question has ${modelData.Options.length} descriptive options, but only ${maxAllowedOptions} will be loaded for editing.`);
            }
        },
        
        // Create descriptive option without triggering button click
        createDescriptiveOption: function(idx) {
            const html = `
                <div class="form-group descriptive-option-group" data-option-index="${idx}">
                    <div class="option-label">Text Input ${idx}</div>
                    <div class="row">
                        <div class="col-md-10">
                            <textarea class="form-control descriptive-input" rows="3" placeholder="Enter text..."></textarea>
                            <input type="hidden" class="option-hidden-field" data-option-index="${idx}" name="Options[${idx-1}].Text" />
                            <input type="hidden" class="option-choiceid-field" data-option-index="${idx}" name="Options[${idx-1}].ChoiceId" />
                            <input type="hidden" class="option-correct-hidden" name="Options[${idx-1}].IsCorrect" value="true" />
                            <span class="text-danger option-error" id="valDescriptiveOption${idx}"></span>
                        </div>
                        <div class="col-md-2">
                            <button type="button" class="btn btn-danger btn-xs btn-remove-descriptive-option mt-2" title="Remove Option">
                                <i class="glyphicon glyphicon-trash"></i> Remove
                            </button>
                        </div>
                    </div>
                </div>
            `;
            
            const $newOption = $(html);
            $('#descriptive-options-wrapper').append($newOption);
            
            // Update field names
            if (window.FieldManager) {
                window.FieldManager.updateDescriptiveFieldNames();
            }
            
            // Bind input handlers for the new textarea
            if (typeof bindDescriptiveInputHandler === 'function') {
                bindDescriptiveInputHandler();
            }
            
            return $newOption;
        },
        
        // Populate pairing data
        populatePairingData: function(modelData) {
            if (!modelData.Pairs || modelData.Pairs.length === 0) return;
            
            // CLEAR existing pairing options except the first one before populating
            $('#pairs-wrapper .pair-group:not(:first)').remove();
            
            // Use the defined MAX_PAIRS limit (currently 10)
            const maxAllowedPairs = MAX_PAIRS; // This matches MAX_PAIRS in question.js
            const pairsToProcess = modelData.Pairs.slice(0, maxAllowedPairs);
            
            pairsToProcess.forEach((pair, i) => {
                if (i >= maxAllowedPairs) return; // Skip if beyond limit
                
                let $pairGroup;
                
                if (i === 0) {
                    // Use the existing first pair
                    $pairGroup = $('#pairs-wrapper .pair-group').first();
                } else {
                    // Create new pair directly (bypass button click to avoid alerts)
                    $pairGroup = this.createPairOption(i + 1);
                }
                
                if ($pairGroup.length) {
                    const leftText = window.FieldManager.htmlToPlainText(pair.LeftText || '');
                    const rightText = window.FieldManager.htmlToPlainText(pair.RightText || '');
                    
                    $pairGroup.find('.pair-left-input').val(leftText);
                    $pairGroup.find('.pair-left-hidden').val(pair.LeftText || '');
                    $pairGroup.find('.pair-right-input').val(rightText);
                    $pairGroup.find('.pair-right-hidden').val(pair.RightText || '');
                    $pairGroup.find('.pair-id-hidden').val(pair.PairId || '');
                    
                    // Trigger input events to sync hidden fields
                    $pairGroup.find('.pair-left-input').trigger('input');
                    $pairGroup.find('.pair-right-input').trigger('input');
                }
            });
            
            // Update global counter to match actual count
            if (typeof pairCount !== 'undefined') {
                window.pairCount = pairsToProcess.length;
            }
            
            // Log warning if more than max pairs were provided
            if (modelData.Pairs.length > maxAllowedPairs) {
                console.warn(`Question has ${modelData.Pairs.length} pairs, but only ${maxAllowedPairs} will be loaded for editing.`);
            }
        },
        
        // Create pair option without triggering button click
        createPairOption: function(idx) {
            const html = `
                <div class="form-group pair-group" data-pair-index="${idx}">
                    <div class="pair-label">Pair ${idx}</div>
                    <div class="row">
                        <div class="col-md-5">
                            <div class="form-group">
                                <label>Left Item</label>
                                <textarea class="form-control pair-left-input" rows="2" placeholder="Enter left item..."></textarea>
                                <input type="hidden" class="pair-left-hidden" data-pair-index="${idx}" name="Pairs[${idx-1}].LeftText" />
                                <input type="hidden" class="pair-id-hidden" data-pair-index="${idx}" name="Pairs[${idx-1}].PairId" />
                                <span class="text-danger pair-error" id="valPairLeft${idx}"></span>
                            </div>
                        </div>
                        <div class="col-md-5">
                            <div class="form-group">
                                <label>Right Item</label>
                                <textarea class="form-control pair-right-input" rows="2" placeholder="Enter right item..."></textarea>
                                <input type="hidden" class="pair-right-hidden" data-pair-index="${idx}" name="Pairs[${idx-1}].RightText" />
                                <span class="text-danger pair-error" id="valPairRight${idx}"></span>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <button type="button" class="btn btn-danger btn-xs btn-remove-pair mt-4" title="Remove Pair">
                                <i class="glyphicon glyphicon-trash"></i> Remove
                            </button>
                        </div>
                    </div>
                </div>
            `;
            
            const $newPair = $(html);
            $('#pairs-wrapper').append($newPair);
            
            // Update field names
            if (window.FieldManager) {
                window.FieldManager.updatePairFieldNames();
            }
            
            // Bind input handlers for the new textareas
            if (typeof bindPairInputHandlers === 'function') {
                bindPairInputHandlers();
            }
            
            return $newPair;
        },
        
        // Populate ordering data
        populateOrderingData: function(modelData) {
            if (!modelData.Orders || modelData.Orders.length === 0) return;
            
            // CLEAR existing ordering items except the first one before populating
            $('#orders-wrapper .order-group:not(:first)').remove();
            
            // Enforce maximum of 4 items for ordering questions (to match MAX_ORDER_ITEMS)
            const maxAllowedItems = MAX_ORDER_ITEMS;
            const itemsToProcess = modelData.Orders.slice(0, maxAllowedItems);
            
            itemsToProcess.forEach((orderItem, i) => {
                if (i >= maxAllowedItems) return; // Skip if beyond limit
                
                let $orderGroup;
                
                if (i === 0) {
                    // Use the existing first order item
                    $orderGroup = $('#orders-wrapper .order-group').first();
                } else {
                    // Create new order item directly (bypass button click to avoid alerts)
                    $orderGroup = this.createOrderItem(i + 1);
                }
                
                if ($orderGroup.length) {
                    const plainText = window.FieldManager.htmlToPlainText(orderItem.ItemText || '');
                    
                    $orderGroup.find('.order-item-input').val(plainText);
                    $orderGroup.find('.order-item-hidden').val(orderItem.ItemText || '');
                    $orderGroup.find('.order-id-hidden').val(orderItem.OrderId || '');
                    $orderGroup.find('.order-correct-hidden').val(i + 1);
                    
                    // Trigger input event to sync hidden field
                    $orderGroup.find('.order-item-input').trigger('input');
                }
            });
            
            // Update global counter to match actual count
            if (typeof orderItemCount !== 'undefined') {
                window.orderItemCount = itemsToProcess.length;
            }
            
            // Log warning if more than 4 items were provided
            if (modelData.Orders.length > maxAllowedItems) {
                console.warn(`Question has ${modelData.Orders.length} ordering items, but only ${maxAllowedItems} will be loaded for editing.`);
            }
        },
        
        // Create order item without triggering button click
        createOrderItem: function(idx) {
            const html = `
                <div class="form-group order-group" data-order-index="${idx}">
                    <div class="order-label">Item ${idx}</div>
                    <div class="row">
                        <div class="col-md-10">
                            <div class="form-group">
                                <textarea class="form-control order-item-input" rows="2" placeholder="Enter item text..."></textarea>
                                <input type="hidden" class="order-item-hidden" data-order-index="${idx}" name="Orders[${idx-1}].ItemText" />
                                <input type="hidden" class="order-id-hidden" data-order-index="${idx}" name="Orders[${idx-1}].OrderId" />
                                <input type="hidden" class="order-correct-hidden" data-order-index="${idx}" name="Orders[${idx-1}].CorrectOrder" value="${idx}" />
                                <span class="text-danger order-error" id="valOrderItem${idx}"></span>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <button type="button" class="btn btn-danger btn-xs btn-remove-order-item mt-2" title="Remove Item">
                                <i class="glyphicon glyphicon-trash"></i> Remove
                            </button>
                        </div>
                    </div>
                </div>
            `;
            
            const $newItem = $(html);
            $('#orders-wrapper').append($newItem);
            
            // Update field names
            if (window.FieldManager) {
                window.FieldManager.updateOrderFieldNames();
            }
            
            // Bind input handlers for the new textarea
            if (typeof bindOrderItemInputHandlers === 'function') {
                bindOrderItemInputHandlers();
            }
            
            return $newItem;
        }
    };
    
    // Expose to global scope
    window.QuestionForm = QuestionForm;
    
})(window);