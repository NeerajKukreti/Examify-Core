// KaTeX Renderer for Quill Editors
(function(window) {
    'use strict';
    
    function renderMath(element) {
        if (!window.katex) return;
        
        var walker = document.createTreeWalker(
            element,
            NodeFilter.SHOW_TEXT,
            null,
            false
        );
        
        var nodesToReplace = [];
        var node;
        
        while (node = walker.nextNode()) {
            var text = node.nodeValue;
            if (text.indexOf('$') !== -1) {
                nodesToReplace.push(node);
            }
        }
        
        nodesToReplace.forEach(function(textNode) {
            var text = textNode.nodeValue;
            var parent = textNode.parentNode;
            
            // Skip if already rendered or inside katex
            if (parent.classList && (parent.classList.contains('katex') || parent.closest('.katex'))) return;
            if (parent.querySelector && parent.querySelector('.katex')) return;
            if (parent.hasAttribute && parent.hasAttribute('data-katex')) return;
            
            var parts = [];
            var lastIndex = 0;
            var regex = /\$\$([\s\S]+?)\$\$|\$([^\$\n]+?)\$/g;
            var match;
            
            while ((match = regex.exec(text)) !== null) {
                if (match.index > lastIndex) {
                    parts.push(document.createTextNode(text.substring(lastIndex, match.index)));
                }
                
                var tex = match[1] || match[2];
                var isDisplay = !!match[1];
                
                try {
                    var span = document.createElement('span');
                    span.contentEditable = 'false';
                    span.setAttribute('data-katex', 'true');
                    katex.render(tex, span, {
                        displayMode: isDisplay,
                        throwOnError: false
                    });
                    parts.push(span);
                } catch(e) {
                    parts.push(document.createTextNode(match[0]));
                }
                
                lastIndex = regex.lastIndex;
            }
            
            if (lastIndex < text.length) {
                parts.push(document.createTextNode(text.substring(lastIndex)));
            }
            
            if (parts.length > 0) {
                var fragment = document.createDocumentFragment();
                parts.forEach(function(part) {
                    fragment.appendChild(part);
                });
                parent.replaceChild(fragment, textNode);
            }
        });
    }
    
    window.renderMathInQuill = renderMath;
    
})(window);

