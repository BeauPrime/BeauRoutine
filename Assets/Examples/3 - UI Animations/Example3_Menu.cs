using BeauRoutine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Example3_Menu : MonoBehaviour
{
    public enum AnimationStyle
    {
        Scale,
        Spin,
        Slide,
        Fade
    }

    [SerializeField]
    private RectTransform m_Root;

    [SerializeField]
    private CanvasGroup m_RootGroup;

    [SerializeField]
    private Button m_CloseButton;

    [Header("Scale Settings")]

    [SerializeField]
    private float m_StartingScale = 0.8f;

    [SerializeField]
    private TweenSettings m_ScaleUpSettings = new TweenSettings(0.5f, Curve.BackOut);

    [SerializeField]
    private TweenSettings m_ScaleDownSettings = new TweenSettings(0.5f, Curve.CubeOut);

    [Header("Spin Settings")]

    [SerializeField]
    private int m_SpinCount = 4;

    [SerializeField]
    private TweenSettings m_SpinInSettings = new TweenSettings(1f, Curve.Smooth);

    [SerializeField]
    private TweenSettings m_SpinOutSettings = new TweenSettings(1f, Curve.Smooth);

    [Header("Slide Settings")]

    [SerializeField]
    private float m_SlideDistance = 50;

    [SerializeField]
    private TweenSettings m_SlideInSettings = new TweenSettings(0.5f, Curve.BackOut);

    [SerializeField]
    private TweenSettings m_SlideOutSettings = new TweenSettings(0.5f, Curve.CubeOut);

    [Header("Fade Settings")]

    [SerializeField]
    private TweenSettings m_FadeSettings = new TweenSettings(0.5f, Curve.Linear);

    private bool m_Open = false;
    private AnimationStyle m_Style = AnimationStyle.Scale;

    private Routine m_TransitionAnimation;
    private Routine m_ButtonSpinAnimation;

    private void Awake()
    {
        m_CloseButton.onClick.AddListener(Close);
        m_Root.gameObject.SetActive(false);
    }

    // Call to action animation for the button
    private IEnumerator ButtonCallToAction()
    {
        yield return m_CloseButton.transform
            .RotateTo(10, 1.5f, Axis.Z, Space.Self) // Rotate 10 degrees on the Z axis
            .Wave(Wave.Function.Sin, 1) // Apply a sine wave to oscillate from 10 to -10
            .Loop() // Loop so it never ends on its owm
            .RevertOnCancel(false); // Once it ends, revert back to our original Z axis rotation.
    }

    // Open the menu with a certain style
    public void Open(AnimationStyle inStyle)
    {
        // If we've already opened or we're still animating,
        // don't start the open animation
        if (m_Open || m_TransitionAnimation)
            return;
        m_Open = true;

        m_Style = inStyle;

        // Select the appropriate opening animation based on our style
        switch (m_Style)
        {
            case AnimationStyle.Fade:
                m_TransitionAnimation.Replace(this, FadeIn());
                break;
            case AnimationStyle.Scale:
                m_TransitionAnimation.Replace(this, ScaleIn());
                break;
            case AnimationStyle.Slide:
                m_TransitionAnimation.Replace(this, SlideIn());
                break;
            case AnimationStyle.Spin:
                m_TransitionAnimation.Replace(this, SpinIn());
                break;
        }
    }

    // Close the menu with our current style
    public void Close()
    {
        // If we've already closed or we're still animating,
        // don't start the close animation
        if (!m_Open || m_TransitionAnimation)
            return;
        m_Open = false;

        // Select the appropriate closing animation based on our style
        switch(m_Style)
        {
            case AnimationStyle.Fade:
                m_TransitionAnimation.Replace(this, FadeOut());
                break;
            case AnimationStyle.Scale:
                m_TransitionAnimation.Replace(this, ScaleOut());
                break;
            case AnimationStyle.Slide:
                m_TransitionAnimation.Replace(this, SlideOut());
                break;
            case AnimationStyle.Spin:
                m_TransitionAnimation.Replace(this, SpinOut());
                break;
        }
    }

    #region Scale Animations

    // This animates the menu to the Open state
    private IEnumerator ScaleIn()
    {
        // Set our initial state
        m_Root.gameObject.SetActive(true);

        m_Root.SetAnchorPos(0, Axis.XY);
        m_Root.SetScale(m_StartingScale, Axis.XY);
        m_Root.SetRotation(0, Axis.Z);

        m_RootGroup.alpha = 0;
        m_RootGroup.interactable = false;

        // Perform the animation
        yield return Routine.Combine(
            m_Root.ScaleTo(1, m_ScaleUpSettings, Axis.XY), // Scale to 1
            m_RootGroup.FadeTo(1, m_ScaleUpSettings.Time) // Fade in
            );

        // Allow the user to interact
        m_RootGroup.interactable = true;
        m_ButtonSpinAnimation.Replace(this, ButtonCallToAction());
    }

    // This animates the menu to the Closed state
    private IEnumerator ScaleOut()
    {
        // Prevent the user from interacting
        m_RootGroup.interactable = false;
        m_ButtonSpinAnimation.Stop();

        // Perform the animation
        yield return Routine.Combine(
            m_Root.ScaleTo(m_StartingScale, m_ScaleDownSettings, Axis.XY), // Scale back to our original scale 
            m_RootGroup.FadeTo(0, m_ScaleDownSettings.Time) // Fade out
            );

        // Deactivate the menu
        m_RootGroup.gameObject.SetActive(false);
    }

    #endregion

    #region Spin Animations

    private IEnumerator SpinIn()
    {
        // Set our initial state
        m_RootGroup.gameObject.SetActive(true);

        m_Root.SetAnchorPos(0, Axis.XY);
        m_Root.SetScale(0, Axis.XY);
        m_Root.SetRotation(0, Axis.Z);

        m_RootGroup.alpha = 1;
        m_RootGroup.interactable = false;

        // Perform the animation
        yield return Routine.Combine(
            m_Root.ScaleTo(1, m_SpinInSettings, Axis.XY), // Scale to 1
            m_Root.RotateTo(360 * m_SpinCount, m_SpinInSettings, Axis.Z, Space.Self, AngleMode.Absolute) // Spin multiple times
            );

        // Allow the user to interact
        m_RootGroup.interactable = true;
        m_ButtonSpinAnimation.Replace(this, ButtonCallToAction());
    }

    private IEnumerator SpinOut()
    {
        // Prevent the user from interacting
        m_RootGroup.interactable = false;
        m_ButtonSpinAnimation.Stop();

        // Perform the animation
        yield return Routine.Combine(
            m_Root.ScaleTo(0, m_SpinOutSettings, Axis.XY), // Scale to 0
            m_Root.RotateTo(-360f * m_SpinCount, m_SpinOutSettings, Axis.Z, Space.Self, AngleMode.Absolute) // Spin in reverse
            );

        // Deactivate the menu
        m_RootGroup.gameObject.SetActive(false);
    }

    #endregion

    #region Slide Animations

    private IEnumerator SlideIn()
    {
        // Set our initial state
        m_RootGroup.gameObject.SetActive(true);

        m_Root.SetAnchorPos(-m_SlideDistance, Axis.Y);
        m_Root.SetScale(1, Axis.XY);
        m_Root.SetRotation(0, Axis.Z);
        
        m_RootGroup.alpha = 0;
        m_RootGroup.interactable = false;

        // Perform the animation
        yield return Routine.Combine(
            m_Root.AnchorTo(0, m_SlideInSettings, Axis.Y), // Move our anchoredPosition to 0
            m_RootGroup.FadeTo(1f, m_SlideInSettings.Time) // Fade in
            );

        // Allow the user to interact
        m_RootGroup.interactable = true;
        m_ButtonSpinAnimation.Replace(this, ButtonCallToAction());
    }

    private IEnumerator SlideOut()
    {
        // Prevent the user from interacting
        m_RootGroup.interactable = false;
        m_ButtonSpinAnimation.Stop();

        // Perform the animation
        yield return Routine.Combine(
            m_Root.AnchorTo(-m_SlideDistance, m_SlideOutSettings, Axis.Y), // Slide back to our original position
            m_RootGroup.FadeTo(0f, m_SlideOutSettings.Time) // Fade out
            );

        // Deactivate the menu
        m_RootGroup.gameObject.SetActive(false);
    }

    #endregion

    #region Fade Animations

    private IEnumerator FadeIn()
    {
        // Set our initial state
        m_RootGroup.gameObject.SetActive(true);

        m_Root.SetAnchorPos(0, Axis.Y);
        m_Root.SetScale(1, Axis.XY);
        m_Root.SetRotation(0, Axis.Z);

        m_RootGroup.alpha = 0;
        m_RootGroup.interactable = false;

        // Perform the animation
        yield return Routine.Combine(
            m_RootGroup.FadeTo(1f, m_FadeSettings)
            );

        // yielding a Routine.Combine with one item
        // is exactly the same as yielding that item by itself

        // Allow the user to interact
        m_RootGroup.interactable = true;
        m_ButtonSpinAnimation.Replace(this, ButtonCallToAction());
    }

    private IEnumerator FadeOut()
    {
        // Prevent the user from interacting
        m_RootGroup.interactable = false;
        m_ButtonSpinAnimation.Stop();

        // Perform the animation
        yield return Routine.Combine(
            m_RootGroup.FadeTo(0f, m_FadeSettings)
            );

        // Deactivate the menu
        m_RootGroup.gameObject.SetActive(false);
    }

    #endregion
}
