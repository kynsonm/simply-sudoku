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
            
            // ----- SOLUTION 1
            string target = proj.TargetGuidByName("Unity-iPhone");
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "false");


            // ----- SOLUTION 2
            // Main
            target = proj.GetUnityMainTargetGuid();
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            // Unity Tests
            target = proj.TargetGuidByName(PBXProject.GetUnityTestTargetName());
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            // Unity Framework
            target = proj.GetUnityFrameworkTargetGuid();
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");


            // ----- SOLUTION 3
            var mainTargetGuid = proj.GetUnityMainTargetGuid();
            foreach (var targetGuid in new[] {mainTargetGuid, proj.GetUnityFrameworkTargetGuid()})
            {
                proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            }


            // ----- SOLUTION 4
            target = proj.TargetGuidByName("Pods-Unity-iPhone");
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "false");


            // ----- SOLUTION 5
            target = proj.TargetGuidByName("GoogleAppMeasurement");
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "false");
            target = proj.TargetGuidByName("XCFrameworkIntermediates");
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "false");


            // ----- SOLUTION 6
            target = proj.GetUnityMainTargetGuid();
            proj.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            target = proj.GetUnityFrameworkTargetGuid();
            proj.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            target = proj.TargetGuidByName("Pods-Unity-iPhone");
            proj.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");


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