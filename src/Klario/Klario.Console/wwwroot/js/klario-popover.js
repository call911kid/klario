(function () {
    // Detects whether truncated text is visually clipped with native ellipsis (...)
    function checkTruncation(e) {
        const wrapper = e.target.closest('[data-popover-wrapper]');
        if (!wrapper) return;

        const textSpan = wrapper.querySelector('[data-popover-text]');
        if (!textSpan) return;

        // Visual truncation: scrollWidth > clientWidth indicates active text-overflow: ellipsis
        const isTruncated = (textSpan.scrollWidth - textSpan.clientWidth) > 1;
        wrapper.setAttribute('data-truncated', isTruncated ? 'true' : 'false');
    }

    document.addEventListener('mouseover', checkTruncation, { passive: true });
    document.addEventListener('focusin', checkTruncation, { passive: true });
})();
