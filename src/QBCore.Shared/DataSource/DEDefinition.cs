using System.Linq.Expressions;

namespace QBCore.DataSource;

public interface IDataEntryBuilder
{
	IDataEntry Build<TDocument>(string fieldName);
	IDataEntry Build<TDocument, TField>(string fieldName);
	IDataEntry Build<TDocument>(LambdaExpression memberSelector);
	IDataEntry Build<TDocument, TField>(Expression<Func<TDocument, TField>> memberSelector);
}

public abstract class DEDefinition<TDocument>
{
	public abstract IDataEntry ToDataEntry(IDataEntryBuilder builder);

	public static implicit operator DEDefinition<TDocument>(string fieldName)
	{
		if (fieldName == null)
		{
			return null!;
		}

		return new StringDEDefinition<TDocument>(fieldName);
	}
}

public abstract class DEDefinition<TDocument, TField>
{
	public abstract IDataEntry ToDataEntry(IDataEntryBuilder builder);

	public static implicit operator DEDefinition<TDocument, TField>(string fieldName)
	{
		if (fieldName == null)
		{
			return null!;
		}

		return new StringDEDefinition<TDocument, TField>(fieldName);
	}

	public static implicit operator DEDefinition<TDocument>(DEDefinition<TDocument, TField> field)
	{
		return new UntypedDEDefinitionAdapter<TDocument, TField>(field);
	}
}

public sealed class ExpressionDEDefinition<TDocument> : DEDefinition<TDocument>
{
	private readonly LambdaExpression _memberSelector;

	public LambdaExpression Expression => _memberSelector;

	public ExpressionDEDefinition(LambdaExpression memberSelector)
	{
		if (memberSelector == null)
		{
			throw new ArgumentNullException(nameof(memberSelector));
		}
		if (memberSelector.Parameters.Count != 1)
		{
			throw new ArgumentException("Only a single parameter lambda expression is allowed.", nameof(memberSelector));
		}
		if (memberSelector.Parameters[0].Type != typeof(TDocument))
		{
			throw new ArgumentException($"The lambda expression parameter must be of type {typeof(TDocument).ToPretty()}.", nameof(memberSelector));
		}
		
		_memberSelector = memberSelector;
	}

	public override IDataEntry ToDataEntry(IDataEntryBuilder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		return builder.Build<TDocument>(_memberSelector);
	}
}

public sealed class ExpressionDEDefinition<TDocument, TField> : DEDefinition<TDocument, TField>
{
	private readonly Expression<Func<TDocument, TField>> _memberSelector;

	public Expression<Func<TDocument, TField>> Expression => _memberSelector;

	public ExpressionDEDefinition(Expression<Func<TDocument, TField>> memberSelector)
	{
		if (memberSelector == null)
		{
			throw new ArgumentNullException(nameof(memberSelector));
		}
		
		_memberSelector = memberSelector;
	}

	public override IDataEntry ToDataEntry(IDataEntryBuilder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		return builder.Build<TDocument, TField>(_memberSelector);
	}
}

public sealed class StringDEDefinition<TDocument> : DEDefinition<TDocument>
{
	private readonly string _fieldName;

	public string FieldName => _fieldName;

	public StringDEDefinition(string fieldName)
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException(nameof(fieldName));
		}
		
		_fieldName = fieldName;
	}

	public override IDataEntry ToDataEntry(IDataEntryBuilder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		return builder.Build<TDocument>(_fieldName);
	}
}

public sealed class StringDEDefinition<TDocument, TField> : DEDefinition<TDocument, TField>
{
	private readonly string _fieldName;

	public string FieldName => _fieldName;

	public StringDEDefinition(string fieldName)
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException(nameof(fieldName));
		}
		
		_fieldName = fieldName;
	}

	public override IDataEntry ToDataEntry(IDataEntryBuilder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		return builder.Build<TDocument, TField>(_fieldName);
	}
}

internal class UntypedDEDefinitionAdapter<TDocument, TField> : DEDefinition<TDocument>
{
	private readonly DEDefinition<TDocument, TField> _adaptee;

	public DEDefinition<TDocument, TField> Field => _adaptee;

	public UntypedDEDefinitionAdapter(DEDefinition<TDocument, TField> adaptee)
	{
		if (adaptee == null)
		{
			throw new ArgumentNullException(nameof(adaptee));
		}
		
		_adaptee = adaptee;
	}

	public override IDataEntry ToDataEntry(IDataEntryBuilder builder) => _adaptee.ToDataEntry(builder);
}