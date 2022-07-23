namespace QBCore.DataSource.QueryBuilder;

public record QBParameter
{
	public readonly QBParamInfo ParamInfo;

	public string Name => ParamInfo.Name;
	public System.Data.ParameterDirection Direction => ParamInfo.Direction;
	public Type UnderlyingType => ParamInfo.UnderlyingType;
	public bool IsNullable => ParamInfo.IsNullable;
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
	public QBParameter(string Name, Type UnderlyingType, bool IsNullable, System.Data.ParameterDirection Direction, string? DbTypeName = null, short Precision = 0, byte Scale = 0)
		=> ParamInfo = new QBParamInfo(Name, UnderlyingType, IsNullable, Direction, DbTypeName, Precision, Scale);
	public QBParameter(string Name, Type UnderlyingType, bool IsNullable, System.Data.ParameterDirection Direction, string DbTypeName)
		=> ParamInfo = new QBParamInfo(Name, UnderlyingType, IsNullable, Direction, DbTypeName);

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
	public readonly System.Data.ParameterDirection Direction;
	public readonly Type UnderlyingType;
	public readonly bool IsNullable;
	public readonly string? DbTypeName;
	public readonly short Precision;
	public readonly byte Scale;
	
	public QBParamInfo(string Name, Type UnderlyingType, bool IsNullable, System.Data.ParameterDirection Direction, string? DbTypeName = null, short Precision = 0, byte Scale = 0)
	{
		if (Name == null)
		{
			throw new ArgumentNullException(nameof(Name));
		}
		if (string.IsNullOrWhiteSpace(Name))
		{
			throw new ArgumentException(nameof(Name));
		}
		if (UnderlyingType == null)
		{
			throw new ArgumentNullException(nameof(UnderlyingType));
		}
		if (DbTypeName != null && (string.IsNullOrWhiteSpace(DbTypeName) || DbTypeName?.IndexOf('(') >= 0))
		{
			throw new ArgumentException(nameof(DbTypeName));
		}
		if (Precision < -1 || Precision > 8000)
		{
			throw new ArgumentException(nameof(Precision));
		}
		if (Scale < 0 || Scale > 38)
		{
			throw new ArgumentException(nameof(Scale));
		}

		this.Name = Name;
		this.UnderlyingType = UnderlyingType;
		this.IsNullable = IsNullable;
		this.Direction = Direction;
		this.DbTypeName = DbTypeName;
		this.Precision = Precision;
		this.Scale = Scale;
	}

	public QBParamInfo(string Name, Type UnderlyingType, System.Data.ParameterDirection Direction, bool IsNullable, string DbTypeName)
	{
		if (Name == null)
		{
			throw new ArgumentNullException(nameof(Name));
		}
		if (string.IsNullOrWhiteSpace(Name))
		{
			throw new ArgumentException(nameof(Name));
		}
		if (UnderlyingType == null)
		{
			throw new ArgumentNullException(nameof(UnderlyingType));
		}
		if (DbTypeName == null)
		{
			throw new ArgumentNullException(nameof(DbTypeName));
		}
		if (string.IsNullOrWhiteSpace(DbTypeName))
		{
			throw new ArgumentException(nameof(DbTypeName));
		}

		this.Name = Name;
		this.UnderlyingType = UnderlyingType;
		this.IsNullable = IsNullable;
		this.Direction = Direction;

		var i = DbTypeName.IndexOf('(');
		if (i > 0)
		{
			var j = DbTypeName.IndexOf(')', i);
			if (j < 0 || j - i - 2 <= 0 || j != DbTypeName.Length - 1)
			{
				throw new ArgumentException(nameof(DbTypeName));
			}

			this.DbTypeName = DbTypeName.Substring(0, i);

			var strArgs = DbTypeName.Substring(i + 1, j - i - 2);
			var args = strArgs.Split(',', 2);
			if (args[0].ToUpper() == "MAX")
			{
				if (args.Length > 1)
				{
					throw new ArgumentException(nameof(DbTypeName));
				}

				this.Precision = -1;
			}
			else
			{
				if (!short.TryParse(args[0], out this.Precision) || this.Precision < 0 || this.Precision > 8000)
				{
					throw new ArgumentException(nameof(DbTypeName));
				}
				if (args.Length > 1)
				{
					if (!byte.TryParse(args[1], out this.Scale) || this.Scale > 38)
					{
						throw new ArgumentException(nameof(DbTypeName));
					}
				}
			}
		}
		else
		{
			this.DbTypeName = DbTypeName;
		}
	}
}