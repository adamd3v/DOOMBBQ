using System.Collections.Generic;

using UnityEngine;

using NEP.DOOMBBQ.Data;
using NEP.DOOMBBQ.Game;
using NEP.DOOMBBQ.Sound;

namespace NEP.DOOMBBQ.Entities
{
    public class MobjBrain : MonoBehaviour
    {
        public bool SeesTarget;

        public Mobj mobj;

        private List<Mobj> listBrainTargets;

        private float[] directions = new float[]
        {
            90,
            45,
            0,
            315,
            270,
            225,
            180,
            135
        };

        private int moveIndex = 0;

        private Mobj.MoveDirection[] opposite = new Mobj.MoveDirection[]
        {
            Mobj.MoveDirection.WEST, Mobj.MoveDirection.SOUTHWEST, Mobj.MoveDirection.SOUTH, Mobj.MoveDirection.SOUTHEAST,
            Mobj.MoveDirection.EAST, Mobj.MoveDirection.NORTHEAST, Mobj.MoveDirection.NORTH, Mobj.MoveDirection.NORTHWEST, Mobj.MoveDirection.NODIR
        };

        private Mobj.MoveDirection[] diags = new Mobj.MoveDirection[]
        {
           Mobj.MoveDirection.NORTHWEST, Mobj.MoveDirection.NORTHEAST, Mobj.MoveDirection.SOUTHWEST, Mobj.MoveDirection.SOUTHEAST,
        };

        private void Awake()
        {
            mobj = GetComponent<Mobj>();
        }

        public void SetMoveDirection(Mobj.MoveDirection direction)
        {
            if (mobj == null || direction == Mobj.MoveDirection.NODIR)
            {
                return;
            }

            mobj.moveDirection = direction;
            Quaternion rotation = Quaternion.AngleAxis(directions[(int)direction], Vector3.up);
            mobj.transform.rotation = rotation;
        }

        public bool Move()
        {
            if (mobj.moveDirection == Mobj.MoveDirection.NODIR)
            {
                return false;
            }

            if ((int)mobj.moveDirection >= 8)
            {
                return false;

            }

            Transform mobjTransform = mobj.transform;
            Transform targetTransform = mobj.target.transform;

            if(Physics.BoxCast(mobjTransform.position + Vector3.up * 0.5f, new Vector3(0.25f, 0.25f, 0.075f), mobjTransform.forward, out RaycastHit hit, mobjTransform.rotation, 1f))
            {
                Mobj other = hit.collider.GetComponent<Mobj>();

                if (other != null)
                {
                    if (other.flags.HasFlag(MobjFlags.MF_CORPSE))
                    {
                        Physics.IgnoreCollision(mobj.collider, other.collider);
                    }
                    else
                    {
                        return false;
                    }
                }

                if(hit.collider && !hit.collider.isTrigger)
                {
                    return false;
                }
            }

            if (mobj.flags.HasFlag(MobjFlags.MF_FLOAT) && mobj.target != null)
            {
                if (!mobj.flags.HasFlag(MobjFlags.MF_SKULLFLY) && !mobj.flags.HasFlag(MobjFlags.MF_INFLOAT))
                {
                    float delta = ((targetTransform.position.y - 1f + (mobj.info.height / 32f) / 2) - mobjTransform.position.y);
                    float closeDistance = Vector3.Distance(mobjTransform.position, targetTransform.position);

                    if (delta < -0.075 && closeDistance < 5f)
                    {
                        mobj.rigidbody.position -= (mobjTransform.up * mobj.info.speed) * Time.fixedDeltaTime;
                    }
                    else if (delta > 0.075 && closeDistance < 5f)
                    {
                        mobj.rigidbody.position += (mobjTransform.up * mobj.info.speed) * Time.fixedDeltaTime;
                    }
                }
            }

            mobj.rigidbody.position += (mobjTransform.forward * mobj.info.speed) * Time.fixedDeltaTime;
            return true;
        }

        public bool TryWalk()
        {
            if (!Move())
            {
                return false;
            }

            mobj.moveCount = DoomGame.RNG.P_Random() & 15;
            return true;
        }

