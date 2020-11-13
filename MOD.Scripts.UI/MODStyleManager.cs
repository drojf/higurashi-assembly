﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MOD.Scripts.UI
{
	public class MODStyleManager
	{
		public class StyleGroup
		{
			public int menuHeight;
			public int menuWidth;
			public GUIStyle modMenuSelectionGrid;   // Used for SelectionGrid widgets
			public GUIStyle errorLabel;
			public GUIStyle button;
			public GUIStyle label;             //Used for normal Label widgets
		}

		// Styles used for Toasts
		public GUIStyle bigToastLabelStyle;
		public GUIStyle smallToastLabelStyle;

		// Styles used for the Mod menu
		public GUIStyle modMenuAreaStyle;		//Used for Area widgets

		public Texture2D modGUIBackgroundTexture;

		StyleGroup style480;
		StyleGroup style720;
		StyleGroup style1080;
		public StyleGroup Group
		{
			get {
				if (Screen.height >= 1080)
				{
					return style1080;
				}
				else if (Screen.height >= 720)
				{
					return style720;
				}

				return style480;
			}
		}

		public float baseFontSize = 14;

		/// <summary>
		/// Note that this static construct is automatically created
		/// </summary>
		public MODStyleManager()
		{
			int width = 1;
			int height = 1;
			Color[] pix = new Color[width * height];
			for (int i = 0; i < pix.Length; i++)
			{
				pix[i] = new Color(0.0f, 0.0f, 0.0f, 0.9f);
			}
			modGUIBackgroundTexture = new Texture2D(width, height);
			modGUIBackgroundTexture.SetPixels(pix);
			modGUIBackgroundTexture.Apply();

			style480 = GenerateWidgetStyles(
				menuWidth: 400,
				menuHeight: 480,
				guiScale: .75f,
				margin: new RectOffset(0, 0, 0, 0),
				padding: new RectOffset(0, 0, 0, 0)
			);

			style720 = GenerateWidgetStyles(
				menuWidth: 700,
				menuHeight: 600,
				guiScale: 1f,
				margin: new RectOffset(1, 1, 1, 1),
				padding: new RectOffset(1, 1, 1, 1)
			);

			style1080 = GenerateWidgetStyles(
				menuWidth: 800,
				menuHeight: 800,
				guiScale: 1.25f,
				margin: new RectOffset(2, 2, 2, 2),
				padding: new RectOffset(2, 2, 2, 2)
			);

			OnGUIGetLabelStyle();
			OnGUIGetModStyle();
		}

		// This sets up the style of the toast notification (mostly to make the font bigger and the text anchor location)
		// From what I've read, The GUIStyle must be initialized in OnGUI(), otherwise
		// GUI.skin.label will not be defined, so please do not move this part elsewhere without testing it.
		private void OnGUIGetLabelStyle()
		{
			bigToastLabelStyle = new GUIStyle(GUI.skin.box) //Copy the default style for 'box' as a base
			{
				alignment = TextAnchor.UpperCenter,
				fontSize = 40,
				fontStyle = FontStyle.Bold,
			};
			bigToastLabelStyle.normal.background = modGUIBackgroundTexture;
			bigToastLabelStyle.normal.textColor = Color.white;

			smallToastLabelStyle = new GUIStyle(bigToastLabelStyle)
			{
				fontSize = 20,
			};
		}

		private void OnGUIGetModStyle()
		{
			modMenuAreaStyle = new GUIStyle(GUI.skin.box) //Copy the default style for 'box' as a base
			{
				//alignment = TextAnchor.UpperCenter,
				//fontSize = 40,
				//fontStyle = FontStyle.Bold,
			};
			modMenuAreaStyle.normal.background = modGUIBackgroundTexture;
			modMenuAreaStyle.normal.textColor = Color.white;
			modMenuAreaStyle.wordWrap = true;
		}

		private StyleGroup GenerateWidgetStyles(int menuWidth, int menuHeight, float guiScale, RectOffset margin, RectOffset padding)
		{
			// Button style
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.fontSize = Mathf.RoundToInt(guiScale * baseFontSize);

			// Label style
			GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = Mathf.RoundToInt(guiScale * baseFontSize),
				margin = margin,
				padding = padding,
			};

			// Menu selection grid/radio
			GUIStyle modMenuSelectionGrid = new GUIStyle(GUI.skin.button) //Copy the default style for 'box' as a base
			{
				//alignment = TextAnchor.UpperCenter,
				fontSize = Mathf.RoundToInt(guiScale * baseFontSize),
				//fontStyle = FontStyle.Bold,
			};
			modMenuSelectionGrid.onHover.textColor = Color.green;
			modMenuSelectionGrid.onNormal.textColor = Color.green; // Color of a selected option

			// Error label (this is used for the description on the right hand side if the game encounters an error
			GUIStyle errorLabelStyle = new GUIStyle(GUI.skin.label);
			errorLabelStyle.normal.textColor = Color.red;
			errorLabelStyle.fontSize = Mathf.RoundToInt(guiScale * baseFontSize);

			return new StyleGroup()
			{
				menuWidth = menuWidth,
				menuHeight = menuHeight,
				modMenuSelectionGrid = modMenuSelectionGrid,
				errorLabel = errorLabelStyle,
				button = buttonStyle,
				label = labelStyle,
			};
		}
	}
}
