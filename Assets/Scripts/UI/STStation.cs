using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class STStation : ScreenTag
{
    [HideInInspector]
    public Station target;

    public void Assign(Transform target_transform, Station target_station)
    {
        base.Assign(target_transform, target_station.data.Name);
        target = target_station;

        ui_target.color = GameManager.inst.team[target.GetTeam()].teamColor;
        nameTag.color = ui_target.color;
    }

    public override void UpdateVisibility()
    {
        base.UpdateVisibility();

        nameTag.enabled = isOnScreen;

        if (isOnScreen)
        {

        }
        else
        {

        }
    }

    public override void UpdatePosition()
    {
        base.UpdatePosition();


    }
}
