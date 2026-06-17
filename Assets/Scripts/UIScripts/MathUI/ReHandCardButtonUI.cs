using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReHandCardButtonUI : MonoBehaviour
{
    public void OnClick()
    {
        ReHandCardGA reHandCardGA = new ReHandCardGA();
        ActionSystem.Instance.Perform(reHandCardGA);
    }
}
