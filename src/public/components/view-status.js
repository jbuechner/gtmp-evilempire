Slim.tag('view-status', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return false; }

    get template() {
        return `<div class="bg-teal lighten-1 fg-white" style="position: absolute; bottom: 0; left: 0; right: 0; height: 21px; font-size: 12px; padding: 3px;">
    
</div>`;
    }
});