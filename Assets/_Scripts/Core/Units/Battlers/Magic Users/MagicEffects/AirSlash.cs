using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSlash : MagicEffect
{
    [SerializeField] private float _shineFXSpeed = 4f;
    [SerializeField] private Color _shineColor = new Color(115, 122, 204, 255);

    public override IEnumerator PostProcessWhileActive(Battler target)
    {
        var allInOneMaterial = target.GetComponent<Renderer>().material;

        yield return new WaitUntil(() => IsActive);

        var shineAngle = Mathf.Lerp(Time.time * _shineFXSpeed, 0.35f, 0.77f);

        allInOneMaterial.SetColor("SHINE_COLOR", _shineColor);
        allInOneMaterial.SetFloat("SHINE_ROTATE", shineAngle);
        allInOneMaterial.EnableKeyword("SHINE_ON");

        yield return new WaitUntil(() => IsActive == false);

        allInOneMaterial.DisableKeyword("SHINE_ON");
    }
}
