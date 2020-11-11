﻿using Assets.Scripts.Core;
using Assets.Scripts.Core.AssetManagement;
using Assets.Scripts.Core.Buriko;
using Assets.Scripts.Core.State;
using MOD.Scripts.Core.State;
using System;
using System.Collections.Generic;
using System.Linq;
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
			this.itemsPerRow = radioContents.Length == 0 ? 1 : radioContents.Length;
			if(itemsPerRow != 0)
			{
				this.itemsPerRow = itemsPerRow;
			}
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

		public void SetContents(GUIContent[] content)
		{
			this.radioContents = content;
		}
	}

	public class MODMenu
	{
		private readonly MODStyleManager styleManager;
		private readonly MODRadio radioADVNVLOriginal;
		private readonly MODRadio radioCensorshipLevel;
		private readonly MODRadio radioLipSync;
		private readonly MODRadio radioOpenings;
		private readonly MODRadio radioHideCG;
		private readonly MODRadio radioBackgrounds;
		private readonly MODRadio radioArtSet;
		private readonly GameSystem gameSystem;
		public bool visible;
		private MODSimpleTimer defaultToolTipTimer;
		private MODSimpleTimer startupWatchdogTimer;
		private bool startupFailed;
		private string screenWidthString;
		private string screenHeightString;

		string lastToolTip = String.Empty;
		string defaultTooltip = @"Hover over a button on the left panel for its description.

[Vanilla Hotkeys]
Enter,Return,RightArrow,PageDown : Advance Text
LeftArrow,Pageup : See Backlog
ESC : Open Menu
Ctrl : Hold Skip Mode
A : Auto Mode
S : Toggle Skip Mode
F, Alt-Enter : FullScreen
Space : Hide Text
L : Swap Language
P : Swap Sprites

[MOD Hotkeys]
F1 : ADV-NVL MODE
F2 : Voice Matching Level
F3 : Effect Level (Not Used)
F5 : QuickSave
F7 : QuickLoad
F10 : Setting Monitor
M : Increase Voice Volume
N : Decrease Voice Volume
7 : Lip-Sync
LShift + F9 : Restore Settings
LShift + M : Voice Volume MAX
LShift + N : Voice Volume MIN";

		string startupFailureToolTip = @"It looks like there was a problem starting up

Please send the developers your log file (output_log.txt or Player.log).

If the log indicates you have corrupt save files, you may wish to delete the offending save file (or all of them).

Use the buttons under 'Save and Log files' on the bottom left to show your save and log files.
If they do not not work, click the button below to open the support page";

		GUIContent[] defaultArtsetDescriptions = new GUIContent[] {
			new GUIContent("Console", "Use the Console sprites and backgrounds"),
			new GUIContent("Remake", "Use Mangagmer's remake sprites with Console backgrounds"),
			new GUIContent("Original/Remake", "Use Original/Ryukishi sprites and backgrounds\n" +
			"Warning: Most users should just enable Original/Ryukishi mode at the top of this menu!"),
		};

		public MODMenu(GameSystem gameSystem, MODStyleManager styleManager)
		{
			this.gameSystem = gameSystem;
			this.styleManager = styleManager;
			this.visible = false;
			this.defaultToolTipTimer = new MODSimpleTimer();
			this.startupWatchdogTimer = new MODSimpleTimer();
			this.startupFailed = false;
			this.screenWidthString = String.Empty;
			this.screenHeightString = String.Empty;

			this.radioADVNVLOriginal = new MODRadio("Set ADV/NVL/Original Mode", new GUIContent[]
			{
				new GUIContent("ADV", "This preset:\n" +
				"- Makes text show at the bottom of the screen in a textbox\n" +
				"- Shows the name of the current character on the textbox\n" +
				"- Uses the console sprites and backgrounds\n" +
				"- Displays in 16:9 widescreen\n\n"),
				new GUIContent("NVL", "This preset:\n" +
				"- Makes text show across the whole screen\n" +
				"- Uses the console sprites and backgrounds\n" +
				"- Displays in 16:9 widescreen\n\n"),
				new GUIContent("Original/Ryukishi", "This preset makes the game behave similarly to the unmodded game:\n" +
				"- Displays in 4:3 'standard' aspect\n" +
				"- CGs are disabled\n" +
				"- Switches to original sprites and backgrounds\n\n")
			}, styleManager);

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

			this.radioOpenings = new MODRadio("Opening Movies", new GUIContent[]
			{
				new GUIContent("Disabled", "Disables all opening videos"),
				new GUIContent("Enabled", "Enables opening videos\n\n" +
				"NOTE: Once the opening video plays the first time, will automatically switch to 'Launch + In-Game'\n\n" +
				"We have setup openings this way to avoid spoilers."),
				new GUIContent("Launch + In-Game", "WARNING: There is usually no need to set this manually.\n\n" +
				"If openings are enabled, the first time you reach an opening while playing the game, this flag will be set automatically\n\n" +
				"That is, after the opening is played the first time, from then on openings will play every time the game launches"),
			}, styleManager);

			this.radioHideCG = new MODRadio("Show/Hide CGs", new GUIContent[]
			{
				new GUIContent("Show CGs", "Shows CGs (You probably want this enabled for Console ADV/NVL mode)"),
				new GUIContent("Hide CGs", "Disables all CGs (mainly for use with the Original/Ryukishi preset)"),
			}, styleManager);

			this.radioArtSet = new MODRadio("Choose Art Set", defaultArtsetDescriptions, styleManager, itemsPerRow: 3);

			this.radioBackgrounds = new MODRadio("Override Art Set Backgrounds", new GUIContent[]{
				new GUIContent("Default BGs", "Use the default backgrounds for the current artset"),
				new GUIContent("Console BGs", "Force Console backgrounds, regardless of the artset"),
				new GUIContent("Original BGs", "Force Original/Ryukishi backgrounds, regardless of the artset"),
				new GUIContent("Original Stretched", "Force Original/Ryukishi backgrounds, stretched to fit, regardless of the artset"),
			}, styleManager, itemsPerRow: 2);

			// Start the watchdog timer as soon as possible, so it starts from "when the game started"
			this.startupWatchdogTimer.Start(5.0f);
		}

		public void Update()
		{
			defaultToolTipTimer.Update();
			startupWatchdogTimer.Update();
		}

		public void LateUpdate()
		{
			if (Input.GetMouseButtonDown(1))
			{
				this.Hide();
			}
		}

		private int GetModeFromFlags()
		{
			if (BurikoMemory.Instance.GetGlobalFlag("GRyukishiMode").IntValue() == 1)
			{
				return 2;
			}
			else if (BurikoMemory.Instance.GetGlobalFlag("GADVMode").IntValue() == 1)
			{
				return 0;
			}
			else
			{
				return 1;
			}
		}

		private void OnGUIRestoreSettings()
		{
			GUILayout.Label($"Restore Settings {(GetGlobal("GMOD_SETTING_LOADER") == 3 ? "" : ": <Restart Pending>")}");

			GUILayout.BeginHorizontal();
			if (GetGlobal("GMOD_SETTING_LOADER") == 3)
			{
				if (GUILayout.Button(new GUIContent("ADV defaults", "This restores flags to the defaults for NVL mode\n" +
					"NOTE: Requires you to relaunch the game 2 times to come into effect")))
				{
					SetGlobal("GMOD_SETTING_LOADER", 0);
				}
				else if (GUILayout.Button(new GUIContent("NVL defaults", "This restores flags to the defaults for NVL mode\n" +
					"NOTE: Requires you to relaunch the game 2 times to come into effect")))
				{
					SetGlobal("GMOD_SETTING_LOADER", 1);
				}
				else if (GUILayout.Button(new GUIContent("Vanilla Defaults", "This restores flags to the same settings as the unmodded game.\n" +
					"NOTE: Requires a relaunch to come into effect. After this, uninstall the mod.")))
				{
					SetGlobal("GMOD_SETTING_LOADER", 2);
				}
			}
			else
			{
				if (GUILayout.Button(new GUIContent("Cancel Pending Restore", "Click this to stop restoring defaults on next game launch")))
				{
					SetGlobal("GMOD_SETTING_LOADER", 3);
				}
			}
			GUILayout.EndHorizontal();
		}

		/// <summary>
		/// Must be called from an OnGUI()
		/// </summary>
		public void OnGUIFragment()
		{
			if(this.startupWatchdogTimer.Finished())
			{
				this.startupWatchdogTimer.Cancel();
				if (!BurikoScriptSystem.Instance.FlowWasReached)
				{
					this.startupFailed = true;
					this.Show();
				}
			}

			if (this.visible)
			{
				float areaWidth = 400;
				float toolTipWidth = 350;
				float totalAreaWidth = areaWidth + toolTipWidth;
				float areaHeight = 600;

				float areaPosX = Screen.width / 2 - totalAreaWidth / 2;
				float areaPosY = Screen.height / 2 - areaHeight / 2;

				float toolTipPosX = areaPosX + areaWidth;

				float exitButtonWidth = toolTipWidth * .1f;
				float exitButtonHeight = areaHeight * .05f;

				// Radio buttons
				{
					GUILayout.BeginArea(new Rect(areaPosX, areaPosY, areaWidth, areaHeight), styleManager.modGUIStyle);

					if (this.radioADVNVLOriginal.OnGUIFragment(this.GetModeFromFlags()) is int newMode)
					{
						if (newMode == 0)
						{
							MODActions.SetAndSaveADV(MODActions.ModPreset.ADV);
						}
						else if (newMode == 1)
						{
							MODActions.SetAndSaveADV(MODActions.ModPreset.NVL);
						}
						else if (newMode == 2)
						{
							MODActions.SetAndSaveADV(MODActions.ModPreset.OG);
						}
						else
						{
							MODActions.SetAndSaveADV(MODActions.ModPreset.ADV);
						}
					}

					if (this.radioCensorshipLevel.OnGUIFragment(GetGlobal("GCensor")) is int censorLevel)
					{
						SetGlobal("GCensor", censorLevel);
					};

					if (this.radioLipSync.OnGUIFragment(GetGlobal("GLipSync")) is int lipSyncEnabled)
					{
						SetGlobal("GLipSync", lipSyncEnabled);
					};

					if (this.radioOpenings.OnGUIFragment(GetGlobal("GVideoOpening") - 1) is int openingVideoLevelZeroIndexed)
					{
						SetGlobal("GVideoOpening", openingVideoLevelZeroIndexed + 1);
					};

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("Advanced Options");
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					if (this.radioHideCG.OnGUIFragment(GetGlobal("GHideCG")) is int hideCG)
					{
						SetGlobal("GHideCG", hideCG);
					};

					if (this.radioArtSet.OnGUIFragment(Core.MODSystem.instance.modTextureController.GetArtStyle()) is int artStyle)
					{
						SetGlobal("GStretchBackgrounds", 0);
						Core.MODSystem.instance.modTextureController.SetArtStyle(artStyle);
					}

					{
						int currentBackground = GetGlobal("GBackgroundSet");
						if(currentBackground == 2)
						{
							if (GetGlobal("GStretchBackgrounds") == 1)
							{
								currentBackground = 3;
							}
						}
						if(this.radioBackgrounds.OnGUIFragment(currentBackground) is int background)
						{
							if(background == 3)
							{
								SetGlobal("GStretchBackgrounds", 1);
								SetGlobal("GBackgroundSet", 2);
							}
							else
							{
								SetGlobal("GStretchBackgrounds", 0);
								SetGlobal("GBackgroundSet", background);
							}
							GameSystem.Instance.SceneController.ReloadAllImages();
						}
					}

					GUILayout.Space(10);
					OnGUIRestoreSettings();

					GUILayout.Label("Save and Log Files");
					{
						GUILayout.BeginHorizontal();
						if (GUILayout.Button(new GUIContent("Show output_log.txt / Player.log",
							"This button shows the location of the 'ouput_log.txt' or 'Player.log' files\n\n" +
							"- This file is called 'output_log.txt' on Windows and 'Player.log' on MacOS/Linux\n" +
							"- This file records errors that occur during gameplay, and during game startup\n" +
							"- This file helps when the game fails start, for example\n" +
							"  - a corrupted save file\n" +
							"  - the wrong UI (sharedassets0.assets) file\n" +
							"- Note that each time the game starts up, the current log file is replaced")))
						{
							MODActions.ShowLogFolder();
						}

						if (GUILayout.Button(new GUIContent("Show Saves", "Clearing your save files can fix some issues with game startup, and resets all mod flags.\n\n" +
							"- NOTE: Steam sync will restore your saves if you manually delete them! Therefore, remember to disable steam sync, otherwise your saves will magically reappear!\n" +
							"- The 'global.dat' file stores your global unlock process and mod flags\n" +
							"- The 'qsaveX.dat' and 'saveXXX.dat' files contain individual save files. Note that these becoming corrupted can break your game\n" +
							"- It's recommended to take a backup of all your saves before you modify them")))
						{
							MODActions.ShowSaveFolder();
						}

						GUILayout.EndHorizontal();
					}

					if (GUILayout.Button(new GUIContent("Open Support Page: 07th-mod.com/wiki/Higurashi/support", "If you have problems with the game, the information on this site may help.\n\n" +
						"There are also instructions on reporting bugs, as well as a link to our Discord server to contact us directly")))
					{
						Application.OpenURL("https://07th-mod.com/wiki/Higurashi/support/");
					}

					{
						GUILayout.BeginHorizontal();
						GUILayout.Label("Custom Resolution");
						screenWidthString = GUILayout.TextField(screenWidthString);
						screenHeightString = GUILayout.TextField(screenHeightString);
						if(GUILayout.Button(new GUIContent("Set", "Sets a custom resolution - mainly for windowed mode.")))
						{
							if(int.TryParse(screenWidthString, out int new_width))
							{
								if(int.TryParse(screenHeightString, out int new_height))
								{
									if(new_width < 800)
									{
										MODToaster.Show("Width too small - must be at least 800 pixels");
										new_width = 800;
									}
									else if(new_width > 15360)
									{
										MODToaster.Show("Width too big - must be at least 15360 pixels");
										new_width = 15360;
									}
									if (new_height < 650)
									{
										MODToaster.Show("Height too small - must be at least 650 pixels");
										new_height = 650;
									}
									else if(new_height > 8640)
									{
										MODToaster.Show("Height too large - must be at least 8640 pixels");
										new_height = 8640;
									}
									screenWidthString = $"{new_width}";
									screenHeightString = $"{new_height}";
									Screen.SetResolution(new_width, new_height, Screen.fullScreen);
								}
							}
						}
						GUILayout.EndHorizontal();
					}


					GUILayout.EndArea();
				}

				// Descriptions for each button are shown on hover, like a tooltip
				GUILayout.BeginArea(new Rect(toolTipPosX, areaPosY, toolTipWidth, areaHeight), styleManager.modGUIStyle);
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("Mod Options Menu");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUIStyle toolTipStyle = GUI.skin.label;
				string displayedToolTip;
				if (GUI.tooltip == String.Empty)
				{
					if (defaultToolTipTimer.timeLeft == 0)
					{
						if(this.startupFailed)
						{
							displayedToolTip = startupFailureToolTip;
							toolTipStyle = styleManager.errorLabelStyle;
						}
						else
						{
							displayedToolTip = defaultTooltip;
						}
					}
					else
					{
						displayedToolTip = lastToolTip;
					}
				}
				else
				{
					lastToolTip = GUI.tooltip;
					displayedToolTip = GUI.tooltip;
					defaultToolTipTimer.Start(.2f);
				}
				// MUST pass in MinHeight option, otherwise Unity will get confused and assume
				// label is one line high on first draw, and subsquent changes will truncate
				// label to one line even if it is multiple lines tall.
				GUILayout.Label(displayedToolTip, toolTipStyle, GUILayout.MinHeight(areaHeight));
				GUILayout.EndArea();

				// Exit button
				GUILayout.BeginArea(new Rect(toolTipPosX + toolTipWidth - exitButtonWidth, areaPosY, exitButtonWidth, exitButtonHeight));
					if(GUILayout.Button("X"))
					{
						this.Hide();
					}
				GUILayout.EndArea();
			}
		}

		public void Show()
		{
			// Update the artset radio buttons/descriptions, as these are set by ModAddArtset() calls in init.txt at runtime
			// Technically only need to do this once after init.txt has been called, but it's easier to just do it each time menu is opened
			GUIContent[] descriptions = Core.MODSystem.instance.modTextureController.GetArtStyleDescriptions();
			for(int i = 0; i < descriptions.Count(); i++)
			{
				if(i < this.defaultArtsetDescriptions.Count())
				{
					descriptions[i] = this.defaultArtsetDescriptions[i];
				}
			}
			this.radioArtSet.SetContents(descriptions);

			this.screenWidthString = $"{Screen.width}";
			this.screenHeightString = $"{Screen.height}";

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

		private int GetGlobal(string flagName) => BurikoMemory.Instance.GetGlobalFlag(flagName).IntValue();
		private void SetGlobal(string flagName, int flagValue) => BurikoMemory.Instance.SetGlobalFlag(flagName, flagValue);
	}
}
