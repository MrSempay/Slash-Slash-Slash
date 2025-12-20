using PlayFab.ClientModels;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static StaticClassForAdditionalFunctions;
using System.IO;

public class SyncManager : ICleanUp
{
    public static SyncManager Instance => _instance ??= new SyncManager();

    private static SyncManager _instance;

    //private readonly string _pathToSyncDataFolder = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\SyncGeneralData";
    //private readonly string _pathToFileGeneralLocalData = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\GeneralLocalData";
    private string _pathToFileGeneralLocalData;
    private string _pathToSyncDataFolder;

    private SyncManager()
    {
        PlayFabManager.Instance.OnGetIDTitleAccountAfterLogin += SyncronizeGeneralData;

        CleanupManager.Register(this);

        _pathToFileGeneralLocalData = Path.Combine(Application.persistentDataPath, C.Paths.GeneralLocalDataJSON);
        _pathToSyncDataFolder = Path.Combine(Application.persistentDataPath, C.Paths.SyncGeneralDataFOLDER);

        if (!Directory.Exists(_pathToSyncDataFolder))
        {
            Directory.CreateDirectory(_pathToSyncDataFolder);
        }
    }

    public void Initialize()
    {
    }


    public void Dispose()
    {
        PlayFabManager.Instance.OnGetIDTitleAccountAfterLogin -= SyncronizeGeneralData;
        Debug.Log("Нещщадно уничтожаем наш SyncManager! Даже жалко как-то...");
    }

    public async Task<int?> GetMaxReachedLevel()
    {
        var request = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { C.Other.MaxReachedLevel }
        };

        var taskCompletionSource = new TaskCompletionSource<GetPlayerStatisticsResult>();

        PlayFabClientAPI.GetPlayerStatistics(
            request,
            result => { taskCompletionSource.SetResult(result); },
            error => {
                taskCompletionSource.TrySetException(new Exception(error.GenerateErrorReport()));
                Debug.LogError(error.GenerateErrorReport()); }
        );

