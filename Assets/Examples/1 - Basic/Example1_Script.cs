using UnityEngine;
using System.Collections;
using BeauRoutine;
using UnityEngine.UI;

public class Example1_Script : MonoBehaviour
{
    [SerializeField]
    private Button m_Button1;

    [SerializeField]
    private Button m_Button2;

    [SerializeField]
    private Button m_Button3;

    [SerializeField]
    private Button m_PauseButton;

    [SerializeField]
    private Button m_ResumeButton;

    private Routine m_Button2Routine;
    private Routine m_Button3Routine;

    private void Start()
    {
        m_Button1.onClick.AddListener(Button1_Click);
        m_Button2.onClick.AddListener(Button2_Click);
        m_Button3.onClick.AddListener(Button3_Click);

        m_PauseButton.onClick.AddListener(PauseButton_Click);
        m_ResumeButton.onClick.AddListener(ResumeButton_Click);
    }

    private void Button1_Click()
    {
        Routine.Start(this, Button1_Routine());
    }

    private void Button2_Click()
    {
        m_Button2Routine.Replace(this, Button2_Routine());
    }

    private void Button3_Click()
    {
        if (!m_Button3Routine)
            m_Button3Routine.Replace(this, Button3_Routine());
    }

    private void PauseButton_Click()
    {
        Routine.PauseAll(this);
    }

    private void ResumeButton_Click()
    {
        Routine.ResumeAll(this);
    }

    private IEnumerator Button1_Routine()
    {
        Debug.Log("Button 1 clicked. Waiting 1 second.");
        yield return 1.0f;
        Debug.Log("Button 1 was clicked 1 second ago.");
    }

    private IEnumerator Button2_Routine()
    {
        Debug.Log("Button 2 clicked. Waiting 1 second.");
        m_Button2.image.color = Color.red;
        yield return 1.0f;
        Debug.Log("Button 2 was clicked 1 second ago.");
        m_Button2.image.color = Color.white;
    }

    private IEnumerator Button3_Routine()
    {
        while(true)
        {
            m_Button3.image.color = Color.green;
            yield return 1.0f;
            m_Button3.image.color = Color.white;
            yield return 1.0f;
        }
    }
}