        public void NewChaseDir()
        {
            if(mobj.target == null)
            {
                return;
            }

            Mobj.MoveDirection oldDir = mobj.moveDirection;
            Mobj.MoveDirection turnAround = opposite[(int)oldDir];
            Mobj.MoveDirection tempDir;

            float deltaX = mobj.target.transform.position.x - mobj.transform.position.x;
            float deltaZ = mobj.target.transform.position.z - mobj.transform.position.z;

            Mobj.MoveDirection[] possibleDirections = new Mobj.MoveDirection[2];

            if (deltaX > 1.5f)
            {
                possibleDirections[0] = Mobj.MoveDirection.EAST;
            }
            else if (deltaX < -1.5f)
            {
                possibleDirections[0] = Mobj.MoveDirection.WEST;
            }
            else
            {
                possibleDirections[0] = Mobj.MoveDirection.NODIR;
            }

            if (deltaZ > 1.5f)
            {
                possibleDirections[1] = Mobj.MoveDirection.NORTH;
            }
            else if (deltaZ < -1.5f)
            {
                possibleDirections[1] = Mobj.MoveDirection.SOUTH;
            }
            else
            {
                possibleDirections[1] = Mobj.MoveDirection.NODIR;
            }

            // try a diagonal/ordinal route
            // possible choices: north-east
            //                   north-west
            //                   south-east
            //                   south-west
            if (possibleDirections[0] != Mobj.MoveDirection.NODIR && possibleDirections[1] != Mobj.MoveDirection.NODIR)
            {
                SetMoveDirection(diags[(deltaZ < 0 ? 1 : 0) * 2 + (deltaX > 0 ? 1 : 0)]);
                 
                if(mobj.moveDirection != turnAround && TryWalk())
                {
                    return;
                }
            }

            if(DoomGame.RNG.P_Random() < 200 || Mathf.Abs(deltaZ) > Mathf.Abs(deltaX))
            {
                tempDir = possibleDirections[0];
                possibleDirections[0] = possibleDirections[1];
                possibleDirections[1] = tempDir;
            }

            if(possibleDirections[0] == turnAround)
            {
                possibleDirections[0] = Mobj.MoveDirection.NODIR;
            }

            if (possibleDirections[1] == turnAround)
            {
                possibleDirections[1] = Mobj.MoveDirection.NODIR;
            }

            if (possibleDirections[0] != Mobj.MoveDirection.NODIR)
            {
                SetMoveDirection(possibleDirections[0]);

                if(TryWalk())
                {
                    return;
                }
            }

            if(possibleDirections[1] != Mobj.MoveDirection.NODIR)
            {
                SetMoveDirection(possibleDirections[1]);

                if(TryWalk())
                {
                    return;
                }
            }

            if(oldDir != Mobj.MoveDirection.NODIR)
            {
                SetMoveDirection(oldDir);

                if(TryWalk())
                {
                    return;
                }
            }

            if ((DoomGame.RNG.P_Random() & 1) != 0)
            {
                for (tempDir = Mobj.MoveDirection.EAST;
                    tempDir <= Mobj.MoveDirection.SOUTHEAST;
                    tempDir++)
                {
                    if (tempDir != turnAround)
                    {
                        SetMoveDirection(tempDir);

                        if (TryWalk())
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                for(tempDir = Mobj.MoveDirection.SOUTHEAST;
                    tempDir != Mobj.MoveDirection.EAST - 1;
                    tempDir--)
                    {
                        if(tempDir != turnAround)
                        {
                            SetMoveDirection(tempDir);

                            if(TryWalk())
                            {
                                return;
                            }
                        }
                    }
            }

            if(turnAround != Mobj.MoveDirection.NODIR)
            {
                SetMoveDirection(turnAround);
                
                if (TryWalk())
                {
                    return;
                }
            }

            mobj.moveDirection = Mobj.MoveDirection.NODIR;
        }

        public bool CheckMeleeRange()
        {
            if (mobj.target == null)
            {
                return false;
            }

            if (Vector3.Distance(mobj.transform.position, mobj.target.transform.position) > 2f)
            {
                return false;
            }

            if(!MobjInteraction.CheckSight(mobj, mobj.target))
            {
                return false;
            }

            return true;
        }

        public bool CheckMissileRange()
        {
            if (!MobjInteraction.CheckSight(mobj, mobj.target))
            {
                return false;
            }

            if (mobj.flags.HasFlag(MobjFlags.MF_JUSTHIT))
            {
                mobj.flags &= ~MobjFlags.MF_JUSTHIT;
                return true;
            }

            if (mobj.reactionTime != 0)
            {
                return false;
            }

            float distance = Vector3.Distance(mobj.transform.position, mobj.target.transform.position);

            if (mobj.info.meleeState == StateNum.S_NULL)
            {
                distance -= 4f;
            }

            distance *= 8;

            if (mobj.type == MobjType.MT_VILE)
            {
                if (distance > 28f)
                {
                    return false;
                }
            }

            if (mobj.type == MobjType.MT_UNDEAD)
            {
                if (distance < 6.125)
                {
                    return false;
                }

                distance *= 2;
            }

            if (mobj.type == MobjType.MT_CYBORG
                || mobj.type == MobjType.MT_SPIDER
                || mobj.type == MobjType.MT_SKULL)
            {
                distance *= 2;
            }

            if (distance > 6.25f)
            {
                distance = 6.25f;
            }

            if (mobj.type == MobjType.MT_CYBORG && distance > 5)
            {
                distance = 5;
            }

            if (DoomGame.RNG.P_Random() < distance)
            {
                return false;
            }

            return true;
        }

        public bool FindPlayer()
        {
            Transform playerTransform = Mobj.player.transform;
            Transform mobjTransform = mobj.transform;

            Vector3 direction = playerTransform.position - mobjTransform.position;
            float angle = Vector3.Angle(direction, mobjTransform.forward);

            if (!MobjInteraction.CheckSight(mobj, Mobj.player))
            {
                // Out of sight
                return false;
            }

            // Out of sight
            if (angle < 45f)
            {
                mobj.target = Mobj.player;
                mobj.sightedPlayer = true;
                return true;
            }
            else if (Vector3.Distance(playerTransform.position, mobjTransform.position) < (mobj.radius / 32f) + 2f)
            {
                mobj.target = Mobj.player;
                mobj.sightedPlayer = true;
                return true;
            }

            return true;
        }

        public void A_Look()
        {
            if (!FindPlayer())
            {
                return;
            }

            if (mobj.target != null && mobj.flags.HasFlag(MobjFlags.MF_SHOOTABLE))
            {
                A_SeeYou();
            }
        }

        public void A_SeeYou()
        {
            SoundType sound;

            switch (mobj.info.seeSound)
            {
                case SoundType.sfx_posit1:
                case SoundType.sfx_posit2:
                case SoundType.sfx_posit3:
                    sound = SoundType.sfx_posit1 + DoomGame.RNG.P_Random() % 3;
                    break;

                case SoundType.sfx_bgsit1:
                case SoundType.sfx_bgsit2:
                    sound = SoundType.sfx_bgsit1 + DoomGame.RNG.P_Random() % 2;
                    break;

                default:
                    sound = mobj.info.seeSound;
                    break;
            }

            if (mobj.type == MobjType.MT_SPIDER || mobj.type == MobjType.MT_CYBORG)
            {
                SoundManager.Instance.PlaySound(sound, Vector3.zero, true);
            }
            else
            {
                SoundManager.Instance.PlaySound(sound, mobj.transform.position, false);
            }

            mobj.SetState(mobj.info.seeState);
        }


        public void A_Chase()
        {
            if (mobj.reactionTime != 0)
            {
                mobj.reactionTime--;
            }

            if (mobj.threshold != 0)
            {
                if (mobj.target != null && mobj.health <= 0)
                {
                    mobj.threshold = 0;
                }
                else
                {
                    mobj.threshold--;
                }
            }

            if (mobj.target == null || !mobj.target.flags.HasFlag(MobjFlags.MF_SHOOTABLE))
            {
                if(mobj.sightedPlayer)
                {
                    mobj.target = Mobj.player;
                }

                mobj.SetState(mobj.info.spawnState);
                return;
            }

            if (mobj.flags.HasFlag(MobjFlags.MF_JUSTATTACKED))
            {
                mobj.flags &= ~MobjFlags.MF_JUSTATTACKED;

                if(!Settings.FastMonsters)
                {
                    NewChaseDir();
                }

                return;
            }

            if (mobj.info.meleeState != StateNum.S_NULL && CheckMeleeRange())
            {
                if (mobj.info.attackSound != SoundType.sfx_None)
                {
                    SoundManager.Instance.PlaySound(mobj.info.attackSound, mobj.transform.position, false);
                }

                mobj.SetState(mobj.info.meleeState);
                return;
            }

            if (mobj.info.missileState != StateNum.S_NULL)
            {
                if (!Settings.FastMonsters && mobj.moveCount != 0)
                {
                    A_NoMissile();
                    return;
                }

                if (!CheckMissileRange())
                {
                    A_NoMissile();
                    return;
                }

                mobj.SetState(mobj.info.missileState);
                mobj.flags |= MobjFlags.MF_JUSTATTACKED;
                return;
            }

            A_NoMissile();
        }

        public void A_NoMissile()
        {
            if (mobj.threshold == 0 && !MobjInteraction.CheckSight(mobj, mobj.target))
            {
                if (FindPlayer())
                {
                    return;
                }
            }

            if (mobj.moveCount-- <= 0 || !Move())
            {
                NewChaseDir();
            }

            if (mobj.info.activeSound != SoundType.sfx_None && DoomGame.RNG.P_Random() < 3)
            {
                SoundManager.Instance.PlaySound(mobj.info.activeSound, mobj.transform.position, false);
            }
        }

        public void A_FaceTarget()
        {
            if (mobj.target == null)
            {
                return;
            }

            Quaternion lookAt = Quaternion.LookRotation(mobj.target.transform.position - mobj.transform.position, Vector3.up);
            mobj.transform.rotation = Quaternion.Euler(Vector3.up * lookAt.eulerAngles.y);
        }

        public void A_Explode()
        {
            MobjInteraction.RadiusAttack(mobj, mobj.target, 128);
        }

        public void A_TroopAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            A_FaceTarget();

            float damage = (DoomGame.RNG.P_Random() % 8 + 1) * 3;

            if (CheckMeleeRange())
            {
                SoundManager.Instance.PlaySound(SoundType.sfx_claw, mobj.transform.position, false);
                MobjInteraction.DamageMobj(mobj.target, mobj, mobj, damage);
                return;
            }

            MobjManager.Instance.SpawnMissile(mobj, mobj.target, Data.MobjType.MT_TROOPSHOT);
        }

        public void A_PosAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            A_FaceTarget();

            float randomAngle = (DoomGame.RNG.P_Random() - DoomGame.RNG.P_Random() & 20) / 10f;
            float damage = ((DoomGame.RNG.P_Random() % 5) + 1) * 3;
            RaycastHit hit;

            SoundManager.Instance.PlaySound(SoundType.sfx_pistol, mobj.transform.position, false);

            MobjInteraction.LineAttack(mobj, mobj.transform.position + Vector3.up, mobj.target.transform.position, damage / 10, 128);
        }

