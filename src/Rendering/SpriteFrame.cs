using NEP.DOOMBBQ.WAD.DataTypes;

namespace NEP.DOOMBBQ.Rendering
{
    public class SpriteFrame
    {
        public SpriteFrame()
        {
            patches = new Patch[8];
            flipBits = new bool[8];
        }

        public int numRotations;
        public bool canRotate;
        public bool[] mirrorSprite;
        public Patch[] patches;
        public bool[] flipBits;
    }
}
