using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObjectPooling;


public class PlayerParticles : MonoBehaviour
{
    [Header("Player Particles")]
    [SerializeField] private Transform[] speedLocator = null;
    [SerializeField] private Transform screamLocator = null;
    [SerializeField] private Transform dustLocator = null;
    [SerializeField] private Transform[] featherLocator = null;
    [SerializeField] private Transform dustJumpGroundLocator = null;
    [SerializeField] private Transform dustJumpLocator = null;
    [SerializeField] private Transform dashLocator = null;
    [SerializeField] private Transform shockWaveLocator = null;
    [SerializeField] private Transform roarLocator = null;
    [SerializeField] private Transform bokMarkerLocator = null;


    [Header("InPlayer Particles")]
    [SerializeField] private TrailRenderer[] jumpTrail = null;
    [SerializeField] private ParticleSystem jumpEffectParticles = null;
    [SerializeField] private ParticleSystem speedEffectParticles = null;
    [SerializeField] private ParticleSystem screamEffectParticles = null;

    [Header("Player Particles")]
    [SerializeField] private PoolableObject speedParticlesPrefab = null;
    [SerializeField] private PoolableObject screamParticlesPrefab = null;
    [SerializeField] private PoolableObject dustParticlesPrefab = null;
    [SerializeField] private PoolableObject featherParticlesPrefab = null;
    [SerializeField] private PoolableObject dustJumpGroundParticlesPrefab = null;
    [SerializeField] private PoolableObject dustJumpParticlesPrefab = null;
    [SerializeField] private PoolableObject dashParticlesPrefab = null;
    [SerializeField] private PoolableObject shockWaveParticlesPrefab = null;
    [SerializeField] private PoolableObject roarParticlesPrefab = null;
    [SerializeField] private PoolableObject bokMarkerParticlesPrefab = null;
   

    ParticleSystem[] speedParticles = null;
    ParticleSystem dustParticles = null;
    ParticleSystem[] featherParticles = null;
    ParticleSystem dustJumpParticles = null;
    ParticleSystem screamParticles = null;
    ParticleSystem dustJumpGroundParticles = null;
    ParticleSystem dashParticles = null;
    ParticleSystem shockWaveParticles = null;
    ParticleSystem roarParticles = null;



    bool speedPartOn = false;
    bool dustPartOn = false;
    bool featherPartOn = false;
    bool dustJumpPartOn = false;
    //Components
    private PlayerController _playerController;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;

