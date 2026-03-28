let room;

let calls = [
    { id: 1748875, topic: "Topic1", participants: 5, activity: 1 },
    { id: 2478784, topic: "Topic2", participants: 10, activity: 1 },
    { id: 38589348, topic: "Topic3", participants: 2, activity: 1 }
];

let placedCircles = [];

const createClusterBtn = document.getElementById("createCluster");
const submitClusterName = document.getElementById("submitNameBtn");
const callInfosBackBtn = document.querySelector(".backBtn");
const joinCallBtn = document.querySelector(".joinCallBtn");
const leaveRoomBtn = document.querySelector(".leaveBtn");
const logInBtn = document.getElementById("logInBtn");
const signInBtn = document.getElementById("signInBtn");

function createCallList(list) {
    const parent = document.getElementById("callArea");
    if (!parent) return;
    parent.innerHTML = "";
    list.forEach(element => {
        const div = document.createElement("div");
        div.className = "element";
        div.textContent = element.topic;
        parent.appendChild(div);
    });
}

function createAllCluster(list) {
    const parent = document.getElementById("clusterArea");
    if (!parent) return;
    parent.innerHTML = "";
    placedCircles = [];
    list.forEach(element => {
        createCluster(element, parent);
    });
}

async function createCluster(element, parent) {
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

    placedCircles.push({ x: center.x, y: center.y, r: circleRadius, id: element.id });
    let clickedCircle;
    let topic;

    circle.addEventListener("click", (e) => {
        clickedCircle = e.currentTarget;
        const overlay = document.getElementById("callInfosOverlay");
        if (overlay) {
            overlay.style.display = "flex";
            document.getElementById("callInfosTitle").textContent = clickedCircle.dataset.topic;
            if (joinCallBtn) joinCallBtn.setAttribute('data-id', clickedCircle.dataset.id);
            if (leaveRoomBtn) leaveRoomBtn.setAttribute('data-id', clickedCircle.dataset.id);
        }
        topic = clickedCircle.dataset.topic;
    });

    const userToken = localStorage.getItem('token');
    const response = await fetch(`http://10.72.10.208:32774/rooms/create?name=${topic}`, {
        method: 'POST',
        headers: { 
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${userToken}`
        }
    });
}

async function joinRoom(roomToken) {
    const livekitUrl = "wss://clustercalls-cmhrjljf.livekit.cloud";
    if (!room) return;
    try {
        if (room.state === 'connected') await room.disconnect();
        await room.connect(livekitUrl, roomToken);
        await room.localParticipant.setCameraEnabled(true);
        await room.localParticipant.setMicrophoneEnabled(true);
    } catch (error) {
        console.error(error);
    }
}

function getCenterFromGuid(element, parent) {
    const pRect = parent.getBoundingClientRect();
    const seed = hashStringToInt(element.id ? element.id.toString() : element.topic);
    const margin = 80;
    const x = (seed % (pRect.width - margin * 2)) + margin;
    const y = ((seed >> 5) % (pRect.height - margin * 2)) + margin;
    return { x, y };
}

function hashStringToInt(str) {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    return Math.abs(hash);
}

function updateParticipantCount(id, newCount) {
    const parent = document.getElementById("clusterArea");
    const wrapper = document.querySelector(`.cluster-wrapper[data-id="${id}"]`);
    if (!wrapper || !parent) return;
    const circle = wrapper.querySelector('.cluster');
    const newRadius = newCount * 5 + 20;
    const center = getCenterFromGuid({ id: id }, parent);
    circle.textContent = newCount;
    circle.style.width = `${newRadius * 2}px`;
    circle.style.height = `${newRadius * 2}px`;
    wrapper.style.left = `${center.x - newRadius}px`;
    wrapper.style.top = `${center.y - newRadius}px`;
}

if (joinCallBtn) {
    joinCallBtn.addEventListener("click", async (e) => {
        const targetId = e.currentTarget.dataset.id;
        const userToken = localStorage.getItem('token');
        
        try {
            const roomsResponse = await fetch("http://10.72.10.208:32774/rooms", {
                method: 'GET',
                headers: { 'Authorization': userToken }
            });

            if (!roomsResponse.ok) throw new Error("Failed to fetch rooms list");
            const allRooms = await roomsResponse.json();

            const targetRoom = allRooms.find(r => r.id == targetId);
            if (!targetRoom) throw new Error("Room not found");

            const url = "http://10.72.10.208:32774/rooms/join";
            
            const joinResponse = await fetch(url, {
                method: 'POST',
                headers: { 
                    'Content-Type': 'application/json',
                    'Authorization': userToken
                },
                body: JSON.stringify({ 
                    roomId: targetRoom.id.toString(), 
                    roomName: targetRoom.topic 
                })
            });

            if (!joinResponse.ok) {
                const errorText = await joinResponse.text();
                throw new Error(errorText || "Join failed");
            }

            const data = await joinResponse.json();

            updateParticipantCount(targetRoom.id, targetRoom.participants + 1);
            
            if (data.token) {
                await joinRoom(data.token);
            }

            const overlay = document.getElementById("callInfosOverlay");
            if (overlay) overlay.style.display = "none";

        } catch (error) {
            console.error("Join error:", error.message);
        }
    });
}

if (createClusterBtn) {
    createClusterBtn.addEventListener("click", () => {
        const overlay = document.querySelector(".overlay");
        if (overlay) overlay.style.display = "flex";
    });
}

if (logInBtn) {
    logInBtn.addEventListener("click", async () => {
        const username = document.getElementById("name")?.value;
        const password = document.getElementById("password")?.value;
        const url = "http://10.72.10.208:32774/auth/login";
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });
            if (!response.ok) throw new Error('Login failed');
            const result = await response.json();
            localStorage.setItem('token', result.token);
            window.location.href = "index.html";
        } catch (error) {
            console.error(error.message);
        }
    });
}

if (callInfosBackBtn) {
    callInfosBackBtn.addEventListener("click", () => {
        const overlay = document.getElementById("callInfosOverlay");
        if (overlay) overlay.style.display = "none";
    });
}

window.addEventListener('DOMContentLoaded', () => {
    if (typeof LiveKitClient !== 'undefined') {
        room = new LiveKitClient.Room({
            adaptiveStream: true,
            dynacast: true,
            publishDefaults: { videoCodec: 'h264' }
        });
    }
    createCallList(calls);
    createAllCluster(calls);
});