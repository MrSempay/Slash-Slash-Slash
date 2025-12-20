using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FsmStateWalkAndAttack : FsmStateWalk
{

    public FsmStateWalkAndAttack(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        
    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        //Debug.Log("Walk and Attack state [ENTER]");

        base.Enter(initialConditionsEntering);

        player.attackAreaScript.isEnemyInAttackArea += MakeDamageToEnemy;
        player.OnAttackFinished += StopHorizontalMovement;
        player.OnSetStateIdle += SetStateIdleCallback;

        if (player.enemiesInAttackArea.Count > 0)
        {
            foreach (Enemy enemy in player.enemiesInAttackArea.ToList())
            {
                MakeDamageToEnemy(true, enemy);
            }
        }

        string nameAnimation = unit.HasUnitStateAdditional(Unit.UNIT_STATE_ADDITIONAL.Berserker) ?
            C.Animations.PlayerAttack + C.StatesAdditional.Berserker :
            C.Animations.PlayerAttack;

        player.animator.Play(nameAnimation);
    }

    public override void Exit()
    {
        //Debug.Log("Walk and Attack state [EXIT]");

        player.attackAreaScript.isEnemyInAttackArea -= MakeDamageToEnemy;
        player.OnAttackFinished -= StopHorizontalMovement;
        player.OnSetStateIdle -= SetStateIdleCallback;

        base.Exit();

        // по идее тут ещё можно бахнуть отмену звуков удара и кровавого удара, но посмотрим, как оно слышаться будет
    }

    private void MakeDamageToEnemyLEGACY(bool isEnemyInArea, Enemy enemy)
    {
        // так как урон можем наносить только во время свайпа, а иметь мгновенную скорость по оси Х также только во время свайпа, проверяем в условии скорость на неравенство нулю.
        try
        {
            //Debug.LogError(">>> ENTRY TOP METHOD <<< " + GetType().FullName + " thisHash=" + this.GetHashCode());
        }
        catch (Exception e)
        {
            //Debug.LogError("EXCEPTION AT ENTRY: " + e);
        }
        var mb = System.Reflection.MethodBase.GetCurrentMethod();
        //Debug.LogError($"Method info: {mb.DeclaringType.FullName}.{mb.Name}  Assembly={mb.DeclaringType.Assembly.FullName}  Module={mb.Module.Name}  Token={mb.MetadataToken}");
        try
        {
            //Debug.Log("Это сюрреализм");
            //Debug.Log(enemy == null ? "enemy IS NULL" : "enemy NOT NULL");
            //Debug.Log("enemy.gameObject? " + (enemy?.gameObject != null));
        }
        catch (Exception e)
        {
            //Debug.LogError("EXCEPTION IN TOP BLOCK: " + e);
        }
        if (enemy.gameObject.CompareTag("Enemy"))
        {
            if (isEnemyInArea)
            {
                if (player.rb.linearVelocityX != 0)
                {
                    enemy.GetDamage(player.damage, player);
                    //AudioManager.Instance.StartSoundEffectAtSpecifiedObject(C.MusicSounds.PlayerAttackPeakHitEnemies, gameObject, AudioManager.TYPE_SOUND.AttackPeak, AudioManager.TYPE_AUDIO_SOURCE._3DStandard);
                    AudioManager.Instance.StartSoundEffectAtSpecifiedEmitter(C.MusicSounds.PlayerAttackPeakHitEnemies,
                                                                             enemy.audioEmitter,
                                                                             AudioManager.TYPE_SOUND.AttackPeak,
                                                                             AudioManager.TYPE_AUDIO_SOURCE._3DStandard);
                }
                try
                {
                    //Debug.LogError(">>> ENTRY TOP METHOD <<< " + GetType().FullName + " thisHash=" + this.GetHashCode());
                }
                catch (Exception e)
                {
                    //Debug.LogError("EXCEPTION AT ENTRY: " + e);
                }
                var mtb = System.Reflection.MethodBase.GetCurrentMethod();
                //Debug.LogError($"Method info: {mtb.DeclaringType.FullName}.{mtb.Name}  Assembly={mtb.DeclaringType.Assembly.FullName}  Module={mtb.Module.Name}  Token={mtb.MetadataToken}");
                player.enemiesInAttackArea.Add(enemy);
            }
            else
            {
                if (player.enemiesInAttackArea.Contains(enemy)) // по идее это защищает от ситуации когда враг УЖЕ находится в зоне удара (то есть в неё не заходил). Не представляю, как это
                                                                // возможно, но пусть будет
                {
                    player.enemiesInAttackArea.Remove(enemy);
                }
            }
        }

    }

    private void MakeDamageToEnemy(bool isEnemyInArea, Enemy enemy)// я не знаю, что тут происходит. Просто выполнение перпрыгивает на середину метода, оттого там и проверяем на
                                                                   // tag != "Enemy", 
    {

        if (enemy?.gameObject != null)
        {
            ////Debug.Log("CompareTag Enemy: " + enemy.gameObject.CompareTag("Enemy"));
            ////Debug.Log("Tag: " + enemy.gameObject.tag);
            ////Debug.Log("CompareTag EnemyDied: " + enemy.gameObject.CompareTag("EnemyDied"));
        }
        if (enemy.gameObject.CompareTag("Enemy")) // НЕ РАБОТАЕТ. НЕВЕДОМО ПОЧЕМУ
        {
            if (isEnemyInArea)
            {
                // так как урон можем наносить только во время свайпа, а иметь мгновенную скорость по оси Х также только во время свайпа, проверяем в условии скорость на неравенство нулю.
                // 23.09.2025 - выше бред написан. Мы можем вызывать этот метод токмо из этого состояния, то есть по умолчанию скорость по Х у нас != 0. А до этого проблема была в том, что
                // мы не отписывались от прослушки метода детекции входа врагов в зону для атаки при выходе из этого состояния и данный метод мог вызываться у нас из любого другого состояния
                if (enemy.gameObject.CompareTag("Enemy"))
                {
                    enemy.GetDamage(player.damage, player);
                    //AudioManager.Instance.StartSoundEffectAtSpecifiedObject(C.MusicSounds.PlayerAttackPeakHitEnemies, gameObject, AudioManager.TYPE_SOUND.AttackPeak, AudioManager.TYPE_AUDIO_SOURCE._3DStandard);
                    AudioManager.Instance.StartSoundEffectAtSpecifiedEmitter(C.MusicSounds.PlayerAttackPeakHitEnemies,
                                                                             enemy.audioEmitter,
                                                                             AudioManager.TYPE_SOUND.AttackPeak,
                                                                             AudioManager.TYPE_AUDIO_SOURCE._3DStandard);
                }                
            }
            else
            {
                if (player.enemiesInAttackArea.Contains(enemy)) // по идее это защищает от ситуации когда враг УЖЕ находится в зоне удара (то есть в неё не заходил). Не представляю, как это
                                                                // возможно, но пусть будет
                {
                    //player.enemiesInAttackArea.Remove(enemy);
                }
            }
        }
    }
}
