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
const callInfosBackBtn = document.querySelector(".backBtn");
const joinCallBtn = document.querySelector(".joinCallBtn");
const leaveRoomBtn = document.querySelector(".leaveBtn");

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
    calls.push(element);
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
    wrapper.setAttribute('data-id', element.id);
    const circle = clone.querySelector('.cluster');
    const title = clone.querySelector('.clusterTitle');

    title.textContent = element.topic;
    circle.textContent = element.participants;
    
    circle.style.height = (circleRadius * 2) + "px";
    circle.style.width = (circleRadius * 2) + "px";

    circle.setAttribute('data-id', element.id);
    circle.setAttribute('data-topic', element.topic);
    
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

    circle.addEventListener("click", (e) => {
        const clickedCircle = e.currentTarget;
        const overlay = document.getElementById("callInfosOverlay");
        overlay.style.display = "flex";

        document.getElementById("callInfosTitle").textContent = clickedCircle.dataset.topic;
        joinCallBtn.setAttribute('data-id', clickedCircle.dataset.id);
        leaveRoomBtn.setAttribute('data-id', clickedCircle.dataset.id);
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


leaveRoomBtn.addEventListener("click", (e) => {
    const id = Number(e.currentTarget.dataset.id);

    const call = calls.find(element => element.id === id);

    if(call) {
        call.participants -= 1;
        if(call.participants === 0){
            //deleteRoom(id);
        }
        updateParticipantCount(id, call.participants);
    }

    const overlay = document.getElementById("callInfosOverlay");
    overlay.style.display = "none";
})

callInfosBackBtn.addEventListener("click", () => {
    const overlay = document.getElementById("callInfosOverlay");
    overlay.style.display = "none";
})

joinCallBtn.addEventListener("click", (e) => {
    const id = Number(e.currentTarget.dataset.id);

    const call = calls.find(element => element.id === id);

    if(call){
        call.participants += 1;
        updateParticipantCount(id, call.participants);
    }

    const overlay = document.getElementById("callInfosOverlay");
    overlay.style.display = "none";
})


//Login
const logInBtn = document.getElementById("logInBtn");
const signInBtn = document.getElementById("signInBtn");

logInBtn.addEventListener("click", async () => {
    const username = document.getElementById("name").value;
    const password = document.getElementById("password").value;
    const url = "url /auth/login"

    data = {
        username: username,
        password: password
    }

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Registration failed');
        }

        const result = await response.json();
        console.log('Success:', result);
        return result;

    } catch (error) {
        console.error('Error during registration:', error.message);
    }
})

signInBtn.addEventListener("click", async () => {
    const username = document.getElementById("name").value;
    const password = document.getElementById("password").value;
    const url = "url /auth/register"

    data = {
        username: username,
        password: password
    }

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Registration failed');
        }

        const result = await response.json();
        console.log('Success:', result);
        return result;

    } catch (error) {
        console.error('Error during registration:', error.message);
    }
})




//Calls
async function createRoom(id){
    const url = "/rooms/create";
    const token = "JWT";

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(id)
        });
    } catch (error) {
        console.error("Error during creating a room:", error.message);
    }
}

async function joinRoom(id){
    const url = "/rooms/join";
    const token = "JWT";

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(id)
        });
    } catch (error) {
        console.error("Error during joining a room:", error.message);
    }
}

async function deleteRoom(id){

    const index = calls.findIndex(c => c.id === id);
    if (index !== -1) calls.splice(index, 1);

    const wrapper = document.querySelector(`.cluster-wrapper[data-id="${id}"]`);
    if (wrapper) {
        wrapper.remove();
    }


    const url = "/rooms/{name}";
    const token = "JWT";

    try {
        const response = await fetch(url, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(id)
        });
    } catch (error) {
        console.error("Error during deleting a room:", error.message);
    }
}



async function refreshToken(){
    const url = "/auth/refresh";
    
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(id)
        });
    } catch (error) {
        console.error("Error during refreshing the token", error.message);
    }
}