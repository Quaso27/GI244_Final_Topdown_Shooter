using UnityEngine;

[CreateAssetMenu(fileName = "NewAugment", menuName = "Augments/Card")]
public class AugmentCard : ScriptableObject
{
    public enum AugmentType { MagnetRadius, HealthRegen, ChargeAttack, MaxHealth, HomingUnlock }

    public string augmentName;
    [TextArea] public string description;
    public Sprite icon;
    public AugmentType type;
    public float value;

    public void ApplyEffect(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        PlayerHealth ph = player.GetComponent<PlayerHealth>(); 

        if (pc == null || ph == null) return;

        switch (type)
        {
            case AugmentType.MagnetRadius:
                pc.UpgradePickupRadius(value);
                break;
            case AugmentType.MaxHealth:
                ph.IncreaseMaxHealth(value); 
                break;
            case AugmentType.ChargeAttack:
                pc.hasChargeAugment = true;
                break;
            case AugmentType.HomingUnlock:
                pc.hasHomingUpgrade = true; 
                break;
            case AugmentType.HealthRegen:
                pc.canRegen = true;       
                pc.regenAmount = (int)value; 
                break;  
        }
    }
}