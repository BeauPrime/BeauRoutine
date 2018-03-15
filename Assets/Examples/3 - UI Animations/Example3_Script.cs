using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BeauRoutine;
using System.Collections;

namespace BeauRoutine.Examples
{
    public class Example3_Script : MonoBehaviour
    {
        [SerializeField]
        private Example3_Menu m_Menu;

        [SerializeField]
        private Button m_StartButton;

        [SerializeField]
        private Button m_SwapSidesButton;

        [SerializeField]
        private ToggleGroup m_StyleGroup;

        private Routine m_SwapSidesRoutine;
        private bool m_OnRightSide = true;

        private void Start()
        {
            m_StartButton.onClick.AddListener(Clicked_StartButton);
            m_SwapSidesButton.onClick.AddListener(Clicked_SwapSides);
        }

        private void Clicked_StartButton()
        {
            Toggle toggle = m_StyleGroup.ActiveToggles().FirstOrDefault();
            if (toggle == null)
                return;

            string toggleName = toggle.gameObject.name;
            if (toggleName == "Scale Up")
                m_Menu.Open(Example3_Menu.AnimationStyle.Scale);
            else if (toggleName == "Newspaper Spin")
                m_Menu.Open(Example3_Menu.AnimationStyle.Spin);
            else if (toggleName == "Slide In")
                m_Menu.Open(Example3_Menu.AnimationStyle.Slide);
            else if (toggleName == "Fade In")
                m_Menu.Open(Example3_Menu.AnimationStyle.Fade);
        }

        private void Clicked_SwapSides()
        {
            if (m_SwapSidesRoutine)
                return;

            m_OnRightSide = !m_OnRightSide;
            m_SwapSidesRoutine = Routine.Start(this, SwapTo(m_OnRightSide ? 1 : 0));
        }

        private IEnumerator SwapTo(float inXAlign)
        {
            RectTransform rect = (RectTransform)transform;

            yield return Routine.Combine(

                // Set the pivot to the left or right to ensure scaling works how we expect
                rect.PivotTo(inXAlign, 0.5f, Axis.X).Ease(Curve.QuartOut),

                // Set the anchor on the x axis to the left or right
                rect.AnchorTo(new Vector2(inXAlign, 0), 0.5f, Axis.X).Ease(Curve.QuartOut),

                // Just for fun, let's also change the size of the panel
                rect.SizeDeltaTo(300 - 100 * inXAlign, 0.5f, Axis.X).Ease(Curve.QuartOut)
                );
        }
    }
}