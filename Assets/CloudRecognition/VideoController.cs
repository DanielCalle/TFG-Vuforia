using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

[RequireComponent(typeof(VideoPlayer))]
public class VideoController : MonoBehaviour
{
    #region PRIVATE_MEMBERS

    private VideoPlayer videoPlayer;
    private Button playButton;
    private Button pauseButton;
    #endregion //PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS

    void Start()
    {

        videoPlayer = GetComponent<VideoPlayer>();

        playButton = videoPlayer.transform.Find("PlayButton").GetComponent<Button>();
        pauseButton = videoPlayer.transform.Find("PauseButton").GetComponent<Button>();

        Utility.showHide(pauseButton.gameObject, false);

        // Setup Delegates
        videoPlayer.errorReceived += HandleVideoError;
        videoPlayer.started += HandleStartedEvent;
        videoPlayer.prepareCompleted += HandlePrepareCompleted;
        videoPlayer.seekCompleted += HandleSeekCompleted;
        videoPlayer.loopPointReached += HandleLoopPointReached;

        LogClipInfo();
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

    #endregion // MONOBEHAVIOUR_METHODS


    #region PUBLIC_METHODS

    public void Play()
    {
        if (videoPlayer)
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

    #endregion // PUBLIC_METHODS


    #region PRIVATE_METHODS

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

    #endregion // PRIVATE_METHODS


    #region DELEGATES

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

    #endregion //DELEGATES

}
