﻿using Xunit;
using Xunit.Abstractions;
using ACadSharp.Tests.TestModels;

namespace ACadSharp.Tests.IO
{
	public class TableEntityTests : IOTestsBase
	{
		public static TheoryData<FileModel> TableSamplesFilePaths { get; } = new();

		static TableEntityTests()
		{
			loadSamples("table_samples", "*", TableSamplesFilePaths);
		}

		public TableEntityTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(TableSamplesFilePaths))]
		public void TableEntityDwg(FileModel test)
		{
			CadDocument doc = this.readDocument(test);
		}
	}
}
