[System.Serializable]
public class WeaponData
{
    public int weaponId;
    public string name;
    public float damage;
    public float attackRate;
    public float scale_x;
    public float scale_y;
    public float cooldown;
    public string texturePath;
    public float stage;
    public string subtype;
    public string type; // 무기 타입 (Melee, Orbit 등)
    public string imagePath;
}