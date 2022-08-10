using System.Data;

namespace QBCore.DataSource.QueryBuilder;

public record QBParameter
{
	public readonly QBParamInfo ParamInfo;

	public string Name => ParamInfo.Name;
	public Type UnderlyingType => ParamInfo.UnderlyingType;
	public ParameterDirection Direction => ParamInfo.Direction;
	public bool IsNullable => ParamInfo.IsNullable;
	public bool IsErrorCode => ParamInfo.IsErrorCode;
	public bool IsErrorMessage => ParamInfo.IsErrorMessage;
	public string? DbTypeName => ParamInfo.DbTypeName;
	public short Precision => ParamInfo.Precision;
	public byte Scale => ParamInfo.Scale;

	protected object? _value;
	public object? Value
	{
		get
		{
			if (!HasValue)
			{
				throw new InvalidOperationException($"A value of parameter '{Name}' has not been set!.");
			}
			return _value;
		}
		set
		{
			HasValue = true;
			IsValueUsed = false;
			_value = value;
		}
	}
	public bool HasValue { get; set; }
	public bool IsValueUsed { get; set; }

	public QBParameter(QBParamInfo param)
		=> ParamInfo = param;
	public QBParameter(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode = false, bool isErrorMessage = false, string? dbTypeName = null, short precision = 0, byte scale = 0)
		=> ParamInfo = new QBParamInfo(name, underlyingType, isNullable, direction, isErrorCode, isErrorMessage, dbTypeName, precision, scale);
	public QBParameter(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode, bool isErrorMessage, string dbTypeName)
		=> ParamInfo = new QBParamInfo(name, underlyingType, isNullable, direction, isErrorCode, isErrorMessage, dbTypeName);

	public void ResetValue()
	{
		Value = null;
		HasValue = false;
		IsValueUsed = false;
	}
}

public record QBParamInfo
{
	public readonly string Name;
	public readonly Type UnderlyingType;
	public readonly string? DbTypeName;
	protected readonly uint _binData;

	public short Precision => unchecked((short)_binData);
	public byte Scale => unchecked((byte)(_binData >> 16));
	public bool IsNullable => (_binData & 0x01000000U) == 0x01000000U;
	public bool IsErrorCode => (_binData & 0x02000000U) == 0x02000000U;
	public bool IsErrorMessage => (_binData & 0x04000000U) == 0x04000000U;
	public ParameterDirection Direction => unchecked((ParameterDirection)(_binData >> 27));

	public QBParamInfo(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode = false, bool isErrorMessage = false, string? dbTypeName = null, short precision = 0, byte scale = 0)
	{
		if (name == null)
		{
			throw new ArgumentNullException(nameof(name));
		}
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(nameof(name));
		}
		if (underlyingType == null)
		{
			throw new ArgumentNullException(nameof(underlyingType));
		}
		if (dbTypeName != null && (string.IsNullOrWhiteSpace(dbTypeName) || dbTypeName?.IndexOf('(') >= 0))
		{
			throw new ArgumentException(nameof(dbTypeName));
		}
		if (precision < -1 || precision > 8000)
		{
			throw new ArgumentException(nameof(precision));
		}
		if (scale > 38)
		{
			throw new ArgumentException(nameof(scale));
		}

		Name = name;
		UnderlyingType = underlyingType;
		DbTypeName = dbTypeName;
		_binData = unchecked((uint)(ushort)precision | (uint)scale << 16 | (uint)direction << 27);
		if (isNullable) _binData |= 0x01000000U;
		if (isErrorCode) _binData |= 0x02000000U;
		if (isErrorMessage) _binData |= 0x04000000U;
	}

	public QBParamInfo(string name, Type underlyingType, bool isNullable, ParameterDirection direction, bool isErrorCode, bool isErrorMessage, string dbTypeName)
	{
		if (name == null)
		{
			throw new ArgumentNullException(nameof(name));
		}
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(nameof(name));
		}
		if (underlyingType == null)
		{
			throw new ArgumentNullException(nameof(underlyingType));
		}
		if (dbTypeName == null)
		{
			throw new ArgumentNullException(nameof(dbTypeName));
		}
		if (string.IsNullOrWhiteSpace(dbTypeName))
		{
			throw new ArgumentException(nameof(dbTypeName));
		}

		Name = name;
		UnderlyingType = underlyingType;

		short precision = 0;
		byte scale = 0;

		var i = dbTypeName.IndexOf('(');
		if (i > 0)
		{
			var j = dbTypeName.IndexOf(')', i);
			if (j < 0 || j - i - 2 <= 0 || j != dbTypeName.Length - 1)
			{
				throw new ArgumentException(nameof(dbTypeName));
			}

			DbTypeName = dbTypeName.Substring(0, i);

			var strArgs = dbTypeName.Substring(i + 1, j - i - 2);
			var args = strArgs.Split(',', 2);
			if (args[0].ToUpper() == "MAX")
			{
				if (args.Length > 1)
				{
					throw new ArgumentException(nameof(dbTypeName));
				}

				precision = -1;
			}
			else
			{
				if (!short.TryParse(args[0], out precision) || precision < 0 || precision > 8000)
				{
					throw new ArgumentException(nameof(dbTypeName));
				}
				if (args.Length > 1)
				{
					if (!byte.TryParse(args[1], out scale) || scale > 38)
					{
						throw new ArgumentException(nameof(dbTypeName));
					}
				}
			}
		}
		else
		{
			this.DbTypeName = dbTypeName;
		}

		_binData = unchecked((uint)(ushort)precision | (uint)scale << 16 | (uint)direction << 27);
		if (isNullable) _binData |= 0x01000000U;
		if (isErrorCode) _binData |= 0x02000000U;
		if (isErrorMessage) _binData |= 0x04000000U;
	}
}