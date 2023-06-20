using UnityEngine;
using System.Collections;

[System.Serializable]
public class LocaleString
{
	public string EN,SE,DK,NO,DE;
	public static implicit operator string (LocaleString val)
    {
        return (string) val.GetType().GetField( Locale.name ).GetValue( val );
    }

    public override string ToString()
    {
        return (string) this.GetType().GetField( Locale.name ).GetValue( this );
    }

    public void Set(string locale, string value)
    {
        this.GetType().GetField( locale ).SetValue( this, value );
    }

    public void SetAll(string value)
    {
		EN = SE = DK = NO = DE = value;
    }
}

