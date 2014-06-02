using UnityEngine;
using System;
using RaceYourself.Models;

public class VoiceFeedbackController : MonoBehaviour
{
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

    private PlayerPosition player;
    private RYWorldObject opponent;
    private Track track;

    private float lastUpdateTime;

    public VoiceFeedbackController (PlayerPosition player, RYWorldObject opponent, Track track)
    {
        this.player = player;
        this.opponent = opponent;
        this.track = track;
        this.lastUpdateTime = 0;
    }

    void Update()
    {
        if (!player.IsTracking)
            return;

        // custom feedback just after start
        if (lastUpdateTime == 0 && player.Time > 5)
        {
            sayDistanceDelta ();
            lastUpdateTime = player.Time;
            return;
        }

        // regular feedback throughout race
        if (player.Time > lastUpdateTime + 10)
        {
            sayDistanceDelta();
            sayPaceDelta();
            sayOutlook();
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

    private void play(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}

