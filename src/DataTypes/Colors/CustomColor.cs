﻿using System;

namespace BinarySerializer
{
    /// <summary>
    /// A standard ARGB color
    /// </summary>
    public class CustomColor : BaseColor 
    {
        public CustomColor() { }
        public CustomColor(float r, float g, float b, float a = 1f) : base(r, g, b, a) { }

        public override float Red { get; set; }
        public override float Green { get; set; }
        public override float Blue { get; set; }
        public override float Alpha { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            throw new BinarySerializableException(this, "Custom colors can't be serialized");
        }
    }
}