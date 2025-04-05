using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UICurrentObjectSelecter : MonoBehaviour
{
    [SerializeField] private GameObject objectSelecterPanel;
    [SerializeField, Range(0.1f, 3f)] private float objectSelecterViewTime = 1f;
    [SerializeField] private List<Image> imageArr;
    [SerializeField] private PlaceableObjectData objectPool;

    private float scroll = 0f;
    private Coroutine turnOffCoroutine;
    private int currentIndex = 0;

    Color previousIndexColor = Color.white;

    private void Start()
    {
        //imageArr = objectSelecterPanel.GetComponentsInChildren<Image>();   // takes parent into account 
        //nextObjectSelecter();
        ObjectSelectionUiSet();
    }

    private void Update()
    {
        scroll = Input.GetAxis("Mouse ScrollWheel");

        if(scroll != 0f)
        {
            PanelState(true);
            //Debug.Log(scroll);

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
            currentIndex = (currentIndex - 1 + imageArr.Count) % imageArr.Count;
        }
        else
        {
            currentIndex = (currentIndex + 1) % imageArr.Count;
        }
        
        imageArr[previousIndex].color = previousIndexColor;
        previousIndexColor = imageArr[currentIndex].color;
        imageArr[currentIndex].color = Color.red;
    }

    void ObjectSelectionUiSet()
    {
        for(int i = 0; i < objectPool.placeablePrefabs.Count; i++)
        {
            PrefabTypes prefabType = objectPool.placeablePrefabs[i].PrefabType;
            PlaceablePrefabs currentPrefab = objectPool.GetCurrentPrefab(prefabType);

            imageArr[i].sprite = currentPrefab.objectPreviewImages;
            //if(prefabType == PrefabTypes.Wall)
            //{
            //    imageArr[i].sprite = currentPrefab.objectPreviewImages;
            //}
            //else if(prefabType == PrefabTypes.Window)
            //{
            //    imageArr[i].sprite = currentPrefab.objectPreviewImages;
            //}
            //else if (prefabType == PrefabTypes.Door1)
            //{
            //    imageArr[i].sprite = currentPrefab.objectPreviewImages;
            //}
        }

        int objectPoolCount = objectPool.placeablePrefabs.Count;
        for (int i = imageArr.Count - 1; i >= objectPoolCount; i--)
        {
            Destroy(imageArr[i].gameObject);
            imageArr.RemoveAt(i);
        }
    }
}
