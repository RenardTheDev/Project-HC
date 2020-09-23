using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeManager : MonoBehaviour
{
    public static StrikeManager current;

    public GameObject prefab;

    public float strikeTimer = 2f;
    public float strikeStep = 0.5f;
    public float maxDeviation = 0.2f;

    public List<Strike> activeList = new List<Strike>();
    public List<Strike> inactiveList = new List<Strike>();

    public AnimationCurve strikeWidthAnim;

    private void Awake()
    {
        current = this;
    }

    private void Update()
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            var strike = activeList[i];
            if (strike.lifetime > strikeTimer)
            {
                inactiveList.Add(strike);
                activeList.Remove(strike);
                strike.go.SetActive(false);
            }
            else
            {
                strike.lifetime += Time.deltaTime;

                for (int s = 1; s < strike.steps.Length - 1; s++)
                {
                    strike.steps[s] += strike.deviation[s] * Time.deltaTime;
                    strike.line.SetPosition(s, strike.steps[s]);
                }

                float w = strikeWidthAnim.Evaluate(strike.lifetime / strikeTimer);
                Keyframe k0 = new Keyframe(0, w);
                Keyframe k1 = new Keyframe(1, w);
                strike.line.widthCurve = new AnimationCurve(new Keyframe[] { k0, k1 });
            }
        }
    }

    public void SpawnStrike(Vector3 start, Vector3 end)
    {
        Strike strike = getFreeStrike();

        Vector3 direction = end - start;
        int steps = (int)Mathf.Clamp(Mathf.Ceil(direction.magnitude * strikeStep), 1, float.MaxValue);
        float stepLength = direction.magnitude / steps;

        Debug.Log($"strike: steps = {steps}, stepLength = {stepLength}");

        Vector3[] pos = new Vector3[steps + 1];
        Vector3[] dev = new Vector3[steps + 1];
        pos[0] = start;

        for (int i = 1; i < pos.Length; i++)
        {
            Vector3 rnd = Random.insideUnitCircle * maxDeviation;
            dev[i] = rnd;
            pos[i] = start + direction.normalized * stepLength * i;
        }

        strike.line.positionCount = pos.Length;
        strike.line.SetPositions(pos);

        strike.steps = pos;
        strike.deviation = dev;

        strike.go.transform.position = start;
        strike.go.SetActive(true);
        strike.lifetime = 0;
    }

    Strike getFreeStrike()
    {
        if (inactiveList.Count> 0)
        {
            var strike = inactiveList[0];
            inactiveList.Remove(strike);
            activeList.Add(strike);
            return strike;
        }
        else
        {
            return createStrike();
        }
    }

    Strike createStrike()
    {
        var go = Instantiate(prefab, transform);
        Strike strike = new Strike(go);
        activeList.Add(strike);
        return strike;
    }
}

[System.Serializable]
public class Strike
{
    public GameObject go;
    public LineRenderer line;

    public float lifetime;

    public Vector3[] steps;
    public Vector3[] deviation;

    public Strike(GameObject obj)
    {
        go = obj;
        line = go.GetComponent<LineRenderer>();
    }
}