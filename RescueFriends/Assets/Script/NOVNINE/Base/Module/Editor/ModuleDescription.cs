/**
* @file ModuleDescription.cs
* @brief
* @author Choi YongWu(amugana@gmail.com)
* @version 1.0
* @date 2013-10-30
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;

namespace NOVNINE
{

public class ModuleDescription
{
    public class Mod
    {
        public string id;
        public string category;
        public string name;
        public string desc;
        public string version;
        public string download;
        public string manual;
        //public bool staticVersion;
        public string contextClass;
        public BuildTargetGroup[] platforms;
        public string[] files;
        public string[] excludes;
        public string[] dependencies;

        [JsonIgnore] public string PackageName
        {
            get {
                return string.Format("{0}-platform-r{1}.unitypackage", this.id, this.version);
            }
        }

        [JsonIgnore] public string Version
        {
            get {
                int latest = 0;
//                foreach(var path in files) {
//                    int ver = SvnUtil.LastChangedRevisionAt(path);
//                    if(latest < ver)
//                        latest = ver;
//                }
                return latest.ToString();
            }
        }

        public bool DependsOn(Mod other)
        {
            if(this.dependencies == null) return false;
            return (System.Array.Find(this.dependencies, x=> (x==other.id)) != null);
        }

        public ModuleDescription.Mod Clone()
        {
            var clone = new ModuleDescription.Mod();
            clone.id = this.id;
            clone.name = this.name;
            clone.category = this.category;
            clone.desc = this.desc;
            clone.download = this.download;
            clone.manual = this.manual;
            clone.version = this.version;
            //clone.staticVersion = this.staticVersion;
            clone.contextClass = this.contextClass;
            if(this.platforms != null)
                clone.platforms = (BuildTargetGroup[])this.platforms.Clone();
            if(this.files != null)
                clone.files = (string[])this.files.Clone();
            if(this.dependencies != null)
                clone.dependencies = (string[])this.dependencies.Clone();
            return clone;
        }

        public bool ExistOnDisk
        {
            get {
                bool missing = false;
                foreach(var f in this.files) {
                    string path = Application.dataPath+f.Substring(6);
                    if(!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path)) {
                        missing = true;
                        break;
                    }
                }
                return !missing;
            }
        }
    }

    public string version;
    public Mod[] module;
    [JsonIgnore] public Dictionary<string, List<Mod>> modules = new Dictionary<string, List<Mod>>();
    private Dictionary<string, Mod> moduleDict;

    private void BuildModuleDict()
    {
        moduleDict = new Dictionary<string, Mod>();
        foreach(var cat in modules) {
            foreach(var m in cat.Value) {
                moduleDict.Add(m.id, m);
            }
        }
    }

    public Mod GetModuleByID(string id)
    {
        if(moduleDict == null) {
            BuildModuleDict();
        }
        if(moduleDict.ContainsKey(id))
            return moduleDict[id];
        return null;
    }

    public void AddModuleAtCategory(string category, ModuleDescription.Mod desc)
    {
        if(GetModuleByID(desc.id) != null) {
            Debug.Log("AddModuleAtCategory module exist : "+desc.id);
            UpdateModule(desc);
            return;
        }

        //Debug.Log("AddModuleAtCategory : "+category+"  "+desc.id);
        if(!modules.ContainsKey(category))
            modules.Add(category, new List<ModuleDescription.Mod>());
        modules[category].Add(desc);
        BuildModuleDict();
    }

    public bool RemoveModule(string id)
    {
        foreach(var cat in modules) {
            for(int i=0; i<cat.Value.Count; ++i) {
                if(cat.Value[i].id == id) {
                    cat.Value.RemoveAt(i);
                    BuildModuleDict();
                    return true;
                }
            }
        }
        return false;
    }

    public bool UpdateModule(ModuleDescription.Mod mod)
    {
        foreach(var cat in modules) {
            for(int i=0; i<cat.Value.Count; ++i) {
                if(cat.Value[i].id == mod.id) {
                    cat.Value[i] = mod;
                    BuildModuleDict();
                    return true;
                }
            }
        }
        return false;
    }

    public List<ModuleDescription.Mod> GetAllModules()
    {
        List<ModuleDescription.Mod> list = new List<ModuleDescription.Mod>();
        foreach(var cat in modules) {
            list.AddRange(cat.Value);
        }
        return list;
    }

    public static ModuleDescription Load(string path)
    {
        ModuleDescription result = null;

        string content = null;
        if(path.StartsWith("http://"))
            content = ModuleManager.DownloadText(path);
        else if(path.StartsWith("file://"))
            content = System.IO.File.ReadAllText(path.Substring(7));
        else
            content = System.IO.File.ReadAllText(path);
        if(content == null) {
            Debug.LogError("Path Not Readable : "+path);
            return result;
        }

        try {
            result = JsonReader.Deserialize<ModuleDescription>(content);
        } catch (JsonDeserializationException exception) {
            int line, col;
            exception.GetLineAndColumn(content, out line, out col);
            Debug.LogError(exception.Message+"  on "+path+ "  Line:"+line+"  Col:"+col);
            return result;
        } catch (System.Exception exception) {
            Debug.LogError(exception.Message);
            return result;
        }

        foreach(var mod in result.module) {
            result.AddModuleAtCategory(mod.category, mod);
        }

        return result;
    }

    public static bool Save(string path, ModuleDescription desc)
    {
        if(path.StartsWith("http://")) return false;
        path = path.Replace("file://", "");

        List<ModuleDescription.Mod> mods = new List<ModuleDescription.Mod>();
        foreach(var cat in desc.modules) {
            mods.AddRange(cat.Value);
        }
        desc.module = mods.ToArray();

        System.IO.File.WriteAllText(path, JsonWriter.Serialize(desc));
        return true;
    }
}

}

