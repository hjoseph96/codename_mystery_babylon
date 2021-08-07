using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is a quick and crude implementation for performing actions, like increasing or decreasing game speed to help with testing
/// <br>Cheat Contents</br>
/// <br>Game Speed Down "-" key on numpad</br>
/// <br>Game Speed Up "+" key on numpad</br>
/// <br>Reset Game Speed "Enter" key on numpad</br>
/// <br>Insta kill all enemies "9" the number</br>
/// <br>Deal 90% of damage to all enemies "8" the number</br>
/// </summary>
public class GlobalVariables : Singleton<GlobalVariables>
{
    public float gameSpeed = 1f;
    public System.Action<float> OnGameSpeedChanged;

    private void Update()
    {
        // Decreases Game Speed
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            Debug.Log("Speed Down");
            gameSpeed -= .5f;
            gameSpeed = Mathf.Clamp(gameSpeed, 0, 5f);
            OnGameSpeedChanged?.Invoke(gameSpeed);
        }
        // Increase Game Speed
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Debug.Log("Speed Up");
            gameSpeed += 1f;
            gameSpeed = Mathf.Clamp(gameSpeed, 0, 5f);
            OnGameSpeedChanged?.Invoke(gameSpeed);
        }
        // Returns game speed to default
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log("Speed Reset");
            gameSpeed = 1f;
            OnGameSpeedChanged?.Invoke(gameSpeed);
        }
        // Deals MaxHealth value of damage to all enemies
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            for(int i = 0; i < CampaignManager.Instance.EnemyUnits().Count; i++)
            {
                CampaignManager.Instance.EnemyUnits()[i].TakeDamage(CampaignManager.Instance.EnemyUnits()[i].MaxHealth + 5);
            }

/*            for (int i = CampaignManager.Instance.EnemyUnits().Count -1; i >= 0; i--)
            {
                if (!CampaignManager.Instance.EnemyUnits()[i].IsDying)
                    CampaignManager.Instance.RemoveUnit(CampaignManager.Instance.EnemyUnits()[i]);
            }*/
        }
        // Deals 90% of damage to all enemies
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            for (int i = 0; i < CampaignManager.Instance.EnemyUnits().Count; i++)
            {
                CampaignManager.Instance.EnemyUnits()[i].TakeDamage(Mathf.FloorToInt(CampaignManager.Instance.EnemyUnits()[i].MaxHealth * .9f));
            }
        }
    }
}

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    public static T Instance {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
