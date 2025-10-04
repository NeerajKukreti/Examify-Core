/**
 * Integration Test Script for Question Form Refactoring
 * This file can be temporarily included to test the integration
 */

$(document).ready(function() {
    console.log('=== Question Form Integration Test ===');
    
    // Test 1: Verify all modules are loaded
    console.log('1. Testing module availability:');
    console.log('   - QuestionTypeManager:', typeof window.QuestionTypeManager !== 'undefined' ? '?' : '?');
    console.log('   - FieldManager:', typeof window.FieldManager !== 'undefined' ? '?' : '?');
    console.log('   - QuestionForm:', typeof window.QuestionForm !== 'undefined' ? '?' : '?');
    
    // Test 2: Verify QuestionTypeManager functions
    if (window.QuestionTypeManager) {
        console.log('2. Testing QuestionTypeManager functions:');
        console.log('   - isTrueFalseType:', typeof window.QuestionTypeManager.isTrueFalseType === 'function' ? '?' : '?');
        console.log('   - isDescriptiveType:', typeof window.QuestionTypeManager.isDescriptiveType === 'function' ? '?' : '?');
        console.log('   - isPairingType:', typeof window.QuestionTypeManager.isPairingType === 'function' ? '?' : '?');
        console.log('   - isOrderingType:', typeof window.QuestionTypeManager.isOrderingType === 'function' ? '?' : '?');
        console.log('   - showSectionByType:', typeof window.QuestionTypeManager.showSectionByType === 'function' ? '?' : '?');
        console.log('   - getQuestionTypeById:', typeof window.QuestionTypeManager.getQuestionTypeById === 'function' ? '?' : '?');
    }
    
    // Test 3: Verify FieldManager functions
    if (window.FieldManager) {
        console.log('3. Testing FieldManager functions:');
        console.log('   - updateOptionFieldNames:', typeof window.FieldManager.updateOptionFieldNames === 'function' ? '?' : '?');
        console.log('   - updateDescriptiveFieldNames:', typeof window.FieldManager.updateDescriptiveFieldNames === 'function' ? '?' : '?');
        console.log('   - updatePairFieldNames:', typeof window.FieldManager.updatePairFieldNames === 'function' ? '?' : '?');
        console.log('   - updateOrderFieldNames:', typeof window.FieldManager.updateOrderFieldNames === 'function' ? '?' : '?');
        console.log('   - syncTextareaToHidden:', typeof window.FieldManager.syncTextareaToHidden === 'function' ? '?' : '?');
        console.log('   - syncQuillToHidden:', typeof window.FieldManager.syncQuillToHidden === 'function' ? '?' : '?');
    }
    
    // Test 4: Verify QuestionForm functions
    if (window.QuestionForm) {
        console.log('4. Testing QuestionForm functions:');
        console.log('   - initialize:', typeof window.QuestionForm.initialize === 'function' ? '?' : '?');
        console.log('   - setupEventHandlers:', typeof window.QuestionForm.setupEventHandlers === 'function' ? '?' : '?');
        console.log('   - populateEditData:', typeof window.QuestionForm.populateEditData === 'function' ? '?' : '?');
    }
    
    // Test 5: Verify legacy functions still exist
    console.log('5. Testing legacy function availability:');
    console.log('   - loadSubjects:', typeof loadSubjects === 'function' ? '?' : '?');
    console.log('   - loadQuestionTypes:', typeof loadQuestionTypes === 'function' ? '?' : '?');
    console.log('   - loadTopics:', typeof loadTopics === 'function' ? '?' : '?');
    console.log('   - QuestionOptions:', typeof window.QuestionOptions !== 'undefined' ? '?' : '?');
    
    // Test 6: Verify DOM elements exist
    console.log('6. Testing DOM elements:');
    console.log('   - #ddlSubject:', $('#ddlSubject').length > 0 ? '?' : '?');
    console.log('   - #ddlQuestionType:', $('#ddlQuestionType').length > 0 ? '?' : '?');
    console.log('   - #ddlTopic:', $('#ddlTopic').length > 0 ? '?' : '?');
    console.log('   - #options-section:', $('#options-section').length > 0 ? '?' : '?');
    console.log('   - #true-false-section:', $('#true-false-section').length > 0 ? '?' : '?');
    console.log('   - #descriptive-section:', $('#descriptive-section').length > 0 ? '?' : '?');
    console.log('   - #pairing-section:', $('#pairing-section').length > 0 ? '?' : '?');
    console.log('   - #ordering-section:', $('#ordering-section').length > 0 ? '?' : '?');
    
    // Test 7: Test question type detection (if types are loaded)
    setTimeout(function() {
        if (window.AllQuestionTypes && window.AllQuestionTypes.length > 0) {
            console.log('7. Testing question type detection with loaded types:');
            window.AllQuestionTypes.forEach(type => {
                const isTF = window.QuestionTypeManager ? window.QuestionTypeManager.isTrueFalseType(type) : false;
                const isDesc = window.QuestionTypeManager ? window.QuestionTypeManager.isDescriptiveType(type) : false;
                const isPair = window.QuestionTypeManager ? window.QuestionTypeManager.isPairingType(type) : false;
                const isOrder = window.QuestionTypeManager ? window.QuestionTypeManager.isOrderingType(type) : false;
                const isMCQ = window.QuestionTypeManager ? window.QuestionTypeManager.isMCQType(type) : false;
                
                console.log(`   - ${type.TypeName}: TF=${isTF}, Desc=${isDesc}, Pair=${isPair}, Order=${isOrder}, MCQ=${isMCQ}`);
            });
        } else {
            console.log('7. Question types not loaded yet or not available');
        }
    }, 2000);
    
    // Test 8: Verify maximum option limits are enforced
    setTimeout(function() {
        console.log('8. Testing maximum option limits:');
        
        // Test MCQ options
        const currentMCQOptions = $('#options-wrapper .option-group').length;
        console.log(`   - MCQ options currently: ${currentMCQOptions} (should be 4 or less)`);
        
        // Test if QuestionOptions respects MAX_OPTIONS
        if (window.QuestionOptions && typeof window.QuestionOptions.add === 'function') {
            try {
                // Try to add beyond limit - should show alert
                for (let i = currentMCQOptions; i < 6; i++) {
                    window.QuestionOptions.add();
                }
                const afterAdd = $('#options-wrapper .option-group').length;
                console.log(`   - MCQ options after adding: ${afterAdd} (should not exceed 4)`);
            } catch (e) {
                console.log(`   - MCQ add limit test: Error caught (expected): ${e.message}`);
            }
        }
        
        // Test descriptive options limit
        const descriptiveOptions = $('#descriptive-options-wrapper .descriptive-option-group').length;
        console.log(`   - Descriptive options: ${descriptiveOptions}`);
        
        // Test ordering items limit
        const orderingItems = $('#orders-wrapper .order-group').length;
        console.log(`   - Ordering items: ${orderingItems}`);
        
        // Test pairing items limit
        const pairingItems = $('#pairs-wrapper .pair-group').length;
        console.log(`   - Pairing items: ${pairingItems}`);
        
    }, 3000);
    
    // Test 9: Test edit mode population (simulate editing a question with multiple options)
    setTimeout(function() {
        console.log('9. Testing edit mode population (no alerts should appear):');
        
        // Test simulating a question with 5 descriptive options (should only load 4)
        const testDescriptiveData = {
            Options: [
                { Text: '<p>Option 1</p>', ChoiceId: 1 },
                { Text: '<p>Option 2</p>', ChoiceId: 2 },
                { Text: '<p>Option 3</p>', ChoiceId: 3 },
                { Text: '<p>Option 4</p>', ChoiceId: 4 },
                { Text: '<p>Option 5</p>', ChoiceId: 5 } // This should be ignored
            ]
        };
        
        // Test simulating a question with 5 pairs (should load all since limit is 10)
        const testPairingData = {
            Pairs: [
                { LeftText: '<p>Left 1</p>', RightText: '<p>Right 1</p>', PairId: 1 },
                { LeftText: '<p>Left 2</p>', RightText: '<p>Right 2</p>', PairId: 2 },
                { LeftText: '<p>Left 3</p>', RightText: '<p>Right 3</p>', PairId: 3 },
                { LeftText: '<p>Left 4</p>', RightText: '<p>Right 4</p>', PairId: 4 },
                { LeftText: '<p>Left 5</p>', RightText: '<p>Right 5</p>', PairId: 5 }
            ]
        };
        
        // Test simulating a question with 5 ordering items (should only load 4)
        const testOrderingData = {
            Orders: [
                { ItemText: '<p>Item 1</p>', OrderId: 1 },
                { ItemText: '<p>Item 2</p>', OrderId: 2 },
                { ItemText: '<p>Item 3</p>', OrderId: 3 },
                { ItemText: '<p>Item 4</p>', OrderId: 4 },
                { ItemText: '<p>Item 5</p>', OrderId: 5 } // This should be ignored
            ]
        };
        
        if (window.QuestionForm) {
            console.log('   - Testing descriptive population...');
            window.QuestionForm.populateDescriptiveData(testDescriptiveData);
            console.log(`   - Descriptive options created: ${$('#descriptive-options-wrapper .descriptive-option-group').length} (expected: 4)`);
            
            console.log('   - Testing pairing population...');
            window.QuestionForm.populatePairingData(testPairingData);
            console.log(`   - Pairing options created: ${$('#pairs-wrapper .pair-group').length} (expected: 5)`);
            
            console.log('   - Testing ordering population...');
            window.QuestionForm.populateOrderingData(testOrderingData);
            console.log(`   - Ordering items created: ${$('#orders-wrapper .order-group').length} (expected: 4)`);
        }
        
        console.log('   - If no alerts appeared above, the fix is working correctly!');
        
    }, 4000);
    
    console.log('=== Integration Test Complete ===');
});