/**
 * QuestionTypeManager.js - Centralized question type management
 * Handles question type detection and section visibility management
 */
(function(window) {
    'use strict';
    
    const QuestionTypeManager = {
        // Question type detection methods
        isTrueFalseType: function(questionType) {
            return questionType && (
                questionType.TypeName.toLowerCase().includes('true/false') || 
                questionType.TypeName.toLowerCase().includes('true false') ||
                questionType.TypeName.toLowerCase().includes('t/f')
            );
        },
        
        isDescriptiveType: function(questionType) {
            return questionType && (
                questionType.TypeName.toLowerCase().includes('descriptive') || 
                questionType.TypeName.toLowerCase().includes('subjective')
            );
        },
        
        isPairingType: function(questionType) {
            return questionType && (
                questionType.TypeName.toLowerCase().includes('matching') || 
                questionType.TypeName.toLowerCase().includes('pair') ||
                questionType.TypeName.toLowerCase().includes('match')
            );
        },
        
        isOrderingType: function(questionType) {
            return questionType && (
                questionType.TypeName.toLowerCase().includes('ordering') || 
                questionType.TypeName.toLowerCase().includes('order') ||
                questionType.TypeName.toLowerCase().includes('sequence') ||
                questionType.TypeName.toLowerCase().includes('arrange')
            );
        },
        
        isMCQType: function(questionType) {
            return questionType && (
                questionType.TypeName.toLowerCase().includes('mcq') || 
                questionType.TypeName.toLowerCase().includes('multiple choice') ||
                questionType.TypeName.toLowerCase().includes('single choice')
            );
        },
        
        // Section visibility management
        showSectionByType: function(questionType) {
            this.hideAllSections();
            
            if (this.isTrueFalseType(questionType)) {
                $('#true-false-section').show();
                $('#IsMultiSelect').prop('checked', false).prop('disabled', true);
            } else if (this.isDescriptiveType(questionType)) {
                $('#descriptive-section').show();
                $('#IsMultiSelect').prop('checked', false).prop('disabled', true);
            } else if (this.isPairingType(questionType)) {
                $('#pairing-section').show();
                $('#IsMultiSelect').prop('checked', false).prop('disabled', true);
            } else if (this.isOrderingType(questionType)) {
                $('#ordering-section').show();
                $('#IsMultiSelect').prop('checked', false).prop('disabled', true);
            } else {
                // Default to MCQ/options section
                $('#options-section').show();
                $('#IsMultiSelect').prop('disabled', false);
            }
        },
        
        hideAllSections: function() {
            $('#options-section, #true-false-section, #descriptive-section, #pairing-section, #ordering-section').hide();
        },
        
        getQuestionTypeById: function(questionTypeId) {
            if (!window.AllQuestionTypes || !questionTypeId) {
                return null;
            }
            return window.AllQuestionTypes.find(type => type.QuestionTypeId === parseInt(questionTypeId)) || null;
        },
        
        // Get default question type (MCQ if available, otherwise first type)
        getDefaultQuestionType: function() {
            if (!window.AllQuestionTypes || window.AllQuestionTypes.length === 0) {
                return null;
            }
            
            // Try to find MCQ type first
            const mcqType = window.AllQuestionTypes.find(type => this.isMCQType(type));
            if (mcqType) {
                return mcqType;
            }
            
            // Return first type if MCQ not found
            return window.AllQuestionTypes[0];
        },
        
        // Centralized question type change handler
        handleQuestionTypeChange: function(questionTypeId) {
            const questionType = this.getQuestionTypeById(questionTypeId);
            
            // Update hidden field if in edit mode
            if ($('#hiddenQuestionTypeId').length) {
                $('#hiddenQuestionTypeId').val(questionTypeId);
            }
            
            // Show appropriate section
            if (questionType) {
                this.showSectionByType(questionType);
            }
            
            return questionType;
        }
    };
    
    // Expose to global scope
    window.QuestionTypeManager = QuestionTypeManager;
    
})(window);