using NEP.DOOMLAB.Data;
using NEP.DOOMLAB.Game;

using SLZ.AI;
using SLZ.Combat;
using SLZ.Marrow.Data;
using SLZ.Marrow.Pool;
using SLZ.Marrow.Warehouse;

using System;

using UnityEngine;

namespace NEP.DOOMLAB.Entities
{
    [MelonLoader.RegisterTypeInIl2Cpp]
    public class Mobj : MonoBehaviour
    {
        public Mobj(System.IntPtr ptr) : base(ptr) { }

        public static Action<Mobj> OnDeath;

        public enum MoveDirection
        {
            EAST,
            NORTHEAST,
            NORTH,
            NORTHWEST,
            WEST,
            SOUTHWEST,
            SOUTH,
            SOUTHEAST,
            NODIR
        }

        public MobjInfo info;
        public State state;
        public StateNum currentState;
        public MobjType type;
        public MobjFlags flags;

        public Vector3 position;
        public Quaternion rotation;

        public SpriteNum sprite;
        public int frame;
        public float radius;
        public float height;

        public MobjBrain brain;
        public MobjProxy proxy;

        public Mobj target;
        public static Mobj player => Main.player;
        public Mobj tracer;

        public Vector3 momentum;

        public int validCount;

        public float health;
        public int tics;
        public MoveDirection moveDirection;
        public int moveCount;
        public int reactionTime;
        public int threshold;
        public int lastLook;

        public Rigidbody rigidbody;
        public BoxCollider collider;

        public Player_Health playerHealth;
        public TriggerRefProxy triggerRefProxy;

        public AudioSource audioSource;

        private bool removedMobj;

        private void Awake()
        {
            info = Info.MobjInfos[(int)type];
            playerHealth = BoneLib.Player.rigManager.GetComponent<Player_Health>();
        }

        private void Start()
        {
            DoomGame.Instance.OnTick += WorldTick;
        }

        private void OnDestroy()
        {
            DoomGame.Instance.OnTick -= WorldTick;
        }

        private void WorldTick()
        {
            Mobj.player.health = Mobj.player.playerHealth.curr_Health;

            if(Settings.DisableAI)
            {
                return;
            }

            UpdateThinker();
        }

        public void UpdateThinker()
        {
            if(Settings.DisableAI)
            {
                return;
            }

            // Too far away, stop updating
            if(Vector3.Distance(transform.position, BoneLib.Player.playerHead.position) > 2048)
            {
                return;
            }

            if (tics != -1)
            {
                tics--;

                if (tics == 0)
                {
                    if(!SetState(state.nextstate))
                    {
                        return;
                    }
                }
            }
        }

        public bool SetState(StateNum state)
        {
            State st;

            currentState = state;

            if (state == StateNum.S_NULL)
            {
                this.state = Info.GetState(StateNum.S_NULL);
                MobjManager.Instance.RemoveMobj(this);
                return false;
            }

            st = Info.GetState(state);
            this.state = st;
            this.tics = st.tics;
            this.sprite = st.sprite;
            this.frame = st.frame;

            System.Action<Mobj> action = st.action;

            BoneLib.SafeActions.InvokeActionSafe(action, this);

            return true;
        }

        public void OnSpawn(Vector3 position, MobjType type)
        {
            this.info = Info.MobjInfos[(int)type];

            this.type = type;
            this.radius = info.radius;
            this.height = info.height;
            this.flags = info.flags;
            this.health = info.spawnHealth;
            this.reactionTime = info.reactionTime;

            State st = Info.GetState(info.spawnState);

            this.state = st;
            this.tics = st.tics;
            this.sprite = st.sprite;
            this.frame = st.frame;

            transform.position = position;
        }

        public void OnRemove()
        {
            var spriteRenderer = GetComponentInChildren<Rendering.MobjRenderer>();
            Destroy(spriteRenderer);
            Destroy(gameObject);
        }

        public void SetFlag(MobjFlags flags)
        {
            this.flags ^= flags;

            collider.enabled = flags.HasFlag(MobjFlags.MF_SOLID);
            rigidbody.useGravity = !flags.HasFlag(MobjFlags.MF_NOGRAVITY);
        }

        public bool LineAttack(float damage, float distance)
        {
            if(target == null)
            {
                return false;
            }

            Ray ray = new Ray(transform.position, target.transform.position - transform.position);

            if(Physics.Raycast(ray, out RaycastHit hit, distance))
            {
                Collider collider = hit.collider;
                ImpactProperties impactProperties = collider.GetComponent<ImpactProperties>();

                if(impactProperties != null)
                {
                    MobjManager.Instance.SpawnMobj(hit.point, MobjType.MT_PUFF);
                    return false;
                }

                Mobj hitMobj = collider.GetComponent<Mobj>();

                if(hitMobj == null)
                {
                    return false;
                }

                if(hitMobj == this)
                {
                    return false; // Can't shoot at ourselves
                }

                if(!hitMobj.flags.HasFlag(MobjFlags.MF_SHOOTABLE))
                {
                    return false; // Corpse
                }

                if(hitMobj.flags.HasFlag(MobjFlags.MF_NOBLOOD))
                {
                    MobjManager.Instance.SpawnMobj(hit.point, MobjType.MT_PUFF);
                }
                else
                {
                    MobjManager.Instance.SpawnMobj(hit.point, MobjType.MT_BLOOD);
                }
                
                if(damage > 0f)
                {
                    if(hitMobj == Mobj.player)
                    {
                        Mobj.player.playerHealth.TAKEDAMAGE(damage);
                    }
                    else
                    {
                        hitMobj.TakeDamage(damage, this, this);
                    }
                }

                return true;
            }

            return false;
        }

