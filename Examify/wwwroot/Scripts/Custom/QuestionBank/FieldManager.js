/**
 * FieldManager.js - Centralized field name and data management
 * Handles form field naming and data synchronization for all question types
 */
(function(window) {
    'use strict';
    
    const FieldManager = {
        // MCQ Options field management
        updateOptionFieldNames: function() {
            $('#options-wrapper .option-group').each(function(i) {
                const $group = $(this);
                
                // Update field names with proper indexing
                $group.find('.option-hidden-field').attr('name', `Options[${i}].Text`);
                $group.find('.option-correct-hidden').attr('name', `Options[${i}].IsCorrect`);
                $group.find('.option-choiceid-field').attr('name', `Options[${i}].ChoiceId`);
                
                // Update data attributes
                $group.find('.option-hidden-field').attr('data-option-index', i + 1);
                $group.find('.option-choiceid-field').attr('data-option-index', i + 1);
            });
        },
        
        // Descriptive options field management
        updateDescriptiveFieldNames: function() {
            $('#descriptive-options-wrapper .descriptive-option-group').each(function(i) {
                const $group = $(this);
                
                // Update field names
                $group.find('.option-hidden-field').attr('name', `Options[${i}].Text`);
                $group.find('.option-correct-hidden').attr('name', `Options[${i}].IsCorrect`);
                $group.find('.option-choiceid-field').attr('name', `Options[${i}].ChoiceId`);
                
                // Update labels and IDs
                $group.find('.option-label').text(`Text Input ${i + 1}`);
                $group.attr('data-option-index', i + 1);
                
                // Update error span ID
                const $error = $group.find('.option-error');
                if ($error.length && $error.attr('id')) {
                    $error.attr('id', `valDescriptiveOption${i + 1}`);
                }
            });
        },
        
        // Pairing options field management
        updatePairFieldNames: function() {
            $('#pairs-wrapper .pair-group').each(function(i) {
                const $group = $(this);
                
                // Update field names
                $group.find('.pair-left-hidden').attr('name', `Pairs[${i}].LeftText`);
                $group.find('.pair-right-hidden').attr('name', `Pairs[${i}].RightText`);
                $group.find('.pair-id-hidden').attr('name', `Pairs[${i}].PairId`);
                
                // Update data attributes and labels
                $group.attr('data-pair-index', i + 1);
                $group.find('.pair-label').text(`Pair ${i + 1}`);
                $group.find('.pair-left-hidden').attr('data-pair-index', i + 1);
                $group.find('.pair-id-hidden').attr('data-pair-index', i + 1);
                
                // Update error span IDs
                $group.find('#valPairLeft' + ($group.attr('data-pair-index') || '1')).attr('id', `valPairLeft${i + 1}`);
                $group.find('#valPairRight' + ($group.attr('data-pair-index') || '1')).attr('id', `valPairRight${i + 1}`);
            });
        },
        
        // Ordering options field management
        updateOrderFieldNames: function() {
            $('#orders-wrapper .order-group').each(function(i) {
                const $group = $(this);
                const orderIndex = i + 1;
                
                // Update field names
                $group.find('.order-item-hidden').attr('name', `Orders[${i}].ItemText`);
                $group.find('.order-id-hidden').attr('name', `Orders[${i}].OrderId`);
                $group.find('.order-correct-hidden').attr('name', `Orders[${i}].CorrectOrder`);
                
                // Update data attributes and labels
                $group.attr('data-order-index', orderIndex);
                $group.find('.order-label').text(`Item ${orderIndex}`);
                $group.find('.order-item-hidden').attr('data-order-index', orderIndex);
                $group.find('.order-id-hidden').attr('data-order-index', orderIndex);
                $group.find('.order-correct-hidden').attr('data-order-index', orderIndex);
                
                // Update correct order value based on position
                $group.find('.order-correct-hidden').val(orderIndex);
                
                // Update error span ID
                const $error = $group.find('.order-error');
                if ($error.length && $error.attr('id')) {
                    $error.attr('id', `valOrderItem${orderIndex}`);
                }
            });
        },
        
        // True/False options field management
        updateTrueFalseFieldNames: function() {
            $('.true-false-option').each(function(i) {
                const $group = $(this);
                
                // Update field names
                $group.find('.option-hidden-field').attr('name', `Options[${i}].Text`);
                $group.find('.option-correct-hidden').attr('name', `Options[${i}].IsCorrect`);
                $group.find('.option-choiceid-field').attr('name', `Options[${i}].ChoiceId`);
                
                // Update data attributes
                $group.attr('data-option-index', i + 1);
                $group.find('.option-hidden-field').attr('data-option-index', i + 1);
                $group.find('.option-choiceid-field').attr('data-option-index', i + 1);
            });
        },
        
        // Data synchronization helpers
        syncTextareaToHidden: function($textarea, $hiddenField) {
            const textValue = $textarea.val().trim();
            $hiddenField.val(textValue);
        },
        
        syncQuillToHidden: function(quillEditor, $hiddenField) {
            if (quillEditor && quillEditor.root) {
                $hiddenField.val(quillEditor.root.innerHTML);
            }
        },
        
        // Remove HTML tags to get plain text for textareas
        htmlToPlainText: function(htmlText) {
            if (!htmlText) return '';
            return htmlText.replace(/<p>/g, '').replace(/<\/p>/g, '\n').trim();
        },
        
        // Update all field names based on current question type
        updateAllFieldNames: function() {
            // Update based on visible sections
            if ($('#options-section').is(':visible')) {
                this.updateOptionFieldNames();
            }
            if ($('#descriptive-section').is(':visible')) {
                this.updateDescriptiveFieldNames();
            }
            if ($('#pairing-section').is(':visible')) {
                this.updatePairFieldNames();
            }
            if ($('#ordering-section').is(':visible')) {
                this.updateOrderFieldNames();
            }
            if ($('#true-false-section').is(':visible')) {
                this.updateTrueFalseFieldNames();
            }
        }
    };
    
    // Expose to global scope
    window.FieldManager = FieldManager;
    
})(window);