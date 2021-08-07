using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;

public class FootstepController : MonoBehaviour
{
    // TODO: Create Custom Drawer for Dictionary<SurfaceType, [SoundGroupAttribute] string> with Odin
    // public Dictionary<SurfaceType, string> FootstepSounds;
    [SoundGroup] public string dirtFootsteps;
    [SoundGroup] public string grassFootsteps;
    [SoundGroup] public string rockFootsteps;
    [SoundGroup] public string woodFootsteps;
    [SoundGroup] public string carpetFootsteps;
    [SoundGroup] public string gravelFootsteps;

    private Dictionary<SurfaceType, string> _footstepSounds = new Dictionary<SurfaceType, string>();

    void Awake()
    {
        _footstepSounds[SurfaceType.Dirt]   = dirtFootsteps;
        _footstepSounds[SurfaceType.Grass]  = grassFootsteps;
        _footstepSounds[SurfaceType.Rock]   = rockFootsteps;
        _footstepSounds[SurfaceType.Wood]   = woodFootsteps;
        _footstepSounds[SurfaceType.Carpet] = carpetFootsteps;
        _footstepSounds[SurfaceType.Gravel] = gravelFootsteps;
    }

    
    public void PlaySound(SurfaceType surfaceType)
    {
        if (!_footstepSounds.Keys.Contains(surfaceType))
            throw new System.Exception($"SurfaceType: {surfaceType} has not been added to {gameObject.name}'s FootstepController...");

        var soundToPlay = _footstepSounds[surfaceType];

        // TODO: replace CampaignManager.AudioListenerTransform with a new static class to manage audio listener transform
        MasterAudio.PlaySound3DFollowTransform(soundToPlay, transform);
    }
}