        public bool RadiusAttack(Mobj spot, float bombDamage)
        {
            if(!flags.HasFlag(MobjFlags.MF_SHOOTABLE))
            {
                return true;
            }

            // take no damage from rockets if we're a cyborg or spider
            if(type == MobjType.MT_CYBORG || type == MobjType.MT_SPIDER)
            {
                return true;
            }

            float dx = Mathf.Abs(transform.position.x - spot.transform.position.x);
            float dz = Mathf.Abs(transform.position.z - spot.transform.position.z);

            float distance = dx > dz ? dx : dz;
            distance = (distance - radius / 32f) * 2;

            if(distance < 0)
            {
                distance = 0;
            }

            if(distance > bombDamage)
            {
                return true;
            }

            if(target.brain.CheckSight(spot))
            {
                target.TakeDamage(bombDamage  - distance, this, spot);
            }

            return true;
        }

        public void TakeDamage(float damage, Mobj source, Mobj inflictor)
        {
            if(!flags.HasFlag(MobjFlags.MF_SHOOTABLE))
            {
                return;
            }

            if (health <= 0f)
            {
                return;
            }

            if(flags.HasFlag(MobjFlags.MF_SKULLFLY))
            {
                rigidbody.velocity = Vector3.zero;
            }

            health -= damage;
            if(health <= 0)
            {
                Kill();
                return;
            }

            if(DoomGame.RNG.P_Random() < info.painChance && !flags.HasFlag(MobjFlags.MF_SKULLFLY))
            {
                flags |= MobjFlags.MF_JUSTHIT;
                SetState(info.painState);
            }

            reactionTime = 0;

            if (threshold == 0 || type == MobjType.MT_VILE
                && source != null && source != target
                && source.type != MobjType.MT_VILE)
            {
                target = source;
                threshold = 8;

                if(target.state == Info.GetState(target.info.spawnState))
                {
                    target.SetState(target.info.seeState);
                }
            }
        }

        public void Kill()
        {
            if(this == player)
            {
                return;
            }

            flags &= ~(MobjFlags.MF_SHOOTABLE | MobjFlags.MF_FLOAT | MobjFlags.MF_SKULLFLY);

            if (!flags.HasFlag(MobjFlags.MF_FLOAT))
            {
                if(rigidbody != null)
                {
                    rigidbody.useGravity = true;
                }
            }

            if(type == MobjType.MT_SKULL)
            {
                flags &= ~MobjFlags.MF_NOGRAVITY;
            }
            
            if(collider != null)
            {
                collider.size = new Vector3(radius / 32f, (height / 32f) / 4f, radius / 32f);
            }

            if (health < -info.spawnHealth && info.xDeathState != StateNum.S_NULL)
            {
                SetState(info.xDeathState);
            }
            else
            {
                SetState(info.deathState);
            }

            tics -= DoomGame.RNG.P_Random() & 3;

            if(tics < 1)
            {
                tics = 1;
            }

            if(flags.HasFlag(MobjFlags.MF_COUNTKILL))
            {
                if(Mobj.player.playerHealth.deathIsImminent)
                {
                    Mobj.player.playerHealth.LifeSavingDamgeDealt();
                }
            }

            SpawnMobjAmmo(type);

            OnDeath?.Invoke(this);
        }

        public void SpawnMobjAmmo(MobjType type)
        {
            string lightAmmoBarcode = "c1534c5a-683b-4c01-b378-6795416d6d6f";
            string mediumAmmoBarcode = "c1534c5a-57d4-4468-b5f0-c795416d6d6f";
            string heavyAmmoBarcode = "c1534c5a-97a9-43f7-be30-6095416d6d6f";

            string targetBarcode;

            switch(type)
            {
                case MobjType.MT_WOLFSS:
                case MobjType.MT_POSSESSED:
                    targetBarcode = lightAmmoBarcode;
                    break;
                case MobjType.MT_SHOTGUY:
                    targetBarcode = heavyAmmoBarcode;
                    break;
                case MobjType.MT_CHAINGUY:
                    targetBarcode = mediumAmmoBarcode;
                    break;
                default:
                    return;
            }

            SpawnableCrateReference ammoCrateRef = new SpawnableCrateReference(targetBarcode);
            Spawnable ammo = new Spawnable()
            {
                crateRef = ammoCrateRef
            };
            
            AssetSpawner.Register(ammo);
            BoneLib.Nullables.NullableMethodExtensions.PoolManager_Spawn(ammo, transform.position, default, default);
        }
    }
}