using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntObject
{
	public int Value;
	
	static public implicit operator int(IntObject obj)
	{
		return obj.Value;
	}
	
	static public implicit operator IntObject(int value)
	{
		IntObject o = new IntObject();
		o.Value = value;
		return o;
	}
	
	public override bool Equals(object o)
	{
		var cmp = o as IntObject;
		if (cmp == null)
			return false;
		return this.Value == cmp.Value;
	}
	
	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
	
	public override string ToString()
	{
		return Value.ToString();
	}
}

public class Util
{
    public delegate bool TraversalCallback(Transform tr);
    public static bool Traversal(Transform tr, TraversalCallback cb)
    {
        foreach (Transform child in tr)
        {
            if (!cb(child))
                return false;
            if (!Traversal(child, cb))
                return false;
        }
        return true;
    }

	public static T GetChildByName<T>(GameObject go, string name) where T:Component
	{
		var child = GetChildByName(go, name);
		if (child == null)
			return default(T);

		return child.gameObject.GetComponent<T>();
	}

	public static GameObject GetChildByName(GameObject go, string name) 
	{
		var child = GetChildByName(go.transform, name);
		return child == null ? null : child.gameObject;
	}

    public static Transform GetChildByName(Transform tr, string name)
    {
        foreach (Transform child in tr)
        {
            if (child.name == name)
                return child;
        }

		foreach (Transform child in tr)
		{
			Transform c = GetChildByName(child, name);
			if (c != null)
				return c;
		}

        return null;
    }

    public static void ChangeLayer(GameObject g, int l)
    {
        g.layer = l;
        foreach (Transform t in g.transform)
        {
            ChangeLayer(t.gameObject, l);
        }
    }
	
	public static string UrlToIP(string url)
	{
		try
		{
            var host = System.Net.Dns.GetHostEntry(url);
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return url;
		}
		catch(System.Exception e)
		{
			Debug.LogException(e);
			return url;
		}
	}

    public static string Md5File(string file)
    {
        using (var stream = System.IO.File.OpenRead(file))
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var data = md5.ComputeHash(stream);
            var sb = new System.Text.StringBuilder();

            foreach(var b in data)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString().ToLower();
        }
    }
}
