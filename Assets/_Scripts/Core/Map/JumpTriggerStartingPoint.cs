using Animancer.Examples.DirectionalSprites;
using UnityEngine;

public class JumpTriggerStartingPoint : MonoBehaviour
{
    private JumpTrigger _parentJumpTrigger;

    private void Awake() => _parentJumpTrigger = GetComponentInParent<JumpTrigger>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // TODO: user UserInput ProcessInput syntax here -- when it is updated from the other branch
        if (collision.tag == "Player")  // this may need to be refactored in the future to account for players following the controlled Player
        {
            var playerController = collision.GetComponent<SpriteCharacterControllerExt>();
            _parentJumpTrigger.AllowJumping(playerController);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _parentJumpTrigger.DisableJumping();
    }

}