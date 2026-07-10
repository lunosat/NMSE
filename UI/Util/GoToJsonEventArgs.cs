using NMSE.Models;

namespace NMSE.UI.Util;

/// <summary>
/// Event arguments for requesting navigation to a specific JSON path
/// in the Raw JSON Editor panel.
/// </summary>
public sealed class GoToJsonEventArgs : EventArgs
{
	/// <summary>
	/// Initialises a new instance of the <see cref="GoToJsonEventArgs"/> class.
	/// </summary>
	/// <param name="pathSegments">
	/// The JSON path segments (e.g. "PlayerStateData", "ShipOwnership", "[3]").
	/// </param>
	public GoToJsonEventArgs(params string[] pathSegments)
	{
		PathSegments = pathSegments;
	}

	/// <summary>The JSON path segments to navigate to.</summary>
	public string[] PathSegments { get; }
}
