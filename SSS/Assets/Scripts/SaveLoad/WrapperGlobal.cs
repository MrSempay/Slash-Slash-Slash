using UnityEngine;

[System.Serializable]
public class WrapperGlobal
{
    public WrapperGeneralData wrapperGeneralData;
    public DataWrapperSettings wrapperSettings;

    public WrapperGlobal()
    {
        wrapperGeneralData = new WrapperGeneralData(0, "");
        wrapperSettings = new DataWrapperSettings();        
    }
}
