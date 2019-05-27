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



			var modifiers = TeamSelect2Modifiers;

			BuildSettings();
		}


		/// <summary>
		/// Applies our settings onto duck game's
		/// </summary>
		public void ApplySettings()
		{
			foreach( var msKV in Settings.MatchSettings )
			{
				var settingID = msKV.Key;
				var matchSetting = msKV.Value;



			}
		}


		/// <summary>
		/// Saves duckgame's match settings onto our own
		/// </summary>
		public void BuildSettings()
		{
			foreach( var matchSetting in TeamSelect2.matchSettings )
			{

			}

			foreach( var matchSetting in TeamSelect2.onlineSettings )
			{

			}

			//foreach( var modifierKV in TeamSelect2._modifiers )
			{

			}
			/*
				DuckNetwork.Host(GetSettingInt("maxplayers"), (NetworkLobbyType)GetSettingInt("type"));
				TeamSelect2.PrepareForOnline(); 
			*/
			//TeamSelect2.SendMatchSettings

			//TeamSelect2.matchSettings , TeamSelect2.onlineSettings
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

			PersistentSettingsMod.SettingsHandler?.SaveSettings();
		}
	}



	#endregion HOOKS

}

