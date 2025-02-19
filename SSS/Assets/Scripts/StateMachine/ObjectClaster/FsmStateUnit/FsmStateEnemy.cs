using System.Collections;
using System.IO;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Text.RegularExpressions;

public class FsmStateEnemy : FsmStateUnit
{

    private float rotationThreshold = 1.0f; // Допустимое отклонение от целевого угла
    private Vector3 baseUpOffsetForStartPointFindingPath = new Vector3 (0, 0.5f, 0); // дополгнительное "поднимание вверх" конечной и стартовой точки для поиска пути (чтоб был запас по отрицательному
                                                               // углу при детекции провалов
    private Coroutine constraintTimeFlipXCoroutine;
    private bool canFlipByTimeDeley;

    protected readonly object _lock = new object();

    public Fsm fsmEnemy;
    public Enemy enemy;
    public MeleeEnemy enemy1;
    public NavMeshPath path; // Переменная для хранения пути
    public bool onFloor;


    public FsmStateEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        fsmEnemy = fsm;
        enemy = gameObject.GetComponent<Enemy>();

        path = new NavMeshPath();
        enemy.scriptFloorDetector.OnObjGetFloor += OnFloor;
    }

    public override void Update()
    {

    }

    //Рассчитывает, отрисовывает путь. Меняет направление взгляда персонажа, смещает все детекторы, двигает по оси х.
    public void CalculateDrawPathChangeDirectionAndMove()
    {
        
        FixingFuckingBuggingRotation();
        if (constraintTimeFlipXCoroutine == null)
        {
            constraintTimeFlipXCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, ConstraintTimeFlipX());
        }
        //Debug.Log(enemy.currentTargetTransform.position);
        //Debug.Log(enemy.currentTargetTransform);
            // Обновляем путь, если необходимо (например, если игрок переместился). По идее надо детектить ещё и y-состовляющую, но да ладно...
        if (Mathf.Abs(enemy.transform.position.x - enemy.currentTargetTransform.position.x) > 0.1f)
        {
            //Debug.Log("Emmm???1");
            enemy.isPathValid = NavMesh.CalculatePath(enemy.transform.position + baseUpOffsetForStartPointFindingPath, enemy.currentTargetTransform.position + baseUpOffsetForStartPointFindingPath, NavMesh.AllAreas, path);
            //Debug.Log(path.corners.Length);
            //enemy.agent.destination = enemy.playerTransform.position;
            //path = enemy.agent.path;
            if (path.corners.Length >= 1) enemy.isPathValid = true;

            if (enemy.isPathValid)
            {
                enemy.currentCornerIndex = 1; // Сбрасываем индекс при пересчете пути
            }
        }
        //Debug.Log("Emmm???2");
        //Действия, если пути нет. Например, поиск пути
        if (!enemy.isPathValid || path.corners.Length < 2)
        {
            //Останавливаем движение, например
            enemy.rb.linearVelocityX = 0;
            return; // Ничего не делаем, если путь не валиден или слишком короткий
        }
        //Debug.Log("Emmm???3");
        // Получаем текущую цель (вторая точка)
        if (enemy.currentCornerIndex < path.corners.Length)
        {
            //Debug.Log("Emmm???4");
            enemy.nextPointInPath = path.corners[enemy.currentCornerIndex];
            //Debug.Log(StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[0], (Vector2)path.corners[enemy.currentCornerIndex]));
            //Debug.Log(enemy.isGrounded);
            //Debug.Log(StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[enemy.currentCornerIndex - 1], (Vector2)path.corners[enemy.currentCornerIndex]));
            if (StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[0], (Vector2)path.corners[enemy.currentCornerIndex]) >= 75f &&
                StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[0], (Vector2)path.corners[enemy.currentCornerIndex]) <= 165f) Jump(false);
        }
        else
        {
            //Debug.Log("Emmm???5");
            //Если все углы пройдены, то останавливаемся
            enemy.rb.linearVelocityX = 0;
            return;
        }
        //Debug.Log("Emmm???6");

        // Перемещение к целевой позиции. Cначала вычислям перемещение (чтоб понять, куда поворачиваться) и только после отзеркаливаем спрайт
        //Debug.Log("Emmm???3");
        MoveTowardsTarget();
        //Ориентация спрайта и прочего
        if (enemy.lookingRight == enemy.nextPointInPath.x < enemy.transform.position.x)
        {
            ChangeDirectionView();
        }

        //Отрисовка пути
        DrawPath();
    }


    // двигаем персонажа по пути
    void MoveTowardsTarget()
    {
        //Debug.Log("Emmm???4");
        float direction = Mathf.Sign(enemy.nextPointInPath.x - enemy.transform.position.x);
        enemy.rb.linearVelocityX = direction * enemy.speed;
        // Проверка достижения цели
        if (Mathf.Abs(enemy.transform.position.x - enemy.nextPointInPath.x) <= enemy.arrivalThreshold)
        {
            //Debug.Log("Emmm???5");
            // Переходим к следующей точке
            enemy.currentCornerIndex++;
            if (enemy.currentCornerIndex >= path.corners.Length)
            {
                //Debug.Log("Emmm???6");
                // Достигли конечной точки
                enemy.rb.linearVelocityX = 0; //Останавливаемся
                enemy.isPathValid = false; //Сбрасываем флаг
                return;
            }
        }
    }

    // отрисовываем путь
    void DrawPath()
    {
        if (enemy.lineRenderer == null || path == null) return;
        // Устанавливаем количество точек LineRenderer
        enemy.lineRenderer.positionCount = path.corners.Length;
        // Устанавливаем позиции точек LineRenderer
        enemy.lineRenderer.SetPositions(path.corners);
    }

    // отзеркаливаем персонажа и все детекторы при повороте
    void ChangeDirectionView()
    {
        if (canFlipByTimeDeley)
        {
            enemy.lookingRight = enemy.nextPointInPath.x > enemy.transform.position.x;
            enemy.selfSprite.flipX = !enemy.selfSprite.flipX;
            enemy.attackAreaTransform.localPosition = new Vector3(-1 * enemy.attackAreaTransform.localPosition.x, enemy.attackAreaTransform.localPosition.y, enemy.attackAreaTransform.localPosition.z);
            enemy.pitDetectorTransform.localPosition = new Vector3(-1 * enemy.pitDetectorTransform.localPosition.x, enemy.pitDetectorTransform.localPosition.y, enemy.pitDetectorTransform.localPosition.z);
            canFlipByTimeDeley = false;
        }
    }

    // прыгаем. Вызывается либо если угол наклона прямой пути больше 45 градусов или была обнаружена яма (эмулируется сигнал из PitDetector)
    protected void Jump(bool jumpOfPit)
    {
        if (enemy.isGrounded) { // Если мы на земле
            if (path.corners.Length > 1 && enemy.currentCornerIndex < path.corners.Length) { // вот на ровном месте оно почему-то тут выдаёт 0 иногда 0_0
                //Debug.Log(enemy.currentCornerIndex - 1);
                //Debug.Log(enemy.currentCornerIndex);
                //Debug.Log(path.corners.Length);
                if (!(StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[enemy.currentCornerIndex - 1], (Vector2)path.corners[enemy.currentCornerIndex]) <= -5f
                && StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[enemy.currentCornerIndex - 1], (Vector2)path.corners[enemy.currentCornerIndex]) >= -175f))
                { // если нам не вниз
                    if (path.corners.Length != 2) fsmEnemy.SetState<FsmStateJumpEnemy>(); //если Player не в прямой видимости (чтоб за нами не прыгал если мы прыгнем, тогда количество точек пути = 2)
                    if (jumpOfPit) fsmEnemy.SetState<FsmStateJumpEnemy>(); // игнорируем прямую видимость героя в том случае, если надо перерыгивать через пропасть
                }

            }     
        }
    }

    private IEnumerator ConstraintTimeFlipX()
    {
        while (true)
        {
            canFlipByTimeDeley = true;
            yield return new WaitForSeconds(0.5f);
        }
    }

    protected void FixingFuckingBuggingRotation()
    {
        float xRotation = enemy.transform.rotation.eulerAngles.x;
        //Debug.Log(xRotation);

        // Нормализуем угол к диапазону 0-360
        xRotation = Mathf.Repeat(xRotation, 360.0f);

        // Приводим к диапазону -180 - 180
        if (xRotation > 180.0f)
            xRotation -= 360.0f;

        if (Mathf.Abs(xRotation - 90f) < rotationThreshold)
        {
            enemy.objForRotate.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            //Debug.Log("Поворачиваем дочерний объект на -90 градусов");
        }
        else if (Mathf.Abs(xRotation + 90f) < rotationThreshold)
        {
            enemy.objForRotate.transform.localRotation = Quaternion.Euler(90, 0, 0);
            //Debug.Log("Поворачиваем дочерний объект на 90 градусов");
        }
        else if (Mathf.Abs(xRotation - 0f) < rotationThreshold)
        {
            enemy.objForRotate.transform.localRotation = Quaternion.Euler(0, 0, 0);
            //Debug.Log("Устанавливаем дочернему объекту нулевой поворот");
        }

        // НЕ НУЖНО обнулять поворот родителя здесь!
        // enemy.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public bool GetFloor(bool onFloor)
    {
        return (onFloor && enemy.rb.linearVelocity.y <= 0); 
    }

    protected bool OnFloor(bool atFloor)
    {
        enemy.isGrounded = atFloor;
        onFloor = atFloor;
        return atFloor;
    }

    public override void OnDestroy()
    {
        enemy.scriptFloorDetector.OnObjGetFloor -= OnFloor;

    }





}
