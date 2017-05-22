Slim.tag('core-watermark', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return false; }

    get template() {
        return `<div class="fg-white" style="position: absolute; top: 5px; left: 5px; opacity: 0.8; font-size: 9px;">GTA Evil Empire</div>`;
    }
});