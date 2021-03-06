// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;

using Wave.XR.Loader;
using UnityEngine;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
using System.Xml;

namespace Wave.XR.BuildCheck
{
	static class CustomBuildProcessor
	{
		const string CustomAndroidManifestPathSrc = "Assets/Wave/XR/Platform/Android/AndroidManifest.xml";
		const string AndroidManifestPathSrc = "Packages/" + Constants.SDKPackageName + "/Runtime/Android/AndroidManifest.xml";
		const string AndroidManifestPathDest = "Assets/Plugins/Android/AndroidManifest.xml";
		const string ForceBuildWVR = "ForceBuildWVR.txt";

		static bool isAndroidManifestPathDestExisted = false;

		internal static void AddHandtrackingAndroidManifest()
		{
			if (File.Exists(AndroidManifestPathDest)) 
				if (!checkHandtrackingFeature(AndroidManifestPathDest))
					appendFile(AndroidManifestPathDest);
			if (File.Exists(CustomAndroidManifestPathSrc))
				if (!checkHandtrackingFeature(CustomAndroidManifestPathSrc))
					appendFile(CustomAndroidManifestPathSrc);
		}

		static void CopyAndroidManifest()
		{
			const string PluginAndroidPath = "Assets/Plugins/Android";
			if (!Directory.Exists(PluginAndroidPath))
				Directory.CreateDirectory(PluginAndroidPath);
			isAndroidManifestPathDestExisted = File.Exists(AndroidManifestPathDest);
			if (isAndroidManifestPathDestExisted)
			{
				Debug.Log("Using the Android Manifest at Assets/Plugins/Android");
				return; // not to overwrite existed AndroidManifest.xml
			}
			if (File.Exists(CustomAndroidManifestPathSrc))
			{
				Debug.Log("Using the Android Manifest at Assets/Wave/XR/Platform/Android");
				File.Copy(CustomAndroidManifestPathSrc, AndroidManifestPathDest, false);
			}
			else if (File.Exists(AndroidManifestPathSrc))
			{
				Debug.Log("Using the Android Manifest at Packages/com.htc.upm.wave.xrsdk/Runtime/Android");
				File.Copy(AndroidManifestPathSrc, AndroidManifestPathDest, false);
			}
			if (EditorPrefs.GetBool(CheckIfHandTrackingEnabled.MENU_NAME, false) && !checkHandtrackingFeature(AndroidManifestPathDest))
				appendFile(AndroidManifestPathDest);
		}

		static void appendFile(string filename)
		{
			string line;

			// Read the file and display it line by line.  
			StreamReader file1 = new StreamReader(filename);
			StreamWriter file2 = new StreamWriter(filename + ".tmp");
			while ((line = file1.ReadLine()) != null)
			{
				System.Console.WriteLine(line);
				if (line.Contains("</manifest>"))
				{
					file2.WriteLine("	<uses-feature android:name=\"wave.feature.handtracking\" android:required=\"true\" />");
				}
				file2.WriteLine(line);
			}

			file1.Close();
			file2.Close();
			File.Delete(filename);
			File.Move(filename + ".tmp", filename);
		}

		static bool checkHandtrackingFeature(string filename)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(filename);
			XmlNodeList metadataNodeList = doc.SelectNodes("/manifest/uses-feature");

			if (metadataNodeList != null)
			{
				foreach (XmlNode metadataNode in metadataNodeList)
				{
					string name = metadataNode.Attributes["android:name"].Value;
					string required = metadataNode.Attributes["android:required"].Value;

					if (name.Equals("wave.feature.handtracking"))
						return true;
				}
			}
			return false;
		}

		static void DelAndroidManifest()
		{
			if (File.Exists(AndroidManifestPathDest))
				File.Delete(AndroidManifestPathDest);

			string AndroidManifestMetaPathDest = AndroidManifestPathDest + ".meta";
			if (File.Exists(AndroidManifestMetaPathDest))
				File.Delete(AndroidManifestMetaPathDest);
		}

