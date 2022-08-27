using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class WeaponCollection
{
    public string PlayerType;
    public string BulletType;
    public bool weaponStatus;
}

public class WeaponController : MonoBehaviour
{
    [ReadOnly]
    public List<WeaponCollection> _weaponCollection;
    public static WeaponController instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
       
    }

    public void AddingValues(string PlayerName,string BulletName)
    {
        WeaponCollection Weaponinit = new WeaponCollection();
        Weaponinit.PlayerType = PlayerName;
        Weaponinit.BulletType = BulletName;
        Weaponinit.weaponStatus = false;
        _weaponCollection.Add(Weaponinit);

    }
    public string GetBulletType(string PlayerName)
    {
        foreach(WeaponCollection bulletInfo in _weaponCollection)
        {
            if (bulletInfo.PlayerType == (PlayerName) && bulletInfo.weaponStatus==false)
            {
                bulletInfo.weaponStatus = true;
                return bulletInfo.BulletType;
            }
        }
        return "";
    }
    public void WeaponValueChanged()
    {
        WeaponController.instance.AddingValues(UiHandler.Instance.PlayerName,  UiHandler.Instance._DropDown.options[UiHandler.Instance._DropDown.value].text);
    }
}