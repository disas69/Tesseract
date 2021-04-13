using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class TesseractDriver
{
    private static readonly List<string> FileNames = new List<string> {"tessdata.tgz"};

    private TesseractWrapper _tesseract;

    public void Setup(UnityAction onSetupComplete)
    {
#if UNITY_EDITOR
        OcrSetup(onSetupComplete);
#elif UNITY_ANDROID
        CopyAllFilesToPersistentData(FileNames, () => OcrSetup(onSetupComplete));
#else
        OcrSetup(onSetupComplete);
#endif
    }

    private void OcrSetup(UnityAction onSetupComplete)
    {
        _tesseract = new TesseractWrapper();

#if UNITY_EDITOR
        string datapath = Path.Combine(Application.streamingAssetsPath, "tessdata");
#elif UNITY_ANDROID
        string datapath = Application.persistentDataPath + "/tessdata/";
#else
        string datapath = Path.Combine(Application.streamingAssetsPath, "tessdata");
#endif

        if (_tesseract.Init("eng", datapath))
        {
            Debug.Log("Init Successful");
            onSetupComplete?.Invoke();
        }
        else
        {
            Debug.LogError(_tesseract.GetErrorMessage());
        }
    }

    public string GetVersion()
    {
        try
        {
            return "Tesseract version: " + _tesseract?.Version();
        }
        catch (Exception e)
        {
            return e.GetType() + " - " + e.Message;
        }
    }

    public string Recognize(Texture2D imageToRecognize)
    {
        return _tesseract?.Recognize(imageToRecognize);
    }

    public Texture2D GetHighlightedTexture()
    {
        return _tesseract?.GetHighlightedTexture();
    }

    public string GetErrorMessage()
    {
        return _tesseract?.GetErrorMessage();
    }

    private async void CopyAllFilesToPersistentData(List<string> fileNames, Action callback)
    {
        var fromPath = "jar:file://" + Application.dataPath + "!/assets/";
        var toPath = Application.persistentDataPath + "/";

        foreach (var fileName in fileNames)
        {
            if (File.Exists(toPath + fileName))
            {
                Debug.Log("File exists! " + toPath + fileName);
            }
            else
            {
                using (var uwr = new UnityWebRequest(fromPath + fileName) { downloadHandler = new DownloadHandlerBuffer() })
                {
                    await uwr.SendWebRequest();

                    if (uwr.isNetworkError || uwr.isHttpError)
                    {
                        Debug.Log(uwr.error);
                    }
                    else
                    {
                        File.WriteAllBytes(toPath + fileName, uwr.downloadHandler.data);
                    }
                }

                UnZipData(fileName);
            }
        }

        callback?.Invoke();
    }

    private static void UnZipData(string fileName)
    {
        if (File.Exists(Application.persistentDataPath + "/" + fileName))
        {
            UnZipUtil.ExtractTGZ(Application.persistentDataPath + "/" + fileName, Application.persistentDataPath);
            Debug.Log("UnZipping Done");
        }
        else
        {
            Debug.LogError(fileName + " not found!");
        }
    }
}