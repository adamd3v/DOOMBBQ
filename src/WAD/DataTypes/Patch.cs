﻿using System.Collections.Generic;
using UnityEngine;

namespace NEP.DOOMLAB.WAD.DataTypes
{
    public struct Patch
    {
        public struct Pixel
        {
            public int x;
            public int y;
            public byte color;
        }
        
        public struct Column
        {
            public List<Pixel> colorData;
            public int offset;
        }

        public Patch(string name, short width, short height, short leftOffset, short topOffset)
        {
            this.name = name;
            this.width = width;
            this.height = height;
            this.leftOffset = leftOffset;
            this.topOffset = topOffset;
            columns = new Column[width];
            output = null;
        }

        public string name;
        public short width;
        public short height;
        public short leftOffset;
        public short topOffset;
        public Column[] columns;
        public Texture2D output;
    }
}
