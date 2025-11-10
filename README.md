# Large Language Model Lung Cancer Patient Advisory in Virtual Reality

![Doctor avatar responding to patient](Assets/program_screenshot.png)

---

## Overview

This project implements a **virtual human** designed to help newly diagnosed lung-cancer patients understand their treatment options through natural conversation.  
It combines speech recognition, large language model inference, and text-to-speech inside an immersive VR experience.  

The system integrates:

- **Speech-to-Text (offline)** using Unity **Sentis Whisper**
- **LLM Inference (offline)** using **LLM for Unity** with the **Google Gemma 2 2b** model  
- **Text-to-Speech (online)** using the **OpenAI TTS API**  
- **Avatar animation** with **Microsoft Rocketbox** and **SALSA LipSync Suite v2**

All major processing can run locally to preserve **privacy** and reduce **latency**, while cloud-based TTS is optionally supported.

---

## Project Goals

- Enable **privacy-preserving medical communication** through offline processing  
- Evaluate **speech recognition accuracy** and **dialogue coherence**  
- Produce **empathetic, believable interactions** via facial animation  
- Lay groundwork for patient-specific advisory systems in XR healthcare

---

## Setup Instructions

### Prerequisites
- **Unity 2022.3.23f1** (or later)
- **Git LFS** installed → [https://git-lfs.com](https://git-lfs.com)

### 1) Clone
git clone https://github.com/PaulS97/LLM-Lung-Cancer-Patient-Advisory-VR.git
cd LLM-Lung-Cancer-Patient-Advisory-VR

### 2) Add Models (kept out of Git)
LLM (.gguf)
- Download Gemma 2 2b-it (8-bit) or another .gguf model.
- Place the file in:
  Assets/StreamingAssets/

Whisper Tiny (ONNX)
- Download from https://huggingface.co/unity/sentis-whisper-tiny/tree/main/ONNX:
  - LogMelSpectro.onnx
  - AudioEncoder_Tiny.onnx
  - AudioDecoder_Tiny.onnx
- Also download vocab.json from the same page.
- Place files in:
  Assets/SentisModels/
  Assets/StreamingAssets/

### 3) Open in Unity
- Open the project in Unity 2022.3 LTS.
- Load scene: Assets/Scenes/Scene.unity.
- If prompted, click Import TMP Essentials.

### 4) Configure Optional TTS
Set your OpenAI key locally (never commit keys):
export OPENAI_API_KEY="sk-..."

Or use a local config in Unity (e.g., environment loader or ScriptableObject).

