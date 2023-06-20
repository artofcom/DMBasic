using UnityEngine;
using System.Collections;

public class SpritePanel : Panel
{
    public Sprite[] sprLinks    = null;

    public Sprite getSpriteByName(string strName)
    {
        if(null==sprLinks)      return null;

        for(int j = 0; j < sprLinks.Length; ++j)
        {
            if(strName.Equals( sprLinks[j].name))
                return sprLinks[j];
        }
        return null;
    }
    
    
    public override void ChangePanel( string fileName) 
	{
        if(null==fileName || fileName=="")
            return;

		this.GetComponent<SpriteRenderer>().sprite = getSpriteByName(fileName);
	}
}
