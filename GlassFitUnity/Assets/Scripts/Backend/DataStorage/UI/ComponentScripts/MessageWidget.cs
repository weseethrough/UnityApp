using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageData
{
    public string title;
    public string content;
    public string iconName;
    public UIAtlas atlas;
}

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
        iddle,
        opening,
        showing,
        closing,
        maxStates
    }

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

        currentState = State.iddle;
         
        AddMessage("CONGRATULATIONS!", "This is my content", "activity_bike");
        AddMessage("HEY!", "You have just get invited", "activity_run");
    }

    static public void AddMessage(string title, string content, string iconName)
    {
        GraphComponent gc = null;
        if (iconName != null && iconName.Length > 0)
        {
            gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        }
        AddMessage(title, content, iconName, gc.m_defaultHexagonalAtlas);        
    }

    static public void AddMessage(string title, string content, string iconName, UIAtlas atlasContainingIcon)
    {
        MessageData md = new MessageData();        
        md.title = title;
        md.content = content; 
        md.atlas = atlasContainingIcon;
        md.iconName = iconName;

        if (messageStack == null) messageStack = new List<MessageData>();
        messageStack.Add(md);
    }

    void Update()
    {
        
        switch (currentState)
        {

            case State.iddle:
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

    private void Play(GameObject go, string animName, bool requireReset, bool notificationRequired)
    {
        Play(go, animName, AnimationOrTween.Direction.Forward, requireReset, notificationRequired);
    }

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

    private void JumpToStart(GameObject go, string animName)
    {
        if (go == null) return;
        Animation anim = go.GetComponentInChildren<Animation>();
        if (anim == null) return;

        anim[animName].time = 0.0f;
    }

    public void AnimFinished()
    {
        Debug.Log("Auto switch");
        currentState = (State)((int)currentState + 1);
        if (currentState == State.maxStates)
        {
            currentState = State.iddle;
        }
    }
}
