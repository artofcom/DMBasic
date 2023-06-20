using UnityEngine;
using System.Reflection;

namespace NOVNINE
{
public static class ObjectExt
{
    #region Reflection
    public static void SetPropValue<T>( this object src, string propName, T val )
    {
        src.GetType( ).GetProperty( propName ).SetValue( src, val, null );
    }

    public static void SetFieldValue<T>( this object src, string propName, T val )
    {
        src.GetType( ).GetField( propName, BindingFlags.Instance|BindingFlags.Public | BindingFlags.NonPublic ).SetValue( src, val );
    }

    public static bool HasProperty( this object src, string propName )
    {
        return src.GetType( ).GetProperty( propName ) != null;
    }

    public static T GetPropValue<T>( this object src, string propName )
    {
        return (T)src.GetType( ).GetProperty( propName ).GetValue( src, null );
    }
    #endregion
}
}

