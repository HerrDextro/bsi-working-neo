let calls = [
    {
        id: 1748875, 
        topic: "Topic1",
        participants: 5,
        activity: 1
    },
    {
        id: 2478784, 
        topic: "Topic2",
        participants: 10,
        activity: 1
    },
    {
        id: 38589348, 
        topic: "Topic3",
        participants: 2,
        activity: 1
    }
]

let placedCircles = [];
const createClusterBtn = document.getElementById("createCluster");
const submitClusterName = document.getElementById("submitNameBtn");

function createCallList(list){
    const parent = document.getElementById("callArea");
    parent.innerHTML = "";

    list.forEach(element => {
        const div = document.createElement("div");
        
        div.className = "element";
        div.textContent = element.topic;

        parent.appendChild(div);
    })
}

function createAllCluster(list) {
    const parent = document.getElementById("clusterArea");
    parent.innerHTML = "";
    placedCircles = []; 

    list.forEach(element => {
        createCluster(element, parent);
    });
}

function getCenterFromGuid(element, parent) {
    const pRect = parent.getBoundingClientRect();

    const seed = hashStringToInt(element.id ? element.id.toString() : element.topic);
    
    const margin = 80;
    
    const x = (seed % (pRect.width - margin * 2)) + margin;
    const y = ((seed >> 5) % (pRect.height - margin * 2)) + margin;
    
    return { x, y };
}

createClusterBtn.addEventListener("click", () => {
    const overlay = document.querySelector(".overlay");
    overlay.style.display = "flex";
})

submitClusterName.addEventListener("click", () => {
    const name = document.getElementById("nameInput").value;
    const parent = document.getElementById("clusterArea");

    element = {
        id: 8989898,
        topic: name,
        participants: 1,
        activity: 1
    }

    createCluster(element, parent);

    const overlay = document.querySelector(".overlay");
    overlay.style.display = "none";
})

function createCluster(element, parent) {
const template = document.getElementById("clusterTemplate");
    if (!template) return;

    const center = getCenterFromGuid(element, parent);
    
    let circleRadius = element.participants * 5 + 20; 

    const clone = template.content.cloneNode(true);
    const wrapper = clone.querySelector('.cluster-wrapper');
    const circle = clone.querySelector('.cluster');
    const title = clone.querySelector('.clusterTitle');

    title.textContent = element.topic;
    circle.textContent = element.participants;
    
    circle.style.height = (circleRadius * 2) + "px";
    circle.style.width = (circleRadius * 2) + "px";
    
    wrapper.style.position = "absolute";

    wrapper.style.left = `${center.x - circleRadius}px`;
    wrapper.style.top = `${center.y - circleRadius}px`;
    
    parent.appendChild(clone);

    placedCircles.push({ 
        x: center.x, 
        y: center.y, 
        r: circleRadius,
        id: element.id
    });

    circle.addEventListener("click", () => {
        
    })

}

window.addEventListener('DOMContentLoaded', () => {
    createCallList(calls);
    createAllCluster(calls);
});

function updateParticipantCount(id, newCount) {
    const parent = document.getElementById("clusterArea");

    const wrapper = document.querySelector(`.cluster-wrapper[data-id="${id}"]`);
    if (!wrapper) return;

    const circle = wrapper.querySelector('.cluster');
    const newRadius = newCount * 5 + 20;

    const center = getCenterFromGuid({ id: id }, parent);

    circle.textContent = newCount;
    circle.style.width = `${newRadius * 2}px`;
    circle.style.height = `${newRadius * 2}px`;

    wrapper.style.left = `${center.x - newRadius}px`;
    wrapper.style.top = `${center.y - newRadius}px`;
}


function hashStringToInt(str) {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    return Math.abs(hash);
}