/**
 * QuestionTypeManager.js - Centralized question type management
 * Handles question type detection only (section management moved to QuestionForm)
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
        }
        
        // Note: Section management methods removed - now handled by QuestionForm
        // This eliminates the duplicate handleQuestionTypeChange calls
    };
    
    // Expose to global scope
    window.QuestionTypeManager = QuestionTypeManager;
    
})(window);