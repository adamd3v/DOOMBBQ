using NEP.DOOMBBQ.Game;
using NEP.DOOMBBQ.Data;

using UnityEngine;

namespace NEP.DOOMBBQ.Entities
{
    public static class MobjInteraction
    {
        public static bool CheckThing(Mobj thing, Mobj tmThing)
        {
            if(thing == null)
            {
                return false;
            }

            if (!thing.flags.HasFlag(MobjFlags.MF_SOLID))
            {
                return false;
            }

            if(!thing.flags.HasFlag(MobjFlags.MF_SHOOTABLE))
            {
                return false;
            }

            if (thing == tmThing)
            {
                return false;
            }

            if (tmThing.flags.HasFlag(MobjFlags.MF_SKULLFLY))
            {
                float damage = ((DoomGame.RNG.P_Random() % 8) + 1) * tmThing.info.damage;

                DamageMobj(thing, tmThing, tmThing, damage);

                if(tmThing.flags.HasFlag(MobjFlags.MF_SKULLFLY))
                {
                    tmThing.flags &= ~MobjFlags.MF_SKULLFLY;
                    tmThing.rigidbody.velocity = Vector3.zero;

                    if(tmThing.sightedPlayer)
                    {
                        tmThing.SetState(tmThing.info.seeState);
                    }
                    else
                    {
                        tmThing.SetState(tmThing.info.spawnState);
                    }
                }

                tmThing.SetState(tmThing.info.spawnState);

                return true;
            }

            if (tmThing.flags.HasFlag(MobjFlags.MF_MISSILE))
            {
                if (tmThing.target != null && (
                tmThing.target.type == thing.type ||
                (tmThing.target.type == MobjType.MT_KNIGHT && thing.type == MobjType.MT_BRUISER) ||
                (tmThing.target.type == MobjType.MT_BRUISER && thing.type == MobjType.MT_KNIGHT)))
                {
                    // Don't damage same species
                    if (thing.type == tmThing.target.type)
                    {
                        return false;
                    }
                }

                if (!thing.flags.HasFlag(MobjFlags.MF_SHOOTABLE))
                {
                    return !thing.flags.HasFlag(MobjFlags.MF_SOLID);
                }

                float damage = ((DoomGame.RNG.P_Random() % 8) + 1) * tmThing.info.damage;
                DamageMobj(thing, tmThing, tmThing.target, damage);

                return true;
            }

            return !thing.flags.HasFlag(MobjFlags.MF_SOLID);
        }

        // Checks if a raycast line is unobstructed.
        public static bool CheckSight(Mobj thing, Mobj other)
        {
            if (other == null)
            {
                thing.brain.SeesTarget = false;
                return false;
            }

            Vector3 origin = thing.transform.position + Vector3.up;
            Vector3 direction = other.transform.position + Vector3.up;

            if(other == Mobj.player)
            {
                direction = other.transform.position;
            }

            Ray ray = new Ray(origin, direction - origin);

            if (Physics.Raycast(ray, out RaycastHit hit, 20))
            {
                Collider collider = hit.collider;
                GameObject hitObject = collider.gameObject;
                var lookup = Mobj.ComponentCache.CacheLookup;

                if(lookup.ContainsKey(hitObject.GetInstanceID()))
                {
                    Mobj hitMobj = lookup[hitObject.GetInstanceID()];

                    if (hitMobj == other)
                    {
                        thing.brain.SeesTarget = true;
                        return true;
                    }
                }

                if(collider != null && !collider.isTrigger)
                {
                    thing.brain.SeesTarget = false;
                    return false;
                }
            }

            thing.brain.SeesTarget = true;
            return true;
        }

