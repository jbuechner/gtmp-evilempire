const ViewInventoryStorage = {
    position: null
};

interface IInventoryViewItem extends IItem {
    itemDescription: IItemDescription;
    displayAmount: string;
}

interface ICharInventoryEventArgs extends CustomEvent {
    detail: { Items: IItem[], Money: IItem[] };
}

Slim.tag('view-inventory', class extends Slim {
    allItems: IInventoryViewItem[];
    selectedItem?: IInventoryViewItem;

    isDragging: boolean;
    dragPoint: IVector;
    capturedOnDragStop: any;
    capturedOnDragMouseMove: any;
    header: any;
    container: any;
    itemDescriptionContent: any;

    quantity: HTMLInputElement;

    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        this.isDragging = false;
        this.dragPoint = { x: 0, y:0 };
        this.selectedItem = null;
        this.allItems = [];

        this.capturedOnDragStop = () => this.onDragStop();
        this.capturedOnDragMouseMove = (e) => this.onDragMouseMove(e);
    }

    onCreated() {
        this.header.addEventListener('mousedown', e => {
            this.dragPoint = { x: e.clientX, y: e.clientY };
            document.addEventListener('mouseup', this.capturedOnDragStop);
            document.addEventListener('mousemove', this.capturedOnDragMouseMove);
        });
        document.addEventListener('res:charInventory', (e: ICharInventoryEventArgs) => {
            let items = e.detail.Items || [];
            let money = e.detail.Money || [];

            let allItems: IInventoryViewItem[] = (items.concat(money) as IInventoryViewItem[]);
            allItems.forEach(item => {
                item.itemDescription = this.lookupItemDescription(item);
                item.Name = item.Name || item.itemDescription.Name;
                item.displayAmount = item.itemDescription.IsStackable ? item.Amount + 'x' : '';
            });

            this.allItems = allItems;
            this.selectItem(this.allItems.length > 0 ? this.allItems[0] : null);
        });

        if (ViewInventoryStorage.position !== null) {
            this.container.style.left = ViewInventoryStorage.position.left;
            this.container.style.top = ViewInventoryStorage.position.top;
        }

        App.requestCharacterInventory();
    }

    close() {
        App.callBackend('closeInventory');
    }

    lookupItemDescription(item) {
        return (window as any).itemDescriptions.get(item.ItemDescriptionId) || { Id: -1, Name: 'n/a', Description: 'n/a', Weight: 0, Volume: 0 };
    }

    selectItem(item: IInventoryViewItem): void {
        this.selectedItem = item;
        if (item && item.itemDescription) {
            this.renderSelectedItemDescription(item.itemDescription.Description);
        } else {
            this.renderSelectedItemDescription('*No item selected*');
        }
        this.quantity.value = item.Amount.toFixed(0);
        this.updateSelected();
    }

    renderSelectedItemDescription(markdown) {
        let converter = new showdown.Converter();
        let html = converter.makeHtml(markdown || '*Keine Beschreibung*');
        this.itemDescriptionContent.innerHTML = html;
        this.postProcessContentDom(this.itemDescriptionContent);
    }

    postProcessContentDom(root) {
        let links = root.querySelectorAll('a');
        for (let i = 0; i < links.length; i++) {
            let link = links.item(i);
            link.onclick = (e) => e.preventDefault();
        }
    }

    updateSelected() {
        let elements = this.querySelectorAll('[data-selected]');
        for (let i = 0; i < elements.length; i++) {
            elements.item(i).setAttribute('data-selected', 'false');
        }

        if (this.selectedItem) {
            let selector: string = '[data-item-id="' + this.selectedItem.Id + '"]';
            let selected = this.querySelectorAll(selector);
            for (let i = 0; i < selected.length; i++) {
                selected.item(i).setAttribute('data-selected', 'true');
            }
        }
    }

    onSelectItem(ev) {
        if (ev && ev.target) {
            let item = null;

            let el = ev.target;
            let itemId = null;
            do {
                let itemIdValue = el.getAttribute('data-item-id');
                if (typeof itemIdValue === 'string') {
                    itemId = itemIdValue;
                    break;
                }
                el = el.parentNode;
            } while (el && el.getAttribute);

            item = this.allItems.find(v => v.Id === itemId);
            this.selectItem(item);
        }
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

    requestDeleteSelected() {
        let selectedItem = this.selectedItem;
        if (selectedItem) {
            let raw = this.quantity.value;
            let quantity = 0;
            if (typeof raw !== 'number') {
                try {
                    quantity = Number.parseInt(raw, 10);
                } catch(ex) {
                    return;
                }
            } else {
                quantity = (raw as number);
            }

            App.requestDeleteItem(selectedItem.Id, quantity);
        }
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
        <div class="inventory list">
            <div class="inventory item common" slim-repeat="allItems" slim-repeat-as="item" data-item-id="[[item.Id]]" click="onSelectItem">
                <div class="item-name">
                    <span bind>[[item.displayAmount]]</span>
                    <span bind>[[item.Name]]</span>
                </div>
                <div class="item-additional monospace">
                    <span bind>[[item.itemDescription.Weight]]</span>
                    <span>kg</span>
                    <span>&nbsp;</span>
                    <span bind>[[item.itemDescription.Volume]]</span>
                    <span>Slots</span>
                </div>
            </div>
        </div>
        <div class="inventory description-box">
            <h1 bind>[[selectedItem.itemDescription.Name]]</h1>
            <p slim-id="itemDescriptionContent"></p>            
        </div>
    </div>
    <div class="footer">
        <div style="display: flex; justify-content: space-between;">
            <div>
                <i class="fa fa-plus-square button" aria-hidden="true"></i>
                <input slim-id="quantity" type="number" value="0" min="0" />
                <i class="fa fa-minus-square button" aria-hidden="true"></i>
                <span class="button" click="requestDeleteSelected">
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