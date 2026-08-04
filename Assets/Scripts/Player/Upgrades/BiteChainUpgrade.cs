using UnityEngine;

public class BiteChainUpgrade : Upgrade
{

    private PlayerAttacks playerAttacks;

    private void Start()
    {
        //from the player, grab the player attacks script
        playerAttacks = transform.parent.parent.GetComponent<PlayerAttacks>();
    }
    
    private void OnEnable()
    {
        GameEventManager.instance.OnBite += BiteChain;
    }

    private void OnDisable()
    {
        GameEventManager.instance.OnBite -= BiteChain;
    }

    private void BiteChain(Transform bittenTransform, float chargeAmount){

        //bite chain only triggers if bite is already fully charged
        if (chargeAmount < 0.95f) return;

        if (Random.value < 0.1f * level)
        {
            playerAttacks.biteTimer = 0;
            Debug.Log("Bite Chain triggered! Cooldown reset.");
        }
        
    }
}
