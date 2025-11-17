using VRBuilder.MCP.Core.Models;

namespace VRBuilder.MCP.Core.Models
{
	/// <summary>
	/// Concrete response model for MCP adapters to return.
	/// Properties are named to match expected JSON { process, path, error }.
	/// </summary>
	public class VRBuilderProcessResponse
	{
		// Keep lower-case names to match documented JSON contract.
		public SerializableProcessData process { get; set; }
		public string path { get; set; }
		public string error { get; set; }

		public static VRBuilderProcessResponse Success(SerializableProcessData process, string path)
		{
			return new VRBuilderProcessResponse
			{
				process = process,
				path = path,
				error = null
			};
		}

		public static VRBuilderProcessResponse Error(string message)
		{
			return new VRBuilderProcessResponse
			{
				process = null,
				path = null,
				error = message
			};
		}
	}
}