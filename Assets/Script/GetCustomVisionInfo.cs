// Copyright(c) 2018 Shingo Mori
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA.WebCam;

public class GetCustomVisionInfo : MonoBehaviour, IInputClickHandler
{
    // UI周りのフィールド
    public Text SystemMessage;       //状況表示用メッセージエリア
    public RawImage photoPanel;      //debug用のキャプチャ表示パネル

    // カメラ周りのパラメータ
    private PhotoCapture photoCaptureObject = null;
    private Resolution cameraResolution;
    private Quaternion cameraRotation;

    // Azure側のパラメータ群
    private string visionAPIKey = "YOUR_APP_KEY"; //Custom VisionのAPPキーをセットする
    private string visionURL    = "YOUR_APP_URL"; //Custom VisionのURLをセットする

    private TextToSpeech tts;
    private float probabilityThreshold = 0.5f; //信頼度の閾値

    public void OnInputClicked(InputClickedEventData eventData)
    {
        AnalyzeScene();
    }

    void Start()
    {
        InputManager.Instance.AddGlobalListener(gameObject);
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        tts = gameObject.GetComponent<TextToSpeech>();
    }

    private void AnalyzeScene()
    {
        DisplaySystemMessage("Detect Start...");
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    //PhotoCaptureの取得は下記参照
    //https://docs.microsoft.com/ja-jp/windows/mixed-reality/locatable-camera-in-unity
    private void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        DisplaySystemMessage("Take Picture...");
        photoCaptureObject = captureObject;

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.JPEG;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
            throw new Exception();
        }
    }

    private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            // Custom Visionに送るimageBufferListにメモリ上の画像をコピーする
            List<byte> imageBufferList = new List<byte>();
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            //ここはデバッグ用 送信画像の出力。どんな画像が取れたのか確認したい場合に使用。邪魔ならphotoPanelごと消してもよい。
            Texture2D debugTexture = new Texture2D(100, 100);
            debugTexture.LoadImage(imageBufferList.ToArray());
            photoPanel.texture = debugTexture;

            StartCoroutine(PostToCustomVisionAPI(imageBufferList.ToArray()));
        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    /*
     * 取得した画像をCustom Vision APIに送信し、タグ、キャプションを取得する
     */
    private IEnumerator<object> PostToCustomVisionAPI(byte[] imageData)
    {
        DisplaySystemMessage("Call Custom Vision API...");
        var headers = new Dictionary<string, string>() {
            { "Prediction-Key", visionAPIKey },
            { "Content-Type", "application/octet-stream" }
        };

        WWW www = new WWW(visionURL, imageData, headers);
        yield return www;

        ResponceJson json = JsonUtility.FromJson<ResponceJson>(www.text);

        float tmpProbability = 0.0f;
        string str = "";

        // probabilityの高い順に格納されているが、念のため全量取得してログ出力
        for (int i = 0; i < json.predictions.Length; i++)
        {
            Prediction obj = (Prediction)json.predictions[i];

            Debug.Log(obj.tagName + "：" + obj.probability.ToString("P"));

            if (tmpProbability < obj.probability)
            {
                str = obj.probability.ToString("P") + "の確率で" + obj.tagName + "です";
                str = "It's a " + obj.tagName + "! The Probability is " + obj.probability.ToString("P");
                tmpProbability = obj.probability;
            }

            // probabilityが閾値未満だったら特定不能とする
            if(tmpProbability < probabilityThreshold)
            {
                str = "Oops! I couldn't identify it. Please Try Again...";
            }
        }

        DisplaySystemMessage(str);
        tts.StartSpeaking(str);
    }

    /*
     * 状況出力用メッセージ
     */
    private void DisplaySystemMessage(string message)
    {
        SystemMessage.text = message;
    }

    /*
     * ここから、Custom Vision APIの戻り値用クラス
     */
    [Serializable]
    private class ResponceJson
    {
        public string id;
        public string project;
        public string iteration;
        public string created;

        public Prediction[] predictions;
    }

    [Serializable]
    private class Prediction
    {
        public float probability;
        public string tagId;
        public string tagName;
    }
}