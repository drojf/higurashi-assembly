﻿using Assets.Scripts.Core;
using Assets.Scripts.Core.Buriko;
using Assets.Scripts.Core.State;
using MOD.Scripts.Core.State;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MOD.Scripts.UI
{
	class MODRadio
	{
		string label;
		private GUIContent[] radioContents;
		MODStyleManager styleManager;
		int itemsPerRow;

		public MODRadio(string label, GUIContent[] radioContents, MODStyleManager styleManager, int itemsPerRow=0) //Action<int> onRadioChange, 
		{
			this.label = label;
			this.radioContents = radioContents;
			this.itemsPerRow = itemsPerRow == 0 ? radioContents.Length : itemsPerRow;
			this.styleManager = styleManager;
		}

		/// <summary>
		/// NOTE: only call this function within OnGUI()
		/// Displays the radio, calling onRadioChange when the user clicks on a different radio value.
		/// </summary>
		/// <param name="displayedRadio">Sets the currently displayed radio. Use "-1" for "None selected"</param>
		/// <returns>If radio did not change value, null is returned, otherwise the new value is returned.</returns>
		public int? OnGUIFragment(int displayedRadio)
		{
			GUILayout.Label(this.label);
			int i = GUILayout.SelectionGrid(displayedRadio, radioContents, itemsPerRow, styleManager.modSelectorStyle);
			if (i != displayedRadio)
			{
				return i;
			}

			return null;
		}
	}

	public class MODMenu
	{
		private readonly MODStyleManager styleManager;
		private readonly MODRadio radioCensorshipLevel;
		private readonly MODRadio radioLipSync;
		private readonly MODRadio radioOpenings;
		private readonly MODRadio radioHideCG;
		private readonly MODRadio radioStretchBackgrounds;
		private readonly GameSystem gameSystem;
		public bool visible;

		public MODMenu(GameSystem gameSystem, MODStyleManager styleManager)
		{
			this.gameSystem = gameSystem;
			this.styleManager = styleManager;
			this.visible = false;

			string baseCensorshipDescription = @"

Sets the script censorship level
- This setting exists because the voices are taken from the censored, Console versions of the game, so no voices exist for the PC uncensored dialogue.
- We recommend the default level (2), the most balanced option. Using this option, only copyright changes, innuendos, and a few words will be changed.
  - 5: Full PS3 script fully voiced (most censored)
  - 2: Default - most balanced option
  - 0: Original PC Script with voices where it fits (least uncensored), but uncensored scenes may be missing voices";


			this.radioCensorshipLevel = new MODRadio("Voice Matching Level", new GUIContent[] {
				new GUIContent("0", "Censorship level 0 - Equivalent to PC" + baseCensorshipDescription),
				new GUIContent("1", "Censorship level 1" + baseCensorshipDescription),
				new GUIContent("2*", "Censorship level 2 (this is the default/recommneded value)" + baseCensorshipDescription),
				new GUIContent("3", "Censorship level 3" + baseCensorshipDescription),
				new GUIContent("4", "Censorship level 4" + baseCensorshipDescription),
				new GUIContent("5", "Censorship level 5 - Equivalent to Console" + baseCensorshipDescription),
				}, styleManager);

			this.radioLipSync = new MODRadio("Lip Sync for Console Sprites", new GUIContent[]
			{
				new GUIContent("Lip Sync Off", "Disables Lip Sync for Console Sprites"),
				new GUIContent("Lip Sync On", "Enables Lip Sync for Console Sprites"),
			}, styleManager);

			this.radioOpenings = new MODRadio("OP Movies", new GUIContent[]
			{
				new GUIContent("Disabled", "Disables all opening videos"),
				new GUIContent("In-Game Only", "Enables opening videos which play during the story"),
				new GUIContent("At Launch + In-Game", "Opening videos will play just after game launches, and also during the story"),
			}, styleManager, itemsPerRow: 2);

			this.radioHideCG = new MODRadio("CG Show/Hide", new GUIContent[]
			{
				new GUIContent("Show CGs", "Shows CGs (You probably want this enabled for Console ADV/NVL mode)"),
				new GUIContent("Hide CGs", "Disables all CGs (mainly for use with the Original/Ryukishi preset)"),
			}, styleManager);

			this.radioStretchBackgrounds = new MODRadio("Stretch Backgrounds", new GUIContent[]
			{
				new GUIContent("Normal Backgrounds", "Displays backgrounds at their original aspect ratio"),
				new GUIContent("Stretch Backgrounds", "Stretches backgrounds to the game's 16:9 aspect ratio (mainly for use with the Original/Ryukishi backgrounds)"),
			}, styleManager);
		}

		private void OnGUIPresetsFragment()
		{
			GUILayout.Label("Load ADV/NVL/Original Presets");

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("ADV", "This preset:\n" +
				"- Makes text show at the bottom of the screen in a textbox\n" +
				"- Shows the name of the current character on the textbox\n" +
				"- Uses the console sprites and backgrounds\n" +
				"- Displays in 16:9 widescreen\n\n")))
			{
				MODActions.SetAndSaveADV(MODActions.ModPreset.ADV);
			}
			else if (GUILayout.Button(new GUIContent("NVL", "This preset:\n" +
				"- Makes text show across the whole screen\n" +
				"- Uses the console sprites and backgrounds\n" +
				"- Displays in 16:9 widescreen\n\n")))
			{
				MODActions.SetAndSaveADV(MODActions.ModPreset.NVL);
			}
			else if (GUILayout.Button(new GUIContent("Original/Ryukishi", "This preset makes the game behave similarly to the unmodded game:\n" +
				"- Displays in 4:3 'standard' aspect\n" +
				"- CGs are disabled\n" +
				"- Switches to original sprites and backgrounds\n\n")))
			{
				MODActions.SetAndSaveADV(MODActions.ModPreset.OG);
			}
			GUILayout.EndHorizontal();
		}

		/// <summary>
		/// Must be called from an OnGUI()
		/// </summary>
		public void OnGUIFragment()
		{
			if (this.visible)
			{
				float areaWidth = 350;
				float totalAreaWidth = areaWidth * 2;
				float areaHeight = 500;

				float areaPosX = Screen.width / 2 - totalAreaWidth / 2;
				float areaPosY = Screen.height / 2 - areaHeight / 2;

				float area2PosX = areaPosX + areaWidth;

				float exitButtonWidth = areaWidth * .1f;
				float exitButtonHeight = areaHeight * .05f;

				int GetGlobal(string flagName) => BurikoMemory.Instance.GetGlobalFlag(flagName).IntValue();
				void SetGlobal(string flagName, int flagValue) => BurikoMemory.Instance.SetGlobalFlag(flagName, flagValue);

				// Radio buttons
				GUILayout.BeginArea(new Rect(areaPosX, areaPosY, areaWidth, areaHeight), styleManager.modGUIStyle);
					this.OnGUIPresetsFragment();

					if(this.radioCensorshipLevel.OnGUIFragment(GetGlobal("GCensor")) is int censorLevel)
					{
						SetGlobal("GCensor", censorLevel);
					};

					if(this.radioLipSync.OnGUIFragment(GetGlobal("GLipSync")) is int lipSyncEnabled)
					{
						SetGlobal("GLipSync", lipSyncEnabled);
					};

					if(this.radioOpenings.OnGUIFragment(GetGlobal("GVideoOpening") - 1) is int openingVideoLevelZeroIndexed)
					{
						SetGlobal("GVideoOpening", openingVideoLevelZeroIndexed + 1);
					};

					if(this.radioHideCG.OnGUIFragment(GetGlobal("GHideCG")) is int hideCG)
					{
						SetGlobal("GHideCG", hideCG);
					};

					if(this.radioStretchBackgrounds.OnGUIFragment(GetGlobal("GStretchBackgrounds")) is int stretchBackgrounds)
					{
						SetGlobal("GStretchBackgrounds", stretchBackgrounds);
					};
				GUILayout.EndArea();

				// Descriptions for each button are shown on hover, like a tooltip
				GUILayout.BeginArea(new Rect(area2PosX, areaPosY, areaWidth, areaHeight), styleManager.modGUIStyle);
					GUILayout.Space(exitButtonHeight);

					// MUST pass in MinHeight option, otherwise Unity will get confused and assume
					// label is one line high on first draw, and subsquent changes will truncate
					// label to one line even if it is multiple lines tall.
					GUILayout.Label(GUI.tooltip == String.Empty ? "Hover over a button to see its description." : GUI.tooltip, GUILayout.MinHeight(areaHeight));
				GUILayout.EndArea();

				// Exit button
				GUILayout.BeginArea(new Rect(area2PosX + areaWidth - exitButtonWidth, areaPosY, exitButtonWidth, exitButtonHeight));
					if(GUILayout.Button("X"))
					{
						this.Hide();
					}
				GUILayout.EndArea();


			}
		}

		public void Show()
		{
			this.visible = true;
			DisableGameInput();
		}

		public void Hide()
		{
			this.visible = false;
			EnableGameInput();
		}

		// These functions disable input to the game, while still letting
		// the mod menu receive inputs.
		/// <summary>
		/// Calling this while game input is already disabled should be fine
		/// as we only do this if there isn't already a DisableInput state on the stack
		/// </summary>
		private void DisableGameInput()
		{
			if (gameSystem.GameState != GameState.MODDisableInput)
			{
				ModChangeState(new MODStateDisableInput());
				gameSystem.HideUIControls();
			}
		}

		/// <summary>
		/// Calling this while game input is already enabled should be fine
		/// as we only do this if there is a DisableInput state available to pop
		/// </summary>
		private void EnableGameInput()
		{
			if (gameSystem.GameState == GameState.MODDisableInput)
			{
				gameSystem.PopStateStack();
				gameSystem.ShowUIControls();
			}
		}

		// This is a modified version of GameSystem.OnApplicationQuit()
		private void ModChangeState(IGameState newState)
		{
			if (gameSystem.GameState == GameState.ConfigScreen)
			{
				gameSystem.RegisterAction(delegate
				{
					gameSystem.LeaveConfigScreen(delegate
					{
						gameSystem.PushStateObject(newState);
					});
				});
				gameSystem.ExecuteActions();
			}
			else
			{
				gameSystem.RegisterAction(delegate
				{
					gameSystem.PushStateObject(newState);
				});
				gameSystem.ExecuteActions();
			}
		}
	}
}
