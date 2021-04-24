﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BinarySerializer
{
    public abstract class BaseColor : BinarySerializable, IEquatable<BaseColor> 
    {
        #region Constructors

        protected BaseColor()
        {
            Alpha = 1f;
        }
        protected BaseColor(float r, float g, float b, float a = 1f) 
        {
            Red = r;
            Green = g;
            Blue = b;
            Alpha = a;
        }
        protected BaseColor(uint colorValue) 
        {
            ColorValue = colorValue;
        }

        #endregion

        #region Protected Methods

        protected float GetFactor(int count) => (float)(Math.Pow(2, count) - 1);

        protected float GetValue(ColorChannel channel)
        {
            if (!ColorFormatting.ContainsKey(channel))
                return channel == ColorChannel.Alpha ? 1f : 0f;

            return GetValue(ColorFormatting[channel]);
        }
        protected float GetValue(ColorChannelFormat format) => BitHelpers.ExtractBits((int)ColorValue, format.Count, format.Offset) / GetFactor(format.Count);

        protected void SetValue(ColorChannel channel, float value)
        {
            if (!ColorFormatting.ContainsKey(channel))
                return;

            SetValue(ColorFormatting[channel], value);
        }
        protected void SetValue(ColorChannelFormat format, float value) => ColorValue = (uint)BitHelpers.SetBits((int)ColorValue, (int)(value * GetFactor(format.Count)), format.Count, format.Offset);

        #endregion

        #region Protected Properties

        protected abstract IReadOnlyDictionary<ColorChannel, ColorChannelFormat> ColorFormatting { get; }

        #endregion

        #region Public Properties

        public uint ColorValue { get; set; }

        public virtual float Red
        {
            get => GetValue(ColorChannel.Red);
            set => SetValue(ColorChannel.Red, value);
        }
        public virtual float Green
        {
            get => GetValue(ColorChannel.Green);
            set => SetValue(ColorChannel.Green, value);
        }
        public virtual float Blue
        {
            get => GetValue(ColorChannel.Blue);
            set => SetValue(ColorChannel.Blue, value);
        }
        public virtual float Alpha
        {
            get => GetValue(ColorChannel.Alpha);
            set => SetValue(ColorChannel.Alpha, value);
        }

        #endregion

        #region Public Static Properties

        public static BaseColor Clear => new CustomColor(0, 0, 0, 0);
        public static BaseColor Black => new CustomColor(0, 0, 0, 1);
        public static BaseColor White => new CustomColor(1, 1, 1, 1);

        #endregion

        #region Equality

        public bool Equals(BaseColor other)
        {
            if (ReferenceEquals(null, other)) 
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Alpha == other.Alpha && Red == other.Red && Green == other.Green && Blue == other.Blue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) 
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;
            if (obj.GetType() != GetType()) 
                return false;

            return Equals((BaseColor)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Alpha.GetHashCode();
                hashCode = (hashCode * 397) ^ Red.GetHashCode();
                hashCode = (hashCode * 397) ^ Green.GetHashCode();
                hashCode = (hashCode * 397) ^ Blue.GetHashCode();
                return hashCode;
            }
        }

        #endregion

        #region Serializable

        public override bool IsShortLog => true;
        public override string ShortLog => ToString();
        public override string ToString() => $"RGBA({(int)(Red * 255)}, {(int)(Green * 255)}, {(int)(Blue * 255)}, {Alpha})";

        public override void SerializeImpl(SerializerObject s)
        {
            var maxBit = ColorFormatting.Values.Max(x => x.Offset + x.Count);

            if (maxBit <= 8)
                ColorValue = s.Serialize<byte>((byte)ColorValue, name: nameof(ColorValue));
            else if (maxBit <= 16)
                ColorValue = s.Serialize<ushort>((ushort)ColorValue, name: nameof(ColorValue));
            else if (maxBit <= 24)
                ColorValue = s.Serialize<UInt24>((UInt24)ColorValue, name: nameof(ColorValue));
            else if (maxBit <= 32)
                ColorValue = s.Serialize<uint>((uint)ColorValue, name: nameof(ColorValue));
            else
                throw new NotImplementedException("Color format with more than 32 bits is currently not supported");
        }

        #endregion

        #region Format Structs

        protected enum ColorChannel
        {
            Red,
            Green,
            Blue,
            Alpha
        }

        protected class ColorChannelFormat
        {
            public ColorChannelFormat(int offset, int count)
            {
                Offset = offset;
                Count = count;
            }

            public int Offset { get; }
            public int Count { get; }
        }

        #endregion
    }
}