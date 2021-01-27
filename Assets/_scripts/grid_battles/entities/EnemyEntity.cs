using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity : MapEntity
{
    new void Awake() {
        base.Awake();

        _entityType = "enemy";
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }
}