        public void A_SPosAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            A_FaceTarget();

            for (int i = 0; i < 3; i++)
            {
                float randomAngle = (DoomGame.RNG.P_Random() - DoomGame.RNG.P_Random() & 20) / 10f;
                float damage = ((DoomGame.RNG.P_Random() % 5) + 1) * 3;
                RaycastHit hit;
                MobjInteraction.LineAttack(mobj, mobj.transform.position + Vector3.up, mobj.target.transform.position, damage / 10, 128);
            }

            SoundManager.Instance.PlaySound(SoundType.sfx_shotgn, mobj.transform.position, false);

        }

        public void A_CPosAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            SoundManager.Instance.PlaySound(SoundType.sfx_shotgn, mobj.transform.position, false);
            A_FaceTarget();

            float randomAngle = (DoomGame.RNG.P_Random() - DoomGame.RNG.P_Random() & 20) / 10f;
            float damage = ((DoomGame.RNG.P_Random() % 5) + 1) * 3;
            RaycastHit hit;
            MobjInteraction.LineAttack(mobj, mobj.transform.position + Vector3.up, mobj.target.transform.position, damage / 10, 128);
        }

        public void A_CPosRefire()
        {
            A_FaceTarget();

            if (DoomGame.RNG.P_Random() < 40)
            {
                return;
            }

            if (mobj.target == null || mobj.target.health <= 0 || !MobjInteraction.CheckSight(mobj, mobj.target))
            {
                mobj.SetState(mobj.info.seeState);
            }
        }

        public void A_SpidRefire()
        {
            A_FaceTarget();

            if (DoomGame.RNG.P_Random() < 10)
            {
                return;
            }

            if (mobj.target == null || mobj.target.health == 0 || !MobjInteraction.CheckSight(mobj, mobj.target))
            {
                mobj.SetState(mobj.info.seeState);
            }
        }

        public void A_BspiAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            A_FaceTarget();

            // launch a missile
            MobjManager.Instance.SpawnMissile(mobj, mobj.target, MobjType.MT_ARACHPLAZ);
        }

        public void A_SargAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            A_FaceTarget();

            float damage = ((DoomGame.RNG.P_Random() % 10) + 1) * 4;

            if (CheckMeleeRange())
            {
                MobjInteraction.DamageMobj(mobj.target, mobj, mobj, damage);
            }
        }

        public void A_Tracer()
        {
            if(DoomGame.Instance.gameTic % 2 != 0)
            {
                return; 
            } 
            
            // MobjManager.Instance.SpawnMobj(mobj.position, MobjType.MT_PUFF);

            // disabled due to lag
            // var th = MobjManager.Instance.SpawnMobj(mobj.position, MobjType.MT_SMOKE);
            // th.tics -= DoomGame.RNG.P_Random() & 3;

            // var dest = th.tracer;

            Quaternion targetRot = Quaternion.LookRotation(mobj.tracer.position - mobj.transform.position, Vector3.up);
            mobj.transform.rotation = Quaternion.Slerp(mobj.transform.rotation, targetRot, 4f * Time.deltaTime);
            mobj.rigidbody.velocity = mobj.transform.forward * mobj.info.speed;
        }

        public void A_SkelMissile()
        {
            if(mobj.target == null)
            {
                return;
            }

            A_FaceTarget();
            mobj.transform.position += Vector3.up * 0.5f;
            Mobj tracer = MobjManager.Instance.SpawnMissile(mobj, mobj.target, MobjType.MT_TRACER);
            mobj.transform.position -= Vector3.up * 0.5f;

            tracer.transform.position += mobj.transform.forward;
            tracer.tracer = mobj.target;
        }

        public void A_SkelWhoosh()
        {
            if (mobj.target == null)
            {
                return;
            }

            A_FaceTarget();
            SoundManager.Instance.PlaySound(SoundType.sfx_skeswg, mobj.transform.position, false);
        }

        public void A_SkelFist()
        {
            if (mobj.target == null)
            {
                return;
            }

            A_FaceTarget();

            if (CheckMeleeRange())
            {
                SoundManager.Instance.PlaySound(SoundType.sfx_skepch, mobj.transform.position, false);
                // damage
            }
        }

        public void A_PainAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            A_FaceTarget();
            A_PainShootSkull();
        }

        public void A_PainDie()
        {
            A_Fall();
            A_PainShootSkull();
            A_PainShootSkull();
            A_PainShootSkull();
        }

        public void A_SkullAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            mobj.flags |= MobjFlags.MF_SKULLFLY;
            mobj.rigidbody.drag = 0f;

            SoundManager.Instance.PlaySound(mobj.info.attackSound, mobj.transform.position, false);
            A_FaceTarget();
            mobj.rigidbody.velocity += mobj.transform.forward * 6.25f;
        }

        public void A_PainShootSkull()
        {
            Mobj newMobj = MobjManager.Instance.SpawnMobj(mobj.transform.position + mobj.transform.forward, MobjType.MT_SKULL);
            newMobj.target = mobj.target;
            newMobj.brain.A_SkullAttack();
        }

        public void A_CyberAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            A_FaceTarget();

            MobjManager.Instance.SpawnMissile(mobj, mobj.target, Data.MobjType.MT_ROCKET);
        }

        public void A_HeadAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            A_FaceTarget();

            float damage = (DoomGame.RNG.P_Random() % 6 + 1) * 10;

            if (CheckMeleeRange())
            {
                MobjInteraction.DamageMobj(mobj.target, mobj, mobj, damage);
                return;
            }

            MobjManager.Instance.SpawnMissile(mobj, mobj.target, MobjType.MT_HEADSHOT);
        }

        public void A_BruisAttack()
        {
            if (mobj.target == null)
            {
                return;
            }

            float damage = (DoomGame.RNG.P_Random() % 8 + 1) * 10;

            if (CheckMeleeRange())
            {
                SoundManager.Instance.PlaySound(SoundType.sfx_claw, mobj.transform.position, false);
                MobjInteraction.DamageMobj(mobj.target, mobj, mobj, damage);
                return;
            }

            // launch a missile
            MobjManager.Instance.SpawnMissile(mobj, mobj.target, MobjType.MT_BRUISERSHOT);
        }

        public void A_FatRaise()
        {
            A_FaceTarget();
            SoundManager.Instance.PlaySound(SoundType.sfx_manatk, mobj.transform.position, false);
        }

        public void A_FatAttack1()
        {
            float fatSpread = (90 / 8) / 32f;
            A_FaceTarget();
            mobj.transform.rotation *= Quaternion.AngleAxis(fatSpread, Vector3.up);
            var firstBall = MobjManager.Instance.SpawnMissile(mobj, mobj.target, MobjType.MT_FATSHOT);

            var secondBall = MobjManager.Instance.SpawnMissile(mobj, mobj.target, MobjType.MT_FATSHOT);
            Physics.IgnoreCollision(firstBall.collider, secondBall.collider);
            secondBall.transform.rotation = mobj.transform.rotation;
            secondBall.rigidbody.velocity = secondBall.transform.forward * secondBall.info.speed;
        }

        public void A_FatAttack2()
        {
            float fatSpread = (90 / 8) / 32f;
            A_FaceTarget();
            mobj.transform.rotation *= Quaternion.AngleAxis(-fatSpread, Vector3.up);
            var firstBall = MobjManager.Instance.SpawnMissile(mobj, mobj.target, MobjType.MT_FATSHOT);

            var secondBall = MobjManager.Instance.SpawnMissile(mobj, mobj.target, MobjType.MT_FATSHOT);
            Physics.IgnoreCollision(firstBall.collider, secondBall.collider);
            secondBall.transform.rotation = mobj.transform.rotation;
            secondBall.rigidbody.velocity = secondBall.transform.forward * secondBall.info.speed;
        }

        public void A_FatAttack3()
        {
            float fatSpread = (90 / 8) / 32f;
            A_FaceTarget();
            mobj.transform.rotation *= Quaternion.AngleAxis(-fatSpread / 2, Vector3.up);
            var firstBall = MobjManager.Instance.SpawnMissile(mobj, mobj.target, MobjType.MT_FATSHOT);

            var secondBall = MobjManager.Instance.SpawnMissile(mobj, mobj.target, MobjType.MT_FATSHOT);
            Physics.IgnoreCollision(firstBall.collider, secondBall.collider);
            secondBall.transform.rotation = mobj.transform.rotation;
            secondBall.rigidbody.velocity = secondBall.transform.forward * secondBall.info.speed;
        }

        public void A_Hoof()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_hoof, mobj.transform.position, false);
            A_Chase();
        }

        public void A_Metal()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_metal, mobj.transform.position, false);
            A_Chase();
        }

        public void A_BabyMetal()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_bspwlk, mobj.transform.position, false);
            A_Chase();
        }

        public void A_Scream()
        {
            SoundType sound;

            switch (mobj.info.deathSound)
            {
                case 0:
                    return;

                case SoundType.sfx_podth1:
                case SoundType.sfx_podth2:
                case SoundType.sfx_podth3:
                    sound = SoundType.sfx_podth1 + DoomGame.RNG.P_Random() % 3;
                    break;

                case SoundType.sfx_bgdth1:
                case SoundType.sfx_bgdth2:
                    sound = SoundType.sfx_bgdth1 + DoomGame.RNG.P_Random() % 2;
                    break;

                default:
                    sound = mobj.info.deathSound;
                    break;
            }

            // Check for bosses.
            if (mobj.type == MobjType.MT_SPIDER || mobj.type == MobjType.MT_CYBORG)
            {
                // full volume
                SoundManager.Instance.PlaySound(sound, UnityEngine.Vector3.zero, true);
            }
            {
                SoundManager.Instance.PlaySound(sound, mobj.transform.position, false);
            }
        }

        public void A_Pain()
        {
            if (mobj.info.painSound != SoundType.sfx_None)
            {
                SoundManager.Instance.PlaySound(mobj.info.painSound, mobj.transform.position, false);
            }
        }

        public void A_XScream()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_slop, mobj.transform.position, false);
        }

        public void A_Fall()
        {
            mobj.flags &= ~MobjFlags.MF_SOLID;
        }

        public void A_VileChase()
        {
            if (mobj.moveDirection != Mobj.MoveDirection.NODIR)
            {
                var hits = Physics.BoxCastAll(mobj.transform.position, Vector3.one * 1, mobj.transform.position + mobj.transform.forward);

                for (int i = 0; i < hits.Length; i++)
                {
                    Mobj hit = hits[i].collider.GetComponent<Mobj>();

                    if(hit == mobj)
                    {
                        continue;
                    }

                    if(hit != null && MobjInteraction.VileCheckIterator(hit, out Mobj corpse))
                    {
                        var temp = mobj.target;
                        mobj.target = corpse;
                        A_FaceTarget();
                        mobj.target = temp;

                        mobj.SetState(StateNum.S_VILE_HEAL1);
                        SoundManager.Instance.PlaySound(SoundType.sfx_slop, mobj.transform.position, false);
                        var info = corpse.info;

                        corpse.SetState(info.raiseState);
                        corpse.collider.center = Vector3.up * (info.height / 32f) / 2f;
                        corpse.collider.size = new Vector3(info.radius / 32f, info.height / 32f, info.radius / 32f);
                        corpse.flags = info.flags;

                        // TODO: method for dynamically setting flags
                        if (!corpse.flags.HasFlag(MobjFlags.MF_SOLID))
                        {
                            corpse.collider.enabled = false;
                        }

                        if (corpse.flags.HasFlag(MobjFlags.MF_SPECIAL))
                        {
                            corpse.collider.enabled = true;
                            corpse.gameObject.AddComponent<MobjCollisionEvents>();
                        }

                        if (corpse.flags.HasFlag(MobjFlags.MF_NOGRAVITY))
                        {
                            corpse.rigidbody.useGravity = false;
                        }

                        if (corpse.flags.HasFlag(MobjFlags.MF_MISSILE))
                        {
                            corpse.collider.enabled = true;
                            corpse.rigidbody.drag = 0;
                            corpse.gameObject.AddComponent<MobjCollisionEvents>();
                        }

                        if (corpse.type == MobjType.MT_SKULL)
                        {
                            corpse.gameObject.AddComponent<MobjCollisionEvents>();
                        }

                        if (corpse.flags.HasFlag(MobjFlags.MF_FLOAT))
                        {
                            corpse.rigidbody.drag = 10f;
                        }

                        corpse.health = info.spawnHealth;
                        corpse.target = null;

                        return;
                    }
                }
            }

            A_Chase();
        }

        public void A_VileStart()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_vilatk, mobj.transform.position, false);
        }

        public void A_StartFire()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_flamst, mobj.transform.position, false);
            A_Fire();
        }

        public void A_FireCrackle()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_flame, mobj.transform.position, false);
            A_Fire();
        }
        public void A_Fire()
        {
            Mobj dest = mobj.tracer;

            if(dest == null)
            {
                return;
            }

            if(!MobjInteraction.CheckSight(mobj, dest))
            {
                return;
            }

            mobj.transform.position = dest.transform.position;
        }

        public void A_VileTarget()
        {
            if(mobj.target == null)
            {
                return;
            }

            Mobj fog = MobjManager.Instance.SpawnMobj(mobj.target.transform.position, MobjType.MT_FIRE);

            mobj.tracer = fog;
            fog.target = mobj;
            fog.tracer = mobj.target;
            fog.brain.A_Fire();
        }

        public void A_VileAttack()
        {
            if(mobj.target == null)
            {
                return;
            }

            A_FaceTarget();

            if(!MobjInteraction.CheckSight(mobj, mobj.target))
            {
                return;
            }

            SoundManager.Instance.PlaySound(SoundType.sfx_barexp, mobj.transform.position, false);

            if(mobj.target == Mobj.player)
            {
                
            }
            else
            {
                MobjInteraction.DamageMobj(mobj.target, mobj, mobj, 20);
                mobj.rigidbody.AddForce(Vector3.up * mobj.target.info.mass * 10f);
            }

            var fire = mobj.tracer;

            if(fire == null)
            {
                return;
            }
            
            fire.transform.position = mobj.target.transform.position;
            MobjInteraction.RadiusAttack(fire, mobj, 70);
        }

        public void A_BrainAwake()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_bossit, Vector3.zero, true);
        }

        public void A_BrainSpit()
        {
            // no targets for now, will implement later
            // instead, just shoot cubes in a random range of the mobjs field of view
            Vector3 randomSpot = (mobj.transform.position + mobj.transform.forward) + (Vector3.up * -1f) + (mobj.transform.right * UnityEngine.Random.Range(-0.5f, 0.5f));
            var target = MobjManager.Instance.SpawnMobj(randomSpot, MobjType.MT_BOSSTARGET);
            var newMobj = MobjManager.Instance.SpawnMissile(mobj, target, MobjType.MT_SPAWNSHOT);
            newMobj.reactionTime = (int)((target.transform.position.z - newMobj.transform.position.z) / newMobj.rigidbody.velocity.y) / newMobj.state.tics;
            SoundManager.Instance.PlaySound(SoundType.sfx_bospit, Vector3.zero, true);
            MobjManager.Instance.RemoveMobj(target);
        }

        public void A_BrainPain()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_bospn, Vector3.zero, true);
        }

        public void A_BrainScream()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_bosdth, Vector3.zero, true);
        }

        public void A_SpawnSound()
        {
            SoundManager.Instance.PlaySound(SoundType.sfx_boscub, mobj.transform.position, false);
            A_SpawnFly();
        }

        public void A_SpawnFly()
        {
            if(mobj.reactionTime-- > 0)
            {
                return;
            }

            MobjManager.Instance.SpawnMobj(mobj.transform.position, MobjType.MT_FIRE);
            SoundManager.Instance.PlaySound(SoundType.sfx_telept, mobj.transform.position, false);

            int r = DoomGame.RNG.P_Random();
            MobjType type;

            if (r < 50)
            {
                type = MobjType.MT_TROOP;
            }
            else if(r < 90)
            {
                type = MobjType.MT_SERGEANT;
            }
            else if(r < 120)
            {
                type = MobjType.MT_SHADOWS;
            }
            else if(r < 130)
            {
                type = MobjType.MT_PAIN;
            }
            else if(r < 160)
            {
                type = MobjType.MT_HEAD;
            }
            else if(r < 162)
            {
                type = MobjType.MT_VILE;
            }
            else if(r < 172)
            {
                type = MobjType.MT_UNDEAD;
            }
            else if(r < 192)
            {
                type = MobjType.MT_BABY;
            }
            else if(r < 222)
            {
                type = MobjType.MT_FATSO;
            }
            else if(r < 246)
            {
                type = MobjType.MT_KNIGHT;
            }
            else
            {
                type = MobjType.MT_BRUISER;
            }

            MobjManager.Instance.SpawnMobj(mobj.transform.position, type);
            MobjManager.Instance.RemoveMobj(mobj);
        }
    }
}
