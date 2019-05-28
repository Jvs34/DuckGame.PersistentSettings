using DuckGame;
using Harmony;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersistentSettings;
using System.Reflection;

namespace PersistentSettings
{
	public class PersistentSettingsHandler
	{
		private IConfigurationRoot Configuration { get; }
		public PersistentMatchSettings Settings { get; set; } = new PersistentMatchSettings();

		private JsonSerializer Serializer { get; } = new JsonSerializer()
		{
			Formatting = Formatting.Indented ,
		};

		public string SettingsPath { get; }

		private static FieldInfo modifierStatus = typeof( TeamSelect2 ).GetField( "_modifierStatus" , BindingFlags.NonPublic | BindingFlags.Static );

		public Dictionary<string , bool> TeamSelect2Modifiers
		{
			get
			{
				return (Dictionary<string , bool>) modifierStatus.GetValue( null );
			}
		}

		public PersistentSettingsHandler( string modDirectory )
		{
			SettingsPath = Path.Combine( modDirectory , ".." , ".." , "Options" , "matchsettings.json" );

			Configuration = new ConfigurationBuilder()
				.AddJsonFile( SettingsPath , true )
			.Build();

			Configuration.Bind( Settings );

			ApplySettings();
		}


		/// <summary>
		/// Applies our settings onto duck game's
		/// </summary>
		public void ApplySettings( bool canStartGame = false )
		{
			IEnumerable<MatchSetting> allSettings = TeamSelect2.matchSettings.Concat( TeamSelect2.onlineSettings );

			foreach( var msKV in Settings.MatchSettings )
			{
				var settingID = msKV.Key;
				var matchSetting = msKV.Value;

				var foundMatchSetting = allSettings.FirstOrDefault( x => x.id == settingID );
				if( foundMatchSetting != null )
				{
					foundMatchSetting.value = matchSetting.GetValue();
				}

			}

			var modifiers = TeamSelect2Modifiers;
			foreach( var modifiersKV in Settings.Modifiers )
			{
				modifiers [modifiersKV.Key] = modifiersKV.Value;
			}

			Editor.activatedLevels.Clear();
			Editor.activatedLevels.AddRange( Settings.Levels );

			TeamSelect2.customLevels = Editor.activatedLevels.Count;
			TeamSelect2.prevCustomLevels = Editor.activatedLevels.Count;

			if( canStartGame && Settings.StartOnline && Level.current is TeamSelect2 teamSelect2 )
			{
				var profileBox = teamSelect2.GetBox( 0 );
				profileBox.profile.slotType = SlotType.Local;
				profileBox.Update();
				profileBox.OpenDoor();

				TeamSelect2.FillMatchmakingProfiles();
				DuckNetwork.Host( TeamSelect2.GetSettingInt( "maxplayers" ) , (NetworkLobbyType) TeamSelect2.GetSettingInt( "type" ) );
				teamSelect2.PrepareForOnline();
			}
		}


		/// <summary>
		/// Saves duckgame's match settings onto our own
		/// </summary>
		public void BuildSettings()
		{
			foreach( var matchSetting in TeamSelect2.matchSettings.Concat( TeamSelect2.onlineSettings ) )
			{
				if( !Settings.MatchSettings.TryGetValue( matchSetting.id , out PersistentMatchOption matchOption ) )
				{
					matchOption = new PersistentMatchOption();
					Settings.MatchSettings [matchSetting.id] = matchOption;
				}

				matchOption.SetValue( matchSetting.value );
			}


			foreach( var modifierKV in TeamSelect2Modifiers )
			{
				Settings.Modifiers [modifierKV.Key] = modifierKV.Value;
			}

			Settings.Levels.Clear();
			Settings.Levels.AddRange( Editor.activatedLevels );


		}

		public void SaveSettings()
		{
			BuildSettings();

			using( var fileWriter = File.CreateText( SettingsPath ) )
			{
				Serializer.Serialize( fileWriter , Settings );
			}
		}
	}

	#region HOOKS

	[HarmonyPatch( typeof( TeamSelect2 ) , nameof( TeamSelect2.UpdateModifierStatus ) )]
	internal static class OnSettingsChanged
	{
		private static void Postfix()
		{
			//I don't know how settings are handled on clients but we don't care about other people's settings
			if( Network.isActive && Network.isClient )
			{
				return;
			}

			//UpdateModifierStatus is called also inside DefaultSettings, so we want to avoid calling savesettings inside of it
			if( OnResetSettings.InsideDefaultSettings )
			{
				return;
			}


			PersistentSettingsMod.SettingsHandler?.SaveSettings();
		}
	}

	[HarmonyPatch( typeof( TeamSelect2 ) , nameof( TeamSelect2.Initialize ) )]
	internal static class OnTeamSelectionScreen
	{
		private static void Postfix()
		{
			//only apply settings if we're the host in multiplayer
			PersistentSettingsMod.SettingsHandler?.ApplySettings( true );
		}
	}

	[HarmonyPatch( typeof( TeamSelect2 ) , nameof( TeamSelect2.DefaultSettings ) )]
	internal static class OnResetSettings
	{
		//we need to at least let it run the first time
		public static bool InsideDefaultSettings { get; set; }

		//duck game wants to reset the settings everytime
		//we honestly couldn't care less so we prevent this function from running

		private static void Prefix()
		{
			InsideDefaultSettings = true;
		}


		private static void Postfix()
		{
			InsideDefaultSettings = false;

			PersistentSettingsMod.SettingsHandler?.ApplySettings();
		}
	}

	#endregion HOOKS

}

