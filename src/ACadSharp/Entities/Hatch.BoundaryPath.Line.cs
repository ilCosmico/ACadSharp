﻿using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public class Line : Edge
			{
				public override EdgeType Type => EdgeType.Line;

				/// <summary>
				/// Start point (in OCS)
				/// </summary>
				[DxfCodeValue(10, 20)]
				public XY Start { get; set; }

				/// <summary>
				/// Endpoint (in OCS)
				/// </summary>
				[DxfCodeValue(11, 21)]
				public XY End { get; set; }

				/// <inheritdoc/>
				public override Entity ToEntity()
				{
					return new Entities.Line(this.Start, this.End);
				}

				/// <inheritdoc/>
				public override BoundingBox GetBoundingBox()
				{
					return BoundingBox.FromPoints([(XYZ)this.Start, (XYZ)this.End]);
				}
			}
		}
	}
}
