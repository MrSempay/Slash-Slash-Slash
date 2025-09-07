using UnityEngine;

[System.Serializable]
public class WrapperGlobal
{
    public WrapperGeneralData wrapperGeneralData;
    public DataWrapperSettings wrapperSettings;

    public WrapperGlobal()
    {
        wrapperGeneralData = new WrapperGeneralData();
        wrapperSettings = new DataWrapperSettings();        
    }
}
