using UnityEngine;
using UnityEngine.UI;

public class StartButtonBehaviour : MonoBehaviour {

    enum StartButtonStatus { 
        Start, AutoRound, AutoRoundEnabled
    }

    private StartButtonStatus status = StartButtonStatus.Start;
    public Color startColor;
    public Color autoRoundColor;
    public Color enabledAutoRoundColor;
    public WaveController waveController;

    private void Start() {
        ShowStartRound();
    }

    public void OnClick() {
        MultiplayerController.DefaultInstance.StartButton();

        HandleButtonClick();
    }

    public void HandleButtonClick() {
        if (status == StartButtonStatus.Start) StartRound();
        else if (status == StartButtonStatus.AutoRound) EnableAutoRound();
        else if (status == StartButtonStatus.AutoRoundEnabled) DisableAutoRound();
    }

    void StartRound() {
        FindObjectOfType<GameController>().GetComponent<WaveController>().StartRound();
        ShowEnableAutoRound();
    }

    public void ShowStartRound() {
        status = StartButtonStatus.Start;
        ChangeButtonColor(startColor);
        GetComponentInChildren<Text>().text = "Start";
    }

    void ShowEnableAutoRound() {
        status = StartButtonStatus.AutoRound;
        ChangeButtonColor(autoRoundColor);
        GetComponentInChildren<Text>().text = "Auto Start";
    }

    void EnableAutoRound() {
        status = StartButtonStatus.AutoRoundEnabled;
        ChangeButtonColor(enabledAutoRoundColor);
        GetComponentInChildren<Text>().text = "Auto Start";
        waveController.EnableAutoRound();
    }

    void DisableAutoRound() {
        ShowEnableAutoRound();
        waveController.DisableAutoRound();
    }

    private void ChangeButtonColor(Color color) {
        ColorBlock colors = GetComponent<Button>().colors;
        colors.normalColor = color;
        colors.selectedColor = color;
        GetComponent<Button>().colors = colors;
    }
}
