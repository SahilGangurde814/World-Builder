using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UICurrentObjectSelecter : MonoBehaviour
{
    [SerializeField] private GameObject objectSelecterPanel;
    [SerializeField, Range(0.1f, 3f)] private float objectSelecterViewTime = 1f;
    [SerializeField] private Image[] imageArr;

    private float scroll = 0f;
    private Coroutine turnOffCoroutine;
    private int currentIndex = 0;

    private void Start()
    {
        //imageArr = objectSelecterPanel.GetComponentsInChildren<Image>();   // takes parent into account 
        //nextObjectSelecter();
    }

    private void Update()
    {
        scroll = Input.GetAxis("Mouse ScrollWheel");

        if(scroll != 0f)
        {
            PanelState(true);
            Debug.Log(scroll);

            if(turnOffCoroutine != null)
            {
                StopCoroutine(turnOffCoroutine);
            }

            turnOffCoroutine = StartCoroutine(WaitToTurnOff(objectSelecterViewTime));
            
            nextObjectSelecter();
        }

    }

    void PanelState(bool activeState)
    {
        objectSelecterPanel.SetActive(activeState);
    }

    IEnumerator WaitToTurnOff(float time)
    {
        yield return new WaitForSeconds(time);
        objectSelecterPanel.SetActive(false);
        turnOffCoroutine = null;
    }

    void nextObjectSelecter()
    {
        int previousIndex = currentIndex;
       
        if (scroll > 0f)
        {
            currentIndex = (currentIndex - 1 + imageArr.Length) % imageArr.Length;
        }
        else
        {
            currentIndex = (currentIndex + 1) % imageArr.Length;
        }

        imageArr[previousIndex].color = Color.white;
        imageArr[currentIndex].color = Color.red;
    }
}
