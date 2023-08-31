using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEP.DOOMLAB.Entities
{
    [MelonLoader.RegisterTypeInIl2Cpp]
    public class MobjCollisionEvents : MonoBehaviour
    {
        public MobjCollisionEvents(System.IntPtr ptr) : base(ptr) { }

        private Mobj mobj;

        private void Awake()
        {
            mobj = GetComponent<Mobj>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(mobj.flags.HasFlag(MobjFlags.MF_MISSILE))
            {
                ExplodeMissile(mobj);
            }
        }

        private void ExplodeMissile(Mobj missile)
        {
            missile.SetState(mobj.info.deathState);
            missile.rigidbody.velocity = Vector3.zero;
            missile.collider.enabled = false;

            var hits = Physics.BoxCastAll(missile.transform.position, Vector3.one, missile.transform.position);
            Player_Health health = null;
            List<Mobj> mobjs = new List<Mobj>();

            foreach(var hit in hits)
            {
                Mobj other = hit.transform.GetComponent<Mobj>();
                MelonLoader.MelonLogger.Msg(hit.rigidbody?.name);

                if(other != null)
                {
                    mobjs.Add(other);
                }
            }

            foreach(var mobj in mobjs)
            {
                mobj.TakeDamage(20, missile);
            }
        }
    }

}