        try
        {
            GetPlayerStatisticsResult result = await taskCompletionSource.Task;
            return OnGetMaxReachedLevel(result);
        }
        catch (Exception e)
        {
            Debug.LogError("Error in StartCloudUpdatePlayerStatsNEWAsync: " + e.Message);
            return null;
            // Handle the error here
        }
    }


    private async void SyncronizeGeneralData(string IDTitleAccount)
    {
        if (PlayFabClientAPI.IsClientLoggedIn()) // если мы залогинены (вызывается после логина, так что, может, рудиментная проверка)
        {
            int? maxReachedLevelServer = await GetMaxReachedLevel();

            if (HasAnyFile(_pathToSyncDataFolder)) // если в папке синхронизирующих данных есть файлы. По идее означает, что там может быть файл для нашего ID-шника. Если нету, то это
                                                   // как минимум будет значить, что это уже не первый аккаунт, с которого играли на этом устройстве, и поэтому просто синхронизировать
                                                   // данные с сервера с GeneralLocalData не выйдет
            {
                string pathGeneralSyncData = Path.Combine(_pathToSyncDataFolder, string.IsNullOrEmpty(IDTitleAccount) ? C.Paths.defaultJSON : IDTitleAccount + ".json");
                if (File.Exists(pathGeneralSyncData)) // если файл синхронизации для ID-шника текущего аккаунта (в который мы зашли) уже есть - то есть мы логинились в этот аккаунт на
                                                      // данном устройстве
                {
                    Debug.Log("Загружаем синхронизирующие настройки с устройства, сравниваем с данными сервера");
                    string json = File.ReadAllText(pathGeneralSyncData);

                    if (!string.IsNullOrEmpty(json)) // Проверяем, что файл не пуст. Но по идее такого быть не может, но проверим 
                    {
                        try
                        {
                            WrapperGeneralData wrapperGeneralData = JsonUtility.FromJson<WrapperGeneralData>(json); // читаем файлик с данными для текущего аккаунта, сериализуем
                            if (maxReachedLevelServer != null) // если сервер содержал статистику (пусть даже 0) MaxReachedLevel. То есть хотя бы раз синхронизация с сервером и загрузка на
                                                               // него статистики когда-нибудь происходила для этого аккаунта
                            {
                                if (wrapperGeneralData.MaxReachedLevel < maxReachedLevelServer) // если на сервере записан больший результат продвижения игрока, чем было записано в файлике
                                                                                                // (например, если сыграли дошли до 2 уровня с этого аккаунта на текущем устрйостве, после
                                                                                                // до 5-го на другом устрйостве, то на сервере будет значииться 5, а на старом устройстве
                                                                                                // всё ещё 2. Вот обновляем в большую сторону
                                {
                                    GameManager.Instance.ResetMaxReachedLevelToZeroAndSetNewValue((int)maxReachedLevelServer); // собсна, обновляем. Только изначально в ноль сбрасываем, ибо
                                                                                                                               // свойство позволяет увеличивать напрямую только в большую сторону
                                    Debug.Log("Залогинены, есть файл синхронизации для аккаунта, на сервере показатель больше, устанавливаем акутальный локально");
                                }
                                else if (wrapperGeneralData.MaxReachedLevel > maxReachedLevelServer)
                                     // если же напротив - на локальном устройстве было наиграно больше игр (допустим, на сереве числился максимальный уровень 2, на устройстве мы наиграли
                                     // до 5 - если мы почему-то не обновили данные на сервере (интернет пропадёт или мы просто ещё раз залогинимся)), то будет считаться, что для текущего
                                     // (ибо файл нашли по ID для текущего) аккаунта мы прошли 5 уровней локально и нам надо данные обновить на сервере. Также записываем эти данные в
                                     // свойство MaxReachedLevel нашей игры. Ибо, например, может, мы до этого были в другом аккаунте, а сейчас нам нужно подтянуть актуальные данные для
                                     // текущего аккаунта, ибо мы могли просто залогиниться в другой, до этого играя на... ином
                                {
                                    GameManager.Instance.ResetMaxReachedLevelToZeroAndSetNewValue(wrapperGeneralData.MaxReachedLevel);

                                    PlayFabManager.Instance.StartCloudUpdateMaxReachedLevel();
                                    Debug.Log("Залогинены, есть файл синхронизации для аккаунта, на сервере показатель меньше, загружаем на сервер больший показатель, обновляем локально");
                                }
                                else if (wrapperGeneralData.MaxReachedLevel == maxReachedLevelServer) // если данные с сервера совпадают с данными для синхронизации
                                {
                                    GameManager.Instance.ResetMaxReachedLevelToZeroAndSetNewValue((int)maxReachedLevelServer); 
                                    Debug.Log("Залогинены, есть файл синхронизации для аккаунта, на сервере показатель равен локальному, обновляем локально");
                                }
                            }
                            else // если на сервере такой статистики не нашли (то есть это первое наше взаимодействие с сервером), просто подгружаем акутальную информацию туда. Стоит
                                 // отметить, что данные else срабатывает с учётом того, что файлик с нашей ID-шкой для текущего аккаунта существует на устрйостве (ибо обращаемся к 
                                 // wrapperGeneralData, который мы, собсно, из него и получаем, но при этом мы с сервером обновления не проводили. Это возникает, по идее, тогда, когда
                                 // мы произвели логин, а после интернет вылетел и мы ничего на сервере не обновили - файлик сохранения для аккаунта, из которого вылетели, будет (ибо ID
                                 // сразу при логине сохраняем и не важно, если потом вылетели). При следующем логине в таком случае мы подгрузим актуальные данные, которые мы наиграли оффлайн
                            {
                                GameManager.Instance.ResetMaxReachedLevelToZeroAndSetNewValue(wrapperGeneralData.MaxReachedLevel);

                                PlayFabManager.Instance.StartCloudUpdateMaxReachedLevel();
                                Debug.Log("Залогинены, есть файл синхронизации для аккаунта, на сервере такой статистики нет, загружаем на сервер показатель из файла синхронизации, обновляем локально");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Error parsing JSON for GeneralLocalData: " + e.Message);
                        }
                    }
                }
                else // ЭТОТ else НУЖНО ОСОЗНАВАТЬ В ТАНДЕМЕ СО СЛЕДУЮЩИМ! Если у нас отсутствует синхронизирующий файлик для аккаунта с этим ID, НО ПРИ ЭТОМ В ПАПКЕ С ФАЙЛИКАМИ СИНХРОНИЗАЦИИ
                     // есть какие-то файлы (то есть мы так или иначе на этом устройстве логинились в другие аккаунты, но не в этот), то мы не обращаемся к нашему общему файлу для локального
                     // использования GeneralLocalData. Мы просто подтягиваем с сервера данные о том, как далеко игрок с текущим аккаунтом (в который залогинились только что, напоминаю)
                     // прошел, ЕСЛИ ВООБЩЕ ПРОШЕЛ. Если на сервере мы не нашли данных по этой статистике (MaxReachedLevel) и на устройстве его нет, то мы считаем, что это мега-новый аккаунт
                     // и просто сбрасываем текущие достижения (достигнутый максимальный уровень) в НОЛЬ.
                     // ОЧЕНЬ ВАЖНО, мы делаем так потому (а не обращаемся к локальному файлу GeneralLocalData), что в папке с синхронизирующими файлами у нас таки есть какие-то файлики, и
                     // информация из GeneralLocalData должна относиться к каким-то из них, ибо GeneralLocalData отображает, по сути, те же синхронизирующие данные, НО ДЛЯ ПОСЛЕДНЕГО
                     // ВХОДЯЩЕГО АККАУНТА 
                {
                    if (maxReachedLevelServer != null)
                    {
                        GameManager.Instance.ResetMaxReachedLevelToZeroAndSetNewValue((int)maxReachedLevelServer);
                        Debug.Log("Залогинены, нет синхронизирующего файла, но папка синхронизации не пуста, не можем брать из локального файла GeneralLocalData" +
                            ", устанавливаем локально значение MaxReachedLevel, записанное на сервере");
                    }
                    else
                    {
                        GameManager.Instance.ResetMaxReachedLevelToZeroAndSetNewValue(0);

                        PlayFabManager.Instance.StartCloudUpdateMaxReachedLevel();
                        Debug.Log("Залогинены, нет синхронизирующего файла, но папка синхронизации не пуста, не можем брать из локального файла GeneralLocalData, на сервере нет такой" +
                            " статистики, устанавливаем локально значение MaxReachedLevel в ноль, обновим на сервере статистику, чтоб хотя бы 0 был");
                    }
                }
            }
            else // (см. else выше) а вот если у нас в папке с синхронизирующими данными ВООБЩЕ НЕТ НИ ОДНОГО ФАЙЛА, то файл GeneralLocalData показывает нам то, что было просто наиграно
                 // в локальном режиме, там данные о последнем логине будут выглядеть в виде: максимальный результат по уровню, который наиграли локально, и ID аккаунта = "".
                 // Если у нас аккаунт чистый, то мы подгружаем данные из этого файла для локального использования и синхронизируем их с сервером. Считается, что вот эти локальные достижения -
                 // - это достижения игрока и мы их подтягиваем к нему в аккаунт на сервер. Если аккаунт не чистый (то есть maxReachedLevelServer != null), то мы уже решаем, чего больше - 
                 // наигранного локально или того, чего записано на сервере. Оставляем больший результат и синхронизируем его с сервером.
            {
                if (File.Exists(_pathToFileGeneralLocalData)) // если у нас вообще есть этот локальный файл, то есть хоть раз мы нечто сохраняли. Но по идее должен быть всегда, ибо ВСЕГДА перед
                                                         // логином мы СОХРАНЯЕМ РЕЗУЛЬТАТ ДЛЯ ПРЕДЫДУЩЕЙ СЕССИИ, пусть даже там ID аккаунта будет "".
                {
                    Debug.Log("Загружаем локальные настройки с устройства, сравниваем с данными сервера");
                    string json = File.ReadAllText(_pathToFileGeneralLocalData);

                    if (!string.IsNullOrEmpty(json)) // Проверяем, что файл не пуст
                    {
                        try
                        {
                            WrapperGeneralData wrapperGeneralData = JsonUtility.FromJson<WrapperGeneralData>(json);
                            if (maxReachedLevelServer != null)
                            {
                                if (wrapperGeneralData.MaxReachedLevel < maxReachedLevelServer)
                                {
                                    GameManager.Instance.ResetMaxReachedLevelToZeroAndSetNewValue((int)maxReachedLevelServer);
                                    Debug.Log("Залогинены, нет файла синхронизации для аккаунта и синхронизирующая папка пуста, сравниваем показатель с сервера с данными в файле" +
                                        " GeneralLocalData, в файле значение меньше, применяем данные с сервера");
                                }
                                else if (wrapperGeneralData.MaxReachedLevel > maxReachedLevelServer)
                                {
                                    GameManager.Instance.ResetMaxReachedLevelToZeroAndSetNewValue(wrapperGeneralData.MaxReachedLevel);

                                    PlayFabManager.Instance.StartCloudUpdateMaxReachedLevel();

                                    Debug.Log("Залогинены, нет файла синхронизации для аккаунта и синхронизирующая папка пуста, сравниваем показатель с сервера с данными в файле" +
                                        " GeneralLocalData, в файле значение больше - обновляем информацию на сервере и обновляем локально");
                                }
                                else if (wrapperGeneralData.MaxReachedLevel == maxReachedLevelServer) // если данные с сервера совпадают с данными для синхронизации
                                {
                                    GameManager.Instance.ResetMaxReachedLevelToZeroAndSetNewValue((int)maxReachedLevelServer);
                                    Debug.Log("Залогинены, нет файла синхронизации для аккаунта и синхронизирующая папка пуста, сравниваем показатель с сервера с данными в файле" +
                                        " GeneralLocalData, значения одинаковы, применяем данные локально (берём с сервера, просто так)");
                                }
                            }
                            else
                            {
                                GameManager.Instance.ResetMaxReachedLevelToZeroAndSetNewValue(wrapperGeneralData.MaxReachedLevel);

                                PlayFabManager.Instance.StartCloudUpdateMaxReachedLevel();
                                Debug.Log("Залогинены, нет файла синхронизации для аккаунта и синхронизирующая папка пуста, на сервере такой статистики нет" +
                                    " применяем локально значение из файла GeneralLocalData и подгружаем данные на сервер");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Error parsing JSON for GeneralLocalData: " + e.Message);
                        }
                    }
                }
            }
        }
        
    }

    private int? OnGetMaxReachedLevel(GetPlayerStatisticsResult result)
    {
        if (result.Statistics != null && result.Statistics.Count > 0)
        {
            var stat = result.Statistics[0]; // так как мы запрашивали только одну
            Debug.Log($"MaxReachedLevel = {stat.Value}");
            return stat.Value;
        }
        else
        {
            Debug.Log("Статистика MaxReachedLevel отсутствует.");
            return null;
        }
    }


}