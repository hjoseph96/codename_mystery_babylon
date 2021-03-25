// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using System;

namespace CreativeSpore
{
#if !UNITY_2018_3_OR_NEWER
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        private int m_X;

        private int m_Y;

        private static readonly Vector2Int s_Zero = new Vector2Int(0, 0);

        private static readonly Vector2Int s_One = new Vector2Int(1, 1);

        private static readonly Vector2Int s_Up = new Vector2Int(0, 1);

        private static readonly Vector2Int s_Down = new Vector2Int(0, -1);

        private static readonly Vector2Int s_Left = new Vector2Int(-1, 0);

        private static readonly Vector2Int s_Right = new Vector2Int(1, 0);

        /// <summary>
        ///   <para>X component of the vector.</para>
        /// </summary>
        public int x
        {
            get
            {
                return this.m_X;
            }
            set
            {
                this.m_X = value;
            }
        }

        /// <summary>
        ///   <para>Y component of the vector.</para>
        /// </summary>
        public int y
        {
            get
            {
                return this.m_Y;
            }
            set
            {
                this.m_Y = value;
            }
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    default:
                        throw new IndexOutOfRangeException(string.Format("Invalid Vector2Int index addressed: {0}!", index));
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException(string.Format("Invalid Vector2Int index addressed: {0}!", index));
                }
            }
        }

        /// <summary>
        ///   <para>Returns the length of this vector (Read Only).</para>
        /// </summary>
        public float magnitude
        {
            get
            {
                return Mathf.Sqrt((float)(this.x * this.x + this.y * this.y));
            }
        }

        /// <summary>
        ///   <para>Returns the squared length of this vector (Read Only).</para>
        /// </summary>
        public int sqrMagnitude
        {
            get
            {
                return this.x * this.x + this.y * this.y;
            }
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector2Int (0, 0).</para>
        /// </summary>
        public static Vector2Int zero
        {
            get
            {
                return Vector2Int.s_Zero;
            }
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector2Int (1, 1).</para>
        /// </summary>
        public static Vector2Int one
        {
            get
            {
                return Vector2Int.s_One;
            }
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector2Int (0, 1).</para>
        /// </summary>
        public static Vector2Int up
        {
            get
            {
                return Vector2Int.s_Up;
            }
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector2Int (0, -1).</para>
        /// </summary>
        public static Vector2Int down
        {
            get
            {
                return Vector2Int.s_Down;
            }
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector2Int (-1, 0).</para>
        /// </summary>
        public static Vector2Int left
        {
            get
            {
                return Vector2Int.s_Left;
            }
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector2Int (1, 0).</para>
        /// </summary>
        public static Vector2Int right
        {
            get
            {
                return Vector2Int.s_Right;
            }
        }

        public Vector2Int(int x, int y)
        {
            this.m_X = x;
            this.m_Y = y;
        }

        /// <summary>
        ///   <para>Set x and y components of an existing Vector2Int.</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Set(int x, int y)
        {
            this.m_X = x;
            this.m_Y = y;
        }

        /// <summary>
        ///   <para>Returns the distance between a and b.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static float Distance(Vector2Int a, Vector2Int b)
        {
            return (a - b).magnitude;
        }

        /// <summary>
        ///   <para>Returns a vector that is made from the smallest components of two vectors.</para>
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        public static Vector2Int Min(Vector2Int lhs, Vector2Int rhs)
        {
            return new Vector2Int(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
        }

        /// <summary>
        ///   <para>Returns a vector that is made from the largest components of two vectors.</para>
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        public static Vector2Int Max(Vector2Int lhs, Vector2Int rhs)
        {
            return new Vector2Int(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
        }

        /// <summary>
        ///   <para>Multiplies two vectors component-wise.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static Vector2Int Scale(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x * b.x, a.y * b.y);
        }

        /// <summary>
        ///   <para>Multiplies every component of this vector by the same component of scale.</para>
        /// </summary>
        /// <param name="scale"></param>
        public void Scale(Vector2Int scale)
        {
            this.x *= scale.x;
            this.y *= scale.y;
        }

        /// <summary>
        ///   <para>Clamps the Vector2Int to the bounds given by min and max.</para>
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Clamp(Vector2Int min, Vector2Int max)
        {
            this.x = Math.Max(min.x, this.x);
            this.x = Math.Min(max.x, this.x);
            this.y = Math.Max(min.y, this.y);
            this.y = Math.Min(max.y, this.y);
        }

        public static implicit operator Vector2(Vector2Int v)
        {
            return new Vector2((float)v.x, (float)v.y);
        }

        /// <summary>
        ///   <para>Converts a Vector2 to a Vector2Int by doing a Floor to each value.</para>
        /// </summary>
        /// <param name="v"></param>
        public static Vector2Int FloorToInt(Vector2 v)
        {
            return new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
        }

        /// <summary>
        ///   <para>Converts a  Vector2 to a Vector2Int by doing a Ceiling to each value.</para>
        /// </summary>
        /// <param name="v"></param>
        public static Vector2Int CeilToInt(Vector2 v)
        {
            return new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
        }

        /// <summary>
        ///   <para>Converts a  Vector2 to a Vector2Int by doing a Round to each value.</para>
        /// </summary>
        /// <param name="v"></param>
        public static Vector2Int RoundToInt(Vector2 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x - b.x, a.y - b.y);
        }

        public static Vector2Int operator *(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x * b.x, a.y * b.y);
        }

        public static Vector2Int operator *(Vector2Int a, int b)
        {
            return new Vector2Int(a.x * b, a.y * b);
        }

        public static bool operator ==(Vector2Int lhs, Vector2Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(Vector2Int lhs, Vector2Int rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        ///   <para>Returns true if the objects are equal.</para>
        /// </summary>
        /// <param name="other"></param>
        public override bool Equals(object other)
        {
            if (!(other is Vector2Int))
            {
                return false;
            }
            return this.Equals((Vector2Int)other);
        }

        public bool Equals(Vector2Int other)
        {
            return this.x.Equals(other.x) && this.y.Equals(other.y);
        }

        /// <summary>
        ///   <para>Gets the hash code for the Vector2Int.</para>
        /// </summary>
        /// <returns>
        ///   <para>The hash code of the Vector2Int.</para>
        /// </returns>
        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
        }

        /// <summary>
        ///   <para>Returns a nicely formatted string for this vector.</para>
        /// </summary>
        public override string ToString()
        {
            return string.Format("({0}, {1})", this.x, this.y);
        }
    }
#endif
}
