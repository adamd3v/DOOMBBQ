using SLZ.AI;
using SLZ.Props.Weapons;

using NEP.DOOMLAB.Entities;

using UnityEngine;
using MelonLoader;

namespace NEP.DOOMLAB.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(Gun), nameof(Gun.OnFire))]
    public static class GunPatches
    {
        public static void Postfix(Gun __instance)
        {
            PropagateSound(__instance);
        }

        public static void PropagateSound(Gun gun)
        {
            LayerMask mask = ~LayerMask.NameToLayer("EnemyColliders");
            var stuff = Physics.BoxCastAll(
                gun.firePointTransform.position, 
                Vector3.one * 16f, 
                gun.firePointTransform.position, 
                Quaternion.identity, 
                16f, 
                mask, 
                QueryTriggerInteraction.Ignore);

            for(int i = 0; i < stuff.Length; i++)
            {
                var gameObject = stuff[i].collider.gameObject;
                int instanceId = gameObject.GetInstanceID();
                var lookup = Mobj.ComponentCache.CacheLookup;

                if(!lookup.ContainsKey(instanceId))
                {
                    continue;
                }

                Mobj heardMobj = lookup[instanceId];

                if(heardMobj == Mobj.player)
                {
                    continue;
                }

                if(heardMobj.target != null)
                {
                    continue;
                }

                heardMobj.target = Mobj.player;
                heardMobj.brain.A_SeeYou();
            }
        }
    }
}