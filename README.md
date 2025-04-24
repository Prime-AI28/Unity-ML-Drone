# Sector-Based Drone Navigation with Reinforcement Learning

This project implements a sector-based reinforcement learning framework for autonomous drone navigation in urban environments using Unity ML-Agents and Proximal Policy Optimization (PPO).

---

## About

This project develops a novel approach for autonomous drone navigation in complex urban settings by combining sector-based environment division with reinforcement learning. The primary goal is to enable a drone to navigate efficiently through dense urban environments while avoiding obstacles and optimizing paths. The system leverages Unity's ML-Agents framework to train a drone agent using PPO, with the environment divided into interconnected sectors to reduce state space complexity and enhance scalability. The intended audience includes researchers, developers, and enthusiasts in autonomous UAV systems, reinforcement learning, and urban navigation.

- Demo Video: `./Demo.mp4` or [Watch Demo](https://youtu.be/your-video-id)
- R&D Document: `./RND.docx`
- Knowledge Transfer Document: `./KnowledgeTransfer.docx`

---

## Quick Start

1. **Prerequisites**:
   - Unity Editor: Version `2021.3.60f1` (LTS recommended)
   - Python: Version `3.9` with `ml-agents` package (`pip install mlagents==0.30.0`)
   - Git (Ensure Git LFS is installed for large assets)
   
   Note: To install Git LFS, run `winget install GitHub.GitLFS`, then in your project directory run `git lfs install`.

2. **Clone the Repository**:

   ```bash
   git clone https://github.com/Prime-AI28/Unity-ML-Drone.git
   cd Unity-ML-Drone
   ```

3. **Open the cloned folder using Unity Hub.**

4. **Run**:
   - Open the main scene: `Assets/Scenes/UrbanEnvironment.unity`
   - Click the Play button ▶️ in the Unity Editor to simulate the environment.
   - For training, run `mlagents-learn` with the configuration file in `Assets/ML-Agents/Configs/DroneNavigation.yaml`.

---

## Features

- **Sector-Based Navigation**: Divides the urban environment into sectors for localized navigation tasks, improving efficiency.
- **Reinforcement Learning with PPO**: Utilizes Proximal Policy Optimization for robust drone control.
- **Continuous Action Space**: Precise control over pitch, roll, yaw, and throttle for realistic drone movement.
- **Curriculum Learning**: Progressive training to handle increasing environmental complexity.
- **Reward System**: Incentivizes goal-reaching, collision avoidance, and path efficiency.
- **Urban Environment Simulation**: Realistic Unity-based urban setting with dynamic obstacles.

---

## Dependencies <!-- (Extra Tools/Frameworks/Packages) -->

- **Unity**:
  - ML-Agents (`com.unity.ml-agents`)
  - TextMesh Pro
  - Cinemachine
- **External**:
  - Python ML-Agents (`mlagents==0.30.0`)
  - PyTorch (for ML-Agents training)
- **Custom Scripts**:
  - `IP_Drone_Agent.cs`: Core RL agent logic
  - `IP_Drone_Engine.cs`: Physics-based drone engine control
  - `Normalization.cs`: Observation normalization utilities


---

## Project Structure Overview

```
Unity-ML-Drone/                     # Root directory
├── Assets/                    # Core Unity assets
│   ├── Scenes/                # .unity scene files
│   ├── Scripts/               # C# scripts
│   ├── Prefabs/               # Prefab templates
│   ├── ML-Agents/             # Models, Training Information
│   ├── Animation/             # Visual effects & animations
│   └── Materials/             # Shaders & materials
├── Builds/                    # Compiled game builds
├── Demo/                      # Demo Records for Behavioral Cloning 
├── Results/                   # Trained Models with training inofrmation
├── Demo.mp4                   # Demo video
├── RND.docx                   # R&D document
├── KnowledgeTransfer.docx     # Knowledge-Transfer document
├── Packages/                  # Unity package dependencies and manifests
├── ProjectSettings/           # Unity configuration files
├── .gitignore                 # Git ignore rules
└── README.md                  # Project overview and setup instructions (This file)
```

---

## Configuration

| Setting | Location | Description | Default Value |
|---------|----------|-------------|---------------|
| Min/Max Pitch | `IP_Drone_Agent.cs` | Limits for drone pitch angle. | `30.0f` |
| Min/Max Roll | `IP_Drone_Agent.cs` | Limits for drone roll angle. | `30.0f` |
| Yaw Power | `IP_Drone_Agent.cs` | Strength of yaw rotation. | `4.0f` |
| Reward Parameters | `IP_Drone_Agent.cs` | Reward multipliers for distance, collision, etc. | See `RewardParameters` class |
| Target Reached Threshold | `IP_Drone_Agent.cs` | Distance to consider target reached. | `1.0f` |
| Training Config | `ML-Agents/Configs/DroneNavigation.yaml` | ML-Agents training hyperparameters. | See `ML-Agents/Configs/DroneNavigation.yaml` |
---

## Contact

- **Intern:** [Prasan Mittal](https://www.linkedin.com/in/prasan-mittal/)
  - Email: [prasan.mittal0211@gmail.com](mailto:prasan.mittal0211@gmail.com)
- **Mentor:** Praveen Krishna