        //Prepare arrays
        speedParticles = new ParticleSystem[speedLocator.Length];
        featherParticles = new ParticleSystem[featherLocator.Length];
    }

    public void Tick()
    {
        if (_playerController.Transforming) return;

        /*PLAYER TICK*/
        if (_playerController.IsPlayerBig == true)
        {
            TickBeast();
        }
        else
        {
            TickChicken();
        }
    }

    public void SetBokRadar()
    {
        GameObject bokRadar = PoolManager.GetObjectFromPool(bokMarkerParticlesPrefab).gameObject;
        bokRadar.transform.SetParentPositionRotaion(bokMarkerLocator);
        bokRadar.SetActive(true);
    }

    #region Beast Particles
    public void SetDashParticles()
    {
        if (dashParticles != null) dashParticles.transform.SetParent(null);
        dashParticles = PoolManager.GetObjectFromPool(dashParticlesPrefab).GetComponent<ParticleSystem>();
        dashParticles.transform.SetParentPositionRotaion(dashLocator);
        dashParticles.gameObject.SetActive(true);
        //dashParticles.Play();
    }

    public void SetShockWaveParticles()
    {
        if (shockWaveParticles != null) shockWaveParticles.transform.SetParent(null);
        shockWaveParticles = PoolManager.GetObjectFromPool(shockWaveParticlesPrefab).GetComponent<ParticleSystem>();
        shockWaveParticles.transform.SetParentPositionRotaion(shockWaveLocator);
        shockWaveParticles.gameObject.SetActive(true);
        //shockWaveParticles.Play();
    }

    void TickBeast()
    {
        //BEAST SPLASH
        if (_playerController.Landed && _playerController.TouchingWater)
        {
            GameManager.instance.SpawnWaterSplash(false, transform.position);
        }
    }
    #endregion
    #region Chicken Particles

    public void SetModifierParticles(Variables.PlayerModifier modifierType)
    {
        switch (modifierType)
        {
            case Variables.PlayerModifier.Speed:
                SetSpeedEffectParticles();
                break;
            case Variables.PlayerModifier.Jump:
                SetJumpEffectParticles();
                break;
            case Variables.PlayerModifier.Scream:
                SetScreamEffectParticles();
                break;
        }
    }

    void SetJumpEffectParticles()
    {
        jumpEffectParticles.gameObject.SetActive(false);
        jumpEffectParticles.gameObject.SetActive(true);
    }

    void SetSpeedEffectParticles()
    {
        speedEffectParticles.gameObject.SetActive(false);
        speedEffectParticles.gameObject.SetActive(true);
    }

    void SetScreamEffectParticles()
    {
        screamEffectParticles.gameObject.SetActive(false);
        screamEffectParticles.gameObject.SetActive(true);
    }

    public void SetDustJumpGroundParticles()
    {
        if (dustJumpGroundParticles != null) dustJumpGroundParticles.transform.SetParent(null);
        dustJumpGroundParticles = PoolManager.GetObjectFromPool(dustJumpGroundParticlesPrefab).GetComponent<ParticleSystem>();
        dustJumpGroundParticles.transform.SetParentPositionRotaion(dustJumpGroundLocator);
        dustJumpGroundParticles.gameObject.SetActive(true);
        //dustJumpGroundParticles.Play();
    }

    public void SetScreamParticles()
    {
        if (screamParticles != null) screamParticles.transform.SetParent(null);
        screamParticles = PoolManager.GetObjectFromPool(screamParticlesPrefab).GetComponent<ParticleSystem>();
        screamParticles.transform.SetParentPositionRotaion(screamLocator);
        screamParticles.gameObject.SetActive(true);
        //screamParticles.Play();
    }

    public void SetRoarParticles()
    {
        if (roarParticles != null) roarParticles.transform.SetParent(null);
        roarParticles = PoolManager.GetObjectFromPool(roarParticlesPrefab).GetComponent<ParticleSystem>();
        roarParticles.transform.SetParentPositionRotaion(roarLocator);
        roarParticles.gameObject.SetActive(true);
    }

    void DustParticlesUpdate(bool enable)
    {
        if (enable && !dustPartOn)
        {
            dustParticles = PoolManager.GetObjectFromPool(dustParticlesPrefab).GetComponent<ParticleSystem>();
            var main = dustParticles.main;
            main.loop = true;
            dustParticles.transform.SetParentPositionRotaion(dustLocator);
            dustParticles.gameObject.SetActive(true);
            //dustParticles.Play();
            dustPartOn = true;
        }
        else if (!enable && dustPartOn)
        {
            var main = dustParticles.main;
            main.loop = false;
            dustParticles.transform.SetParent(null);
            dustParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            dustParticles = null;
            dustPartOn = false;
        }
    }

    void FeatherParticlesUpdate(bool enable)
    {
        if (enable && !featherPartOn)
        {
            for (int i = 0; i < featherParticles.Length; i++)
            {
                featherParticles[i] = PoolManager.GetObjectFromPool(featherParticlesPrefab).GetComponent<ParticleSystem>();
                var main = featherParticles[i].main;
                main.loop = true;
                featherParticles[i].transform.SetParentPositionRotaion(featherLocator[i]);
                featherParticles[i].gameObject.SetActive(true);
                //featherParticles[i].Play();
            }
            featherPartOn = true;
        }
        else if (!enable && featherPartOn)
        {
            for (int i = 0; i < featherParticles.Length; i++)
            {
                var main = featherParticles[i].main;
                main.loop = false;
                featherParticles[i].transform.SetParent(null);
                featherParticles[i].Stop(false, ParticleSystemStopBehavior.StopEmitting);                
                featherParticles[i] = null;
            }
            featherPartOn = false;
        }
    }

    void JumpDustParticlesUpdate(bool enable)
    {
        if (enable && !dustJumpPartOn)
        {
            dustJumpParticles = PoolManager.GetObjectFromPool(dustJumpParticlesPrefab).GetComponent<ParticleSystem>();
            var main = dustJumpParticles.main;
            main.loop = true;
            dustJumpParticles.transform.SetParentPositionRotaion(dustJumpLocator);
            dustJumpParticles.gameObject.SetActive(true);
            //dustJumpParticles.Play();
            dustJumpPartOn = true;
        }
        else if (!enable && dustJumpPartOn)
        {
            var main = dustJumpParticles.main;
            main.loop = false;
            dustJumpParticles.transform.SetParent(null);
            dustJumpParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);           
            dustJumpParticles = null;
            dustJumpPartOn = false;
        }
    }

    void SpeedParticlesUpdate(bool enable)
    {
        if (enable && !speedPartOn)
        {
            speedPartOn = true;
            for (int i = 0; i < speedParticles.Length; i++)
            {
                speedParticles[i] = PoolManager.GetObjectFromPool(speedParticlesPrefab).GetComponent<ParticleSystem>();
                var main = speedParticles[i].main;
                main.loop = true;
                speedParticles[i].transform.SetParentPositionRotaion(speedLocator[i]);
                speedParticles[i].gameObject.SetActive(true);
                //speedParticles[i].Play();
            }
        }
        else if (!enable && speedPartOn)
        {
            for (int i = 0; i < speedParticles.Length; i++)
            {
                var main = speedParticles[i].main;
                main.loop = false;
                speedParticles[i].transform.SetParent(null);
                speedParticles[i].Stop(false, ParticleSystemStopBehavior.StopEmitting);
                speedParticles[i] = null;
            }
            speedPartOn = false;
        }
    }

    void TickChicken()
    {
        float speed = _playerController.Speed;
        bool isFast = speed >= .5f;

        //DUST PARTICLES
        bool spawnDustParticles = (isFast && _playerController.TouchingGround && !_playerController.TouchingWater) || _playerController.IsSliding;
        DustParticlesUpdate(spawnDustParticles);

        //FEATHER PARTICLES
        bool spawnFeatherParticles = _playerController.IsSliding || _playerController.IsGliding || _playerController.Jumping;
        FeatherParticlesUpdate(spawnFeatherParticles);

        //DUST JUMP PAERICLES
        JumpDustParticlesUpdate(_playerController.Jumping);

        //MODIFIER
        if (_playerController.ModifierLevel == 0) return;
        switch (_playerController.ActiveModifier)
        {
            case Variables.PlayerModifier.Speed:
                //UPDATE ANIMATION SPEED
                isFast = speed >= 1f;
                SpeedParticlesUpdate(isFast);
                break;
            case Variables.PlayerModifier.Jump:
                bool enableTrail = _playerController.IsGliding || _playerController.Jumping;
                for (int i = 0; i < jumpTrail.Length; i++)
                {
                    jumpTrail[i].enabled = enableTrail;
                }
                break;
        }

        //CHICKEN SPLASH
        if (_playerController.Landed && _playerController.TouchingWater)
        {
            GameManager.instance.SpawnWaterSplash(true, transform.position);
        }
    }
    #endregion

    #region Stop and Disable
    public void StopJumpTrail()
    {
        for (int i = 0; i < jumpTrail.Length; i++)
        {
            jumpTrail[i].enabled = false;
        }
    }

    public void StopSpeedParticles()
    {
        if (!speedPartOn) return;
        speedPartOn = false;
        for (int i = 0; i < speedParticles.Length; i++)
        {
            var main = speedParticles[i].main;
            main.loop = false;
            speedParticles[i].transform.SetParent(null);
            speedParticles[i].Stop(false, ParticleSystemStopBehavior.StopEmitting);            
            speedParticles[i] = null;
        }
    }

    void StopDustParticles()
    {
        if (!dustPartOn) return;
        var main = dustParticles.main;
        main.loop = false;
        dustParticles.transform.SetParent(null);
        dustParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);        
        dustParticles = null;
        dustPartOn = false;
    }

    void StopFeatherParticles()
    {
        if (!featherPartOn) return;
        featherPartOn = false;
        for (int i = 0; i < featherParticles.Length; i++)
        {
            var main = featherParticles[i].main;
            main.loop = false;
            featherParticles[i].transform.SetParent(null);
            featherParticles[i].Stop(false, ParticleSystemStopBehavior.StopEmitting);
            featherParticles[i] = null;
        }
    }

    void StopJumpDustParticles()
    {
        if (!dustJumpPartOn) return;
        var main = dustJumpParticles.main;
        main.loop = false;
        dustJumpParticles.transform.SetParent(null);
        dustJumpParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);        
        dustJumpParticles = null;
        dustJumpPartOn = false;
    }

    void StopSingleChickenParticles()
    {
        if (dustJumpGroundParticles != null) dustJumpGroundParticles.transform.SetParent(null);
        if (screamParticles != null) screamParticles.transform.SetParent(null);
        if (roarParticles != null) roarParticles.transform.SetParent(null);
        jumpEffectParticles.gameObject.SetActive(false);
        speedEffectParticles.gameObject.SetActive(false);
        screamEffectParticles.gameObject.SetActive(false);
    }

    void StopSingleBeastParticles()
    {
        if (shockWaveParticles != null) shockWaveParticles.transform.SetParent(null);
        if (dashParticles != null) dashParticles.transform.SetParent(null);
    }
    #endregion
    #region Player Stuff
    public void DisablePlayer(bool IsPlayerBig)
    {
        if (IsPlayerBig)
        {
            StopSingleBeastParticles();
        }
        else
        {
            StopSingleChickenParticles();
            StopJumpTrail();
            StopSpeedParticles();
            StopDustParticles();
            StopFeatherParticles();
            StopJumpDustParticles();
        }
    }

    public void SwapCharacters(bool IsPlayerBig)
    {
        if (IsPlayerBig)
        {
            StopSingleChickenParticles();
            StopJumpTrail();
            StopSpeedParticles();
            StopDustParticles();
            StopFeatherParticles();
            StopJumpDustParticles();
        }
        else
        {
            StopSingleBeastParticles();
        }
    }
    #endregion

}