        public static bool LineAttack(Mobj shootThing, Vector3 origin, Vector3 direction, float damage, float distance)
        {
            Ray ray = new Ray(origin, direction - origin);

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, distance))
            {
                Collider collider = hit.collider;

                Mobj hitMobj = collider.GetComponent<Mobj>();

                if (hitMobj == null)
                {
                    return false;
                }

                if (hitMobj == shootThing)
                {
                    // we cant shoot ourselves
                    return false;
                }

                if (!hitMobj.flags.HasFlag(MobjFlags.MF_SHOOTABLE))
                {
                    return false; // Corpse
                }

                if (hitMobj.flags.HasFlag(MobjFlags.MF_NOBLOOD))
                {
                    MobjManager.Instance.SpawnMobj(hit.point, MobjType.MT_PUFF);
                }
                else
                {
                    MobjManager.Instance.SpawnMobj(hit.point, MobjType.MT_BLOOD);
                }

                if (damage > 0f)
                {
                    DamageMobj(hitMobj, shootThing, shootThing, damage);
                }

                return true;
            }

            return false;
        }

        public static void RadiusAttack(Mobj spot, Mobj source, float damage)
        {
            var hits = Physics.BoxCastAll(spot.position, Vector3.one * 4, spot.position);

            for (int i = 0; i < hits.Length; i++)
            {
                GameObject obj = hits[i].collider.gameObject;
                var lookup = Mobj.ComponentCache.CacheLookup;
                int id = obj.GetInstanceID();
                
                if(!lookup.ContainsKey(id))
                {
                    continue;
                }

                Mobj hit = lookup[id];
                RadiusAttackIterator(hit, spot, source, damage);
            }
        }

        public static bool RadiusAttackIterator(Mobj thing, Mobj spot, Mobj source, float damage)
        {
            if(thing == null)
            {
                return false;
            }

            if (!thing.flags.HasFlag(MobjFlags.MF_SHOOTABLE))
            {
                return false;
            }

            // take no damage from rockets if we're a cyborg or spider
            if (thing.type == MobjType.MT_CYBORG || thing.type == MobjType.MT_SPIDER)
            {
                return false;
            }

            float dx = Mathf.Abs(thing.position.x - spot.position.x);
            float dz = Mathf.Abs(thing.position.z - spot.position.z);

            float distance = dx > dz ? dx : dz;
            distance = distance - (thing.radius / 32f);

            if (distance < 0)
            {
                distance = 0;
            }

            if (distance > damage)
            {
                return false;
            }

            if (thing.brain == null)
            {
                return false;
            }

            if (CheckSight(thing, spot))
            {
                DamageMobj(thing, spot, source, damage - distance);
            }

            return true;
        }

        public static void DamageMobj(Mobj target, Mobj inflictor, Mobj source, float damage)
        {
            if (!target.flags.HasFlag(MobjFlags.MF_SHOOTABLE))
            {
                return;
            }

            if (target.health <= 0f)
            {
                return;
            }

            if (target.flags.HasFlag(MobjFlags.MF_SKULLFLY))
            {
                target.rigidbody.velocity = Vector3.zero;
            }

            if(target == Mobj.player)
            {
                
            }
            else
            {
                target.health -= damage;
            }
            
            if (target.health <= 0)
            {
                target.Kill();
                return;
            }

            if (DoomGame.RNG.P_Random() < target.info.painChance && !target.flags.HasFlag(MobjFlags.MF_SKULLFLY))
            {
                target.flags |= MobjFlags.MF_JUSTHIT;
                target.SetState(target.info.painState);
            }

            target.reactionTime = 0;

            if (target.threshold == 0 || target.type == MobjType.MT_VILE
                && source != null && source != target
                && source.type != MobjType.MT_VILE)
            {
                target.target = source;
                target.threshold = 100;

                if (target.currentState == target.info.spawnState && target.info.seeState != StateNum.S_NULL)
                {
                    target.SetState(target.info.seeState);
                }
            }
        }

        public static bool VileCheckIterator(Mobj thing, out Mobj corpseHit)
        {
            if(!thing.flags.HasFlag(MobjFlags.MF_CORPSE))
            {
                corpseHit = null;
                return false;
            }

            if(thing.info.raiseState == StateNum.S_NULL)
            {
                corpseHit = null;
                return false;
            }

            corpseHit = thing;
            corpseHit.rigidbody.velocity = Vector3.zero;

            return true;
        }
    }
}