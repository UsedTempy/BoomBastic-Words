using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UI_Preset {
    public Color MainColour;
    public Color ShadingColour;
    public Color ExtraShadingColour;
    public string Preset_Name;

    public UI_Preset(Color mainColour, Color shadingColour, Color extraShadingColour, string preset_Name = null)
    {
        this.MainColour = mainColour;
        this.ShadingColour = shadingColour;
        this.ExtraShadingColour = extraShadingColour;
        this.Preset_Name = preset_Name;
    }
}
