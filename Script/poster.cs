using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.StringLoading;
using VRC.SDK3.Data;
using System;

namespace Nomlas.Poster
{
    [HelpURL("https://github.com/nomlasvrc/202310VRCPoster")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Poster : UdonSharpBehaviour
    {
        [HideInInspector] public readonly string version = "v1.1.2";
        [SerializeField] public bool JapaneseMode;
        [SerializeField] private VRCUrl[] picUrls;
        [SerializeField] private VRCUrl lengthURL;
        [SerializeField] public int slideTime;
        [SerializeField] public int startDelayTime;
        [SerializeField] public float aspectRaito;

        [SerializeField] public GameObject picture;
        [SerializeField] public TextMeshProUGUI message;

        [SerializeField] public Animator animator;
        private Material material;

        private int nextIndex = 0;
        private int loadedPosterIndex = -1;
        private VRCImageDownloader downloader;
        private IUdonEventReceiver udonEventReceiver;
        private Texture2D[] downloadedTextures;
        private TextureInfo texInfo;
        private int posterLength = 1; //1枚はある前提
        private bool stringLoaded = false;
        private string json;
        readonly DateTime startOfYear = new DateTime(2024, 1, 1, 0, 0, 0); // 2024/01/01 00:00:00

        void Start()
        {
            // SerializeFieldのチェック
            if (slideTime <= 0)
            {
                Dlog("slideTimeが0以下になっています。修正してください。", LogType.Error);
            }
            if (picture == null)
            {
                Dlog("ターゲットが正しくありません。修正してください。", LogType.Error);
            }

            if (JapaneseMode)
            {
                Dlog("日本語モードがオンになっています。TextMeshProのメッセージが日本語で表示されます。");
            }
            else
            {
                Dlog("Japanese Mode is OFF. TextMeshPro's messages are written in English, but logs are written in Japanese.");
            }
            TMPMessage("Now Loading... (0/?) Poster is not synced.", "読み込み中... (0/?) ポスターは同期していません。");
            //初期設定
            downloadedTextures = new Texture2D[picUrls.Length];
            downloader = new VRCImageDownloader();
            udonEventReceiver = (IUdonEventReceiver)this;
            texInfo = new TextureInfo();
            texInfo.GenerateMipMaps = true;
            texInfo.MaterialProperty = "_SubTex";
            material = picture.GetComponent<MeshRenderer>().sharedMaterial;
            if (startDelayTime > 0)
            {
                SendCustomEventDelayedSeconds(nameof(StartLoading), startDelayTime);
            }
            else
            {
                StartLoading();
            }
        }

        public void StartLoading()
        {
            //StringLoading開始
            Dlog($"StringLoading開始、ポスター枚数を取得中...");
            VRCStringDownloader.LoadUrl(lengthURL, udonEventReceiver);

            //同時にImageLoadingも開始
            Dlog("1枚目のポスターの先行読み込みを開始します");
            LoadFirstPoster();
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            json = result.Result;
            if (VRCJson.TryDeserializeFromJson(json, out DataToken res))
            {
                if (res.DataDictionary.TryGetValue("length", out DataToken value))
                {
                    posterLength = int.Parse(value.String);
                    stringLoaded = true;
                    Dlog($"StringLoading成功、ポスターは{posterLength}枚です");
                    if (loadedPosterIndex >= 0) //1枚目ImageLoadingより遅かった場合
                    {
                        Dlog("2枚目以降のポスターのImageLoadingを開始します");
                        LoadPoster();
                    }
                }
                else
                {
                    Dlog($"解析に失敗しました。 {value}", LogType.Warning);
                    TMPMessage($"Parsing failed. Detail: {value}", $"解析に失敗しました。詳細：{value}");
                }
            }
            else
            {
                Dlog($"デシリアライズに失敗しました。 {result}", LogType.Warning);
                TMPMessage($"Deserialization failed. Detail: {result}", $"デシリアライズに失敗しました。詳細：{result}");
            }
        }
        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Dlog($"ポスター枚数を取得できませんでした。詳細：{result.Error}", LogType.Warning);
            TMPMessage($"Failed to get data. Detail: {result.Error}", $"ポスター枚数を取得できませんでした。詳細：{result.Error}");
        }

        private void LoadFirstPoster()
        {
            downloader.DownloadImage(picUrls[0], material, udonEventReceiver, texInfo);
        }

        private void LoadPoster() //ここでポスターをImageLoading
        {
            Dlog($"{loadedPosterIndex + 2}枚目のポスターをダウンロードします");
            downloader.DownloadImage(picUrls[loadedPosterIndex + 1], material, udonEventReceiver, texInfo);
        }

        private void SyncPosterIndex() //発火タイミングを調整する
        {
            int elapsedSeconds = GetElapsedSeconds();
            nextIndex = elapsedSeconds % (slideTime * posterLength) / slideTime - 1;
            int offset = slideTime - (elapsedSeconds % slideTime);
            Dlog($"タイミング調整中。{offset}秒後にスライドショーを開始します");
            TMPMessage("All posters have been loaded. Adjusting timing...", "全ポスター読み込み完了。まもなく同期します...");
            SendCustomEventDelayedSeconds(nameof(StartLoadNextPoster), offset);
        }

        public void StartLoadNextPoster()
        {
            Dlog("スライドショーを開始します");
            TMPMessage("", "");
            LoadNextPoster();
        }

        public void LoadNextPoster() //ImageLoadingがすべて完了したとき開始
        {
            LoadNext();
            SendCustomEventDelayedSeconds(nameof(LoadNextPoster), slideTime);
        }

        private void LoadNext() //スライドショー機能
        {
            ++nextIndex;
            if (nextIndex > posterLength - 1)
            {
                nextIndex = 0;
            }
            FitPicture(downloadedTextures[nextIndex]);
        }

        private void FitPicture(Texture2D tempTex)
        {
            var oldTex = material.GetTexture("_MainTex");
            material.SetTexture("_SubTex", oldTex); //今表示中のポスターをSubTexにコピー
            animator.Play("transition", 0, 0.0f); // SubTex => MainTexのアニメーションを再生
            material.SetTexture("_MainTex", tempTex); //新しいポスターをMainTexに
            material.SetFloat("_MainTexAspect", Mathf.Clamp(tempTex.width / (float)tempTex.height, float.Epsilon, float.MaxValue) / aspectRaito);
            material.SetFloat("_SubTexAspect", Mathf.Clamp(oldTex.width / (float)oldTex.height, float.Epsilon, float.MaxValue) / aspectRaito);
        }

        public override void OnImageLoadSuccess(IVRCImageDownload result)
        {
            loadedPosterIndex++;
            Dlog($"{loadedPosterIndex + 1}枚目のポスターのダウンロードに成功しました");
            downloadedTextures[loadedPosterIndex] = result.Result;
            FitPicture(result.Result);
            TMPMessage($"Now Loading... ({loadedPosterIndex + 1}/{posterLength}) Poster is not synced.", $"読み込み中... ({loadedPosterIndex + 1}/{posterLength}) ポスターは同期していません。");
            //posterLengthは枚数だがloadedPosterIndexはインデックスなので1少なくなる
            if ((posterLength - 1 > loadedPosterIndex) && stringLoaded) //ImageLoadingがすべて完了していない & StringLoading終わってる
            {
                LoadPoster();
            }
            if ((posterLength - 1 == loadedPosterIndex) && stringLoaded)
            {
                Dlog("全てのポスターのImageLoadingが完了しました。スライドショーを開始します");
                SyncPosterIndex();
            }
        }

        public override void OnImageLoadError(IVRCImageDownload result) //カナシイネ
        {
            Dlog($"ポスターの読み込みに失敗しました。エラー内容: {result.Error} 詳細: {result.ErrorMessage}.", LogType.Warning);
            TMPMessage($"Failed to load poster.Error: {result.Error} Detail: {result.ErrorMessage}.", $"ポスターの読み込みに失敗しました。エラー内容: {result.Error} 詳細: {result.ErrorMessage}.");
        }

        private int GetElapsedSeconds()
        {
            //2024年からの経過秒数を取得する関数
            DateTime now = Networking.GetNetworkDateTime();
            TimeSpan elapsedTime = now - startOfYear;
            int yearTime = (int)elapsedTime.TotalSeconds;
            if (yearTime > 0)
            {
                return yearTime;
            }
            else //タイムトラベラー用に一応対策
            {
                return 1;
            }
        }

        private void OnDestroy()
        {
            downloader.Dispose();
        }

        private void TMPMessage(string _TextMeshProMessageEnglish, string _TextMeshProMessageJapanese)
        {
            if (message == null)
            {
                return;
            }
            if (JapaneseMode)
            {
                message.text = _TextMeshProMessageJapanese;
            }
            else
            {
                message.text = _TextMeshProMessageEnglish;
            }
        }

        private void Dlog(string logText, LogType logType = LogType.Log) //Debug.Logを少し楽にする用
        {
            string log = $"[<color=orange>2023年10月VRC同期会ポスター {version}</color>]{logText}";
            switch (logType)
            {
                case LogType.Warning:
                    Debug.LogWarning(log);
                    break;
                case LogType.Error:
                    Debug.LogError(log);
                    break;
                case LogType.Log:
                    Debug.Log(log);
                    break; 
                default:
                    Debug.Log(log);
                    break;
            }
        }
    }
}