using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

[RequireComponent(typeof(VideoPlayer))]
public class VideoUrlController : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private Button playButton;
    private Button pauseButton;
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        playButton = videoPlayer.transform.Find("PlayButtonUrl").GetComponent<Button>();
        pauseButton = videoPlayer.transform.Find("PauseButtonUrl").GetComponent<Button>();

        if (!string.IsNullOrEmpty(videoPlayer.url))
        {
            StartCoroutine(prepareVideo());
        }
    }

    IEnumerator prepareVideo()
    {
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
    }

    void Update()
    {
        Utility.showHide(playButton.gameObject, !videoPlayer.isPlaying);
        Utility.showHide(pauseButton.gameObject, videoPlayer.isPlaying);
    }

    void OnApplicationPause(bool pause)
    {
        Debug.Log("OnApplicationPause(" + pause + ") called.");
        if (pause)
            Pause();
    }

    public void Play()
    {
        if (videoPlayer && videoPlayer.isPrepared)
        {
            Debug.Log("Play Video");
            videoPlayer.Play();
            Utility.showHide(playButton.gameObject, false);
            Utility.showHide(pauseButton.gameObject, true);
        }
    }

    public void Pause()
    {
        if (videoPlayer)
        {
            Debug.Log("Pause Video");
            videoPlayer.Pause();
            Utility.showHide(playButton.gameObject, true);
            Utility.showHide(pauseButton.gameObject, false);
        }
    }

    private void PauseAudio(bool pause)
    {
        for (ushort trackNumber = 0; trackNumber < videoPlayer.audioTrackCount; ++trackNumber)
        {
            if (pause)
                videoPlayer.GetTargetAudioSource(trackNumber).Pause();
            else
                videoPlayer.GetTargetAudioSource(trackNumber).UnPause();
        }
    }

    private void LogClipInfo()
    {
        if (videoPlayer.clip != null)
        {
            string stats =
                "\nName: " + videoPlayer.clip.name +
                "\nAudioTracks: " + videoPlayer.clip.audioTrackCount +
                "\nFrames: " + videoPlayer.clip.frameCount +
                "\nFPS: " + videoPlayer.clip.frameRate +
                "\nHeight: " + videoPlayer.clip.height +
                "\nWidth: " + videoPlayer.clip.width +
                "\nLength: " + videoPlayer.clip.length +
                "\nPath: " + videoPlayer.clip.originalPath;

            Debug.Log(stats);
        }
    }

    void HandleVideoError(VideoPlayer video, string errorMsg)
    {
        Debug.LogError("Error: " + video.clip.name + "\nError Message: " + errorMsg);
    }

    void HandleStartedEvent(VideoPlayer video)
    {
        Debug.Log("Started: " + video.clip.name);
    }

    void HandlePrepareCompleted(VideoPlayer video)
    {
        Debug.Log("Prepare Completed: " + video.clip.name);
    }

    void HandleSeekCompleted(VideoPlayer video)
    {
        Debug.Log("Seek Completed: " + video.clip.name);
    }

    void HandleLoopPointReached(VideoPlayer video)
    {
        Debug.Log("Loop Point Reached: " + video.clip.name);
        Utility.showHide(playButton.gameObject, true);
    }
}
