using System;
using UnityEngine;
using UnityEngine.UI;

public enum ControlsState
{
    Capture,
    Processing,
    Back
}

public class TesseractDemoScript : MonoBehaviour
{
    private TesseractDriver _tesseractDriver;

    [SerializeField] private Texture2D _texture;
    [SerializeField] private DeviceCamera _deviceCamera;
    [SerializeField] private Text _displayText;
    [SerializeField] private RawImage _output;
    [SerializeField] private Button _cameraButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private GameObject _loading;

    private void Awake()
    {
        _output.gameObject.SetActive(false);
        _cameraButton.onClick.AddListener(OnCameraButtonPressed);
        _backButton.onClick.AddListener(OnBackButtonPressed);

        SetControlsState(ControlsState.Processing);

        _tesseractDriver = new TesseractDriver();
        _tesseractDriver.Setup(() =>
        {
            SetControlsState(ControlsState.Capture);
        });
    }

    private void OnCameraButtonPressed()
    {
        SetControlsState(ControlsState.Processing);

        CaptureTexture(texture =>
        {
            Recognize(texture);
            PlayDeviceCamera(false);
            SetControlsState(ControlsState.Back);
        });
    }

    private void OnBackButtonPressed()
    {
        SetControlsState(ControlsState.Capture);
        PlayDeviceCamera(true);
        ClearTextDisplay();
    }

    private void SetControlsState(ControlsState state)
    {
        switch (state)
        {
            case ControlsState.Capture:
                _cameraButton.gameObject.SetActive(true);
                _backButton.gameObject.SetActive(false);
                _loading.gameObject.SetActive(false);
                break;
            case ControlsState.Processing:
                _cameraButton.gameObject.SetActive(false);
                _backButton.gameObject.SetActive(false);
                _loading.gameObject.SetActive(true);
                break;
            case ControlsState.Back:
                _cameraButton.gameObject.SetActive(false);
                _backButton.gameObject.SetActive(true);
                _loading.gameObject.SetActive(false);
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
        var result = _tesseractDriver?.Recognize(texture);
        AddToTextDisplay(result);
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