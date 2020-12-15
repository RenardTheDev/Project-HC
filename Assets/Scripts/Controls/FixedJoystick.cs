using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FixedJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Canvas parent;

    public bindState state = bindState.none;

    protected RectTransform Background;
    protected int PointerId;
    public RectTransform Handle;
    [Range(0f, 2f)]
    public float HandleRange = 1f;

    public Image circle;
    float circlePressed = 0;
    public Gradient circleInteract;

    //[HideInInspector]
    public Vector2 InputVector = Vector2.zero;
    public Vector2 AxisNormalized { get { return InputVector.magnitude > 0.25f ? InputVector.normalized : (InputVector.magnitude < 0.01f ? Vector2.zero : InputVector * 4f); } }

    private void Awake()
    {
        if (Handle == null)
            Handle = transform.GetChild(0).GetComponent<RectTransform>();
        Background = GetComponent<RectTransform>();
        Background.pivot = new Vector2(0.5f, 0.5f);

        parent = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        circle.color = circleInteract.Evaluate(circlePressed);

        switch (state)
        {
            case bindState.down:
                StateChange(bindState.hold);
                break;

            case bindState.hold:
                break;

            case bindState.up:
                StateChange(bindState.none);
                break;
        }

        if (state == bindState.hold)
        {
            Vector2 direction = (PointerId >= 0 && PointerId < Input.touches.Length) ? Input.touches[PointerId].position - new Vector2(Background.position.x, Background.position.y) : new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(Background.position.x, Background.position.y);
            InputVector = (direction.magnitude > Background.sizeDelta.x / 2f) ? direction.normalized : direction / (Background.sizeDelta.x / 2f);
            Handle.anchoredPosition = (InputVector * Background.sizeDelta.x / 2f) * HandleRange;
        }

        if (!parent.gameObject.activeSelf && state != bindState.none) StateChange(bindState.none);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StateChange(bindState.down);

        PointerId = eventData.pointerId;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StateChange(bindState.up);

        InputVector = Vector2.zero;
        Handle.anchoredPosition = Vector2.zero;
    }

    private void OnDisable()
    {
        StateChange(bindState.none);

        InputVector = Vector2.zero;
        Handle.anchoredPosition = Vector2.zero;
    }

    void StateChange(bindState newState)
    {
        if (!gameObject.activeSelf) return;

        state = newState;

        if (gameObject.activeSelf)
        {
            switch (newState)
            {
                case bindState.none:
                    if (fadeCircle != null) StopCoroutine(fadeCircle);
                    fadeCircle = StartCoroutine(FadeCircle(false));
                    break;
                case bindState.hold:
                    if (fadeCircle != null) StopCoroutine(fadeCircle);
                    fadeCircle = StartCoroutine(FadeCircle(true));
                    break;
            }
        }
    }

    Coroutine fadeCircle;
    IEnumerator FadeCircle(bool show)
    {
        if (show)
        {
            while (circlePressed < 1)
            {
                circlePressed = Mathf.MoveTowards(circlePressed, 1, Time.deltaTime * 4);
                yield return new WaitForEndOfFrame();
            }
            circlePressed = 1;
        }
        else
        {
            while (circlePressed > 0)
            {
                circlePressed = Mathf.MoveTowards(circlePressed, 0, Time.deltaTime * 2);
                yield return new WaitForEndOfFrame();
            }
            circlePressed = 0;
        }

        yield return new WaitForEndOfFrame();
    }
}
