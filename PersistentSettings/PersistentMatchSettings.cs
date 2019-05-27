using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentSettings
{
	public class PersistentMatchSettings
	{
		public Dictionary<string , PersistentMatchOption> MatchSettings { get; set; } = new Dictionary<string , PersistentMatchOption>();

		public Dictionary<string , bool> Modifiers { get; set; } = new Dictionary<string , bool>();

		public List<string> Levels { get; set; } = new List<string>();

		/// <summary>
		/// Whether to start an online match right away with the currently saved settings
		/// </summary>
		public bool StartOnline { get; set; }
	}
}
