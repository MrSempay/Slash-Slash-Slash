using UnityEngine;
using static StaticClassForAdditionalFunctions;


[System.Serializable]
public class WrapperGeneralData // для записи информация для локального пользования, если мы не можем выйти в интернет. Последующие данные будут записываться в \SyncGeneralData\IDTitleLastSignedAccount,
                                // если IDTitleLastSignedAccount != "" (иначе мы вообще ни разу в аккаунт не заходили). Хотя не, это для всей информации. Не будем отдельный класс создавать
                                // для информации на синхронизацию
{
    public int MaxReachedLevel;
    public string IDTitleLastSignedAccount;

    public WrapperGeneralData(int MaxReachedLevel, string IDTitleLastSignedAccount)
    {
        this.MaxReachedLevel = MaxReachedLevel;
        this.IDTitleLastSignedAccount = IDTitleLastSignedAccount;
    }
}
