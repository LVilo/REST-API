let currentDatabase="";
let currentCollection="";
let fields=[];

let editingDocumentId = null;
let originalDocument = null;

const login_input = document.getElementById("logintext");
const password_input = document.getElementById("password");

const registration = document.getElementById("registration");
const login = document.getElementById("login");
let token = "";


document
.getElementById("addFilter")
.onclick=addFilter;

document
.getElementById("search")
.onclick=search;


ShowRecords.addEventListener('click', () =>
{
    loadTree();
});

async function authFetch(url, options = {}) {
  const token = localStorage.getItem('token');
  const headers = {
  'Content-Type': 'application/json'
};
if (token) {
  headers['Authorization'] = `Bearer ${token}`;
}
  return await fetch(url, {
    ...options,
    headers: {
      ...options.headers,
      'Authorization': `Bearer ${token}`
    }
  });
}


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

function ShowTabSetting(divid,btn)
{
    document.querySelectorAll(".pageSetting").forEach(page =>{
        page.style.display = "none"
    });
    document.getElementById(divid).style.display = "block";

    document.querySelectorAll(".ButtonMenuSetting").forEach(b =>{
        b.classList.remove("active");
    })
    btn.classList.add("active");
}
async function loadTree() {

    const tree = document.getElementById("databaseTree");

    const databases = await fetch("http://nir.tik.local:32000/api/REST/v1/Collection/Tree",
        {
            method: 'GET',
            headers:
            {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        }
    )
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


async function selectCollection(db,collection){
currentDatabase = db;
currentCollection = collection;
const FieldsRequest ={
    database: db,
    collection: collection,
  }
    const response = await fetch('http://nir.tik.local:32000/api/REST/v1/Collection/Fields',{
        method: 'POST',
        headers: 
        { 
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json' 
        },
        body: JSON.stringify(FieldsRequest)
    })
    fields = await response.json();   // fields — это массив объектов
    
    document.getElementById("filterContainer").innerHTML="";

    // addFilter();
    search();
}
function addFilter() {
    const row = document.createElement("div");
    row.className = "filter-row";

    row.innerHTML = `
        <select class="field">
            ${fields.map(f => `<option value="${f.name}">${f.name}</option>`).join("")}
        </select>
        <select class="operator">
            <option value="eq">=</option>
            <option value="contains">contains</option>
            <option value="gt">></option>
            <option value="lt"><</option>
        </select>
        <input class="value" placeholder="Значение">
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


async function search() {
    const filters = [];
    document.querySelectorAll(".filter-row").forEach(r => {
        const valueInput = r.querySelector(".value");
        const IsString = fields.find(f => f.name === r.querySelector(".field").value)?.isString ?? false;
        let value = valueInput.value;

        // Если чекбокс "Строка" снят, пытаемся распарсить как JSON
        if (!IsString) {
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
            headers: { 
                'Authorization': `Bearer ${token}`,
                "Content-Type": "application/json" },
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
    ul.style.paddingLeft = '0px';
    ul.style.margin = '0px';

    if (Array.isArray(data)) {
        // Массив: каждый элемент — отдельный узел
        data.forEach((item, index) => {
            const li = document.createElement('li');
            li.innerHTML = `<span class="toggle" >[${index}]</span>`;
            const child = buildTree(item);
            child.style.marginLeft = '20px';
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

function renderTree(documents) {
    const container = document.getElementById('treeContainer');
    container.innerHTML = '';

    if (!documents || documents.length === 0) {
        container.textContent = 'Нет документов';
        return;
    }

    documents.forEach((doc, index) => {
        const docWrapper = document.createElement('div');
        docWrapper.className = 'document-wrapper';
        docWrapper.dataset.index = index;
        
        // Заголовок документа
        const docHeader = document.createElement('div');
        docHeader.className = 'document-header';
        docHeader.style.display = 'flex';
        docHeader.style.alignItems = 'center';
        docHeader.style.gap = '10px';
        docHeader.style.marginTop = '10px';
        docHeader.style.fontWeight = 'bold';

        const title = document.createElement('span');
        title.textContent = `Документ ${index + 1}`;
        docHeader.appendChild(title);

        // Кнопка редактирования
        const editBtn = document.createElement('button');
        editBtn.textContent = '✏️ Редактировать';
        editBtn.className = 'edit-btn';
        editBtn.style.marginLeft = 'auto';
        editBtn.onclick = () => toggleEditMode(doc, docWrapper, index);
        docHeader.appendChild(editBtn);

        // Кнопка сохранения (скрыта по умолчанию)
        const saveBtn = document.createElement('button');
        saveBtn.textContent = '💾 Сохранить';
        saveBtn.className = 'save-btn';
        saveBtn.style.display = 'none';
        saveBtn.style.background = '#4CAF50';
        saveBtn.style.color = 'white';
        saveBtn.style.border = 'none';
        saveBtn.style.padding = '5px 10px';
        saveBtn.style.borderRadius = '4px';
        saveBtn.style.cursor = 'pointer';
        saveBtn.onclick = () => saveDocument(docWrapper, doc);
        docHeader.appendChild(saveBtn);

        // Кнопка отмены
        const cancelBtn = document.createElement('button');
        cancelBtn.textContent = '❌ Отмена';
        cancelBtn.className = 'cancel-btn';
        cancelBtn.style.display = 'none';
        cancelBtn.style.background = '#f44336';
        cancelBtn.style.color = 'white';
        cancelBtn.style.border = 'none';
        cancelBtn.style.padding = '5px 10px';
        cancelBtn.style.borderRadius = '4px';
        cancelBtn.style.cursor = 'pointer';
        cancelBtn.onclick = () => cancelEdit(docWrapper, doc);
        docHeader.appendChild(cancelBtn);

        // Контейнер для дерева
        const treeContainer = document.createElement('div');
        treeContainer.className = 'tree-container';
        
        if (index % 2 !== 0) {
            docWrapper.style.background = '#66b4ba';
        }

        const tree = buildTree(doc);
        treeContainer.appendChild(tree);
        
        docWrapper.appendChild(docHeader);
        docWrapper.appendChild(treeContainer);
        container.appendChild(docWrapper);
    });
}
function toggleEditMode(doc, wrapper, index) {
    const isEditing = wrapper.dataset.editing === 'true';
    
    if (isEditing) {
        cancelEdit(wrapper, doc);
        return;
    }

    // Сохраняем оригинальный документ для отмены
    originalDocument = JSON.parse(JSON.stringify(doc));
    editingDocumentId = doc._id;

    // Скрываем/показываем кнопки
    const editBtn = wrapper.querySelector('.edit-btn');
    const saveBtn = wrapper.querySelector('.save-btn');
    const cancelBtn = wrapper.querySelector('.cancel-btn');
    
    editBtn.style.display = 'none';
    saveBtn.style.display = 'inline-block';
    cancelBtn.style.display = 'inline-block';

    // Делаем все текстовые узлы редактируемыми
    makeEditable(wrapper, doc);
    
    wrapper.dataset.editing = 'true';
}

function makeEditable(wrapper, doc) {
    const treeContainer = wrapper.querySelector('.tree-container');
    // Находим все листья с текстом
    const textNodes = treeContainer.querySelectorAll('li:not(:has(ul))');
    
    textNodes.forEach((node) => {
        // Проверяем, является ли это значением (не ключом)
        const text = node.textContent;
        const colonIndex = text.indexOf(':');
        
        if (colonIndex !== -1) {
            const key = text.substring(0, colonIndex).trim();
            const value = text.substring(colonIndex + 1).trim();
            
            // Пропускаем _id - его нельзя редактировать
            if (key === '_id') {
                const idSpan = document.createElement('span');
                idSpan.textContent = text;
                idSpan.style.color = '#666';
                idSpan.style.fontStyle = 'italic';
                node.innerHTML = '';
                node.appendChild(idSpan);
                return;
            }

            // Создаем поле ввода
            const input = document.createElement('input');
            input.type = 'text';
            input.value = value;
            input.className = 'edit-input';
            input.dataset.key = key;
            input.style.width = '200px';
            input.style.padding = '2px 5px';
            input.style.border = '1px solid #ccc';
            input.style.borderRadius = '3px';
            
            // Сохраняем ключ и значение
            node.innerHTML = '';
            const keySpan = document.createElement('span');
            keySpan.textContent = key + ': ';
            node.appendChild(keySpan);
            node.appendChild(input);
        }
    });

    // Обрабатываем массивы - делаем редактируемыми отдельные элементы
    const arrayItems = treeContainer.querySelectorAll('li > span.toggle + span + ul > li');
    arrayItems.forEach((item) => {
        const text = item.textContent;
        const match = text.match(/\[(\d+)\]/);
        if (match) {
            const index = match[1];
            // Создаем поле ввода для элемента массива
            const input = document.createElement('input');
            const currentValue = item.querySelector('ul') ? '' : item.textContent.trim();
            input.type = 'text';
            input.value = currentValue;
            input.className = 'edit-input-array';
            input.dataset.arrayIndex = index;
            input.style.width = '200px';
            input.style.padding = '2px 5px';
            input.style.border = '1px solid #ccc';
            input.style.borderRadius = '3px';
            
            if (!item.querySelector('ul')) {
                item.innerHTML = '';
                const indexSpan = document.createElement('span');
                indexSpan.textContent = `[${index}] `;
                item.appendChild(indexSpan);
                item.appendChild(input);
            }
        }
    });
}

async function saveDocument(wrapper, doc) {
    try {
        // Собираем измененные данные
        const updatedDoc = collectUpdatedData(wrapper, doc);
        
        // Проверяем, что _id существует
        if (!updatedDoc._id) {
            throw new Error('Не найден _id документа');
        }

        // Запрос на обновление
        const updateRequest = {
            database: currentDatabase,
            collection: currentCollection,
            filter: { _id: updatedDoc._id },
            update: updatedDoc
        };

        const response = await fetch('http://nir.tik.local:32000/api/REST/v1/Collection/Update', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(updateRequest)
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Ошибка обновления: ${errorText}`);
        }

        // Успешно обновлено
        alert('Документ успешно обновлен!');
        
        // Обновляем отображение
        const result = await response.json();
        wrapper.dataset.editing = 'false';
        
        // Перезагружаем данные
        await search();
        
    } catch (error) {
        console.error('Ошибка при сохранении:', error);
        alert(`Ошибка при сохранении: ${error.message}`);
    }
}

