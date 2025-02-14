﻿using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="DimensionAligned"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.AlignedDimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.AlignedDimension)]
	public class DimensionAligned : Dimension
	{
		/// <summary>
		/// Linear dimension types with an oblique angle have an optional group code 52.
		/// When added to the rotation angle of the linear dimension(group code 50),
		/// it gives the angle of the extension lines
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Optional, 52)]
		public double ExtLineRotation { get; set; }

		/// <summary>
		/// Insertion point for clones of a dimension—Baseline and Continue (in OCS)
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ FirstPoint { get; set; }

		/// <inheritdoc/>
		public override double Measurement
		{
			get
			{
				return this.FirstPoint.DistanceFrom(this.SecondPoint);
			}
		}

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityDimension;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMENSION_ALIGNED;

		/// <summary>
		/// Definition point for linear and angular dimensions(in WCS)
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ SecondPoint { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.AlignedDimension;

		public override XYZ DefinitionPoint { get => base.DefinitionPoint; set => base.DefinitionPoint = value; }

		public double Offset { get; set; }

		/// <summary>
		/// Default constructor.
		/// </summary>
		public DimensionAligned() : base(DimensionType.Aligned) { }

		protected DimensionAligned(DimensionType type) : base(type)
		{
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return new BoundingBox(this.FirstPoint, this.SecondPoint);
		}

		/// <inheritdoc/>
		public override void UpdateBlock()
		{
			List<Entity> entities = new();

			base.UpdateBlock();

			double measure = this.Measurement;

			XY ref1 = this.FirstPoint.Convert<XY>();
			XY ref2 = this.SecondPoint.Convert<XY>();
			XY vec = ((ref2 - ref1).Perpendicular().Normalize());

			XY dimRef1 = ref1 + this.Offset * vec;
			XY dimRef2 = ref2 + this.Offset * vec; //this.DimLinePosition;

			double refAngle = (ref2 - ref1).GetAngle();

			// reference points
			Layer defPointLayer = new Layer("Defpoints") { PlotFlag = false };
			entities.Add(new Point(ref1.Convert<XYZ>()) { Layer = defPointLayer });
			entities.Add(new Point(ref2.Convert<XYZ>()) { Layer = defPointLayer });
			entities.Add(new Point(dimRef2.Convert<XYZ>()) { Layer = defPointLayer });

			if (!Style.SuppressFirstDimensionLine && !Style.SuppressSecondDimensionLine)
			{
				entities.Add(dimensionLine(dimRef1, dimRef2, refAngle, Style));
				//Draw start arrow
				//Draw end arrow
			}

			// extension lines
			double thisexo = Math.Sign(this.Offset) * Style.ExtensionLineOffset * Style.ScaleFactor;
			double thisexe = Math.Sign(this.Offset) * Style.ExtensionLineExtension * Style.ScaleFactor;
			if (!Style.SuppressFirstExtensionLine)
			{
				entities.Add(extensionLine(ref1 + thisexo * vec, dimRef1 + thisexe * vec, Style, Style.ExtensionLine1LineType));
			}

			if (!Style.SuppressSecondExtensionLine)
			{
				entities.Add(extensionLine(ref2 + thisexo * vec, dimRef2 + thisexe * vec, Style, Style.ExtensionLine2LineType));
			}

			// thisension text
			XY textRef = dimRef1.Mid(dimRef2);
			double gap = Style.DimensionLineGap * Style.ScaleFactor;
			double textRot = refAngle;
			if (textRot > Math.PI / 2 && textRot <= (3 * Math.PI * 0.5))
			{
				gap = -gap;
				textRot += Math.PI;
			}

			//List<string> texts = FormatDimensionText(measure, this.DimensionType, this.UserText, Style, this.Owner);

			//MText mText = DimensionText(textRef + gap * vec, AttachmentPointType.BottomCenter, textRot, texts[0], Style);
			//if (mText != null)
			//{
			//	entities.Add(mText);
			//}

			//// there might be an additional text if the code \X has been used in the thisension UserText 
			//// this additional text appears under the thisension line
			//if (texts.Count > 1)
			//{
			//	MText mText2 = DimensionText(textRef - gap * vec, AttachmentPointType.TopCenter, textRot, texts[1], Style);
			//	if (mText2 != null)
			//	{
			//		entities.Add(mText2);
			//	}
			//}

			this.TextMiddlePoint = (textRef + gap * vec).Convert<XYZ>();
			this.IsTextUserDefinedLocation = false;

			this._block.Entities.AddRange(entities);

			List<Line> lines = new List<Line>
			{
				new Line
				{
					Color = new Color(255, 0, 0),
					StartPoint = this.FirstPoint,
					EndPoint = this.SecondPoint,
				},
				new Line
				{
					Color = new Color(0, 255, 0),
					StartPoint = new XYZ(),
					EndPoint = this.DefinitionPoint,
				}
			};

			this._block.Entities.AddRange(lines);
		}
	}
}