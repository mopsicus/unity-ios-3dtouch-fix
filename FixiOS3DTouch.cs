using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.IO;
using UnityEditor.iOS.Xcode;
using System.Collections.Generic;

public class iOSPostBuildProcess {

	[PostProcessBuild]
	public static void iOSPostProcess(BuildTarget buildTarget, string pathToBuiltProject) {
		if (buildTarget == BuildTarget.iOS) {
			Fix3DTouchDelay (pathToBuiltProject);
		}
	}

	private static void Fix3DTouchDelay (string pathToBuiltProject) {
		string targetString = "[self onUpdateSurfaceSize:frame.size];";
		string targetString2 = "- (void)touchesBegan";
		string targetString3 = "@interface UnityView : UnityRenderingView";
		string filePath = Path.Combine(pathToBuiltProject, "Classes");
		filePath = Path.Combine(filePath, "UI");
		filePath = Path.Combine(filePath, "UnityView.mm");
		if (File.Exists(filePath)) {
			string classFile = File.ReadAllText(filePath);
			string newClassFile = classFile.Replace (targetString, "[self onUpdateSurfaceSize:frame.size];\n\rUILongPressGestureRecognizer *longPR = [[UILongPressGestureRecognizer alloc] initWithTarget:self action:NULL];\nlongPR.delaysTouchesBegan = false;\nlongPR.minimumPressDuration = 0;\nlongPR.delegate = self;\n[self addGestureRecognizer:longPR];");
			newClassFile = newClassFile.Replace (targetString2, "- (BOOL)gestureRecognizer:(UIGestureRecognizer *)gestureRecognizer shouldReceiveTouch:(UITouch *)touch {\nCGPoint point= [touch locationInView:touch.view];\nif (point.x < 35) {\nNSSet *set = [NSSet setWithObjects:touch, nil];\nUnitySendTouchesBegin(set, NULL);\n}\n}\n\r- (void)touchesBegan");
			if (classFile.Length != newClassFile.Length) {
				File.WriteAllText(filePath, newClassFile);    
				Debug.Log("Fix3DTouchDelay succeeded for file: " + filePath);
			} else {
				Debug.LogWarning("Fix3DTouchDelay FAILED -- Target string not found: \"" + targetString + "\"");
			}
		} else {
			Debug.LogWarning("Fix3DTouchDelay FAILED -- File not found: " + filePath);
		}
		filePath = Path.Combine(pathToBuiltProject, "Classes");
		filePath = Path.Combine(filePath, "UI");
		filePath = Path.Combine(filePath, "UnityView.h");
		if (File.Exists(filePath)) {
			string classFile = File.ReadAllText(filePath);
			string newClassFile = classFile.Replace (targetString3, "@interface UnityView : UnityRenderingView <UIGestureRecognizerDelegate>");
			if (classFile.Length != newClassFile.Length) {
				File.WriteAllText(filePath, newClassFile);    
				Debug.Log("Fix3DTouchDelay succeeded for file: " + filePath);
			} else {
				Debug.LogWarning("Fix3DTouchDelay FAILED -- Target string not found: \"" + targetString3 + "\"");
			}
		} else {
			Debug.LogWarning("Fix3DTouchDelay FAILED -- File not found: " + filePath);
		}		
	}
    
}    