function collectUpdatedData(wrapper, originalDoc) {
    const updatedDoc = JSON.parse(JSON.stringify(originalDoc));
    const inputs = wrapper.querySelectorAll('.edit-input');
    
    inputs.forEach((input) => {
        const key = input.dataset.key;
        const value = input.value;
        
        // Парсим значение, если это JSON
        try {
            updatedDoc[key] = JSON.parse(value);
        } catch (e) {
            updatedDoc[key] = value;
        }
    });

    // Обрабатываем изменения в массивах
    const arrayInputs = wrapper.querySelectorAll('.edit-input-array');
    arrayInputs.forEach((input) => {
        const index = parseInt(input.dataset.arrayIndex);
        let value = input.value;
        try {
            value = JSON.parse(value);
        } catch (e) {
            // Оставляем как строку
        }
        // Находим массив и обновляем элемент
        // Это сложнее, нужно найти родительский массив
        // В упрощенном варианте можно обновить весь документ
    });

    return updatedDoc;
}

function cancelEdit(wrapper, originalDoc) {
    const editBtn = wrapper.querySelector('.edit-btn');
    const saveBtn = wrapper.querySelector('.save-btn');
    const cancelBtn = wrapper.querySelector('.cancel-btn');
    
    editBtn.style.display = 'inline-block';
    saveBtn.style.display = 'none';
    cancelBtn.style.display = 'none';
    
    // Восстанавливаем оригинальное отображение
    const treeContainer = wrapper.querySelector('.tree-container');
    const tree = buildTree(originalDoc);
    treeContainer.innerHTML = '';
    treeContainer.appendChild(tree);
    
    wrapper.dataset.editing = 'false';
    editingDocumentId = null;
    originalDocument = null;
}


login.addEventListener('click', () =>
{
  const LoginRequest =
  {
    login: login_input.value,
    password: password_input.value,
  }
  fetch('http://nir.tik.local:32000/api/REST/v1/Auth/Login',
    {
        method: 'POST',
        headers: { 
        'Content-Type': 'application/json' },
        body: JSON.stringify(LoginRequest) 
    })
  .then(response => 
  {
    // Проверяем, успешен ли запрос
    if (!response.ok)
    {
      throw new Error(`Ошибка HTTP: ${response.status}`);
    }
    return response.json(); // или response.text() для обычного текста
  })
  .then(data => {
    console.log('Полученные данные:', data);
    // Берём токен из поля token
     token = data.token;
    // console.log('Токен:', token);
    // Сохраняем или используем 
    // localStorage.setItem('token', token);
    // alert('Вход выполнен!');
})
  .catch(error => {
    console.error('Ошибка запроса:', error);
});
});