using UnityEngine;
using UnityEngine.EventSystems;

public class TimelineOnClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    float timelineStart;
    float timelineEnd;

    bool isClicked = false;

    RectTransform rect;

    PointerEventData data;
    void Start()
    {
        rect = GetComponent<RectTransform>();

        timelineStart = transform.position.x;
        timelineEnd = transform.position.x + rect.rect.width;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        data = eventData;
        isClicked = true;
    }

    
    public void OnPointerUp(PointerEventData eventData)
    {
        isClicked = false;
    }

    void Update()
    {
        if(isClicked)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            data.position,
            data.pressEventCamera,
            out Vector2 localPoint
            );

            float normalizedClick = Mathf.InverseLerp(
                -rect.rect.width / 2f, 
                rect.rect.width / 2f, 
                localPoint.x
            );

            ReplayManager.Instance.MoveReplayToPoint(normalizedClick);
        }
    }

}
