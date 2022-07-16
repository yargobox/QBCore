namespace QBCore.DataSource;

/// <summary>
/// Filter Operations
/// </summary>
[Flags]
public enum FO : ulong
{
	None = 0,
	Equal = 1,
	NotEqual = 2,
	Greater = 4,
	GreaterOrEqual = 8,
	Less = 0x10,
	LessOrEqual = 0x20,
	IsNull = 0x40,
	IsNotNull = 0x80,
	In = 0x100,
	NotIn = 0x200,
	Between = 0x400,
	NotBetween = 0x800,
	Like = 0x1000,
	NotLike = 0x2000,
	BitsAnd = 0x4000,
	NotBitsAnd = 0x8000,
	BitsOr = 0x10000,
	NotBitsOr = 0x20000,
	Mod = 0x40000,
	Regex = 0x80000,
	Text = 0x100000,

	All = 0x200000,
	AnyEqual = 0x400000,
	AnyNotEqual = 0x800000,
	AnyGreater = 0x1000000,
	AnyGreaterOrEqual = 0x2000000,
	AnyLess = 0x4000000,
	AnyLessOrEqual = 0x8000000,
	AnyIn = 0x10000000,
	AnyNotIn = 0x20000000,

	TrueWhenNull = 0x40000000,
	CaseInsensitive = 0x80000000,

	SizeEqual = 0x100000000,
	SizeGreater = 0x200000000,
	SizeGreaterOrEqual = 0x400000000,
	SizeLess = 0x800000000,
	SizeLessOrEqual = 0x1000000000,

	GeoIntersects = 0x2000000000,
	GeoWithin = 0x4000000000,
	GeoWithinBox = 0x8000000000,
	GeoWithinCenter = 0x10000000000,
	GeoWithinCenterSphere = 0x20000000000,
	GeoWithinPolygon = 0x40000000000,
	Near = 0x80000000000,
	NearSphere = 0x100000000000,

	Type = 0x200000000000
}