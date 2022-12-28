using System.Data;

namespace QBCore.DataSource.QueryBuilder;

public record QBParameter
{
	public QBParamInfo ParamInfo { get; }

	public string ParameterName => ParamInfo.ParameterName;
	public Type ClrType => ParamInfo.ClrType;
	public Enum? DbType => ParamInfo.DbType;
	public string? DbTypeName => ParamInfo.DbTypeName;
	public string? SourceColumn => ParamInfo.SourceColumn;
	public int Size => ParamInfo.Size;
	public ParameterDirection Direction => ParamInfo.Direction;
	public byte Precision => ParamInfo.Precision;
	public byte Scale => ParamInfo.Scale;
	public bool IsNullable => ParamInfo.IsNullable;
	public bool IsErrorCode => ParamInfo.IsErrorCode;
	public bool IsErrorMessage => ParamInfo.IsErrorMessage;

	protected object? _value;
	public object? Value
	{
		get
		{
			if (!HasValue) throw new InvalidOperationException($"A value of parameter '{ParameterName}' has not been set!.");

			return _value;
		}
		set
		{
			HasValue = true;
			IsValueUsed = false;
			_value = value;
		}
	}
	public bool HasValue { get; protected set; }
	public bool IsValueUsed { get; set; }

	public QBParameter(QBParamInfo paramInfo)
	{
		if (paramInfo == null) throw new ArgumentNullException(nameof(paramInfo));

		ParamInfo = paramInfo;
	}

	public QBParameter(string parameterName, Type clrType, bool isNullable = false, ParameterDirection direction = ParameterDirection.Input, Enum? dbType = null, string? dbTypeName = null, int size = 0, byte precision = 0, byte scale = 0, bool isErrorCode = false, bool isErrorMessage = false, string? sourceColumn = null)
	{
		ParamInfo = new QBParamInfo(parameterName, clrType, isNullable, direction, dbType, dbTypeName, size, precision, scale, isErrorCode, isErrorMessage, sourceColumn);
	}

	public void ResetValue()
	{
		Value = null;
		HasValue = false;
		IsValueUsed = false;
	}
}

public record QBParamInfo
{
	public string ParameterName { get; }
	public Type ClrType { get; }
	public Enum? DbType { get; }
	public string? DbTypeName { get; }
	public string? SourceColumn { get; }
	public int Size { get; }
	private readonly uint _binData;

	public ParameterDirection Direction => unchecked((ParameterDirection)(_binData >> 27));
	public byte Precision => unchecked((byte)_binData);
	public byte Scale => unchecked((byte)(_binData >> 8));
	public bool IsNullable => (_binData & 0x01000000U) == 0x01000000U;
	public bool IsErrorCode => (_binData & 0x02000000U) == 0x02000000U;
	public bool IsErrorMessage => (_binData & 0x04000000U) == 0x04000000U;

	public QBParamInfo(string parameterName, Type clrType, bool isNullable, ParameterDirection direction, Enum? dbType, string? dbTypeName = null, int size = 0, byte precision = 0, byte scale = 0, bool isErrorCode = false, bool isErrorMessage = false, string? sourceColumn = null)
	{
		if (parameterName == null) throw new ArgumentNullException(nameof(parameterName));
		if (clrType == null) throw new ArgumentNullException(nameof(clrType));
		if (string.IsNullOrEmpty(dbTypeName)) dbTypeName = null;
		if (dbTypeName?.IndexOf('(') >= 0) throw new ArgumentException(nameof(dbTypeName));
		if (size < -1) throw new ArgumentException(nameof(size));
		if (string.IsNullOrEmpty(sourceColumn)) sourceColumn = null;

		ParameterName = parameterName;
		ClrType = clrType;
		DbType = dbType;
		DbTypeName = dbTypeName;
		Size = size;
		SourceColumn = sourceColumn;
		_binData = unchecked((uint)precision | (uint)scale << 8 | (uint)direction << 27);
		if (isNullable) _binData |= 0x01000000U;
		if (isErrorCode) _binData |= 0x02000000U;
		if (isErrorMessage) _binData |= 0x04000000U;
	}
}