using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Assets.Scripts.Utils;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class DeviceCamera : MonoBehaviour
{
    private bool _camAvailable;
    private WebCamTexture _cameraTexture;

    public RawImage Output;
    public AspectRatioFitter Fitter;
    public bool IsFrontFacing;
    public int CaptureScale = 2;

    private void Start()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif
        TryInitialize();
    }

    private void TryInitialize()
    {
        var devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            return;
        }

        for (var i = 0; i < devices.Length; i++)
        {
            var device = devices[i];
            if (device.isFrontFacing == IsFrontFacing)
            {
                _cameraTexture = new WebCamTexture(device.name, Screen.width, Screen.height);
                break;
            }
        }

        if (_cameraTexture != null)
        {
            _camAvailable = true;
            _cameraTexture.Play();
            Output.texture = _cameraTexture;
        }
    }

    public void Play(bool isPlaying)
    {
        if (_cameraTexture == null || !_camAvailable)
        {
            return;
        }

        if (isPlaying)
        {
            _cameraTexture.Play();
        }
        else
        {
            _cameraTexture.Pause();
        }
    }

    private void Update()
    {
        if (_cameraTexture == null || !_camAvailable)
        {
            TryInitialize();
            return;
        }

        var ratio = (float)_cameraTexture.width / (float)_cameraTexture.height;
        Fitter.aspectRatio = ratio;

        var scaleY = _cameraTexture.videoVerticallyMirrored ? -1f : 1f;
        Output.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        var orient = -_cameraTexture.videoRotationAngle;
        Output.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }

    public void Capture(Action<Texture2D> callback)
    {
        StartCoroutine(CaptureTexture(callback));
    }

    private IEnumerator CaptureTexture(Action<Texture2D> callback)
    {
        if (_cameraTexture == null || !_camAvailable)
        {
            yield break;
        }

        yield return new WaitForEndOfFrame();

        var photo = new Texture2D(_cameraTexture.width, _cameraTexture.height, TextureFormat.ARGB32, false);
        photo.SetPixels32(_cameraTexture.GetPixels32());
        photo.Apply();

        var newH = _cameraTexture.height / CaptureScale;
        var newW = Mathf.FloorToInt(((float)newH / (float)_cameraTexture.height) * _cameraTexture.width);
        TextureScale.Bilinear(photo, newW, newH);

        yield return new WaitForSeconds(0.1f);

        callback?.Invoke(TextureUtils.RotateImage(photo, -_cameraTexture.videoRotationAngle));
    }
}