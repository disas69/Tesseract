using System;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonState
{
    Ready,
    Processing,
    Restart
}

public class TesseractDemoScript : MonoBehaviour
{
    private bool _isCapturing;
    private TesseractDriver _tesseractDriver;

    [SerializeField] private DeviceCamera _deviceCamera;
    [SerializeField] private Text _displayText;
    [SerializeField] private RawImage _output;
    [SerializeField] private Button _button;

    private void Awake()
    {
        _isCapturing = true;
        _output.gameObject.SetActive(false);
        _button.onClick.AddListener(OnButtonPressed);

        SetButtonState(ButtonState.Processing);

        _tesseractDriver = new TesseractDriver();
        _tesseractDriver.Setup(() =>
        {
            SetButtonState(ButtonState.Ready);
        });
    }

    private void OnButtonPressed()
    {
        if (_isCapturing)
        {
            SetButtonState(ButtonState.Processing);
            
            CaptureTexture(texture =>
            {
                Recognize(texture);
                PlayDeviceCamera(false);
                SetButtonState(ButtonState.Restart);
            });
        }
        else
        {
            SetButtonState(ButtonState.Ready);
            PlayDeviceCamera(true);
            ClearTextDisplay();
        }

        _isCapturing = !_isCapturing;
    }

    private void SetButtonState(ButtonState state)
    {
        switch (state)
        {
            case ButtonState.Ready:
                _button.interactable = true;
                _button.GetComponentInChildren<Text>().text = "Capture";
                break;
            case ButtonState.Processing:
                _button.interactable = false;
                _button.GetComponentInChildren<Text>().text = "Processing...";
                break;
            case ButtonState.Restart:
                _button.interactable = true;
                _button.GetComponentInChildren<Text>().text = "Continue";
                break;
        }
    }

    private void CaptureTexture(Action<Texture2D> callback)
    {
        _deviceCamera.Capture(texture =>
        {
            callback?.Invoke(texture);
        });
    }

    private void PlayDeviceCamera(bool isPlaying)
    {
        _deviceCamera.Play(isPlaying);
        _output.gameObject.SetActive(!isPlaying);
    }

    private void Recognize(Texture2D texture)
    {
        AddToTextDisplay(_tesseractDriver.Recognize(texture));
        SetImageDisplay();
    }

    private void ClearTextDisplay()
    {
        _displayText.text = string.Empty;
    }

    private void AddToTextDisplay(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        _displayText.text = text;
    }

    private void SetImageDisplay()
    {
        _output.texture = _tesseractDriver.GetHighlightedTexture();
        _output.gameObject.SetActive(true);
    }
}