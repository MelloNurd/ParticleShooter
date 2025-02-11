using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions
{
    // Created by: Ben Bonus
    // This is a collection of extension  methods made for using within the Unity Game Engine.
    // These are here simply to make code a little easier to read and write. You can use them as you would with any other function on the specified type.

    #region String Extensions
    /// <summary>
    /// Gets the first x characters of a string.
    /// </summary>
    /// <param name="count">The number of characters to get</param>
    /// <returns>The first x characters of a string</returns>
    public static string GetFirst(this string text, int count)
    {
        return text.Substring(0, count);
    }

    /// <summary>
    /// Gets the last x characters of a string.
    /// </summary>
    /// <param name="count">The number of characters to get</param>
    /// <returns>The last x characters of a string</returns>
    public static string GetLast(this string text, int count)
    {
        return text.Substring(text.Length - count, count);
    }

    /// <summary>
    /// Returns true if the string is null or empty.
    /// </summary>
    public static bool IsBlank(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Removes all non-digit from a string and returns the result.
    /// </summary>
    /// <returns>The new digit-only string</returns>
    public static string OnlyDigits(this string value)
    {
        return new string(value?.Where(c => char.IsDigit(c)).ToArray());
    }

    /// <summary>
    /// Removes all non-letter characters from a string and returns the result.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The new letter-only string</returns>
    public static string OnlyLetters(this string value)
    {
        return new string(value?.Where(c => char.IsLetter(c)).ToArray());
    }

    /// <summary>
    /// Removes all non-letter and non-digit characters from a string and returns the result.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The new letter-and-digit-only string</returns>
    public static string OnlyLettersAndDigits(this string value)
    {
        return new string(value?.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }
    #endregion

    #region List Extensions
    /// <summary>
    /// Gets a random value from the list
    /// </summary>
    /// <returns>A random value from the list, or the default value for an empty list.</returns>
    public static T GetRandom<T>(this IList<T> list) {
        if(list.Count == 0)
        {
            return default(T);
        }
        return list[Random.Range(0, list.Count)];
    }
    #endregion

    #region Dictonary Extensions
    /// <summary>
    /// Reverses the keys and values of a dictionary.
    /// </summary>
    /// <returns>A new dictonary with the keys and values swapped.</returns>
    public static Dictionary<TValue, TKey> Reverse<TKey, TValue>(this IDictionary<TKey, TValue> source)
    {
        var dictionary = new Dictionary<TValue, TKey>();
        foreach (var entry in source)
        {
            if (!dictionary.ContainsKey(entry.Value))
                dictionary.Add(entry.Value, entry.Key);
        }
        return dictionary;
    }

    /// <summary>
    /// Finds the key associated with the given value in a dictionary.
    /// </summary>
    /// <param name="lookup">The value to lookup</param>
    /// <returns>The key associating with the given lookup value</returns>
    public static TKey ReverseLookup<TKey, TValue>(this IDictionary<TKey, TValue> source, TValue lookup) where TValue : class
    {
        return source.FirstOrDefault(x => x.Value == lookup).Key;
    }
    #endregion

    #region Vector3 Extensions
    /// <summary>
    /// Returns a new Vector3 with the x value changed to the given value.
    /// </summary>
    /// <param name="x">The new value for x</param>
    /// <returns>The vector3 with the new x value</returns>
    public static Vector3 WithX(this Vector3 vector, float x)
    {
        vector.x = x;
        return vector;
    }
    /// <summary>
    /// Returns a new Vector3 with the y value changed to the given value.
    /// </summary>
    /// <param name="y">The new value for y</param>
    /// <returns>The vector3 with the new y value</returns>
    public static Vector3 WithY(this Vector3 vector, float y)
    {
        vector.y = y;
        return vector;
    }
    /// <summary>
    /// Returns a new Vector3 with the z value changed to the given value.
    /// </summary>
    /// <param name="z">The new value for z</param>
    /// <returns>The vector3 with the new z value</returns>
    public static Vector3 WithZ(this Vector3 vector, float z)
    {
        vector.z = z;
        return vector;
    }

    /// <summary>
    /// Converts a Vector3 to a Vector2 by removing the z value.
    /// </summary>
    /// <returns>A vector2 made with the x and y of the vector3</returns>
    public static Vector2 ToVector2(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }

    /// <summary>
    /// Clamps a Vector3 between the given min and max value bounds.
    /// </summary>
    /// <param name="min">The minimum value allowed</param>
    /// <param name="max">The maximum value allowed</param>
    /// <returns>A Vector3 guaranteed to be within the given bounds</returns>
    public static Vector3 Clamp(this Vector3 value, float min, float max)
    {
        value.x = Mathf.Clamp(value.x, min, max);
        value.y = Mathf.Clamp(value.y, min, max);
        value.z = Mathf.Clamp(value.z, min, max);

        return value;
    }

    /// <summary>
    /// Clamps a Vector3 between the given min and max vector bounds.
    /// </summary>
    /// <param name="min">The minimum vector allowed</param>
    /// <param name="max">The maximum vector allowed</param>
    /// <returns>A Vector3 guaranteed to be within the given bounds</returns>
    public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
    {
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        value.z = Mathf.Clamp(value.z, min.z, max.z);

        return value;
    }
    #endregion

    #region Vector2 Extensions
    /// <summary>
    /// Returns a new Vector2 with the x value changed to the given value.
    /// </summary>
    /// <param name="x">The new value for x</param>
    /// <returns>The vector2 with the new x value</returns>
    public static Vector2 WithX(this Vector2 vector, float x)
    {
        vector.x = x;
        return vector;
    }
    /// <summary>
    /// Returns a new Vector2 with the y value changed to the given value.
    /// </summary>
    /// <param name="y">The new value for y</param>
    /// <returns>The vector2 with the new y value</returns>
    public static Vector2 WithY(this Vector2 vector, float y)
    {
        vector.y = y;
        return vector;
    }

    /// <summary>
    /// Converts a Vector2 to a Vector3 by adding a z value of 0.
    /// </summary>
    /// <returns>A new Vector3 with the x and y of the Vector2 and a z of 0.</returns>
    public static Vector3 ToVector3(this Vector2 vector)
    {
        return new Vector3(vector.x, vector.y, 0);
    }

    /// <summary>
    /// Converts a Vector2 to a Vector3 by adding the given z value.
    /// </summary>
    /// <param name="z">The new value for z</param>
    /// <returns>A new Vector3 with the x and y of the Vector2 and the given value for z.</returns>
    public static Vector3 ToVector3(this Vector2 vector, int z)
    {
        return new Vector3(vector.x, vector.y, z);
    }

    /// <summary>
    /// Clamps a Vector2 between the given min and max value bounds.
    /// </summary>
    /// <param name="min">The minimum value allowed</param>
    /// <param name="max">The maximum value allowed</param>
    /// <returns>A Vector2 guaranteed to be within the given bounds</returns>
    public static Vector2 Clamp(this Vector2 value, float min, float max)
    {
        value.x = Mathf.Clamp(value.x, min, max);
        value.y = Mathf.Clamp(value.y, min, max);

        return value;
    }

    /// <summary>
    /// Clamps a Vector2 between the given min and max vector bounds.
    /// </summary>
    /// <param name="min">The minimum vector allowed</param>
    /// <param name="max">The maximum vector allowed</param>
    /// <returns>A Vector2 guaranteed to be within the given bounds</returns>
    public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max)
    {
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);

        return value;
    }
    #endregion

    #region Transform Extensions
    /// <summary>
    /// Sets position and rotation to zero and scale to one.
    /// </summary>
    public static void Reset(this Transform transform)
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    /// <summary>
    /// Sets local position and rotation to zero and scale to one.
    /// </summary>
    public static void LocalReset(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    #endregion
}