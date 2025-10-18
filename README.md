> ⚠️ **AI-GENERATED README** — created with assistance from ChatGPT (GPT-5)

# 🚀 Drone League – Networked Physics & Prediction System

A **multiplayer drone arena** inspired by **Rocket League**, built using **Unity’s Netcode for GameObjects**.  
Players control flying drones that interact with a **fully networked ball**, featuring **client-side prediction**, **server reconciliation**, and **deterministic tick-based physics**.

---

## 🎮 Concept

Imagine **Rocket League**, but with **drones instead of cars**.  
Each player pilots a customizable drone capable of hovering, flipping, boosting, and performing aerial maneuvers.  
The objective: use physics-based control to push or launch a shared ball into the opponent’s goal — with full network synchronization.

---

## ⚙️ Core Features

| Feature | Description |
|----------|--------------|
| 🧠 **Client-Side Prediction** | Each drone predicts its own movement locally and corrects mismatches smoothly. |
| 🛰️ **Networked Ball Physics** | The ball runs on the **authoritative server** and is synchronized across all clients. |
| ⚡ **Tick-Based Simulation** | A global tick system ensures consistent and deterministic physics updates. |
| 🔁 **Reconciliation System** | Clients store past states and reconcile after receiving authoritative data. |
| 🧩 **Modular Design** | Built to easily extend — plug in new drone behaviors or ball mechanics. |
| 🔒 **Fair Play** | Input rate limiting and buffered RPCs prevent cheating or desync exploits. |

---

## 🧱 Architecture Overview

Assets/
├── Drones/ # Drone logic, physics, and input prediction
├── Ball/ # Networked ball system and reconciliation
├── Netcode/ # Core multiplayer logic (Unity Netcode for GameObjects)
├── TickSystem/ # Global tick synchronization & state buffers
├── SceneManagement/ # Optional scene loader & utilities
└── Singletons/ # Required singletons for tick + CSP systems


---

## 🔧 Technical Details

- 🧮 **Prediction & Reconciliation**  
  Each drone predicts its own inputs locally. When the server responds with the authoritative state, the client checks for deviation and rewinds/re-simulates as needed.  

- 🏐 **Ball Authority**  
  The ball is simulated **only** on the server. Drones send force and collision impulses to the server, which applies them and broadcasts the updated state.  

- ⏱️ **Tick Synchronization**  
  All entities — drones, ball, inputs — are tied to the same tick index. This enables rollback debugging, accurate physics replays, and future collider rollback systems.  

- ⚙️ **Modularity**  
  Components like Input Handlers, State Buffers, and Tick Synchronizers are decoupled, making it easy to reuse or extend the architecture in other projects.

---

## 🌍 Networking Model

| Role | Responsibility |
|------|----------------|
| **Client** | Predicts drone movement, sends inputs, and reconciles after updates. |
| **Server** | Maintains authoritative states for all entities, handles physics, and distributes snapshots. |
| **Transport** | Uses Unity’s default **UTP (Unity Transport Protocol)** — easily replaceable with another solution. |

---

## 🧩 Physics System

- Built on **Rigidbody-based motion** for both drones and the ball.  
- Utilizes **force-based control** rather than direct position updates for realism.  
- Prediction and correction occur per-tick to prevent visual snapping.  
- Each tick records **position**, **velocity**, and **angular velocity** in a circular buffer for rollback and reconciliation.

---

## 🛠️ Getting Started

1. Clone this repository  
2. Open in Unity (recommended version: **2022.3+**)  
3. Launch the **ArenaDemo** scene  
4. Press **Play** to host or join a match  
5. Fly your drone, interact with the ball, and test full network prediction in action 🚁⚽  

---

## 🔮 Roadmap

- [ ] Implement **boost system** and drone customization  
- [ ] Add **spectator camera** and replay mode  
- [ ] Introduce **tick rollback** for the ball (Level 5 CSP)  
- [ ] Create a **public multiplayer demo build**  
- [ ] Add **mobile controller support** (optional)  

---

## 🧾 Credits

- Built with ❤️ by **Jonas Lindinger**  
- Inspired by **Rocket League’s Netcode** and **GDC Vault networking talks**  
- Powered by **Unity Netcode for GameObjects (NGO)**  
- Learned through **YouTube tutorials**, **docs**, and **lots of trial and error**

---

> *“Networking physics is one of the hardest things to get right — but when it works, it feels like magic.”*  
> — **Jonas Lindinger**
