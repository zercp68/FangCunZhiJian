using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ReHandCardGA : GameAction
{
    public void OnClick()
    {
        ReHandCardGA reHandCardGA = new ReHandCardGA();
        ActionSystem.Instance.Perform(reHandCardGA);
    }
}
