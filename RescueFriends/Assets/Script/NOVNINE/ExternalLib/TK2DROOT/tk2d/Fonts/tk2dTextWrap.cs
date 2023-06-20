using UnityEngine;

static class tk2dTextMeshExt
{
    public static tk2dTextMesh From<T>(this tk2dTextMesh mesh, T val)
    {
        mesh.text = val.ToString();
        mesh.Commit();
        return mesh;
    }
}

public class tk2dTextWrap<ValType>
{
    public tk2dTextMesh mesh;
    public ValType Value
    {
        get {
            return myValue ;
        } set {
            if(!value.Equals(myValue)) {
                myValue = value;
                if(customConv != null) {
                    string newTx = customConv(myValue);
                    if(mesh.text != newTx) {
                        mesh.text = newTx;
                        mesh.Commit();
                    }
                } else {
                    mesh.From<ValType>(myValue);
                }
            }
        }
    }

    public delegate string customConverter(ValType val);

    private ValType myValue;
    private customConverter customConv = null;

    public tk2dTextWrap(tk2dTextMesh _mesh)
    {
        mesh = _mesh;
    }

    public tk2dTextWrap(tk2dTextMesh _mesh, ValType initVal, string formatStr = "")
    {
        mesh = _mesh;
        setValue(initVal);
    }

    private void setValue(ValType val)
    {
        myValue = val;
        mesh.From<ValType>(myValue);
    }

    public void SetCustomConverter(customConverter conv)
    {
        customConv = conv;
    }

    /*
    public void Add(ValType rhs)
    {
        this.Value = myValue+rhs;
    }

    public void Sub(ValType rhs)
    {
        this.Value = myValue-rhs;
    }
    */
}

