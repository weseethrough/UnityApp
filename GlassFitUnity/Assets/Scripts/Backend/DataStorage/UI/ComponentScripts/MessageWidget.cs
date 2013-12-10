using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// message data class containing all information required to display single message to the user
/// </summary>
public class MessageData
{
    public string title;
    public string content;
    public string iconName;
    public UIAtlas atlas;
}

/// <summary>
/// widget component controlling animations and display order of the messages
/// </summary>
public class MessageWidget : MonoBehaviour
{
    const string LABEL_TITLE_ENTER      = "MessageEnterAnim";
    const string LABEL_CONTENT_ENTER    = "MessageContentEnterAnim";
    const string ICON_ENTER             = "MessageIconEnterAnim";
    const float MAX_TIME_VISIBLE        = 2.0f;

    private static List<MessageData> messageStack;

    public GameObject title;
    public GameObject content;
    public GameObject icon;

    UILabel labelTitle;
    UILabel labelContent;
    UISprite iconSprite;

    MessageData currentAnimation;
    bool animationVisible;    
    float curentVisibleTime;

    State currentState;

    enum State
    {
        idle,
        opening,
        showing,
        closing,
        maxStates
    }

    /// <summary>
    /// standard unity function initializing widget
    /// </summary>
    /// <returns></returns>
    void Start()
    {
        JumpToStart(title, LABEL_TITLE_ENTER);
        JumpToStart(content, LABEL_CONTENT_ENTER);
        JumpToStart(icon, ICON_ENTER);
        animationVisible = false;

        if (title != null)
        {
            labelTitle = title.GetComponentInChildren<UILabel>();
        }
        if (content != null)
        {
            labelContent = content.GetComponentInChildren<UILabel>();
        }
        if (icon != null)
        {
            iconSprite = icon.GetComponentInChildren<UISprite>();
        }

        currentState = State.idle;         
        messageStack = new List<MessageData>();
        //AddMessage("CONGRATULATIONS!", "This is my content", "activity_bike");
        //AddMessage("HEY!", "You have just get invited", "activity_run");
    }

    /// <summary>
    /// static function which adds message data to the queue to be shown to the user when widget instance is initialized
    /// </summary>
    /// <param name="title">title of the message, for example: "Congratulations!"</param>
    /// <param name="content">message content with more detailed description of what we want to tell to the user</param>
    /// <param name="iconName">icon to be shown along with the message. Its important to understand that icon animate from 0.5 to 0.8 of its maximum screen size when planing size of the icon. It could be changed by changing scael on icon container</param>
    /// <returns></returns>
    static public void AddMessage(string title, string content, string iconName)
    {
        GraphComponent gc = null;
        if (iconName != null && iconName.Length > 0)
        {
            gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        }
        AddMessage(title, content, iconName, gc.m_defaultHexagonalAtlas);        
    }

    /// <summary>
    /// static function which adds message data to the queue to be shown to the user when widget instance is initialized
    /// </summary>
    /// <param name="title">title of the message, for example: "Congratulations!"</param>
    /// <param name="content">message content with more detailed description of what we want to tell to the user</param>
    /// <param name="iconName">icon to be shown along with the message. Its important to understand that icon animate from 0.5 to 0.8 of its maximum screen size when planing size of the icon. It could be changed by changing scael on icon container</param>
    /// <param name="atlasContainingIcon">atlass instance if not default altas is used. </param>
    /// <returns></returns>
    static public void AddMessage(string title, string content, string iconName, UIAtlas atlasContainingIcon)
    {
        MessageData md = new MessageData();        
        md.title = title;
        md.content = content; 
        md.atlas = atlasContainingIcon;
        md.iconName = iconName;

        messageStack.Add(md);
    }

