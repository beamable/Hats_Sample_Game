using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hats.Utilities
{
	public class TakeScreenshotBehaviour : MonoBehaviour
	{
		[SerializeField]
		private int _supersampleFactor = 2;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
			{
				string screenshotFilename = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
				Debug.Log($"Saving screenshot to file={screenshotFilename}");
				ScreenCapture.CaptureScreenshot(screenshotFilename, _supersampleFactor);
			}
		}
	}
}