		static bool SetBuildingWave()
		{
			var androidGenericSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
			var androidXRSettings = androidGenericSettings.AssignedSettings;
			
			if (androidXRSettings == null)
			{
				androidXRSettings = ScriptableObject.CreateInstance<XRManagerSettings>() as XRManagerSettings;
			}
			var didAssign = XRPackageMetadataStore.AssignLoader(androidXRSettings, "Wave.XR.Loader.WaveXRLoader", BuildTargetGroup.Android);
			if (!didAssign)
			{
				Debug.LogError("Fail to add android WaveXRLoader.");
			}
			return didAssign;
		}

	static bool CheckIsBuildingWave()
        {
            var androidGenericSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (androidGenericSettings == null)
                return false;

            var androidXRMSettings = androidGenericSettings.AssignedSettings;
            if (androidXRMSettings == null)
                return false;

            var loaders = androidXRMSettings.loaders;
            foreach (var loader in loaders)
            {
                if (loader.GetType() == typeof(WaveXRLoader))
                {
                    return true;
                }
            }
            return false;
        }

		private class CustomPreprocessor : IPreprocessBuildWithReport
        {
            public int callbackOrder { get { return 0; } }

            public void OnPreprocessBuild(BuildReport report)
            {
				if (File.Exists(ForceBuildWVR))
				{
					//SetBuildingWave();
					AddHandtrackingAndroidManifest();
					CopyAndroidManifest();
				}
				else if (report.summary.platform == BuildTarget.Android && CheckIsBuildingWave())
                {
                    CopyAndroidManifest();
                }
            }
        }

        private class CustomPostprocessor : IPostprocessBuildWithReport
        {
            public int callbackOrder { get { return 0; } }

            public void OnPostprocessBuild(BuildReport report)
            {
				if (File.Exists(ForceBuildWVR))
				{
					if (!isAndroidManifestPathDestExisted) // not to delete existed AndroidManifest.xml
						DelAndroidManifest();
					File.Delete(ForceBuildWVR);
				}
				else if (report.summary.platform == BuildTarget.Android && CheckIsBuildingWave())
                {
					if (!isAndroidManifestPathDestExisted) // not to delete existed AndroidManifest.xml
						DelAndroidManifest();
                }
            }
        }
    }

	[InitializeOnLoad]
	public static class CheckIfHandTrackingEnabled
	{
		internal const string MENU_NAME = "Wave/HandTracking/EnableHandTracking";

		private static bool enabled_;
		static CheckIfHandTrackingEnabled()
		{
			CheckIfHandTrackingEnabled.enabled_ = EditorPrefs.GetBool(CheckIfHandTrackingEnabled.MENU_NAME, false);

			/// Delaying until first editor tick so that the menu
			/// will be populated before setting check state, and
			/// re-apply correct action
			EditorApplication.delayCall += () =>
			{
				PerformAction(CheckIfHandTrackingEnabled.enabled_);
			};
		}

		[MenuItem(CheckIfHandTrackingEnabled.MENU_NAME, priority = 601)]
		private static void ToggleAction()
		{
			/// Toggling action
			PerformAction(!CheckIfHandTrackingEnabled.enabled_);
		}

		public static void PerformAction(bool enabled)
		{
			/// Set checkmark on menu item
			Menu.SetChecked(CheckIfHandTrackingEnabled.MENU_NAME, enabled);
			if (enabled)
				CustomBuildProcessor.AddHandtrackingAndroidManifest();
			/// Saving editor state
			EditorPrefs.SetBool(CheckIfHandTrackingEnabled.MENU_NAME, enabled);

			CheckIfHandTrackingEnabled.enabled_ = enabled;
		}

		[MenuItem(CheckIfHandTrackingEnabled.MENU_NAME, validate = true, priority = 601)]
		public static bool ValidateEnabled()
		{
			Menu.SetChecked(CheckIfHandTrackingEnabled.MENU_NAME, enabled_);
			return true;
		}
	}
}
#endif
