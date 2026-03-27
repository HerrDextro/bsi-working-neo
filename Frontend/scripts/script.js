let calls = [
    "Topic1",
    "Topic2",
    "Topic3",
    "Topic4",
    "Topic5"
]

const circleRadius = 50; 
let placedCircles = [];

function createCallList(list){
    const parent = document.getElementById("callArea");
    parent.innerHTML = "";

    list.forEach(element => {
        const div = document.createElement("div");
        
        div.className = "element";
        div.textContent = element;

        parent.appendChild(div);
    })
}

function createCluster(list){
    const parent = document.getElementById("clusterArea");
    parent.style.position = "relative";
    parent.innerHTML = "";

    placedCircles = [];

    list.forEach(element => {
        const div = document.createElement("div");
        div.className = "cluster";

        let position = findSafePosition(parent);
        
        if (position) {
            div.style.left = `${position.x}px`;
            div.style.top = `${position.y}px`;
            parent.appendChild(div);
            placedCircles.push(position);
        }
    });
}

function findSafePosition(parent) {
    const pRect = parent.getBoundingClientRect();
    let maxAttempts = 100;

    while (maxAttempts > 0) {
        let x = Math.random() * (pRect.width - circleRadius * 2);
        let y = Math.random() * (pRect.height - circleRadius * 2);

        let overlapping = placedCircles.some(other => {
            let dx = x - other.x;
            let dy = y - other.y;
            let distance = Math.sqrt(dx * dx + dy * dy);
            return distance < (circleRadius * 2 + 10);
        });

        if (!overlapping) {
            return { x, y };
        }
        maxAttempts--;
    }
    return null;
}
createCallList(calls);
createCluster(calls);