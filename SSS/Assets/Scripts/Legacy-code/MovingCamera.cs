/*
 * 
 *     IEnumerator MoveCameraPlayerWithSpeedToPoint(Transform transformCameraPlayer, Transform targetTransform, float baseSpeed)
    {
        yield return null;

        float distanceTreshold = 0.01f;
        Vector3 specifyTargetPointForCamera = targetTransform.position + scriptPlayer.localPositionCamera;
        float adaptiveSpeed;

        while (Vector3.Distance(transformCameraPlayer.position, specifyTargetPointForCamera) > distanceTreshold)
        {
            specifyTargetPointForCamera = targetTransform.position + scriptPlayer.localPositionCamera;
            float distance = Vector3.Distance(transformCameraPlayer.position, specifyTargetPointForCamera);
            adaptiveSpeed = Mathf.Max(baseSpeed * 1/distance, 1.5f); // јдаптируем скорость: умножаем baseSpeed на рассто€ние, но не больше 10f
            transformCameraPlayer.position = Vector3.SmoothDamp(transformCameraPlayer.position, specifyTargetPointForCamera, ref _velocity, adaptiveSpeed);
            yield return null;
        }

        transformCameraPlayer.position = targetTransform.position + scriptPlayer.localPositionCamera;
        transformCameraPlayer.SetParent(transformPlayer);
        transformCameraPlayer.localPosition = scriptPlayer.localPositionCamera;
        MovingCameraPlayerWasFinished();
    }
 * 
 * 
    IEnumerator MoveCameraPlayerWithSpeedToPoint(Transform transformCameraPlayer, Transform targetTransform, float speed)
    {
        yield return null;

        float distanceTreshold = 0.01f;
        Vector3 specifyTargetPointForCamera = targetTransform.position + scriptPlayer.localPositionCamera;

        while (Vector3.Distance(transformCameraPlayer.position, specifyTargetPointForCamera) > distanceTreshold)
        {
            specifyTargetPointForCamera = targetTransform.position + scriptPlayer.localPositionCamera;
            transformCameraPlayer.position = Vector3.MoveTowards(transformCameraPlayer.position, specifyTargetPointForCamera, 6 * Time.deltaTime);
            yield return null;
        }

        transformCameraPlayer.position = targetTransform.position + scriptPlayer.localPositionCamera; // ”станавливаем точную позицию
        transformCameraPlayer.SetParent(transformPlayer);
        transformCameraPlayer.localPosition = scriptPlayer.localPositionCamera;
        MovingCameraPlayerWasFinished();
    }
 * 
 * 
 * C «амедлением:
IEnumerator MoveCameraPlayerWithSpeedToPoint(Transform transformCameraPlayer, Transform targetTransform, float speed)
{
    yield return null; // очень важно! ∆дЄм следующего кадра, чтоб, если нам нужно следовать за телепортировавшимс€ героем, у того успела обновитьс€ position

    float distanceTreshold = 0.01f;
    Vector3 specifyTargetPointForCamera = targetTransform.position + scriptPlayer.localPositionCamera;

    while (Vector3.Distance(transformCameraPlayer.position, specifyTargetPointForCamera) > distanceTreshold)
    {
        Debug.Log(Vector3.Distance(transformCameraPlayer.position, targetTransform.position));
        specifyTargetPointForCamera = targetTransform.position + scriptPlayer.localPositionCamera; // всегда камера будет иметь смещение относительно целовой точки такое
                                                                                                   // же, как и относительно игрока
        transformCameraPlayer.position = Vector3.SmoothDamp(transformCameraPlayer.position, specifyTargetPointForCamera, ref _velocity, speed);
        yield return null; // ∆дем следующий кадр
    }

    // ќстанавливаем корутину и устанавливаем точную позицию
    transformCameraPlayer.position = targetTransform.position;
    transformCameraPlayer.SetParent(transformPlayer);
    transformCameraPlayer.localPosition = scriptPlayer.localPositionCamera;
    MovingCameraPlayerWasFinished();

}*/