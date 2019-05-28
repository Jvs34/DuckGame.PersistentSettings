using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentSettings
{
	public class PersistentMatchOption
	{
		public PersistentOptionType OptionType { get; set; }
		public int Value { get; set; }

		public override string ToString()
		{
			return $"{OptionType}: {Value}";
		}

		public void SetValue( object value )
		{
			OptionType = value is bool ? PersistentOptionType.Boolean : PersistentOptionType.Integer;
			Value = Convert.ToInt32( value );
		}

		public object GetValue()
		{
			return OptionType == PersistentOptionType.Boolean ? Convert.ToBoolean( Value ) : (object) Value;
		}
	}
}
