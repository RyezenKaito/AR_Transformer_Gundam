using UnityEngine;
using UnityEngine.UI;

namespace GundamLobby
{
    [RequireComponent(typeof(Button))]
    public class LobbyButtonAction : MonoBehaviour
    {
        public string actionLabel;
        HomeLobbyController controller;
        Button button;
        void OnEnable()
        {
            button = GetComponent<Button>();
            controller = FindFirstObjectByType<HomeLobbyController>();
            button.onClick.AddListener(Notify);
        }
        void OnDisable() { if (button) button.onClick.RemoveListener(Notify); }
        void Notify()
        {
            if (!controller) controller = FindFirstObjectByType<HomeLobbyController>();
            if (controller) controller.ShowFeedback(actionLabel);
        }
    }
}
