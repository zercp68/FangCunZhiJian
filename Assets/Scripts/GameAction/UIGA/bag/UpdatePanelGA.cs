using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePanelGA : GameAction
{
    public int beginIndex;
    public int endIndex;
    public UpdatePanelGA(int BeginIndex, int EndIndex)
    {
        this.beginIndex = BeginIndex;
        this.endIndex = EndIndex;
    }
}
