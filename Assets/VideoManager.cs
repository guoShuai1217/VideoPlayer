/*
 *		Description : 
 *
 *		CreatedBy : guoShuai
 *
 *		DataTime : 2018.09
 */
using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
public class VideoManager : MonoBehaviour
{
    public static VideoManager Instance;

    private MediaPlayer loadPlayer;

    private DisplayUGUI disPlayUGUI;
    
    private string urlNetWork;

    private Slider audioSlider;

    private Slider _videoSeekSlider;

    private RectTransform _bufferedSliderRect;

    private Image _bufferedSliderImage;

    private void Awake()
    {
        Instance = this;
        InitUI();        
    }

    void InitUI()
    {
        disPlayUGUI = transform.Find("AVPro Video").GetComponent<DisplayUGUI>();
        loadPlayer = GameObject.Find("MediaPlayer").GetComponent<MediaPlayer>();
        disPlayUGUI._mediaPlayer = loadPlayer;

        Button[] btns = transform.GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            BtnArgs args = new BtnArgs(i + 1, btns[i].gameObject);
            btns[i].onClick.AddListener(() => { OnClickBtn(args); });
        }

        audioSlider = transform.Find("audioSlider").GetComponent<Slider>();
        audioSlider.value = 1;
        audioSlider.onValueChanged.AddListener(delegate 
        {
            loadPlayer.Control.SetVolume(audioSlider.value);
        });

        _videoSeekSlider = transform.Find("VideoSeekSlider").GetComponent<Slider>();

        _bufferedSliderRect = _videoSeekSlider.transform.Find("Buffered Area/Fill").GetComponent<RectTransform>();
        _bufferedSliderImage = _bufferedSliderRect.GetComponent<Image>();
    }

    private void OnClickBtn(BtnArgs args)
    {
        switch (args.Id)
        {
            case 1:
                OpenFile();
                break;
            case 2:
                loadPlayer.Play();
                break;
            case 3:
                loadPlayer.Pause();
                break;
            case 4:
                loadPlayer.Rewind(false);
                break;
            case 5:
                loadPlayer.Stop();
                break;
            default:
                break;
        }
    }

    public void OnVideoSeekSlider()
    {     
       loadPlayer.Control.Seek(_videoSeekSlider.value * loadPlayer.Info.GetDurationMs()); 
    }
    bool _wasPlayingOnScrub;
    public void OnVideoSliderDown()
    {
        if (loadPlayer)
        {
            _wasPlayingOnScrub = loadPlayer.Control.IsPlaying();
            if (_wasPlayingOnScrub)
            {
                loadPlayer.Control.Pause();
                //					SetButtonEnabled( "PauseButton", false );
                //					SetButtonEnabled( "PlayButton", true );
            }
            OnVideoSeekSlider();
        }
    }
    public void OnVideoSliderUp()
    {
        if (loadPlayer && _wasPlayingOnScrub)
        {
            loadPlayer.Control.Play();
            _wasPlayingOnScrub = false;

            //				SetButtonEnabled( "PlayButton", false );
            //				SetButtonEnabled( "PauseButton", true );
        }
    }

    void Update()
    {
        if (loadPlayer && loadPlayer.Info != null && loadPlayer.Info.GetDurationMs() > 0f)
        {
            float time = loadPlayer.Control.GetCurrentTimeMs();
            float duration = loadPlayer.Info.GetDurationMs();
            float d = Mathf.Clamp(time / duration, 0.0f, 1.0f);

            // Debug.Log(string.Format("time: {0}, duration: {1}, d: {2}", time, duration, d));

           // _setVideoSeekSliderValue = d;
            _videoSeekSlider.value = d;

            if (_bufferedSliderRect != null)
            {
                float t1 = 0f;
                float t2 = loadPlayer.Control.GetBufferingProgress();
                if (t2 <= 0f)
                {
                    if (loadPlayer.Control.GetBufferedTimeRangeCount() > 0)
                    {
                        loadPlayer.Control.GetBufferedTimeRange(0, ref t1, ref t2);
                        t1 /= loadPlayer.Info.GetDurationMs();
                        t2 /= loadPlayer.Info.GetDurationMs();
                    }
                }

                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                if (_bufferedSliderImage != null &&
                    _bufferedSliderImage.type == Image.Type.Filled)
                {
                    _bufferedSliderImage.fillAmount = d;
                }
                else
                {
                    anchorMin[0] = t1;
                    anchorMax[0] = t2;
                }

                _bufferedSliderRect.anchorMin = anchorMin;
                _bufferedSliderRect.anchorMax = anchorMax;
            }
        }
    }


    private void OpenFile()
    {
        if (loadPlayer)
            loadPlayer.Stop();
        
        OpenFileName openFileName = new OpenFileName();

        openFileName.structSize = Marshal.SizeOf(openFileName);

        openFileName.filter = "视频文件(*.mp4)\0*.mp4";
        // openFileName.filter = "视频文件|*.mp4;*.avi;*.mov;*.mpg;*.mpeg;*.asf";

        openFileName.file = new string(new char[256]);

        openFileName.maxFile = openFileName.file.Length;

        openFileName.fileTitle = new string(new char[64]);

        openFileName.maxFileTitle = openFileName.fileTitle.Length;

        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径

        openFileName.title = "窗口标题";

        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (LocalDialog.GetOpenFileName(openFileName))
        {

            Debug.Log(openFileName.file); // 全路径
            urlNetWork = openFileName.file.Replace('\\', '/');
            //设置播放路径
            loadPlayer.m_VideoPath = urlNetWork;
            loadPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, loadPlayer.m_VideoPath, true);
            Debug.Log(openFileName.fileTitle); // 文件名

        }

    }

}

public class BtnArgs
{
    public int Id;
    public GameObject obj;

    public BtnArgs(int id , GameObject obj)
    {
        this.Id = id;
        this.obj = obj;
    }
}