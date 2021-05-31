//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System.Collections;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Responsible for creating a string representation of any
	///   arbitrary object.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class provides only one method, RenderObject, which is
	///   capable of creating a string representation of an object. It
	///   renders dictionaries, collections or any other object.
	/// </remarks>
	/// <threadsafety>
	///   The public static members of this class are threadsafe.
	/// </threadsafety>
	/// -->

	internal class ObjectRenderer
	{
		private ObjectRenderer() {}

		/// <summary>
		///   Creates a string representation of an object.
		/// </summary>
		/// <param name="o">The object to render. Can be null.</param>
		/// <returns>
		///   A string representation of the supplied object.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method is capable of creating a string representation
		///   of an object. For most types this method simply calls the
		///   ToString method of the supplied object. Some objects, like
		///   dictionaries or collections, are handled special.
		/// </remarks>
		/// -->

		public static string RenderObject(object o)
		{
			if (o != null)
			{
				IDictionary d = o as IDictionary;

				if (d != null)
				{
					return RenderDictionary(d);
				}
				else
				{
					ICollection c = o as ICollection;

					if (c != null)
					{
						return RenderCollection(c);
					}
					else 
					{
						return o.ToString().Trim();
					}
				}
			}
			else
			{
				return "<null>";
			}
		}

		private static string RenderCollection(ICollection c)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("[");
			if (c.Count > 0)
			{
				foreach (object o in c)
				{
					if (o == c)
					{
						sb.Append("<cycle>");
					}
					else 
					{
						sb.Append(RenderObject(o));
					}
					sb.Append(", ");
				}

				sb.Length -= 2;
			}

			sb.Append("]");
			return sb.ToString();
		}

		private static string RenderDictionary(IDictionary d)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("{");
			if (d.Count > 0)
			{
				foreach (object key in d.Keys)
				{
					object val = d[key];

					sb.Append(
						(key == d ? "<cycle>" : RenderObject(key)) + "=" +
						(val == d ? "<cycle>" : RenderObject(val)) + ", "
					);
				}

				sb.Length -= 2;
			}

			sb.Append("}");
			return sb.ToString();
		}
	}
}
