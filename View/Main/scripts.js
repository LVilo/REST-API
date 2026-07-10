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

    const databases = await fetch("/api/REST/v1/Collection/Tree")
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
            ul.appendChild(li);
        });

        dbLi.appendChild(ul);
        tree.appendChild(dbLi);

    });
}
let currentDatabase="";
let currentCollection="";
let fields=[];

async function selectCollection(db,collection){

const FieldsRequest ={
    database: db,
    collection: collection,
  }
    const response = await fetch('/api/REST/v1/Collection/Fields',{
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(FieldsRequest)
    })
    const fields = await response.json();   // fields — это массив строк

    document.getElementById("filterContainer").innerHTML="";

    addFilter();
}
function addFilter()
{

    const row=document.createElement("div");

    row.className="filter-row";

    row.innerHTML=`

    <select class="field">

    ${fields.map(x=>`<option>${x}</option>`).join("")}

    </select>

    <select class="operator">

    <option value="eq">=</option>

    <option value="contains">contains</option>

    <option value="gt">></option>

    <option value="lt"><</option>

    </select>

    <input class="value">

    `;

    document
        .getElementById("filterContainer")
        .appendChild(row);
}

document
.getElementById("addFilter")
.onclick=addFilter;
 async function search()
 {

    const filters=[];

    document.querySelectorAll(".filter-row")
        .forEach(r=>{

            filters.push({

                field:r.querySelector(".field").value,

                operator:r.querySelector(".operator").value,

                value:r.querySelector(".value").value

            });

        });

    const response=await fetch("/api/REST/v1/Documents",{

        method:"POST",

        headers:{

            "Content-Type":"application/json"

        },

        body:JSON.stringify({

            database:currentDatabase,

            collection:currentCollection,

            filters:filters

        })

    });

    const documents=await response.json();

    renderTable(documents);
}

document
.getElementById("search")
.onclick=search;
function renderTable(documents)
{

    const header=document.getElementById("header");

    const body=document.getElementById("body");

    header.innerHTML="";
    body.innerHTML="";

    if(documents.length===0)
        return;

    const columns=new Set();

    documents.forEach(d=>{

        Object.keys(d).forEach(k=>columns.add(k));

    });

    [...columns].forEach(c=>{

        const th=document.createElement("th");

        th.textContent=c;

        header.appendChild(th);

    });

    documents.forEach(d=>{

        const tr=document.createElement("tr");

        [...columns].forEach(c=>{

            const td=document.createElement("td");

            td.textContent=d[c] ?? "";

            tr.appendChild(td);

        });

        body.appendChild(tr);

    });

}