    /// <summary>
    /// standard unity update function called once every frame. Used for state progress of internal message manager
    /// </summary>
    /// <returns></returns>
    void Update()
    {        
        switch (currentState)
        {

            case State.idle:
                if ( messageStack.Count > 0)
                {
                    currentAnimation = messageStack[0];
                    UILabel label = title.GetComponent<UILabel>();

                    if (currentAnimation.title != null && currentAnimation.title.Length > 0)
                    {
                        labelTitle.text = currentAnimation.title;
                        Play(title, LABEL_TITLE_ENTER, true, false);
                    }
                    else
                    {
                        JumpToStart(title, LABEL_TITLE_ENTER);
                    }

                    if (currentAnimation.content != null && currentAnimation.content.Length > 0)
                    {
                        labelContent.text = currentAnimation.content;
                        Play(content, LABEL_CONTENT_ENTER, true, false);
                    }
                    else
                    {
                        JumpToStart(content, LABEL_CONTENT_ENTER);
                    }

                    if (currentAnimation.atlas      != null &&
                        currentAnimation.iconName   != null &&
                        currentAnimation.iconName.Length > 0)
                    {
                        iconSprite.atlas = currentAnimation.atlas;
                        iconSprite.spriteName = currentAnimation.iconName;

                        Play(icon, ICON_ENTER, true, true);
                    }
                    else
                    {
                        JumpToStart(icon, ICON_ENTER);
                    }
                    animationVisible = true;
                    curentVisibleTime = 0.0f;
                    currentState = State.opening;
                }
                break;
            case State.showing:
                curentVisibleTime += Time.deltaTime;
                Debug.Log(curentVisibleTime + " vs " + MAX_TIME_VISIBLE);
                if (curentVisibleTime >= MAX_TIME_VISIBLE)
                {
                    currentState = State.closing;
                    Play(title,     LABEL_TITLE_ENTER,      AnimationOrTween.Direction.Reverse, false, false);
                    Play(content,   LABEL_CONTENT_ENTER,    AnimationOrTween.Direction.Reverse, false, false);
                    Play(icon,      ICON_ENTER,             AnimationOrTween.Direction.Reverse, false, true);
                    messageStack.RemoveAt(0);
                }
                break;
        }
    }

    /// <summary>
    /// Function to trigger animation on specified clip
    /// </summary>
    /// <param name="go">game object containing animation component among children. Only one is expected</param>
    /// <param name="animName">namination name existing on animation component</param>
    /// <param name="requireReset">do you want to reset anmiation before play?</param>
    /// <param name="notificationRequired">should this class eb notified by this play call and progress its state?</param>
    /// <returns></returns>
    private void Play(GameObject go, string animName, bool requireReset, bool notificationRequired)
    {
        Play(go, animName, AnimationOrTween.Direction.Forward, requireReset, notificationRequired);
    }

    /// <summary>
    /// Function to trigger animation on specified clip
    /// </summary>
    /// <param name="go">game object containing animation component among children. Only one is expected</param>
    /// <param name="animName">namination name existing on animation component</param>
    /// <param name="direction">direction which animation should be animated to</param>
    /// <param name="requireReset">do you want to reset anmiation before play?</param>
    /// <param name="notificationRequired">should this class eb notified by this play call and progress its state?</param>
    /// <returns></returns>
    private void Play(GameObject go, string animName, AnimationOrTween.Direction direction, bool requireReset, bool notificationRequired)
    {
        if (go == null) return;
        Animation anim = go.GetComponentInChildren<Animation>();
        if (anim == null) return;

        ActiveAnimation activeAnim = ActiveAnimation.Play(anim, animName, direction);

        if(activeAnim != null)
        {
            if (notificationRequired)
            {
                EventDelegate ed = new EventDelegate(this, "AnimFinished");
                activeAnim.onFinished.Add(ed);
            }
                        
            if (requireReset) activeAnim.Reset();            
        }
    }

    /// <summary>
    /// simplified animate call which instead just forces animation to jump to starting point and stop
    /// </summary>
    /// <param name="go">game object containing animation component among children. Only one is expected </param>
    /// <param name="animName">namination name existing on animation component</param>
    /// <returns></returns>
    private void JumpToStart(GameObject go, string animName)
    {
        if (go == null) return;
        Animation anim = go.GetComponentInChildren<Animation>();
        if (anim == null) return;

        anim[animName].time = 0.001f;
    }

    /// <summary>
    /// Animation function which should be triggered when longest animation among sequence ends, it allows to progress to next step (eg finished enter animation or finished closing animation)
    /// </summary>
    /// <returns></returns>
    public void AnimFinished()
    {
        Debug.Log("Auto switch");
        currentState = (State)((int)currentState + 1);
        if (currentState == State.maxStates)
        {
            currentState = State.idle;
        }
    }
}
