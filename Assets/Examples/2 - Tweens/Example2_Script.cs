using BeauRoutine;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Example2_Script : MonoBehaviour
{
    [SerializeField]
    private Transform m_Object;

    [SerializeField]
    private Transform m_Target;

    [SerializeField]
    private Button m_StartButton;

    [SerializeField]
    private Slider m_DurationSlider;

    [SerializeField]
    private ToggleGroup m_EaseGroup;

    private TransformState m_OriginalState;
    private Routine m_ObjectTween;

    private void Start()
    {
        m_StartButton.onClick.AddListener(Clicked_StartButton);

        m_OriginalState = TransformState.WorldState();
        m_OriginalState.Refresh(m_Object, TransformProperties.Position);
    }

    private void Clicked_StartButton()
    {
        Tween tween = ApplySettings(MakeMoveTween());
        m_ObjectTween.Replace(this, tween);
    }

    private Tween MakeMoveTween()
    {
        m_OriginalState.Apply(m_Object, TransformProperties.Position);
        return m_Object.MoveTo(m_Target, 2f, Axis.XY);
    }

    private Tween ApplySettings(Tween inTween)
    {
        inTween.Duration(m_DurationSlider.value);

        Toggle toggle = m_EaseGroup.ActiveToggles().FirstOrDefault();
        if (toggle != null)
        {
            string toggleName = toggle.gameObject.name;
            if (toggleName == "BackOut")
                inTween.Ease(Curve.BackOut);
            else if (toggleName == "CubeOut")
                inTween.Ease(Curve.CubeOut);
            else if (toggleName == "Smooth")
                inTween.Ease(Curve.Smooth);
        }

        return inTween;
    }
}
