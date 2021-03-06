﻿using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PersistentSettings
{
	public class PersistentSettingsMod : DuckGame.DisabledMod
	{
		public static PersistentSettingsHandler SettingsHandler { get; set; }

		public PersistentSettingsMod()
		{
#if DEBUG
			System.Diagnostics.Debugger.Launch();
#endif

			AppDomain.CurrentDomain.AssemblyResolve += ModResolve;
		}

		~PersistentSettingsMod()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= ModResolve;
		}

		protected override void OnPreInitialize()
		{
			SettingsHandler = new PersistentSettingsHandler( configuration.directory );
			HarmonyInstance.Create( GetType().Namespace ).PatchAll( Assembly.GetExecutingAssembly() );
		}

		private Assembly ModResolve( object sender , ResolveEventArgs args )
		{
			string dllFolder = Path.GetFileNameWithoutExtension( GetType().Assembly.ManifestModule?.ScopeName ?? GetType().Namespace );
			string cleanName = args.Name.Split( ',' ) [0];
			//now try to load the requested assembly


			string assemblyFolder = Path.Combine( configuration.directory , dllFolder , "Output" , "net471" );
			string assemblyPath = Path.GetFullPath( Path.Combine( assemblyFolder , cleanName + ".dll" ) );

			if( File.Exists( assemblyPath ) )
			{
				byte [] assemblyBytes = File.ReadAllBytes( assemblyPath );

				return Assembly.Load( assemblyBytes );
			}

			return null;
		}

	}
}
