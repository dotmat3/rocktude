using UnityEngine;
using UnityEngine.UI;

public class StartButtonBehaviour : MonoBehaviour
{

    enum StartButtonStatus { 
        Start, AutoRound, AutoRoundEnabled
    }

    private StartButtonStatus status = StartButtonStatus.Start;
    public Sprite startSprite;
    public Sprite autoRoundSprite;
    public Sprite enabledAutoRoundSprite;
    public WaveController waveController;

    private void Start() {
        ShowStartRound();
    }

    public void OnClick() {
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
        GetComponent<Image>().sprite = startSprite;
    }

    void ShowEnableAutoRound() {
        status = StartButtonStatus.AutoRound;
        GetComponent<Image>().sprite = autoRoundSprite;
    }

    void EnableAutoRound() {
        status = StartButtonStatus.AutoRoundEnabled;
        GetComponent<Image>().sprite = enabledAutoRoundSprite;
        waveController.EnableAutoRound();
    }

    void DisableAutoRound() {
        ShowEnableAutoRound();
        waveController.DisableAutoRound();
    }
}
