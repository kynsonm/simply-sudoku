using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;

public class BL_BuildPostProcess 
{

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path) 
    {
         
        if (buildTarget == BuildTarget.iOS) 
        {
            string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            
            string target = proj.TargetGuidByName("Unity-iPhone");
            
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "false");
            
            File.WriteAllText(projPath, proj.WriteToString());
            
            
            // Add url schema to plist file
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            
            // Get root
            PlistElementDict rootDict = plist.root;
            rootDict.SetBoolean("UIRequiresFullScreen",true);
            plist.WriteToFile(plistPath);
        }
    }
}