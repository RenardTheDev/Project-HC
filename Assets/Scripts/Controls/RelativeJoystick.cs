using UnityEngine;
using UnityEngine.EventSystems;

public class RelativeJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Canvas parent;

    public bindState state = bindState.none;

    protected RectTransform Background;
    protected int PointerId;
    public RectTransform Pad;
    public RectTransform Handle;
    [Range(0f, 2f)]
    public float HandleRange = 1f;

    //[HideInInspector]
    public Vector2 startPoint;
    public Vector2 InputVector = Vector2.zero;
    public Vector2 AxisNormalized { get { return InputVector.magnitude > 0.25f ? InputVector.normalized : (InputVector.magnitude < 0.01f ? Vector2.zero : InputVector * 4f); } }


    private void Awake()
    {
        if (Handle == null)
            Handle = transform.GetChild(0).GetComponent<RectTransform>();
        Background = GetComponent<RectTransform>();
        //Background.pivot = new Vector2(0.5f, 0.5f);
        parent = GetComponentInParent<Canvas>();

        StateChange(bindState.none);
    }

    private void Update()
    {

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
            Vector2 direction = (PointerId >= 0 && PointerId < Input.touches.Length) ? Input.touches[PointerId].position - new Vector2(Pad.position.x, Pad.position.y) : new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(Pad.position.x, Pad.position.y);
            InputVector = (direction.magnitude > Pad.sizeDelta.x / 2f) ? direction.normalized : direction / (Pad.sizeDelta.x / 2f);
            Handle.anchoredPosition = (InputVector * Pad.sizeDelta.x / 2f) * HandleRange;
        }

        if (!parent.gameObject.activeSelf && state != bindState.none) StateChange(bindState.none);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerId = eventData.pointerId;
        startPoint = eventData.position;

        StateChange(bindState.down);
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
        state = newState;

        switch (newState)
        {
            case bindState.down:
                Pad.position = startPoint;
                break;
            case bindState.up:
                Pad.position = new Vector2(0, -1000);
                break;
            case bindState.none:
                Pad.position = new Vector2(0, -1000);
                break;
        }
    }
}
