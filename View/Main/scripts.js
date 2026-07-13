ShowRecords.addEventListener('click', () =>
{
    loadTree();
});

function ShowTab(divid,btn)
{
    document.querySelectorAll(".page").forEach(page =>{
        page.style.display = "none"
    });
    document.getElementById(divid).style.display = "block";

    document.querySelectorAll(".ButtonMenu").forEach(b =>{
        b.classList.remove("active");
    })
    btn.classList.add("active");
}
async function loadTree() {

    const tree = document.getElementById("databaseTree");

    const databases = await fetch("http://nir.tik.local:32000/api/REST/v1/Collection/Tree")
        .then(r => r.json());

    tree.innerHTML = "";

    databases.forEach(db => {

        const dbLi = document.createElement("li");
        dbLi.textContent = "📁 " + db.databaseName;

        const ul = document.createElement("ul");

        db.collectionNames.forEach(collection => {

            const li = document.createElement("li");
            li.textContent = "📄 " + collection;

            li.onclick = () => selectCollection(db.databaseName, collection);
            li.style.cursor = 'pointer';
            li.style.paddingLeft = "0px"
            ul.appendChild(li);
        });
        ul.style.paddingLeft = "20px"
        ul.style.cursor = 'pointer';
        dbLi.appendChild(ul);
        tree.appendChild(dbLi);

    });
}
let currentDatabase="";
let currentCollection="";
let fields=[];

async function selectCollection(db,collection){
currentDatabase = db;
currentCollection = collection;
const FieldsRequest ={
    database: db,
    collection: collection,
  }
    const response = await fetch('http://nir.tik.local:32000/api/REST/v1/Collection/Fields',{
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(FieldsRequest)
    })
    fields = await response.json();   // fields — это массив строк
    
    document.getElementById("filterContainer").innerHTML="";

    // addFilter();
    search();
}
function addFilter() {
    const row = document.createElement("div");
    row.className = "filter-row";

    row.innerHTML = `
        <select class="field">
            ${fields.map(x => `<option>${x}</option>`).join("")}
        </select>
        <select class="operator">
            <option value="eq">=</option>
            <option value="contains">contains</option>
            <option value="gt">></option>
            <option value="lt"><</option>
        </select>
        <input class="value" placeholder="Значение">
        <label>
            <input type="checkbox" class="isString" checked> Строка
        </label>
        <button class="delete-filter" onclick="removeFilter(this)">✕</button>
    `;

    document.getElementById("filterContainer").appendChild(row);
}

function removeFilter(button) {
    const row = button.closest('.filter-row'); // находим родительский .filter-row
    if (row) {
        row.remove(); // удаляем строку
    }
}

document
.getElementById("addFilter")
.onclick=addFilter;
async function search() {
    const filters = [];
    document.querySelectorAll(".filter-row").forEach(r => {
        const valueInput = r.querySelector(".value");
        const isString = r.querySelector(".isString").checked;
        let value = valueInput.value;

        // Если чекбокс "Строка" снят, пытаемся распарсить как JSON
        if (!isString) {
            try {
                value = JSON.parse(value);
            } catch (e) {
                // Если не удалось распарсить, оставляем как строку
                console.warn('Не удалось распарсить значение как JSON, оставляем строкой:', value);
            }
        }

        filters.push({
            field: r.querySelector(".field").value,
            operator: r.querySelector(".operator").value,
            value: value // теперь это может быть строка, число, boolean или null
        });
    });

    try {
        const response = await fetch("http://nir.tik.local:32000/api/REST/v1/Collection/Documents", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                database: currentDatabase,
                collection: currentCollection,
                filters: filters
            })
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Ошибка HTTP ${response.status}: ${errorText}`);
        }

        const contentLength = response.headers.get('content-length');
        if (contentLength === '0' || response.status === 204) {
            renderTree([]);
            return;
        }

        let documents = await response.json();
        if (Array.isArray(documents) && documents.length > 0 && typeof documents[0] === 'string') {
            documents = documents.map(doc => JSON.parse(doc));
        }

        renderTree(documents);
    } catch (error) {
        console.error('Ошибка при выполнении запроса:', error);
        // Показать сообщение пользователю
    }
}
function buildTree(data, key = 'root') {
    const ul = document.createElement('ul');
    ul.style.listStyle = 'none';
    ul.style.paddingLeft = '20px';

    if (Array.isArray(data)) {
        // Массив: каждый элемент — отдельный узел
        data.forEach((item, index) => {
            const li = document.createElement('li');
            li.innerHTML = `<span class="toggle">[${index}]</span>`;
            const child = buildTree(item);
            li.appendChild(child);
            ul.appendChild(li);
        });
        return ul;
    }

    if (typeof data === 'object' && data !== null) {
        // Объект: перебираем ключи
        Object.entries(data).forEach(([key, value]) => {
            const li = document.createElement('li');

            // Если значение — объект или массив, делаем сворачиваемый узел
            if (value && typeof value === 'object') {
                const toggle = document.createElement('span');
                toggle.className = 'toggle';
                toggle.textContent = '▸ ';
                toggle.style.cursor = 'pointer';
                toggle.onclick = function(e) {
                    e.stopPropagation();
                    const childUl = this.parentElement.querySelector('ul');
                    if (childUl) {
                        const isCollapsed = childUl.style.display === 'none';
                        childUl.style.display = isCollapsed ? '' : 'none';
                        this.textContent = isCollapsed ? '▾ ' : '▸ ';
                    }
                };
                li.appendChild(toggle);
                // Текст ключа
                const keySpan = document.createElement('span');
                keySpan.textContent = key + ': ';
                li.appendChild(keySpan);
                // Рекурсивно строим дерево для значения
                const childTree = buildTree(value);
                li.appendChild(childTree);
            } else {
                // Простое значение (строка, число, boolean, null)
                li.textContent = `${key}: ${value === null ? 'null' : value}`;
            }
            ul.appendChild(li);
        });
        return ul;
    }

    // Примитив (если вызвана для примитива)
    const li = document.createElement('li');
    li.textContent = String(data);
    ul.appendChild(li);
    return ul;
}
document
.getElementById("search")
.onclick=search;
function renderTree(documents) {
    const container = document.getElementById('treeContainer');
    container.innerHTML = ''; // очищаем

    if (!documents || documents.length === 0) {
        container.textContent = 'Нет документов';
        return;
    }
    let i = 1;
    documents.forEach((doc, index) => {
        const docHeader = document.createElement('div');
        docHeader.textContent = `Документ ${index + 1}`;
        docHeader.style.fontWeight = 'bold';
        docHeader.style.marginTop = '10px';
        // if(i % 2 !== 0){docHeader.style.background = "#66b4ba";}
        container.appendChild(docHeader);

        const tree = buildTree(doc);
        if(i % 2 !== 0){tree.style.background = "#66b4ba";}

        container.appendChild(tree);
        i ++;
    });
}