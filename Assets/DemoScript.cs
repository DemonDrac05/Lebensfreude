using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    public GameObject gameObj;

    public KeyCode keycode;

    private void Start()
    {
        gameObj.SetActive(false);
    }

    private void Update()
    {
        HandleKeyHoldInput();
    }

    private void HandleKeyHoldInput()
    {
        if (Input.GetKeyDown(keycode))
        {
            gameObj.SetActive(true);
        }
        else if (Input.GetKeyUp(keycode))
        {
            var layout = gameObj.GetComponent<CircularLayoutGroup>();
            foreach (RectTransform child in gameObj.transform)
            {
                StartCoroutine(layout.SlidingEffect(child, Vector3.zero, 0.25f)); 
            }
            StartCoroutine(EndAnimation(gameObj.GetComponentInChildren<RectTransform>()));
        }
    }

    private IEnumerator EndAnimation(RectTransform child)
    {
        yield return new WaitUntil(() => child.localPosition == Vector3.zero);
        gameObj.SetActive(false);
    }
}
