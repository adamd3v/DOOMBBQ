using UnityEngine;

using NEP.DOOMBBQ.Entities;
using NEP.DOOMBBQ.Sound;

namespace NEP.DOOMBBQ
{
    public class DoomPlayer : MonoBehaviour
    {
        public static DoomPlayer Instance { get; private set; }

        public int ItemCount => itemCount;

        private int itemCount = 0;

        private void Awake()
        {
            Instance = this;
        }

        public void TouchSpecialThing(Mobj special)
        {
            SoundType sound = SoundType.sfx_itemup;

            switch(special.sprite)
            {
                case Data.SpriteNum.SPR_ARM1:
                    break;
                    
                case Data.SpriteNum.SPR_ARM2:
                    break;

                case Data.SpriteNum.SPR_BON1:
                    break;

                case Data.SpriteNum.SPR_BON2:
                    break;

                case Data.SpriteNum.SPR_SOUL:
                    sound = SoundType.sfx_getpow;
                    break;

                case Data.SpriteNum.SPR_MEGA:
                    sound = SoundType.sfx_getpow;
                    break;

                case Data.SpriteNum.SPR_BKEY:
                    break;

                case Data.SpriteNum.SPR_YKEY:
                    break;

                case Data.SpriteNum.SPR_RKEY:
                    break;

                case Data.SpriteNum.SPR_BSKU:
                    break;
                
                case Data.SpriteNum.SPR_YSKU:
                    break;

                case Data.SpriteNum.SPR_RSKU:
                    break;

                case Data.SpriteNum.SPR_STIM:
                    sound = SoundType.sfx_getpow;
                    break;

                case Data.SpriteNum.SPR_MEDI:
                    sound = SoundType.sfx_getpow;
                    break;

                case Data.SpriteNum.SPR_PINV:
                    sound = SoundType.sfx_getpow;
                    break;

                case Data.SpriteNum.SPR_PSTR:
                    sound = SoundType.sfx_getpow;
                    break;

                case Data.SpriteNum.SPR_PINS:
                    sound = SoundType.sfx_getpow;
                    break;

                case Data.SpriteNum.SPR_SUIT:
                    sound = SoundType.sfx_getpow;
                    break;

                case Data.SpriteNum.SPR_PMAP:
                    sound = SoundType.sfx_getpow;
                    break;

                case Data.SpriteNum.SPR_PVIS:
                    sound = SoundType.sfx_getpow;
                    break;

                case Data.SpriteNum.SPR_CLIP:
                    break;

                case Data.SpriteNum.SPR_AMMO:
                    break;

                case Data.SpriteNum.SPR_ROCK:
                    break;

                case Data.SpriteNum.SPR_BROK:
                    break;
                
                case Data.SpriteNum.SPR_CELL:
                    break;

                case Data.SpriteNum.SPR_CELP:
                    break;

                case Data.SpriteNum.SPR_SHEL:
                    break;

                case Data.SpriteNum.SPR_SBOX:
                    break;

                case Data.SpriteNum.SPR_BPAK:
                    break;
            }

            if(special.flags.HasFlag(MobjFlags.MF_COUNTITEM))
            {
                itemCount++;
            }

            MobjManager.Instance.RemoveMobj(special);
            SoundManager.Instance.PlaySound(sound, Vector3.zero, true);
        }
    }
}