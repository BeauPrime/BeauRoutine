using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Example3_Script : MonoBehaviour
{
    [SerializeField]
    private Example3_Menu m_Menu;

    [SerializeField]
    private Button m_StartButton;

    [SerializeField]
    private ToggleGroup m_StyleGroup;

    private void Start()
    {
        m_StartButton.onClick.AddListener(Clicked_StartButton);
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
}
