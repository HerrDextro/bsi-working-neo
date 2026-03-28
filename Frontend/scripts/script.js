const CONFIG = {
    API_BASE: "http://10.72.10.208:32788",
    LIVEKIT_URL: "wss://clustercalls-cmhrjljf.livekit.cloud"
};

const State = {
    room: null,
    currentToken: localStorage.getItem('token') || null
};

const ApiService = {
    async request(endpoint, options = {}) {
        const url = `${CONFIG.API_BASE}${endpoint}`;
        const headers = { 'Content-Type': 'application/json' };
        
        if (State.currentToken) {
            headers['Authorization'] = `Bearer ${State.currentToken}`;
        }

        console.log(`Requesting: ${url}`);
        
        const response = await fetch(url, { ...options, headers });
        
        if (!response.ok) {
            const errorBody = await response.text();
            throw new Error(`API ${response.status}: ${errorBody || response.statusText}`);
        }
        
        return response.json();
    },

    getRooms: () => ApiService.request('/rooms'),
    getTeams: () => ApiService.request('/teams?activeOnly=true'),
    joinRoom: (id, topic) => ApiService.request(`/rooms/join?roomId=${id.toString()}`, {
        method: 'POST'
    }),
    login: (username, pwd) => ApiService.request('/auth/login', {method : 'POST', body : JSON.stringify({username : username, password : pwd})})
};

const CallManager = {
    initLiveKit() {
        if (typeof LiveKitClient !== 'undefined') {
            State.room = new LiveKitClient.Room({
                adaptiveStream: true,
                dynacast: true
            });
        }
    },

    async joinLiveKit(id, topic) {
        try {
            const data = await ApiService.joinRoom(id, topic);
            if (State.room.state === 'connected') await State.room.disconnect();
            await State.room.connect(CONFIG.LIVEKIT_URL, data.token);

            await State.room.localParticipant.setCameraEnabled(true);
            await State.room.localParticipant.setMicrophoneEnabled(true);
        } catch (err) {
            console.error("Failed to join LiveKit:", err);
            alert("Join failed: " + err.message);
        }
    }
};

const UIManager = {
    renderClusters(items) {
        const parent = document.getElementById("clusterArea");
        if (!parent) return;
        parent.innerHTML = "";

        items.forEach(item => {
            const template = document.getElementById("clusterTemplate");
            if (!template) return;

            const isTeams = !!item.teamsMeetingUrl;
            const topic = item.topic || item.name;
            const id = item.id;
            const participants = item.participants || 0;
            
            const radius = participants * 5 + 30;
            const pRect = parent.getBoundingClientRect();
            const seed = this.hashString(id ? id.toString() : topic);
            const x = (seed % (pRect.width - 150)) + 75;
            const y = ((seed >> 5) % (pRect.height - 150)) + 75;

            const clone = template.content.cloneNode(true);
            const wrapper = clone.querySelector('.cluster-wrapper');
            const circle = clone.querySelector('.cluster');
            const title = clone.querySelector('.clusterTitle');

            title.textContent = topic;
            circle.textContent = participants;
            circle.style.width = circle.style.height = `${radius * 2}px`;
            
            if (isTeams) circle.style.border = "3px solid #464EB8";

            wrapper.style.position = "absolute";
            wrapper.style.left = `${x - radius}px`;
            wrapper.style.top = `${y - radius}px`;

            circle.onclick = () => {
                const overlay = document.getElementById("callInfosOverlay");
                if (!overlay) return;
                overlay.style.display = "flex";
                document.getElementById("callInfosTitle").textContent = topic;
                
                const joinBtn = document.querySelector(".joinCallBtn");
                joinBtn.onclick = () => {
                    overlay.style.display = "none";
                    if (isTeams) {
                        window.open(item.teamsMeetingUrl, '_blank');
                    } else {
                        CallManager.joinLiveKit(id, topic);
                    }
                };
            };

            parent.appendChild(clone);
        });
    },

    hashString(str) {
        let hash = 0;
        for (let i = 0; i < str.length; i++) hash = str.charCodeAt(i) + ((hash << 5) - hash);
        return Math.abs(hash);
    }
};

async function initApp() {
    CallManager.initLiveKit();

    let allCalls = [];

    try {
        const rooms = await ApiService.getRooms();
        allCalls = [...allCalls, ...rooms];
    } catch (e) {
        console.warn("Could not load Rooms API", e);
    }

    try {
        const teams = await ApiService.getTeams();
        allCalls = [...allCalls, ...teams];
    } catch (e) {
        console.warn("Could not load Teams API", e);
    }

    if (allCalls.length > 0) {
        UIManager.renderClusters(allCalls);
    } else {
        console.error("Zero data received from API.");
    }
}

document.querySelector(".backBtn")?.addEventListener("click", () => {
    document.getElementById("callInfosOverlay").style.display = "none";
});

window.addEventListener('DOMContentLoaded', initApp);





async function initApp() {
    const token = localStorage.getItem('token');
    const loginOverlay = document.getElementById("loginOverlay"); 
    const mainContent = document.getElementById("clusterArea");

    if (!token) {
        console.log("No token found. Showing login.");
        if (loginOverlay) loginOverlay.style.display = "flex";
        return; 
    }

    if (loginOverlay) loginOverlay.style.display = "none";
    CallManager.initLiveKit();

    try {
        console.log("Token found. Fetching data...");
        const [rooms, teams] = await Promise.all([
            ApiService.getRooms().catch(() => []),
            ApiService.getTeams().catch(() => [])
        ]);
        
        const combined = [...rooms, ...teams];
        if (combined.length > 0) {
            UIManager.renderClusters(combined);
        } else {
            console.warn("Connected, but no rooms found on server.");

        }
    } catch (e) {
        console.error("Fetch error:", e);
        if (e.message.includes("401") || e.message.includes("Unauthorized")) {
            localStorage.removeItem('token');
            window.location.reload();
        }
    }
}


document.getElementById("logInBtn")?.addEventListener("click", async (e) => {
    e.preventDefault();
    
    const user = document.getElementById("name")?.value;
    const pass = document.getElementById("password")?.value;

    if (!user || !pass) {
        alert("Enter your credentials, chief.");
        return;
    }

    try {
        console.log("Attempting login...");
        const res = await ApiService.login(user, pass);

        if (res && res.token) {

            localStorage.setItem('token', res.token);
            console.log("Token saved. Redirecting...");


            window.location.href = "index.html";
        } else {
            alert("Login failed: Server didn't send a token.");
        }
    } catch (err) {
        console.error("Login Error:", err);

        alert("Connection failed. Is the server actually reachable at " + CONFIG.API_BASE + "?");
    }
});