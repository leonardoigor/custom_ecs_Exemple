using System;
using UnityEngine;

[Serializable]
public struct vector2
{
    public float x;
    public float y;

    public vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    // -------- Magnitude --------
    public float magnitude => MathF.Sqrt(x * x + y * y);
    public float sqrMagnitude => x * x + y * y;

    public vector2 normalized
    {
        get
        {
            var mag = magnitude;
            return mag > 1e-5f ? this / mag : default;
        }
    }

    // -------- Operators --------
    public static vector2 operator +(vector2 a, vector2 b)
        => new vector2(a.x + b.x, a.y + b.y);

    public static vector2 operator *(vector2 a, float d)
        => new vector2(a.x * d, a.y * d);

    // -------- Unity interop --------

    // Unity -> MiniECS (ÚNICO implícito permitido)
    public static implicit operator vector2(Vector2 v)
        => new vector2(v.x, v.y);

    public static implicit operator vector2(Vector3 v)
        => new vector2(v.x, v.y);

    // MiniECS -> Unity (EXPLÍCITO)
    public static explicit operator Vector2(vector2 v)
        => new Vector2(v.x, v.y);

    public static explicit operator Vector3(vector2 v)
        => new Vector3(v.x, v.y, 0f);
    public static vector2 operator /(vector2 a, float d)
    => new vector2(a.x / d, a.y / d);


    public override string ToString()
        => $"({x}, {y})";
}



[Serializable]
public struct vector3
{
    public float x;
    public float y;
    public float z;

    public vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // -------- Magnitude --------
    public float magnitude => MathF.Sqrt(x * x + y * y + z * z);
    public float sqrMagnitude => x * x + y * y + z * z;

    public vector3 normalized
    {
        get
        {
            var mag = magnitude;
            return mag > 1e-5f ? this / mag : default;
        }
    }

    // -------- Operators --------
    public static vector3 operator +(vector3 a, vector3 b)
        => new vector3(a.x + b.x, a.y + b.y, a.z + b.z);

    public static vector3 operator *(vector3 a, float d)
        => new vector3(a.x * d, a.y * d, a.z * d);

    // -------- Unity interop --------

    // Unity -> MiniECS
    public static implicit operator vector3(Vector3 v)
        => new vector3(v.x, v.y, v.z);

    public static implicit operator vector3(Vector2 v)
        => new vector3(v.x, v.y, 0f);

    // MiniECS -> Unity (EXPLÍCITO)
    public static explicit operator Vector3(vector3 v)
        => new Vector3(v.x, v.y, v.z);

    public static explicit operator Vector2(vector3 v)
        => new Vector2(v.x, v.y);
    public static vector3 operator /(vector3 a, float d)
    => new vector3(a.x / d, a.y / d, a.z / d);
    public override string ToString()
        => $"({x}, {y}, {z})";
}