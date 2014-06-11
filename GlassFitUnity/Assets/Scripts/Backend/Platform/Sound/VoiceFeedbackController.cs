using UnityEngine;
using System;
using System.Collections.Generic;
using RaceYourself.Models;

public class VoiceFeedbackController : MonoBehaviour
{
    Log log = new Log ("VoiceFeedbackController");

    public AudioSource audioSource;
    public AudioClip paceSlower;
    public AudioClip paceSimilar;
    public AudioClip paceFaster;
    public AudioClip distanceBehind;
    public AudioClip distanceSimilar;
    public AudioClip distanceAhead;
    public AudioClip outlookLose;
    public AudioClip outlookDraw;
    public AudioClip outlookWin;

    public PlayerPosition player;
    public RYWorldObject opponent;
    public Track track;

    private long lastUpdateTime;

    public VoiceFeedbackController() {
        log.info ("Created");
        this.lastUpdateTime = 0;
    }

    public VoiceFeedbackController (PlayerPosition player, RYWorldObject opponent, Track track)
    {
        log.info ("Created");
        this.player = player;
        this.opponent = opponent;
        this.track = track;
        this.lastUpdateTime = 0L;
    }

    void Update()
    {
        if (player == null || opponent == null || track == null || !player.IsTracking)
            return;

        log.info ("Updating");

        // play queued clip if there's nothing currently playing
        if (!audioSource.isPlaying && audioQueue.Count > 0)
        {
            audioSource.clip = audioQueue.Dequeue();
            audioSource.Play();
        }
                                  
                                  // custom feedback just after start
        if (lastUpdateTime == 0 && player.Time > 5000)
        {
            log.info ("Playing 5-sec feedback");
            sayDistanceDelta ();
            lastUpdateTime = player.Time;
            return;
        }

        // regular feedback throughout race
        if (player.Time > lastUpdateTime + 10000)
        {
            log.info ("Playing regular feedback: " + player.Time + "s into race");
            sayDistanceDelta();
            sayPaceDelta();
            sayOutlook();
            lastUpdateTime = player.Time;
        }
    }

    private const float SIMILAR_DISTANCE_THRESHOLD = 5;  // m ... may need to use % too
    private void sayDistanceDelta()
    {
        if (player.Distance > opponent.getRealWorldDist() + SIMILAR_DISTANCE_THRESHOLD)
            play(distanceAhead);
        else if (player.Distance < opponent.getRealWorldDist() - SIMILAR_DISTANCE_THRESHOLD)
            play(distanceBehind);
        else
            play(distanceSimilar);
    }

    private const float SIMILAR_SPEED_THRESHOLD = 0.1f;  // m/s
    private void sayPaceDelta()
    {
        if (player.Pace > opponent.getRealWorldSpeed() + SIMILAR_SPEED_THRESHOLD)
            play(paceFaster);
        else if (player.Pace < opponent.getRealWorldSpeed() - SIMILAR_SPEED_THRESHOLD)
            play(paceSlower);
        else
            play(paceSimilar);
    }

    private void sayOutlook()
    {
        play(outlookWin);
    }

    private Queue<AudioClip> audioQueue = new Queue<AudioClip> (5);
    private void play(AudioClip audioClip)
    {
        audioQueue.Enqueue(audioClip);
    }
}

