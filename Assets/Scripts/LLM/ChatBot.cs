using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using LLMUnity;

namespace LLMUnitySamples
{
    public class ChatBot : MonoBehaviour
    {
        [SerializeField] private TTSManager ttsManager;
        public Transform chatContainer;
        public Color playerColor = new Color32(81, 164, 81, 255);
        public Color aiColor = new Color32(29, 29, 73, 255);
        public Color fontColor = Color.white;
        public Font font;
        public int fontSize = 16;
        public int bubbleWidth = 600;
        public LLMCharacter llmCharacter;
        public float textPadding = 10f;
        public float bubbleSpacing = 10f;
        public Sprite sprite;

        [SerializeField] private GameObject inputBlocker;

        private InputBubble inputBubble;
        private List<Bubble> chatBubbles = new List<Bubble>();
        private bool blockInput = true;
        private BubbleUI playerUI, aiUI;
        private bool warmUpDone = false;
        private int lastBubbleOutsideFOV = -1;

        private string lastResponse = "";  // To store the latest LLM response

        void Start()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            playerUI = new BubbleUI
            {
                sprite = sprite,
                font = font,
                fontSize = fontSize,
                fontColor = fontColor,
                bubbleColor = playerColor,
                bottomPosition = 0,
                leftPosition = 0,
                textPadding = textPadding,
                bubbleOffset = bubbleSpacing,
                bubbleWidth = bubbleWidth,
                bubbleHeight = -1
            };
            aiUI = playerUI;
            aiUI.bubbleColor = aiColor;
            aiUI.leftPosition = 1;

            inputBubble = new InputBubble(chatContainer, playerUI, "InputBubble", "Loading...", 4);
            inputBubble.AddSubmitListener(onInputFieldSubmit);
            inputBubble.AddValueChangedListener(onValueChanged);
            inputBubble.setInteractable(false);
            _ = llmCharacter.Warmup(WarmUpCallback);
        }

        public void SubmitVoiceCommand(string message)
        {
            inputBubble.SetText(message);
            onInputFieldSubmit(message);
        }

        void onInputFieldSubmit(string newText)
        {
            inputBubble.ActivateInputField();
            if (blockInput || string.IsNullOrWhiteSpace(newText))
            {
                StartCoroutine(BlockInteraction());
                return;
            }

            blockInput = true;
            string message = inputBubble.GetText().Replace("\v", "\n");

            // Player's message bubble
            Bubble playerBubble = new Bubble(chatContainer, playerUI, "PlayerBubble", message);
            Bubble aiBubble = new Bubble(chatContainer, aiUI, "AIBubble", "...");

            chatBubbles.Add(playerBubble);
            chatBubbles.Add(aiBubble);
            playerBubble.OnResize(UpdateBubblePositions);
            aiBubble.OnResize(UpdateBubblePositions);

            // Send the message to the chatbot and handle the response
            Task chatTask = llmCharacter.Chat(message, response =>
            {
                lastResponse = response;  // Update the latest LLM response
                aiBubble.SetText(response);  // Update the chat bubble with the response
            }, AllowInput);

            inputBubble.SetText("");
        }

        void SendToTTS(string text)
        {
            if (ttsManager == null)
            {
                Debug.LogError("TTSManager is not assigned! Please assign it in the Inspector.");
                return;
            }

            Debug.Log($"Sending to TTS: {text}");
            ttsManager.SynthesizeAndPlay(text);
        }

        public void AllowInput()
        {
            // This is triggered when the chatbot has completed its response
            if (!string.IsNullOrEmpty(lastResponse))
            {
                Debug.Log($"Final chatbot response: {lastResponse}");
                SendToTTS(lastResponse);  // Trigger TTS for the final response
            }

            blockInput = false;  // Allow the user to input again
            inputBubble.ReActivateInputField();
        }

        public void WarmUpCallback()
        {
            inputBlocker.SetActive(false);
            warmUpDone = true;
            inputBubble.SetPlaceHolderText("Message me");
            AllowInput();
        }

        IEnumerator<string> BlockInteraction()
        {
            inputBubble.setInteractable(false);
            yield return null;
            inputBubble.setInteractable(true);
            inputBubble.MoveTextEnd();
        }

        void onValueChanged(string newText)
        {
            if (Input.GetKey(KeyCode.Return) && string.IsNullOrWhiteSpace(inputBubble.GetText()))
            {
                inputBubble.SetText("");
            }
        }

        public void UpdateBubblePositions()
        {
            float y = inputBubble.GetSize().y + inputBubble.GetRectTransform().offsetMin.y + bubbleSpacing;
            float containerHeight = chatContainer.GetComponent<RectTransform>().rect.height;

            for (int i = chatBubbles.Count - 1; i >= 0; i--)
            {
                Bubble bubble = chatBubbles[i];
                RectTransform childRect = bubble.GetRectTransform();
                childRect.anchoredPosition = new Vector2(childRect.anchoredPosition.x, y);

                if (y > containerHeight && lastBubbleOutsideFOV == -1)
                {
                    lastBubbleOutsideFOV = i;
                }
                y += bubble.GetSize().y + bubbleSpacing;
            }
        }

        void Update()
        {
            if (!inputBubble.inputFocused() && warmUpDone)
            {
                inputBubble.ActivateInputField();
                StartCoroutine(BlockInteraction());
            }

            if (lastBubbleOutsideFOV != -1)
            {
                for (int i = 0; i <= lastBubbleOutsideFOV; i++)
                {
                    chatBubbles[i].Destroy();
                }
                chatBubbles.RemoveRange(0, lastBubbleOutsideFOV + 1);
                lastBubbleOutsideFOV = -1;
            }
        }

        public void ExitGame()
        {
            Debug.Log("Exit button clicked");
            Application.Quit();
        }

        void OnValidate()
        {
            if (!llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
            {
                Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
            }
        }
    }
}
