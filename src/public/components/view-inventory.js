const ViewInventoryStorage = {
    position: null
};

Slim.tag('view-inventory', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        this.app = window.app;
        this.isDragging = false;
        this.dragPoint = { x: 0, y:0 };

        this.capturedOnDragStop = () => this.onDragStop();
        this.capturedOnDragMouseMove = (e) => this.onDragMouseMove(e);
    }

    onCreated() {
        this.header.addEventListener('mousedown', e => {
            this.dragPoint = { x: e.clientX, y: e.clientY };
            document.addEventListener('mouseup', this.capturedOnDragStop);
            document.addEventListener('mousemove', this.capturedOnDragMouseMove);
        });

        if (ViewInventoryStorage.position !== null) {
            this.container.style.left = ViewInventoryStorage.position.left;
            this.container.style.top = ViewInventoryStorage.position.top;
        }
    }

    close() {
        this.app.callBackend('closeInventory');
    }

    onDragStop() {
        document.removeEventListener('mouseup', this.capturedOnDragStop);
        document.removeEventListener('mousemove', this.capturedOnDragMouseMove);
    }

    onDragMouseMove(e) {
        if (e.clientX < 0 || e.clientY < 0 || e.clientX > window.innerWidth || e.clientY > window.innerHeight) {
            return;
        }

        let deltaX = e.clientX - this.dragPoint.x;
        let deltaY = e.clientY - this.dragPoint.y;
        this.dragPoint = { x: e.clientX, y: e.clientY };

        let bbox = this.container.getBoundingClientRect();
        this.container.style.left = (bbox.left + deltaX) + 'px';
        this.container.style.top = (bbox.top + deltaY) + 'px';

        ViewInventoryStorage.position = { left: this.container.style.left, top: this.container.style.top };
    }

    get template() {
        return `
<div slim-id="container" style="position: absolute; left: 100px; top: 100px; z-index: 9999;" class="dialogue">
    <div slim-id="header" class="outer-header">
        <div class="header">
            <div style="display: flex; justify-content: space-between;">
                <div>Inventory</div>
                <div>
                    <i class="fa fa-window-close-o button" aria-hidden="true" click="close"></i>
                </div>
            </div>
        </div>
    </div>
    <div class="content" style="display:flex;">
        <div class="inventory list" >
            <div class="inventory item common">
                <div class="item name">
                    <span>1x</span>
                    <span>Schlüssel</span>
                </div>
                <div class="item additional monospace">0.0 kg, 0 Slots</div>
            </div> 
                   
            <div class="inventory item trash selected">
                <div class="item name">
                    <span></span>
                    <span>Flyer</span>
                </div>
                <div class="item additional monospace">0.0 kg, 0 Slots</div>
            </div>     
        </div>
        <div class="inventory description-box">
            <h1>Schlüssel</h1>
            <p>
                Ein einfacher Schlüssel nichts besonderes.            
            </p>
        </div>
    </div>
    <div class="footer">
        <div style="display: flex; justify-content: space-between;">
            <div>
                <i class="fa fa-plus-square button" aria-hidden="true"></i>
                <input type="number" value="0" min="0" />
                <i class="fa fa-minus-square button" aria-hidden="true"></i>
                <span class="button">
                    <i class="fa fa-trash-o" aria-hidden="true"></i>&nbsp;<span>Trash</span>
                </span>
            </div>
            <div class="monospace" style="display: flex; justify-content: flex-end">0 / 0 kg | 0 Slots</div>
        </div>
    </div>
</div>
`;
    }
});