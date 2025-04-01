using UnityEngine;

namespace NEP.DOOMBBQ.Entities
{
    public class MobjProxy : MonoBehaviour 
    {
        private Mobj mobj;
        private SphereCollider sphereCollider;

        private void Awake()
        {
            mobj = GetComponentInParent<Mobj>();

            sphereCollider = GetComponent<SphereCollider>();
        }
    }
}